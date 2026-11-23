using Microsoft.AspNetCore.Mvc;
using UsageData.Application.Features.Usage;

namespace UsageData.Application.Features.Controllers;

[ApiController]
[Route("v1")]
[Produces("application/json")]
public class UsageDataController(IUsageService service) : ControllerBase
{
    [HttpGet("user-agents")]
    [ProducesResponseType<IReadOnlyList<UserAgentDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserAgentDto>>> GetUserAgentsAsync(
        CancellationToken cancellationToken)
    {
        var userAgents = await service.GetUserAgentsAsync(cancellationToken);

        if (userAgents.Count == 0)
            return NotFound();

        return Ok(userAgents);
    }

    [HttpGet("user-agents/{userAgentId:int}")]
    [ProducesResponseType<UserAgentDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserAgentDto?>> GetUserAgentByIdAsync(int userAgentId, CancellationToken cancellationToken)
    {
        var userAgent = await service.GetUserAgentByIdAsync(userAgentId, cancellationToken);

        if (userAgent is null)
            return NotFound();

        return Ok(userAgent);
    }

    [HttpGet("anonymous-users")]
    [ProducesResponseType<IReadOnlyList<AnonymousUserDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AnonymousUserDto>>> GetAnonymousUsersAsync(
        CancellationToken cancellationToken)
    {
        var anonymounsUsers = await service.GetAnonymousUsersAsync(cancellationToken);

        if (anonymounsUsers.Count == 0)
            return NotFound();

        return Ok(anonymounsUsers);
    }

    [HttpGet("anonymous-users/{anonymousUserId:int}")]
    [ProducesResponseType<AnonymousUserDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AnonymousUserDto?>> GetAnonymousUserByIdAsync(int anonymousUserId, CancellationToken cancellationToken)
    {
        var anonymousUser = await service.GetAnonymousUserByIdAsync(anonymousUserId, cancellationToken);

        if (anonymousUser is null)
            return NotFound();

        return Ok(anonymousUser);
    }

    [HttpGet("access-records")]
    [ProducesResponseType<IReadOnlyList<AccessedRecordDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AccessedRecordDto>>> GetAccessRecordsAsync(CancellationToken cancellationToken)
    {
        var accessRecords = await service.GetAccessRecordsAsync(cancellationToken);

        if (accessRecords.Count == 0)
            return NotFound();

        return Ok(accessRecords);
    }

    [HttpGet("access-records/{anonymousUserId:int}/{userAgentId:int}")]
    [ProducesResponseType<AccessedRecordDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AccessedRecordDto?>> GetAccessRecordByIdAsync(int anonymousUserId, int userAgentId, CancellationToken cancellationToken)
    {
        var accessRecord = await service.GetAccessRecordByIdAsync(anonymousUserId, userAgentId, cancellationToken);

        if (accessRecord is null)
            return NotFound();

        return Ok(accessRecord);
    }
}
