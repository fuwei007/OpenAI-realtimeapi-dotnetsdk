﻿using log4net;
using Microsoft.MixedReality.WebRTC;
using NAudio.Wave;
using Navbot.RealtimeApi.Dotnet.SDK.Core.CommuteDriver;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using static Microsoft.MixedReality.WebRTC.DataChannel;
using SessionUpdate = Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Request.SessionUpdate;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core
{
    internal class NetworkProtocolWebRTC : NetworkProtocolBase
    {
        private PeerConnection pc;
        private DataChannel dataChannel;
        private DeviceAudioTrackSource _microphoneSource;
        private LocalAudioTrack _localAudioTrack;
        private SessionUpdate _sessionUpdate;
        private string ephemeralKey;

        public event EventHandler<AudioEventArgs> RtcPlaybackDataAvailable;

        public NetworkProtocolWebRTC(OpenAiConfig openAiConfig, ILog ilog) : base(openAiConfig, ilog)
        {
            pc = new PeerConnection();
            _sessionUpdate = new SessionUpdate();
        }

        protected override async Task ConnectAsyncCor(SessionConfiguration sessionConfiguration, Dictionary<FunctionCallSetting, Func<FunctionCallArgument, JObject>> functionRegistries)
        {
            _sessionUpdate.session = sessionConfiguration.ToSession(functionRegistries);

            Log.Info($"Initialize Connection");

            var tokenResponse = await GetSessionAsync(GetAuthorization());
            var data = JsonConvert.DeserializeObject<dynamic>(tokenResponse);
            ephemeralKey = data.client_secret.value;


            var config = new PeerConnectionConfiguration
            {
                IceServers = new List<IceServer> {
                            new IceServer{ Urls = { "stun:stun.l.google.com:19302" } }
                        }
            };

            await pc.InitializeAsync(config);

            pc.IceStateChanged += Pc_IceStateChanged;

            dataChannel = await pc.AddDataChannelAsync(1, "response", true, true);

            dataChannel.MessageReceived += DataChannel_MessageReceived;
            dataChannel.StateChanged += DataChannel_StateChanged;

            _microphoneSource = await DeviceAudioTrackSource.CreateAsync();
            _localAudioTrack = LocalAudioTrack.CreateFromSource(_microphoneSource, new LocalAudioTrackInitConfig { trackName = "microphone_track" });

            Transceiver _audioTransceiver = pc.AddTransceiver(MediaKind.Audio);
            _audioTransceiver.DesiredDirection = Transceiver.Direction.SendReceive;
            _audioTransceiver.LocalAudioTrack = _localAudioTrack;

            pc.LocalSdpReadytoSend += Pc_LocalSdpReadytoSendAsync;

            pc.AudioTrackAdded += Pc_AudioTrackAdded;
            pc.DataChannelAdded += Pc_DataChannelAdded;

            bool offer = pc.CreateOffer();
        }

        private async void Pc_LocalSdpReadytoSendAsync(SdpMessage sdpMessage)
        {
            string openAiSdpStr = await ConnectRTCAsync(ephemeralKey, new SdpMessage { Content = sdpMessage.Content, Type = SdpMessageType.Offer });
            SdpMessage openAiSdpObj = new SdpMessage()
            {
                Content = openAiSdpStr,
                Type = SdpMessageType.Answer
            };

            await pc.SetRemoteDescriptionAsync(openAiSdpObj);
        }

        private void Pc_DataChannelAdded(DataChannel channel)
        {
            channel.MessageReceived += Channel_MessageReceived;
        }

        private void Channel_MessageReceived(byte[] message)
        {
            string decodedMessage = Encoding.UTF8.GetString(message);
            Log.Info("Received message: " + decodedMessage);
        }

        private void Pc_AudioTrackAdded(RemoteAudioTrack track)
        {
            track.AudioFrameReady += Track_AudioFrameReady;
        }

        private void Track_AudioFrameReady(AudioFrame frame)
        {
            if (frame.audioData == IntPtr.Zero || frame.sampleCount == 0)
            {
                Log.Info("Audio frame is invalid.");
                return;
            }

            byte[] audioData = new byte[frame.sampleCount * (frame.bitsPerSample / 8) * (int)frame.channelCount];
            Marshal.Copy(frame.audioData, audioData, 0, audioData.Length);

            if (frame.bitsPerSample == 16)
            {
                short[] shortAudioData = new short[audioData.Length / 2];
                Buffer.BlockCopy(audioData, 0, shortAudioData, 0, audioData.Length);

                byte[] pcmData = new byte[shortAudioData.Length * 2];
                Buffer.BlockCopy(shortAudioData, 0, pcmData, 0, pcmData.Length);

                RtcPlaybackDataAvailable.Invoke(this, new AudioEventArgs(pcmData));
            }
        }

        private void Pc_IceStateChanged(IceConnectionState newState)
        {
            Log.Info($"ICE State Changed: {newState}");
            if (newState == IceConnectionState.Connected)
            {
                Log.Info("ICE Connected, dataChannel should be open soon.");
            }
            else if (newState == IceConnectionState.Failed)
            {
                Log.Info("ICE Connection Failed. Please check network configurations.");
            }
        }

        private void DataChannel_MessageReceived(byte[] message)
        {
            string jsonMessage = Encoding.UTF8.GetString(message);
            Log.Info("Received message: " + jsonMessage);
        }

        private void DataChannel_StateChanged()
        {
            Log.Info($"DataChannel_State:{dataChannel?.State}");
            if (dataChannel?.State == ChannelState.Open)
            {
                string message = JsonConvert.SerializeObject(_sessionUpdate);
                dataChannel.SendMessage(Encoding.UTF8.GetBytes(message));

                Log.Info("Sent session update: " + message);
            }
        }

        protected override async Task DisconnectAsyncCor()
        {
            try
            {
                if (_localAudioTrack != null)
                {
                    _localAudioTrack.Dispose();
                    _localAudioTrack = null;
                }

                if (_microphoneSource != null)
                {
                    _microphoneSource.Dispose();
                    _microphoneSource = null;
                }

                if (dataChannel != null)
                {
                    dataChannel.StateChanged -= DataChannel_StateChanged;
                    dataChannel.MessageReceived -= DataChannel_MessageReceived;
                    dataChannel = null;
                }

                if (pc != null)
                {
                    pc.IceStateChanged -= Pc_IceStateChanged;
                    pc.LocalSdpReadytoSend -= null;
                    pc.AudioTrackAdded -= Pc_AudioTrackAdded;
                    pc.DataChannelAdded -= Pc_DataChannelAdded;

                    pc.Close();
                    pc.Dispose();
                    pc = null;
                }

            }
            catch (Exception ex)
            {
                Log.Error($"Error during resource cleanup: {ex.Message}");
            }
        }

        protected override async Task<Task> CommitAudioBufferAsyncCor()
        {
            return Task.CompletedTask;
        }

        protected override Task SendDataAsyncCor(byte[]? messageBytes)
        {
            return Task.CompletedTask;
        }

        private async Task<string> GetSessionAsync(string authorization)
        {
            try
            {
                var requestBody = new { model = OpenAiConfig.Model, voice = OpenAiConfig.Voice, };
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/realtime/sessions")
                {
                    Headers =
                    {
                        { "Authorization", $"{authorization}" }
                    },
                    Content = content
                };

                HttpClient client = new HttpClient();
                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            catch (Exception ex)
            {
                Log.Info($"An error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task<string> ConnectRTCAsync(string ephemeralKey, SdpMessage localSdp)
        {
            var url = $"{OpenAiConfig.OpenApiRtcUrl.TrimEnd('/').TrimEnd('?')}?model={OpenAiConfig.Model}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ephemeralKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/sdp"));

            var content = new StringContent(localSdp.Content);
            Log.Info("local sdp：" + localSdp.Content);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/sdp");

            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                Log.Info($"Error: {response.StatusCode} - {errorResponse}");
                throw new HttpRequestException($"OpenAI API error: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        protected override async Task<Task> ReceiveMessagesCor()
        {
            return Task.CompletedTask;
        }


    }
}
