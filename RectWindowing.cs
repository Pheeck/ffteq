using System;

namespace ffteq
{
    class RectWindowing : Windowing
    {
        private WindowingState state = WindowingState.NoSignal;
        private int winLen;
        private Signal inSignal;
        private Signal outSignal;
        private int index;

        public RectWindowing(int winLen)
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
                outSignal[index + i] = window[i];
            }

            index += winLen;
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
