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
