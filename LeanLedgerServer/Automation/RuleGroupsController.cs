namespace LeanLedgerServer.Automation;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/rule-groups")]
public class RuleGroupsController: Controller {
    [HttpGet]
    public async Task<IActionResult> ListRuleGroups() {
        return Ok();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRuleGroup(Guid id) {
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> CreateRuleGroup() {
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateRuleGroup() {
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteRuleGroup() {
        return Ok();
    }
}
