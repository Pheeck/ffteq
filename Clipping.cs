using System;

namespace ffteq
{
    class Clipping : Effect
    {
        public override Signal Process(Signal inSignal)
        {
            int n = inSignal.SampleNum;
            Signal outSignal = new Signal(n, inSignal.SampleRate);
            for (int i = 0; i < n; i++)
            {
                if (inSignal[i] > 1.0)
                {
                    outSignal[i] = 1.0;
                }
                else if (inSignal[i] < -1.0)
                {
                    outSignal[i] = -1.0;
                }
                else
                {
                    outSignal[i] = inSignal[i];
                }
            }
            return outSignal;
        }
    }
}
