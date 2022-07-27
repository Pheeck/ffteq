using System;

namespace ffteq
{
    class Signal
    {
        public double[] Data { get; private set; }
        public double this[int index]
        {
            get
            {
                return Data[index];
            }

            set
            {
                Data[index] = value;
            }
        }
        public int SampleRate { get; private set; }
        public int SampleNum
        {
            get
            {
                return Data.Length;
            }
        }

        public Signal(double[] data, int sampleRate)
        {
            this.Data = data;
            SampleRate = sampleRate;
        }

        /// <summary>
        /// Signal of given length with all samples 0.
        /// </summary>
        public Signal(int length, int sampleRate)
        {
            this.Data = new double[length];
            SampleRate = sampleRate;
        }
        
        /// <summary>
        /// Create a new signal of given length. Copy samples from this signal
        /// into it starting with given start index. If there are no more
        /// samples to copy into the new signal, fill the rest with zeroes.
        /// </summary>
        public Signal Subsignal(int startIndex, int length)
        {
            double[] data = new double[length];
            if (startIndex + length > SampleNum)
            {
                length = SampleNum - startIndex;
            }
            Array.Copy(Data, startIndex, data, 0, length);
            return new Signal(data, SampleRate);
        }
    }
}
