using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core.Events
{
    internal class DataReceivedEventArgs : EventArgs
    {
        public string JsonResponse { get; }

        public DataReceivedEventArgs(string jsonResponse)
        {
            JsonResponse = jsonResponse;
        }
    }
}
