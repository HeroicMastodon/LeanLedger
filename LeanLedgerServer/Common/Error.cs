namespace LeanLedgerServer.Common;

public abstract record Err();

public record InvalidRequest(string FieldName, string Message): Err() {
    public string Title => $"Invalid {FieldName}";
}
