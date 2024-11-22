using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Shop.Core.Extensions
{
    public static class StreamExtensions
    {
        public static IEnumerable<T> ReadCsv<T>(this Stream stream) where T : class
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanRead) throw new InvalidOperationException("The stream must be readable.");

            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };

            stream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, configuration);

            return csv.GetRecords<T>().ToList();
        }
    }
}
