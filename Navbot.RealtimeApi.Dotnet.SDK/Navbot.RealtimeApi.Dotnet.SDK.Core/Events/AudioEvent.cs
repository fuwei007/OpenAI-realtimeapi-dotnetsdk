namespace Navbot.RealtimeApi.Dotnet.SDK.Core.Events
{
    public class AudioEventArgs : EventArgs
    {
        public byte[] AudioBuffer { get; private set; }

        public AudioEventArgs(byte[] audioBuffer)
        {
            AudioBuffer = audioBuffer;
        }

        public byte[] GetIEEEAudioBuffer()
        {
            return ConvertPcmToFloat(this.AudioBuffer);
        }

        // Convert PCM data to IEEE Float data
        private byte[] ConvertPcmToFloat(byte[] pcmData)
        {
            // Each sample occupies 2 bytes (16-bit PCM)
            int pcmSampleSize = 2;
            int totalSamples = pcmData.Length / pcmSampleSize;

            // Each float sample occupies 4 bytes
            int floatSampleSize = 4;
            byte[] floatData = new byte[totalSamples * floatSampleSize];

            for (int i = 0; i < totalSamples; i++)
            {
                // Read 16-bit integer sample from PCM data
                short pcmSample = BitConverter.ToInt16(pcmData, i * pcmSampleSize);

                // Normalize the integer sample to a float
                float floatSample = pcmSample / 32768f;

                // Store the float sample into the target data
                byte[] floatBytes = BitConverter.GetBytes(floatSample);
                Array.Copy(floatBytes, 0, floatData, i * floatSampleSize, floatSampleSize);
            }

            return floatData;
        }
        // TODO 2 bit vs 4 bit
        public float[] GetWaveBuffer()
        {
            List<float> audioBuffer = new List<float>();
            for (int i = 0; i < AudioBuffer.Length; i += 2)
            {
                short value = BitConverter.ToInt16(AudioBuffer, i);
                float normalized = value / 32768f;
                audioBuffer.Add(normalized);
                //audioBuffer.Add(BitConverter.ToSingle(e.Buffer, i));
            }

            return audioBuffer.ToArray();
        }

        //public float[] GetWaveBuffer()
        //{
        //    short[] samples = new short[AudioBuffer.Length / 2];
        //    Buffer.BlockCopy(AudioBuffer, 0, samples, 0, AudioBuffer.Length);

        //    float[] waveform = samples.Select(s => s / 32768f).ToArray();
        //    return waveform;
        //}

    }
}
