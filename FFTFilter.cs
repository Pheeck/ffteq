using System;
using System.Numerics;

namespace ffteq
{
    class FFTFilter : Effect
    {
        private bool lowPass;
        private double hzFrom;
        private double hzTo;

        /// <summary>
        /// Given total size of FFT and sample rate, converts hz into index of
        /// the corresponding FFT bin.
        /// </summary>
        private int HzToBinNum(double hz, int sampleRate, int binsTotal)
        {
            return (int) (hz / sampleRate * binsTotal);
        }

        private Complex[] DoubleToComplex(double[] xs)
        {
            Complex[] result = new Complex[xs.Length];
            for (int i = 0; i < xs.Length; i++)
            {
                result[i] = new Complex(xs[i], 0);
            }
            return result;
        }

        private double[] ComplexToDouble(Complex[] xs)
        {
            double[] result = new Double[xs.Length];
            for (int i = 0; i < xs.Length; i++)
            {
                result[i] = xs[i].Real;
            }
            return result;
        }

        public FFTFilter(bool lowPass, double hzFrom, double hzTo)
        {
            this.lowPass = lowPass;
            this.hzFrom = hzFrom;
            this.hzTo = hzTo;
        }

        public override Signal Process(Signal inSignal)
        {
            int n = inSignal.SampleNum;
            int sampleRate = inSignal.SampleRate;
            int binFrom = HzToBinNum(hzFrom, sampleRate, n);
            int binTo = HzToBinNum(hzTo, sampleRate, n);

            Complex[] inSamples = DoubleToComplex(inSignal.Data);
            Complex[] bins = FFT.DoFFT(inSamples);

            if (lowPass)
            {
                // Create linear slope
                int slopeLen = binTo - binFrom;
                for (int i = binFrom; i < Math.Min(binTo, n / 2); i++)
                {
                    double coef = 1.0 - ((double) (i - binFrom)) / slopeLen;
                    bins[i] *= coef;
                    bins[n - 1 - i] *= coef;
                }
                // Fully filter out
                for (int i = binTo; i < n / 2; i++)
                {
                    bins[i] = new Complex(0, 0);
                    bins[n - 1 - i] = new Complex(0, 0);
                }
            }
            else // High pass
            {
                // Fully filter out
                for (int i = 0; i < Math.Min(binFrom, n / 2); i++)
                {
                    bins[i] = new Complex(0, 0);
                    bins[n - 1 - i] = new Complex(0, 0);
                }
                // Create linear slope
                int slopeLen = binTo - binFrom;
                for (int i = binFrom; i < Math.Min(binTo, n / 2); i++)
                {
                    double coef = ((double) (i - binFrom)) / slopeLen;
                    bins[i] *= coef;
                    bins[n - 1 - i] *= coef;
                }
            }

            double[] outSamples = ComplexToDouble(FFT.DoIFFT(bins));
            Signal outSignal = new Signal(outSamples, sampleRate);
            return outSignal;
        }
    }
}
