﻿using log4net;
using NAudio.Wave;
using Navbot.RealtimeApi.Dotnet.SDK.Core.CommuteDriver;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Function;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Request;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using SessionUpdate = Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Request.SessionUpdate;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core
{
    internal class NetworkProtocolWebSocket : NetworkProtocolBase
    {
        private ClientWebSocket webSocketClient;
        private bool isPlayingAudio = false;
        private bool isUserSpeaking = false;
        private bool isModelResponding = false;
        private bool isRecording = false;
        private WaveInEvent waveIn;
        private WaveOutEvent? waveOut;
        private BufferedWaveProvider waveInBufferedWaveProvider;
        private readonly object playbackLock = new object();
        private ConcurrentQueue<byte[]> audioQueue = new ConcurrentQueue<byte[]>();
        private CancellationTokenSource playbackCancellationTokenSource;
        private Dictionary<FunctionCallSetting, Func<FuncationCallArgument, JObject>> FunctionRegistries = new Dictionary<FunctionCallSetting, Func<FuncationCallArgument, JObject>>();
        public SessionConfiguration SessionConfiguration { get; set; }

        private SessionUpdate sessionUpdateRequest = new SessionUpdate();

        public NetworkProtocolWebSocket(OpenAiConfig openAiConfig, ILog ilog) : base(openAiConfig, ilog)
        {

        }

        public async void InitializeAudio()
        {
            waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(24000, 16, 1)
            };
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveInBufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(24000, 16, 1))
            {
                BufferDuration = TimeSpan.FromSeconds(100),
                DiscardOnBufferOverflow = true
            };
            await ReceiveMessages();
        }


        protected override async Task ConnectAsyncCor(SessionConfiguration sessionConfiguration, Dictionary<FunctionCallSetting, Func<FuncationCallArgument, JObject>> functionRegistries)
        {
            SessionConfiguration = sessionConfiguration;
            FunctionRegistries = functionRegistries;

            webSocketClient = new ClientWebSocket();
            webSocketClient.Options.SetRequestHeader("Authorization", GetAuthorization());
            foreach (var item in OpenAiConfig.RequestHeaderOptions)
            {
                webSocketClient.Options.SetRequestHeader(item.Key, item.Value);
            }

            try
            {
                await webSocketClient.ConnectAsync(new Uri(GetOpenAIRequestUrl()), CancellationToken.None);
                InitializeAudio();
                
                log.Info("WebSocket connected!");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to connect WebSocket: {ex.Message}");
                throw new Exception($"Failed to connect WebSocket: {ex.Message}");
            }
        }

        protected override async Task DisconnectAsyncCor()
        {
            StopAudioRecording();
            StopAudioPlayback();
            ClearAudioQueue();

            isPlayingAudio = false;
            isUserSpeaking = false;
            isModelResponding = false;
            isRecording = false;


            if (webSocketClient != null && webSocketClient.State == WebSocketState.Open)
            {
                await webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                webSocketClient.Dispose();
                webSocketClient = null;
                log.Info("WebSocket closed successfully.");
            }
        }

        protected override async Task SendDataAsyncCor(byte[] messageBytes)
        {
            if (webSocketClient?.State == WebSocketState.Open)
            {
                await webSocketClient.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private async void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
        {
            if (IsMuted) return;

            string base64Audio = Convert.ToBase64String(e.Buffer, 0, e.BytesRecorded);
            var audioMessage = new JObject
            {
                ["type"] = "input_audio_buffer.append",
                ["audio"] = base64Audio
            };

            byte[] messageBytes = Encoding.UTF8.GetBytes(audioMessage.ToString());
            await SendDataAsync(messageBytes);


            OnSpeechDataAvailable(new AudioEventArgs(e.Buffer));
        }
        protected override async Task CommitAudioBufferAsyncCor()
        {
            if (webSocketClient != null && webSocketClient.State == WebSocketState.Open)
            {
                await webSocketClient.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes("{\"type\": \"input_audio_buffer.commit\"}")),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );

                await webSocketClient.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes("{\"type\": \"response.create\"}")),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
            }
        }

        //TODO Hander Data>16k
        protected override async Task ReceiveMessagesCor()
        {
            var buffer = new byte[1024 * 16];
            var messageBuffer = new StringBuilder();

            while (webSocketClient?.State == WebSocketState.Open)
            {
                var result = await webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                var chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                messageBuffer.Append(chunk);

                if (result.EndOfMessage)
                {
                    var jsonResponse = messageBuffer.ToString();
                    messageBuffer.Clear();

                    if (jsonResponse.Trim().StartsWith("{"))
                    {
                        HandleJsonResponse(jsonResponse);
                    }
                }
            }
        }

        private async void HandleJsonResponse(string jsonResponse)
        {
            try
            {
                JObject json = JObject.Parse(jsonResponse);
                var type = json["type"]?.ToString();
                log.Info($"Received type: {type}");

                BaseResponse baseResponse = BaseResponse.Parse(json);
                await HandleBaseResponse(baseResponse, json);

            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        private async Task HandleBaseResponse(BaseResponse baseResponse, JObject json)
        {
            switch (baseResponse)
            {
                case SessionCreated:
                    log.Info($"Received json: {json}");
                    SendSessionUpdate();
                    break;

                case Core.Model.Response.SessionUpdate sessionUpdate:
                    log.Info($"Received json: {json}");
                    if (!isRecording)
                        await StartAudioRecordingAsync();
                    break;

                case Core.Model.Response.SpeechStarted:
                    log.Info($"Received json: {json}");
                    HandleUserSpeechStarted();
                    break;

                case SpeechStopped:
                    log.Info($"Received json: {json}");
                    HandleUserSpeechStopped();
                    break;

                case ResponseDelta responseDelta:
                    await HandleResponseDelta(responseDelta);
                    break;

                case TranscriptionCompleted transcriptionCompleted:
                    log.Info($"Received json: {json}");
                    OnSpeechTextAvailable(new TranscriptEventArgs(transcriptionCompleted.Transcript));
                    break;

                case ResponseAudioTranscriptDone textDone:
                    log.Info($"Received json: {json}");
                    OnPlaybackTextAvailable(new TranscriptEventArgs(textDone.Transcript));
                    break;

                case FuncationCallArgument argument:
                    log.Info($"Received json: {json}");
                    HandleFunctionCall(argument);
                    break;

                case ConversationItemCreated:
                case BufferCommitted:
                case ResponseCreated:
                case ConversationCreated:
                case TranscriptionFailed:
                case ConversationItemTruncate:
                case ConversationItemDeleted:
                case BufferClear:
                case ResponseDone:
                case ResponseOutputItemAdded:
                case ResponseOutputItemDone:
                case ResponseContentPartAdded:
                case ResponseContentPartDone:
                case ResponseTextDone:
                case ResponseFunctionCallArgumentsDelta:
                case RateLimitsUpdated:
                    log.Info($"Received json: {json}");
                    break;

                case ResponseError error:
                    log.Error(error);
                    log.Error($"Received json: {json}");
                    break;
            }
        }

        private async Task HandleResponseDelta(ResponseDelta responseDelta)
        {
            switch (responseDelta.ResponseDeltaType)
            {
                case ResponseDeltaType.AudioTranscriptDelta:
                    // Handle AudioTranscriptDelta if necessary
                    log.Info($"Received json: {responseDelta}");
                    break;

                case ResponseDeltaType.AudioDelta:
                    log.Debug($"Received json: {responseDelta}");
                    ProcessAudioDelta(responseDelta);
                    break;

                case ResponseDeltaType.AudioDone:
                    log.Info($"Received json: {responseDelta}");
                    isModelResponding = false;
                    ResumeRecording();
                    break;
                case ResponseDeltaType.TextDelta:
                    log.Info($"Received json: {responseDelta}");
                    break;
            }
        }
        private async void SendSessionUpdate()
        {
            var sessionUpdateRequest = new Model.Request.SessionUpdate
            {
                session = this.SessionConfiguration.ToSession(FunctionRegistries),
            };

            string message = JsonConvert.SerializeObject(sessionUpdateRequest);
            await SendDataAsync(Encoding.UTF8.GetBytes(message));
            log.Debug("Sent session update: " + message);
        }
        private void HandleUserSpeechStarted()
        {
            // 1) Stop playback *before* setting isModelResponding = false
            isUserSpeaking = true;
            log.Debug("User started speaking.");
            StopAudioPlayback();
            // 2) Now set isModelResponding = false after we already canceled playback
            isModelResponding = false;

            ClearAudioQueue();

            OnSpeechStarted(new EventArgs());
            OnSpeechActivity(true);
        }
        private void HandleUserSpeechStopped()
        {
            isUserSpeaking = false;
            log.Debug("User stopped speaking. Processing audio queue...");
            ProcessAudioQueue();

            OnSpeechActivity(false, new AudioEventArgs(new byte[0]));
        }
        private void ProcessAudioDelta(ResponseDelta responseDelta)
        {
            if (isUserSpeaking) return;

            var base64Audio = responseDelta.Delta;
            if (!string.IsNullOrEmpty(base64Audio))
            {
                var audioBytes = Convert.FromBase64String(base64Audio);
                audioQueue.Enqueue(audioBytes);
                isModelResponding = true;

                OnPlaybackDataAvailable(new AudioEventArgs(audioBytes));
                StopAudioRecording();
                OnSpeechActivity(true);
            }
        }
        private void ResumeRecording()
        {
            if (waveIn != null && !isRecording && !isModelResponding)
            {
                waveIn.StartRecording();
                isRecording = true;
                log.Debug("Recording resumed after audio playback.");
                OnSpeechStarted(new EventArgs());
                OnSpeechActivity(true);
            }
        }

        private async Task StartAudioRecordingAsync()
        {
            waveIn.StartRecording();
            isRecording = true;

            OnSpeechStarted(new EventArgs());

            log.Info("Audio recording started.");
        }



        private void StopAudioRecording()
        {
            if (waveIn != null && isRecording)
            {
                waveIn.StopRecording();

                isRecording = false;
                log.Debug("Recording stopped to prevent echo.");
            }
        }

        public void Stop()
        {
            StopAudioRecording();
            StopAudioPlayback();
            ClearAudioQueue();

            isPlayingAudio = false;
            isUserSpeaking = false;
            isModelResponding = false;
            isRecording = false;
        }

        private void StopAudioPlayback()
        {
            log.Debug("StopAudioPlayback() called...");
            if (playbackCancellationTokenSource != null && !playbackCancellationTokenSource.IsCancellationRequested)
            {
                playbackCancellationTokenSource.Cancel();
                log.Info("AI audio playback stopped due to user interruption.");
            }

            if (waveOut != null)
            {
                try
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                }
                catch (Exception ex)
                {
                    log.Error($"Error stopping waveOut: {ex.Message}");
                }
                finally
                {
                    waveOut = null; // Clear reference so we can re-init next time
                }
            }
            // 3) Clear out any leftover audio in the buffer
            waveInBufferedWaveProvider?.ClearBuffer();

            // 4) Indicate playback ended in the logs/events
            log.Info("AI audio playback force-stopped due to user interruption.");
            OnPlaybackEnded(EventArgs.Empty);
        }

        private void ProcessAudioQueue()
        {
            if (!isPlayingAudio)
            {
                isPlayingAudio = true;
                audioQueue = new ConcurrentQueue<byte[]>();
                playbackCancellationTokenSource = new CancellationTokenSource();

                Task.Run(() =>
                {
                    try
                    {
                        OnPlaybackStarted(new EventArgs());

                        using var waveOut = new WaveOutEvent { DesiredLatency = 200 };
                        // 1) Create and store waveOut so we can stop it later

                        waveOut.PlaybackStopped += (s, e) => { OnPlaybackEnded(new EventArgs()); };
                        waveOut.Init(waveInBufferedWaveProvider);
                        waveOut.Play();

                        while (!playbackCancellationTokenSource.Token.IsCancellationRequested)
                        {
                            if (audioQueue.TryDequeue(out var audioData))
                            {
                                waveInBufferedWaveProvider.AddSamples(audioData, 0, audioData.Length);

                                //float[] waveform = ExtractWaveform(audioData);
                            }
                            else
                            {
                                log.Debug("No audio in queue; waiting...");
                                Task.Delay(100).Wait();
                            }
                        }

                        log.Debug("Playback loop exited; calling waveOut.Stop()");
                        if (waveOut != null)
                        {
                            waveOut.Stop();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error($"Error during audio playback: {ex.Message}");
                    }
                    finally
                    {
                        isPlayingAudio = false;
                    }
                });
            }
        }



        private void HandleFunctionCall(FuncationCallArgument argument)
        {
            string functionName = argument.Name;
            foreach (var item in FunctionRegistries)
            {
                if (item.Key.Name == functionName)
                {
                    JObject functionCallResultJson = item.Value(argument);
                    var callId = argument.CallId;
                    SendFunctionCallResult(functionCallResultJson, callId);
                }
            }
        }

        private void SendFunctionCallResult(JObject functionCallResultJson, string callId)
        {
            string outputStr = functionCallResultJson == null ? "" : functionCallResultJson.ToString();
            var functionCallResult = new FunctionCallResult
            {
                Type = "conversation.item.create",
                Item = new FunctionCallItem
                {
                    Type = "function_call_output",
                    Output = outputStr,
                    CallId = callId
                }
            };

            string resultJsonString = JsonConvert.SerializeObject(functionCallResult);

            SendDataAsync(Encoding.UTF8.GetBytes(resultJsonString)).Wait();
            Console.WriteLine("Sent function call result: " + resultJsonString);

            ResponseCreate responseJson = new ResponseCreate();
            string rpJsonString = JsonConvert.SerializeObject(responseJson);

            SendDataAsync(Encoding.UTF8.GetBytes(rpJsonString)).Wait();
        }

        private void ClearAudioQueue()
        {
            lock (playbackLock)
            {
                while (audioQueue.TryDequeue(out _)) { }
                log.Info("Audio queue cleared.");
            }
        }
    }
}