using log4net;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core
{
    internal class WebRTCCommuteDriver : ICommuteDriver
    {
        public event EventHandler<EventArgs> ReceivedDataAvailable;


        //internal event EventHandler<AudioEventArgs> PlaybackDataAvailable;

        public async Task ConnectAsync()
        {
          

            //webSocketClient = new ClientWebSocket();
            //webSocketClient.Options.SetRequestHeader("Authorization", GetAuthorization());
            //foreach (var item in this.RequestHeaderOptions)
            //{
            //    webSocketClient.Options.SetRequestHeader(item.Key, item.Value);
            //}

            //try
            //{
            //    await webSocketClient.ConnectAsync(new Uri(this.GetOpenAIRequestUrl()), CancellationToken.None);
            //    log.Info("WebSocket connected!");
            //}
            //catch (Exception ex)
            //{
            //    log.Error($"Failed to connect WebSocket: {ex.Message}");
            //    throw new Exception($"Failed to connect WebSocket: {ex.Message}");
            //}
        }

        public async Task DisconnectAsync()
        {
           
        }

        public async Task SendDataAsync()
        {
            
        }

        //private async Task SendTextAsync() { }

        protected virtual void OnReceivedDataAvailable(EventArgs e)
        {
            ReceivedDataAvailable?.Invoke(this, e);
        } 
    }
}
