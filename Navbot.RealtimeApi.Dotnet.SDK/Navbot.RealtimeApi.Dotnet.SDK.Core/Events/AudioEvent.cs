namespace Navbot.RealtimeApi.Dotnet.SDK.Core.Events
{
    public class AudioEventArgs : EventArgs
    {
        public byte[] AudioBuffer { get; private set; }

        public AudioEventArgs()
            : this(new byte[0])
        {
        }

        public AudioEventArgs(byte[] audioBuffer)
        {
            AudioBuffer = audioBuffer;
        }

        //// TODO 2 bit vs 4 bit
        //public float[] GetWaveBuffer()
        //{
        //    List<float> rtn = new List<float>();
        //    //for (int i = 0; i < AudioBuffer.Length; i += 2)
        //    //{
        //    //    short value = BitConverter.ToInt16(AudioBuffer, i);
        //    //    float normalized = value / 32768f;
        //    //    rtn.Add(normalized);
        //    //    //audioBuffer.Add(BitConverter.ToSingle(e.Buffer, i));
        //    //}

        //    short[] samples = new short[AudioBuffer.Length / 2];
        //    Buffer.BlockCopy(AudioBuffer, 0, samples, 0, AudioBuffer.Length);

        //    float[] waveform = samples.Select(s => s / 32768f).ToArray();
        //    return waveform;


        //    //return rtn.ToArray();
        //}
    }
}
