using System;

namespace ffteq
{
    public enum WindowingState
    {
        NoSignal,
        WindowReady,
        AwaitingWindow,
        SignalDone
    }

    /// <summary>
    /// 1 Signal is passed to windowing through StartProcessing()
    /// 2 NextWindow() returns a window to be processed outside of windowing
    /// 3 Window is passed back to windowing through PutBack()
    /// 4 This is repeated until there are no more windows to process
    /// 5 FinishProcessing() returns the whole processed signal
    /// </summary>
    abstract class Windowing
    {
        public abstract void StartProcessing(Signal inSignal);
        /// <summary>
        /// Shall return 'null' if there are no more windows to process.
        /// </summary>
        public abstract Signal NextWindow();
        public abstract void PutBack(Signal window);
        public abstract Signal FinishProcessing();
    }
}
