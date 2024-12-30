using Microsoft.AspNetCore.Mvc;

namespace LeanLedgerServer.Automation;

using Common;

[ApiController]
[Route("api/[controller]")]
public class RulesController(
    LedgerDbContext dbContext
): Controller {
    [HttpGet]
    public async Task<IActionResult> ListRules() {
        return Ok();
    }
}
