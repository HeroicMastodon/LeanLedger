namespace LeanLedgerServer.Common;

using Microsoft.AspNetCore.Mvc;

public record QueryByMonth([FromQuery] int? Month, [FromQuery] int? Year);
