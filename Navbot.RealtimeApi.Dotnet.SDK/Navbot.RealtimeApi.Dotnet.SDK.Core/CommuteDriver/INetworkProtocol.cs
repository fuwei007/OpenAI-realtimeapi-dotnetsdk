using log4net;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Entity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core
{
    interface INetworkProtocol
    {
        event EventHandler<DataReceivedEventArgs> DataReceived;
        
        Task ConnectAsync(SessionConfiguration sessionConfiguration);

        Task DisconnectAsync();

        Task SendDataAsync(byte[] messageBytes);

        Task CommitAudioBufferAsync();

        Task ReceiveMessages();
        
    }
}
