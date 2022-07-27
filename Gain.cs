using System;

namespace ffteq
{
    class Gain : Effect
    {
        private double ratio;

        /// <summary>
        /// Convert decibels to amplitude ratio.
        /// </summary>
        private static double DbToRatio(double db)
        {
            return Math.Sqrt(Math.Pow(10, db / 10));
        }

        public Gain(double db)
        {
            ratio = DbToRatio(db);
        }

        public override Signal Process(Signal inSignal)
        {
            int n = inSignal.SampleNum;
            Signal outSignal = new Signal(n, inSignal.SampleRate);
            for (int i = 0; i < n; i++)
            {
                outSignal[i] = inSignal[i] * ratio;
            }
            return outSignal;
        }
    }
}
