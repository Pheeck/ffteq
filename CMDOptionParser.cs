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
        private CMDOption[] availableOpts;
        private string programDesc;

        private void PrintOptions()
        {
            foreach (CMDOption o in availableOpts)
            {
                Console.Write(o.Name);
                foreach (CMDOptionParam a in o.Params)
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

        private static void PrintParamHelp(CMDOption o, CMDOptionParam p)
        {
            Console.WriteLine("Option: {0}", o.Name);
            Console.WriteLine("Param: {0}", p.Name);
            Console.WriteLine(p.Description);
        }

        public CMDOptionParser(CMDOption[] availableOpts, string programDesc)
        {
            this.availableOpts = availableOpts;
            this.programDesc = programDesc;
        }

        public void PrintHelp()
        {
            Console.WriteLine(programDesc);
            Console.WriteLine();
            PrintOptions();
        }

        public List<CMDParsedOption> Parse(string[] args)
        {
            List<CMDParsedOption> parsed = new List<CMDParsedOption>();

            for (int i = 0; i < args.Length; i++)
            {
                string s = args[i];
                bool wrongOpt = true;
                foreach (CMDOption o in availableOpts)
                {
                    if (s == o.Name)
                    {
                        wrongOpt = false;

                        // Prepare array of args of this option
                        if (i + o.ParamNum >= args.Length)
                        {
                            throw new ApplicationException(String.Format(
                                "Not enough arguments for option {0}",
                                o.Name
                            ));
                        }
                        string[] oArgs = new string[o.ParamNum];
                        Array.Copy(args, i + 1, oArgs, 0, o.ParamNum);

                        // Validate args
                        for (int j = 0; j < o.ParamNum; j++)
                        {
                            if (!o.Params[j].Validate(oArgs[j]))
                            {
                                PrintParamHelp(o, o.Params[j]);
                                throw new ApplicationException(
                                    "Invalid argument"
                                );
                            }
                        }

                        parsed.Add(new CMDParsedOption(o, oArgs));

                        // Skip args of this option
                        i += o.ParamNum;

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
