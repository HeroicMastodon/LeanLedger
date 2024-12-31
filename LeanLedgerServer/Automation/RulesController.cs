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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRule(Guid id) {
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> CreateRule() {
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRule() {
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteRule() {
        return Ok();
    }
}
