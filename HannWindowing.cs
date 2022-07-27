using System;

namespace ffteq
{
    class HannWindowing : Windowing
    {
        private WindowingState state = WindowingState.NoSignal;
        private int winLen;
        private Signal inSignal;
        private Signal outSignal;
        private int index;
        private bool firstWindow;

        /// <summary>
        /// Computes value of von Hann function of width winLen in point i.
        /// </summary>
        private double Hann(int i)
        {
            double a0 = 0.5;
            return a0 - (1 - a0) * Math.Cos(2 * Math.PI * i / winLen);
        }

        public HannWindowing(int winLen)
        {
            this.winLen = winLen;
        }

        public override void StartProcessing(Signal inSignal)
        {
            if (inSignal.SampleNum < winLen)
            {
                throw new ApplicationException(String.Format(
                    "Signal too short for window length of {0} samples",
                    winLen
                ));
            }

            this.inSignal = inSignal;
            outSignal = new Signal(inSignal.SampleNum, inSignal.SampleRate);
            state = WindowingState.WindowReady;
            index = 0;
            firstWindow = true;
        }

        public override Signal NextWindow()
        {
            switch (state)
            {
                case WindowingState.NoSignal:
                    throw new ApplicationException(
                        "Internal error: Windowing not initialized"
                    );
                case WindowingState.WindowReady:
                    break;
                case WindowingState.AwaitingWindow:
                    throw new ApplicationException(
                        "Internal error: Next window not ready"
                    );
                case WindowingState.SignalDone:
                    return null;
            }

            Signal window = inSignal.Subsignal(index, winLen);
            if (firstWindow)
            {
                // First winLen / 2 samples of signal shouldn't be put through
                // window function
                for (int i = winLen / 2; i < winLen; i++)
                {
                    window[i] *= Hann(i);
                }
                firstWindow = false;
            }
            else
            {
                for (int i = 0; i < winLen; i++)
                {
                    window[i] *= Hann(i);
                }
            }
            state = WindowingState.AwaitingWindow;
            return window;
        }

        public override void PutBack(Signal window)
        {
            for (int i = 0; i < winLen; i++)
            {
                if (index + i >= outSignal.SampleNum)
                {
                    break;
                }
                outSignal[index + i] += window[i];
            }

            index += winLen / 2;
            if (index >= inSignal.SampleNum)
            {
                state = WindowingState.SignalDone;
            }
            else
            {
                state = WindowingState.WindowReady;
            }
        }

        public override Signal FinishProcessing()
        {
            return outSignal;
        }
    }
}
