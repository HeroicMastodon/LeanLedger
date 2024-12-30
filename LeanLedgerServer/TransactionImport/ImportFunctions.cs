namespace LeanLedgerServer.TransactionImport;

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

public static class ImportFunctions {
    public static async Task<List<Dictionary<string, string?>>> ReadCsvToList(IFormFile csvFile) {
        await using var csvStream = csvFile.OpenReadStream();
        using var csvStreamReader = new StreamReader(csvStream);
        using var csvReader = new CsvReader(
            csvStreamReader,
            new CsvConfiguration(CultureInfo.InvariantCulture) {
                HasHeaderRecord = true, // Assumes the CSV file has a header row
            }
        );

        await csvReader.ReadAsync();
        csvReader.ReadHeader();
        var headers = csvReader.HeaderRecord!; // Array of column names

        // Read all rows dynamically
        var rows = new List<Dictionary<string, string?>>();
        while (await csvReader.ReadAsync()) {
            var row = new Dictionary<string, string?>();
            foreach (var header in headers) {
                row[header] = csvReader.GetField(header);
            }

            rows.Add(row);
        }

        return rows;
    }
}
