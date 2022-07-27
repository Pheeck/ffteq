using System;

namespace ffteq
{
    abstract class CMDOptionArg
    {
        abstract public string Name { get; }
        abstract public string Description { get; }

        /// <returns>
        /// Is string s valid as this argument?
        /// </returns>
        abstract public bool Validate(String s);
    }

    class CMDOptionArgDouble : CMDOptionArg
    {
        public double Value { private set; get; }
        private string name;
        private string description;
        public override string Name { get { return name; } }
        public override string Description { get { return description; } }

        public CMDOptionArgDouble(string name, string description)
        {
            this.name = name;
            this.description = description;
        }
        
        public override bool Validate(String s)
        {
            double v;
            return Double.TryParse(s, out v);
        }
    }

    abstract class CMDOption
    {
        abstract public string Name { get; }
        abstract public string Description { get; }
        abstract public CMDOptionArg[] Args { get; }
        public int ArgNum { get { return Args.Length; } }

        /// <summary>
        /// Shall return signal with the same number of samples and sample
        /// rate. The signal should be processed by an effect.
        /// </summary>
        abstract public Signal Execute(Signal inSignal, string[] argStrs);
    }
}
