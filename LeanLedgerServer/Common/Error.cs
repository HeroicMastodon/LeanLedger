namespace LeanLedgerServer.Common;

public abstract record Err() {
    public abstract IResult ToHttpResult();
}

public record InvalidRequest(string FieldName, string Message): Err() {
    public string Title => $"Invalid {FieldName}";

    public override IResult ToHttpResult() => Results.Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: Title,
        detail: Message
    );
}

public record AccountNotFound(Guid Id): Err() {
    public override IResult ToHttpResult() => Results.Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Account not found",
        detail: $"Could not find account with id: {Id}"
    );
}
