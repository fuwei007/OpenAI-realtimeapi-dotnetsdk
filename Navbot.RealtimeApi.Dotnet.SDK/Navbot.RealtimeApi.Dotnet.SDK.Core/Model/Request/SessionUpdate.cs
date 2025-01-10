using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core.Model.Request
{
    public class SessionUpdate
    {
        public string type { get; set; } = "session.update";
        public Session session { get; set; }
    }

    public class Session
    {
        public string instructions { get; set; }
        public TurnDetection turn_detection { get; set; }
        public string voice { get; set; }
        public double temperature { get; set; }
        public long max_response_output_tokens { get; set; }
        public List<string> modalities { get; set; }
        public string input_audio_format { get; set; }
        public string output_audio_format { get; set; }
        public AudioTranscription input_audio_transcription { get; set; }
        public string tool_choice { get; set; }
        public JArray tools { get; set; }
    }

    public class TurnDetection
    {
        public string type { get; set; }
        public double threshold { get; set; }
        public int prefix_padding_ms { get; set; }
        public int silence_duration_ms { get; set; }
    }

    public class AudioTranscription
    {
        public string model { get; set; }
    }
}
