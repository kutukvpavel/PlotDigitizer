using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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

            //Digitize
            Console.WriteLine("Digitizing...");
            var work = new ImageDigitizer();
            try
            {
                if (args.Any(x => x.Split(':').FirstOrDefault() == "-n"))
                {
                    work.RequiredNeighbours = int.Parse(args.First(x => x.Split(':').First() == "-n").Split(':').Last());
                    Console.WriteLine("Set required neighbours number to " + work.RequiredNeighbours.ToString());
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid command line parameters!");
                return (int)ExitCodes.CLIError;
            }
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
    }
}
