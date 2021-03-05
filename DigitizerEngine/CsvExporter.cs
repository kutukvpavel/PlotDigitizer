using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;

namespace DigitizerEngine
{
    public static class CsvExporter
    {
        public static CsvConfiguration Configuration { get; set; } = new CsvConfiguration(CultureInfo.CurrentCulture);

        public static void Export(int?[] data, string path)
        {
            using (TextWriter tw = new StreamWriter(path))
            using (CsvWriter writer = new CsvWriter(tw, Configuration))
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == null) continue;
                    writer.WriteField(i);
                    writer.WriteField(data[i]);
                    writer.NextRecord();
                }
                writer.Flush();
            }
        }
    }
}
