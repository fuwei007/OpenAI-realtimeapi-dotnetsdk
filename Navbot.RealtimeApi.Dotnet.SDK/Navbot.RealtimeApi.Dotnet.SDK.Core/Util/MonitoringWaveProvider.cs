using NAudio.Wave;
using Navbot.RealtimeApi.Dotnet.SDK.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navbot.RealtimeApi.Dotnet.SDK.Core
{
    // Custom WaveProvider for monitoring audio data stream
    internal class MonitoringWaveProvider : IWaveProvider
    {
        private readonly IWaveProvider sourceProvider;

        public event EventHandler<AudioEventArgs> AudioDataCaptured;

        public MonitoringWaveProvider(IWaveProvider sourceProvider)
        {
            this.sourceProvider = sourceProvider;
            WaveFormat = sourceProvider.WaveFormat;
        }

        public WaveFormat WaveFormat { get; }

        public virtual void OnAudioDataCaptured(AudioEventArgs e)
        {
            AudioDataCaptured?.Invoke(this, e);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            // Read audio data from the source
            int bytesRead = sourceProvider.Read(buffer, offset, count);

            // Capture the currently playing audio data
            if (bytesRead > 0)
            {
                byte[] capturedData = new byte[bytesRead];
                Array.Copy(buffer, offset, capturedData, 0, bytesRead);

                OnAudioDataCaptured(new AudioEventArgs(capturedData));
            }

            return bytesRead;
        }
    }

}
