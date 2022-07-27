using System;

namespace ffteq
{
    abstract class Effect
    {
        /// <summary>
        /// Shall return signal with the same number of samples and sample
        /// rate.
        /// </summary>
        public abstract Signal Process(Signal inSignal);
    }
}
