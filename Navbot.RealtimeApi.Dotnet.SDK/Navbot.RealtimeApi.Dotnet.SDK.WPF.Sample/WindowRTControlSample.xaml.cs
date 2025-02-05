using Navbot.RealtimeApi.Dotnet.SDK.Core.Enum;
using Navbot.RealtimeApi.Dotnet.SDK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Navbot.RealtimeApi.Dotnet.SDK.WPF.Sample
{
    /// <summary>
    /// Interaction logic for WindowRTControlSample.xaml
    /// </summary>
    public partial class WindowRTControlSample : Window
    {
        public WindowRTControlSample()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string openAiApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_EASTUS2_API_KEY");
            //string openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";

            myRealtimeControlFull.OpenAiApiKey = openAiApiKey;
            myRealtimeControlFull.NetworkProtocolType = NetworkProtocolType.WebSocket;
            myRealtimeControlFull.VoiceVisualEffect = VoiceVisualEffect.SoundWave;
            myRealtimeControlFull.ShowChatTranscript = true;
            myRealtimeControlFull.ShowButtonBar = true;
            myRealtimeControlFull.Instructions = "You are a fun and helpful minion, loyal to your master, your goal is to entertain as well as to help. Also you strive to be brief and short, but also emotional.";
        }
    }
}
