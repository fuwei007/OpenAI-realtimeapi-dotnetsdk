using log4net;
using log4net.Config;
using Microsoft.MixedReality.WebRTC;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.MixedReality.WebRTC.DataChannel;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core.Communication
{
    public class WebRTCCommunication
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private PeerConnection pc;
        private DataChannel dataChannel;
        private DeviceAudioTrackSource _microphoneSource;
        private LocalAudioTrack _localAudioTrack;
        private Transceiver _audioTransceiver;
        private WaveInEvent waveInEvent;
        private WaveOutEvent waveOutEvent;
        private BufferedWaveProvider waveProvider;
        private static readonly HttpClient client = new HttpClient();
        private static readonly string OpenaiApiUrl = "https://api.openai.com/v1/realtime";
        private static readonly string DefaultInstructions = "You are helpful and have some tools installed.\n\nIn the tools you have the ability to control a robot hand.";

        public WebRTCCommunication() : this("")
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
        }
        public WebRTCCommunication(string apiKey)
        {
            ApiKey = apiKey;

            waveInEvent = new WaveInEvent();
            waveInEvent.DataAvailable += WaveInEvent_DataAvailable;

            waveProvider = new BufferedWaveProvider(new WaveFormat(16000, 16, 1));
            waveProvider.BufferLength = 1024 * 16;
            waveProvider.DiscardOnBufferOverflow = true;

            waveOutEvent = new WaveOutEvent();
            waveOutEvent.Volume = 0.5f;
            waveOutEvent.Init(waveProvider);
        }

        public string ApiKey { get; set; }


        public void StartCall()
        {
            waveInEvent.StartRecording();

            Task.Run(() => InitializeConnectionAsync());
        }

        public void StopCall()
        {
            log.Debug("Stopping call and releasing resources...");

            try
            {
                if (waveInEvent != null)
                {
                    waveInEvent.StopRecording();
                    waveInEvent.Dispose();
                    waveInEvent = null;
                }

                if (waveOutEvent != null)
                {
                    waveOutEvent.Stop();
                    waveOutEvent.Dispose();
                    waveOutEvent = null;
                }

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

                if (waveProvider != null)
                {
                    waveProvider = null;
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

                log.Info("All resources have been released successfully.");
            }
            catch (Exception ex)
            {
                log.Info($"Error during resource cleanup: {ex.Message}");
            }
        }


        public async Task InitializeConnectionAsync()
        {
            log.Info($"Initialize Connection");

            var tokenResponse = await GetSessionAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(tokenResponse);
            string ephemeralKey = data.client_secret.value;

            pc = new PeerConnection();

            var config = new PeerConnectionConfiguration
            {
                IceServers = new List<IceServer> {
                            new IceServer{ Urls = { "stun:stun.l.google.com:19302" } }
                        }
            };

            await pc.InitializeAsync(config);
            //await pc.InitializeAsync();

            pc.IceStateChanged += Pc_IceStateChanged;

            dataChannel = await pc.AddDataChannelAsync(1, "response", true, true);

            dataChannel.MessageReceived += DataChannel_MessageReceived;
            dataChannel.StateChanged += DataChannel_StateChanged;

            _microphoneSource = await DeviceAudioTrackSource.CreateAsync();
            _localAudioTrack = LocalAudioTrack.CreateFromSource(_microphoneSource, new LocalAudioTrackInitConfig { trackName = "microphone_track" });

            _audioTransceiver = pc.AddTransceiver(MediaKind.Audio);
            _audioTransceiver.DesiredDirection = Transceiver.Direction.SendReceive;
            _audioTransceiver.LocalAudioTrack = _localAudioTrack;

            pc.LocalSdpReadytoSend += async (sdp) =>
            {
                log.Info("Local SDP offer (copy to Peer 2):");
                //log.Info(Convert.ToBase64String(Encoding.UTF8.GetBytes(sdp.Content)));

                string modifiedSdp = SetPreferredCodec(sdp.Content, "opus/48000/2");

                string openAiSdpStr = await ConnectRTCAsync(ephemeralKey, new SdpMessage { Content = modifiedSdp, Type = SdpMessageType.Offer });
                //string openAiSdpStr = await ConnectRTCAsync(ephemeralKey, new SdpMessage { Content = sdp.Content, Type = SdpMessageType.Offer });

                SdpMessage openAiSdpObj = new SdpMessage()
                {
                    Content = openAiSdpStr,
                    Type = SdpMessageType.Answer
                };

                await pc.SetRemoteDescriptionAsync(openAiSdpObj);
            };

            pc.AudioTrackAdded += Pc_AudioTrackAdded;
            pc.DataChannelAdded += Pc_DataChannelAdded;

            bool offer = pc.CreateOffer();
        }


        private void Pc_IceStateChanged(IceConnectionState newState)
        {
            log.Info($"ICE State Changed: {newState}");
            if (newState == IceConnectionState.Connected)
            {
                log.Info("ICE Connected, dataChannel should be open soon.");
            }
            else if (newState == IceConnectionState.Failed)
            {
                log.Info("ICE Connection Failed. Please check network configurations.");
            }
        }

        private void WaveInEvent_DataAvailable(object? sender, WaveInEventArgs e)
        {
            byte[] audioData = e.Buffer;

            if (dataChannel?.State == ChannelState.Open)
            {
                dataChannel.SendMessage(audioData);
            }
        }

        private void Pc_AudioTrackAdded(RemoteAudioTrack track)
        {
            track.AudioFrameReady += Track_AudioFrameReady;
        }

        private void Pc_DataChannelAdded(DataChannel channel)
        {
            channel.MessageReceived += Channel_MessageReceived;
        }

        private void Channel_MessageReceived(byte[] message)
        {
            string decodedMessage = Encoding.UTF8.GetString(message);
            log.Info("Received message: " + decodedMessage);
        }

        private void Track_AudioFrameReady(AudioFrame frame)
        {
            if (frame.audioData == IntPtr.Zero || frame.sampleCount == 0)
            {
                log.Info("Audio frame is invalid.");
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
                waveProvider?.AddSamples(pcmData, 0, pcmData.Length);
            }
            else
            {
                waveProvider.AddSamples(audioData, 0, audioData.Length);
            }

            if (waveOutEvent != null && waveOutEvent.PlaybackState != PlaybackState.Stopped)
            {
                waveOutEvent.Play();
            }
        }

        private void DataChannel_StateChanged()
        {
            log.Info($"DataChannel_State:{dataChannel?.State}");
            if (dataChannel?.State == ChannelState.Open)
            {
                SendSessionUpdate();
            }
        }

        private void SendSessionUpdate()
        {
            var sessionUpdateRequest = new
            {
                type = "session.update",
                session = new
                {
                    instructions = DefaultInstructions,
                    turn_detection = new
                    {
                        type = "server_vad",
                        threshold = 0.6,
                        prefix_padding_ms = 300,
                        silence_duration_ms = 500
                    },
                    voice = "alloy",
                    temperature = 1,
                    max_response_output_tokens = 4096,
                    modalities = new List<string> { "text", "audio" },
                    input_audio_format = "pcm16",
                    output_audio_format = "pcm16",
                    input_audio_transcription = new { model = "whisper-1" },
                    tool_choice = "auto",
                }
            };

            string message = JsonConvert.SerializeObject(sessionUpdateRequest);
            dataChannel.SendMessage(Encoding.UTF8.GetBytes(message));

            log.Info("Sent session update: " + message);
        }

        
        private void DataChannel_MessageReceived(byte[] message)
        {
            string jsonMessage = Encoding.UTF8.GetString(message);
            log.Info("Received message: " + jsonMessage);
        }


        private string SetPreferredCodec(string sdp, string preferredCodec)
        {
            var lines = sdp.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            var audioLineIndex = lines.FindIndex(line => line.StartsWith("m=audio"));
            if (audioLineIndex == -1)
            {
                log.Info("No audio line found in SDP.");
                return sdp;
            }

            var audioLineParts = lines[audioLineIndex].Split(' ').ToList();

            var codecPayloadTypes = audioLineParts.Skip(3).ToList(); 

            var codecMap = new Dictionary<string, string>(); // payload type -> codec name
            foreach (var line in lines)
            {
                if (line.StartsWith("a=rtpmap"))
                {
                    var parts = line.Split(new[] { ':', ' ' }, StringSplitOptions.None);
                    if (parts.Length >= 3)
                    {
                        codecMap[parts[1]] = parts[2]; // payload type -> codec name
                    }
                }
            }

            var preferredPayloadType = codecMap.FirstOrDefault(x => x.Value.StartsWith(preferredCodec)).Key;
            if (preferredPayloadType == null)
            {
                log.Info($"Preferred codec '{preferredCodec}' not found in SDP.");
                return sdp;
            }

            audioLineParts.Remove(preferredPayloadType);
            audioLineParts.Insert(3, preferredPayloadType);

            lines[audioLineIndex] = string.Join(" ", audioLineParts);

            return string.Join("\r\n", lines);
        }

        public async Task<string> GetSessionAsync()
        {
            try
            {
                var requestBody = new
                {
                    model = "gpt-4o-realtime-preview-2024-12-17",
                    voice = "verse",
                    modalities = new[] { "audio", "text" },
                    instructions = "You are a friendly assistant."
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/realtime/sessions")
                {
                    Headers =
                    {
                        { "Authorization", $"Bearer {ApiKey}" }
                    },
                    Content = content
                };

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
                log.Info($"An error occurred: {ex.Message}");
                return null;
            }
        }

        public async Task<string> ConnectRTCAsync(string ephemeralKey, SdpMessage localSdp)
        {

            var url = $"{OpenaiApiUrl}?model=gpt-4o-realtime-preview-2024-12-17&instructions={Uri.EscapeDataString(DefaultInstructions)}&voice=ash";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ephemeralKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/sdp"));

            var content = new StringContent(localSdp.Content);
            log.Info("本地sdp：" + localSdp.Content);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/sdp");

            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                log.Info($"Error: {response.StatusCode} - {errorResponse}");
                throw new HttpRequestException($"OpenAI API error: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync();
        }



    }
}
