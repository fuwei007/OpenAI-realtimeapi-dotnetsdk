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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";

            myRealtimeControlFull.OpenAiApiKey = openAiApiKey;
            myRealtimeControlFull.NetworkProtocolType = NetworkProtocolType.WebSocket;
            myRealtimeControlFull.VoiceVisualEffect = VoiceVisualEffect.SoundWave;
            myRealtimeControlFull.ShowChatTranscript = true;
            myRealtimeControlFull.ShowButtonBar = true;
        }
    }
}
