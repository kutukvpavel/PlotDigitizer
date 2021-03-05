using System;
using System.IO;
using System.Linq;
using System.Drawing;

namespace DigitizerEngine
{
    enum ExitCodes
    {
        OK,
        FileException,
        FileNotFound,
        DigitizerException,
        SaveError,
        CLIError
    }
    class Program
    {
        static int Main(string[] args)
        {
            //Probe the file
            Console.WriteLine("Acquiring file access...");
            try
            {
                args[0] = args[0].Trim('"', ' ');
                if (!File.Exists(args[0]))
                {
                    return (int)ExitCodes.FileNotFound;
                }
            }
            catch (Exception)
            {
                return (int)ExitCodes.FileException;
            }

            //Process CLI
            Console.WriteLine("Parsing arguments...");
            var work = new ImageDigitizer();
            work.RequiredNeighbours = (int)(ParseNumericArgument("-n", args) ?? work.RequiredNeighbours);
            work.MaxBackgroundDistance = (int)(ParseNumericArgument("-c", args) ?? work.MaxBackgroundDistance);
            double? c = ParseNumericArgument("-b", args);
            if (c != null) work.BackgroundEdge = Color.FromArgb((int)c);
            c = ParseNumericArgument("-s", args);
            if (c != null) CsvExporter.ScalingFactor = (double)c;

            //Digitize
            Console.WriteLine("Digitizing...");
            try
            {
                work.Digitize(args[0]);
            }
            catch (Exception)
            {
                return (int)ExitCodes.DigitizerException;
            }

            //Save
            Console.WriteLine("Saving output...");
            try
            {
                var paths = MakeOutputPaths(args[0]);
                if (args.Contains("-d"))
                {
                    Console.WriteLine("Using semicolon as CSV delimeter.");
                    CsvExporter.Configuration.Delimiter = ";";
                }
                CsvExporter.Export(work.Output, paths[0]);
                work.OutputBitmap.Bitmap.Save(paths[1]);
                if (args.Contains("-s")) work.SourceBitmap.Save(paths[2]);
            }
            catch (Exception)
            {
                return (int)ExitCodes.SaveError;
            }

            Console.WriteLine("Success.");
            if (args.Contains("-k")) Console.ReadKey();
            return (int)ExitCodes.OK;
        }

        static string[] MakeOutputPaths(string source)
        {
            var name = Path.GetFileNameWithoutExtension(source);
            var dir = Path.GetDirectoryName(source);
            return new string[]
            {
                Path.Combine(dir, name + "_digitized.csv"),
                Path.Combine(dir, name + "_digitized.png"),
                Path.Combine(dir, name + "_source.png")
            };
        }

        static double? ParseNumericArgument(string name, string[] args)
        {
            try
            {
                if (FindArgument(name, args))
                {
                    return double.Parse(args.First(x => x.Split(':').First() == name).Split(':').Last());
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid command line parameter: " + name);
            }
            return null;
        }
        static bool FindArgument(string name, string[] args)
        {
            return args.Any(x => x.Split(':').FirstOrDefault() == name);
        }
    }
}
