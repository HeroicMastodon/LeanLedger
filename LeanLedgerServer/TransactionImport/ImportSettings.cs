namespace LeanLedgerServer.TransactionImport;

using System.Text.Json.Serialization;
using Accounts;

// TODO: create dbset. Mappings should be stored in a json column
public class ImportSettings {
    public Guid Id { get; init; }
    public char? CsvDelimiter { get; set; }
    public string? DateFormat { get; set; }
    public List<ImportMapping> ImportMappings { get; set; }
    public bool IsDeleted { get; set; }

    public Account? AttachedAccount { get; set; }
    public Guid AttachedAccountId { get; set; }
}

public record ImportMapping(
    string SourceColumnName,
    ImportTransactionField DestinationField
);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ImportTransactionField {
    Ignore,
    Amount,
    NegatedAmount,
    Date,
    Description,
    Category,
}
