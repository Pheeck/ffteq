/**
 * ffteq
 * Filip Kastl, II. ročník
 * zimní semestr 2021/22
 * Programování v jazyce C# NPRG035
 */

using System;
using System.Numerics;
using System.IO;
using System.Collections.Generic;

namespace ffteq
{
    class CMDOptionFilterLow : CMDOption
    {
        private const int WINDOW_SIZE = 2048; // In bytes; Dont use odd numbers
        private const string DESCRIPTION =
@"Filter low frequencies so that anything lower than <hz-from> will be filtered
out fully, frequencies higher than <hz-to> won't be filtered at all and there
will a be a linear slope between these two values.";

        public override string Name { get { return "filterlo"; } }
        public override string Description
        {
            get
            {
                return DESCRIPTION;
            }
        }
        private CMDOptionArg[] args = new CMDOptionArg[] {
            new CMDOptionArgDouble("hz-from",
                    "Decimal number. Hz where slope starts."),
            new CMDOptionArgDouble("hz-to",
                    "Decimal number. Hz where slope ends.")
        };
        public override CMDOptionArg[] Args { get { return args; } }

        public override Signal Execute(Signal inSignal, string[] argStrs)
        {
            double hzFrom = Double.Parse(argStrs[0]);
            double hzTo = Double.Parse(argStrs[1]);

            Windowing windowing = new HannWindowing(WINDOW_SIZE);
            Effect filter = new FFTFilter(true, hzFrom, hzTo);

            windowing.StartProcessing(inSignal);
            Signal window;
            while ((window = windowing.NextWindow()) != null)
            {
                window = filter.Process(window);
                windowing.PutBack(window);
            }
            return windowing.FinishProcessing();
        }
    }

    class CMDOptionFilterHigh : CMDOption
    {
        private const int WINDOW_SIZE = 2048; // In bytes
        private const string DESCRIPTION =
@"Filter high frequencies so that anything lower than <hz-from> won't be
filtered at all, frequencies higher than <hz-to> will be filtered fully and
there will a be a linear slope between these two values.";

        public override string Name { get { return "filterhi"; } }
        public override string Description
        {
            get
            {
                return DESCRIPTION;
            }
        }
        private CMDOptionArg[] args = new CMDOptionArg[] {
            new CMDOptionArgDouble("hz-from",
                    "Decimal number. Hz where slope starts."),
            new CMDOptionArgDouble("hz-to",
                    "Decimal Number. Hz where slope ends.")
        };
        public override CMDOptionArg[] Args { get { return args; } }

        public override Signal Execute(Signal inSignal, string[] argStrs)
        {
            double hzFrom = Double.Parse(argStrs[0]);
            double hzTo = Double.Parse(argStrs[1]);

            Windowing windowing = new HannWindowing(WINDOW_SIZE);
            Effect filter = new FFTFilter(false, hzFrom, hzTo);

            windowing.StartProcessing(inSignal);
            Signal window;
            while ((window = windowing.NextWindow()) != null)
            {
                window = filter.Process(window);
                windowing.PutBack(window);
            }
            return windowing.FinishProcessing();
        }
    }

    class CMDOptionGain : CMDOption
    {
        public override string Name { get { return "gain"; } }
        public override string Description
        {
            get
            {
                return "Manipulate volume of the whole file.";
            }
        }
        private CMDOptionArg[] args = new CMDOptionArg[] {
            new CMDOptionArgDouble("db", "Decimal number. Ammount of gain.")
        };
        public override CMDOptionArg[] Args { get { return args; } }

        public override Signal Execute(Signal inSignal, string[] argStrs)
        {
            double db = Double.Parse(argStrs[0]);
            Effect gain = new Gain(db);
            return gain.Process(inSignal);
        }
    }

    class Program
    {
        private static CMDOption[] availableOpts = new CMDOption[] {
            new CMDOptionFilterLow(),
            new CMDOptionFilterHigh(),
            new CMDOptionGain()
        };

        private const string PROGRAM_DESC =
@"ffteq - Edit 8bit 44,1kHz mono pcm wav files
Usage: ./ffteq input-path output-path list-of-options
Options are applied in the order they apper on command line";

        public static int Main(string[] args)
        {
            // Parse args
            CMDOptionParser parser = new CMDOptionParser(
                availableOpts, PROGRAM_DESC
            );

            foreach (string arg in args)
            {
                if (arg == "-h" || arg == "--help")
                {
                    parser.PrintHelp();
                    return 0;
                }
            }

            if (args.Length < 3)
            {
                parser.PrintHelp();
                return 1;
            }
            string inPath = args[0];
            string outPath = args[1];

            string[] foo = new string[args.Length - 2];
            Array.Copy(args, 2, foo, 0, args.Length - 2);
            List<CMDParsedOption> parsedOpts = null;
            try
            {
                parsedOpts = parser.Parse(foo);
            }
            catch (ApplicationException e)
            {
                Console.WriteLine(
                    "Error while parsing CMD options: {0}",
                    e.Message
                );
                return 1;
            }

            // Open files
            FileStream inFile;
            FileStream outFile;
            try
            {
                inFile = File.Open(inPath, FileMode.Open);
            }
            catch
            {
                Console.WriteLine("Couldn't open input file");
                return 2;
            }
            try
            {
                outFile = File.Open(outPath, FileMode.Create);
            }
            catch
            {
                inFile.Dispose();
                Console.WriteLine("Couldn't open output file");
                return 2;
            }

            // Read Wav
            Wav wav;
            try
            {
                wav = new Wav(inFile);
            }
            catch (ApplicationException e)
            {
                inFile.Dispose();
                outFile.Dispose();
                Console.WriteLine("Error reading wav file: {0}", e.Message);
                return 3;
            }
            Signal signal = wav.WavSignal;

            // Proces signal according to cmd options
            for (int i = 0; i < parsedOpts.Count; i++)
            {
                CMDOption o = parsedOpts[i].Opt;
                string[] strArgs = parsedOpts[i].Args;

                Console.WriteLine("Applying option {0}...", o.Name);
                try
                {
                    signal = o.Execute(signal, strArgs);
                }
                catch (ApplicationException e)
                {
                    inFile.Dispose();
                    outFile.Dispose();
                    Console.WriteLine(
                        "Error applying option {0}: {1}",
                        o.Name,
                        e.Message
                    );
                    return 4;
                }
            }

            // Clip signal
            Effect clip = new Clipping();
            signal = clip.Process(signal);

            // Write wav to file
            wav.WavSignal = signal;
            wav.WriteFile(outFile);

            // Close files
            inFile.Dispose();
            outFile.Dispose();

            return 0;
        }
    }
}
