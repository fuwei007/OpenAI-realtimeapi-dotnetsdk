using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core
{
    interface ICommuteDriver
    {
       event EventHandler<EventArgs> ReceivedDataAvailable;

        Task ConnectAsync();

        Task DisconnectAsync();

        Task SendDataAsync();


    }
}
