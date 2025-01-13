using log4net;
using log4net.Config;
using NAudio.Wave;
using Navbot.RealtimeApi.Dotnet.SDK.Core.CommuteDriver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataReceivedEventArgs = Navbot.RealtimeApi.Dotnet.SDK.Core.Events.DataReceivedEventArgs;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core
{
    internal class WebSocketCommuteDriver : DriverBase, ICommuteDriver
    {

        public event EventHandler<DataReceivedEventArgs> ReceivedDataAvailable;
        private ClientWebSocket webSocketClient;
        private Func<string, Task> dataReceivedCallback;

        //internal event EventHandler<AudioEventArgs> PlaybackDataAvailable;

        public WebSocketCommuteDriver(string apiKey, ILog log) : base(apiKey, log)
        {

        }

        public async Task ConnectAsync()
        {
            webSocketClient = new ClientWebSocket();
            webSocketClient.Options.SetRequestHeader("Authorization", GetAuthorization());
            foreach (var item in RequestHeaderOptions)
            {
                webSocketClient.Options.SetRequestHeader(item.Key, item.Value);
            }

            try
            {
                await webSocketClient.ConnectAsync(new Uri(GetOpenAIRequestUrl()), CancellationToken.None);
                log.Info("WebSocket connected!");
            }
            catch (Exception ex)
            {
                log.Error($"Failed to connect WebSocket: {ex.Message}");
                throw new Exception($"Failed to connect WebSocket: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (webSocketClient != null && webSocketClient.State == WebSocketState.Open)
            {
                await webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                webSocketClient.Dispose();
                webSocketClient = null;
                log.Info("WebSocket closed successfully.");
            }
        }

        public async Task SendDataAsync(byte[]? messageBytes)
        {
            await webSocketClient.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }


        public async Task CommitAudioBufferAsync()
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
        public async Task ReceiveMessages()
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
                        OnReceivedDataAvailable(new DataReceivedEventArgs(jsonResponse));
                    }
                }
            }
        }



        protected virtual void OnReceivedDataAvailable(DataReceivedEventArgs e)
        {
            ReceivedDataAvailable?.Invoke(this, e);
        }


        
    }
}
