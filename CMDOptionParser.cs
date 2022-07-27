using System;
using System.Collections.Generic;

namespace ffteq
{
    struct CMDParsedOption
    {
        public CMDOption Opt;
        public string[] Args;

        public CMDParsedOption(CMDOption opt, string[] args)
        {
            Opt = opt;
            Args = args;
        }
    }

    class CMDOptionParser
    {
        private const int LINE_WIDTH = 80;

        private CMDOption[] availableOpts;
        private string programDesc;

        public CMDOptionParser(CMDOption[] availableOpts, string programDesc)
        {
            this.availableOpts = availableOpts;
            this.programDesc = programDesc;
        }

        public void PrintOptions()
        {
            foreach (CMDOption o in availableOpts)
            {
                Console.Write(o.Name);
                foreach (CMDOptionArg a in o.Args)
                {
                    Console.Write(' ');
                    Console.Write(a.Name);
                }
                Console.Write('\n');

                Console.WriteLine();
                Console.WriteLine(o.Description);
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        public void PrintHelp()
        {
            Console.WriteLine(programDesc);
            Console.WriteLine();
            PrintOptions();
        }

        public static void PrintArgHelp(CMDOption o, CMDOptionArg a)
        {
            Console.WriteLine("Option: {0}", o.Name);
            Console.WriteLine("Arg: {0}", a.Name);
            Console.WriteLine(a.Description);
        }

        public List<CMDParsedOption> Parse(string[] strs)
        {
            List<CMDParsedOption> parsed = new List<CMDParsedOption>();

            for (int i = 0; i < strs.Length; i++)
            {
                string s = strs[i];
                bool wrongOpt = true;
                foreach (CMDOption o in availableOpts)
                {
                    if (s == o.Name)
                    {
                        wrongOpt = false;

                        // Prepare array of args of this option
                        if (i + o.ArgNum >= strs.Length)
                        {
                            throw new ApplicationException(String.Format(
                                "Not enough arguments for option {0}",
                                o.Name
                            ));
                        }
                        string[] argStrs = new string[o.ArgNum];
                        Array.Copy(strs, i + 1, argStrs, 0, o.ArgNum);

                        // Validate args
                        for (int j = 0; j < o.ArgNum; j++)
                        {
                            if (!o.Args[j].Validate(argStrs[j]))
                            {
                                PrintArgHelp(o, o.Args[j]);
                                throw new ApplicationException(
                                    "Invalid argument"
                                );
                            }
                        }

                        parsed.Add(new CMDParsedOption(o, argStrs));

                        // Skip args of this option
                        i += o.ArgNum;

                        break;
                    }
                }
                if (wrongOpt)
                {
                    throw new ApplicationException(String.Format(
                        "Unrecognized option: {0}",
                        s
                    ));
                }
            }

            return parsed;
        }
    }
}
