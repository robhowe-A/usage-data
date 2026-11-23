namespace UsageData.Application.Features.Usage;

public sealed record UserAgentDto(
    int AgentId, string UserAgent, int? Count, string? Browser, DateTime? CreatedDate, string? BrowserVersion);
public sealed record AnonymousUserDto(
    int AnonymousUserId, int UserAgentId, string? IpAddressHash, DateTime? CreatedDate, DateTime? LastAccessed);
public sealed record AccessedRecordDto(
    int AnonymousUserId, int UserAgentId, DateTime? LastAccessed, string? Site, string? Uri);

public interface IUsageService
{
    Task<IReadOnlyList<UserAgentDto>> GetUserAgentsAsync(CancellationToken cancellationToken);
    Task<UserAgentDto?> GetUserAgentByIdAsync(int uid, CancellationToken cancellationToken);
    Task<IReadOnlyList<AnonymousUserDto>> GetAnonymousUsersAsync(CancellationToken cancellationToken);
    Task<AnonymousUserDto?> GetAnonymousUserByIdAsync(int anonymousUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AccessedRecordDto>> GetAccessRecordsAsync(CancellationToken cancellationToken);
    Task<AccessedRecordDto?> GetAccessRecordByIdAsync(int anonymousUserId, int userAgentId, CancellationToken cancellationToken);
}
