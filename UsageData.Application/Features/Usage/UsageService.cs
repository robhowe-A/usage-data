using Microsoft.EntityFrameworkCore;
using UsageData.Application.Data;

namespace UsageData.Application.Features.Usage;

public sealed class UsageService(UserDataContext db) : IUsageService
{
    public async Task<IReadOnlyList<UserAgentDto>> GetUserAgentsAsync(CancellationToken cancellationToken) =>
        await db.UserAgents
            .AsNoTracking()
            .OrderBy(u => u.AgentId)
            .Select(u => new UserAgentDto(
                u.AgentId, u.UserAgentFull, u.Count, u.Browser, u.CreatedDate, u.BrowserVersion))
            .ToListAsync(cancellationToken);

    public async Task<UserAgentDto?> GetUserAgentByIdAsync(int userAgentId, CancellationToken cancellationToken) =>
        await db.UserAgents
            .AsNoTracking()
            .Where(u => u.AgentId == userAgentId)
            .Select(u => new UserAgentDto(
                u.AgentId, u.UserAgentFull, u.Count, u.Browser, u.CreatedDate, u.BrowserVersion))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<AnonymousUserDto>> GetAnonymousUsersAsync(CancellationToken cancellationToken) =>
        await db.AnonymousUsers
            .AsNoTracking()
            .OrderBy(a => a.AnonymousUserId)
            .Select(a => new AnonymousUserDto(
                a.AnonymousUserId, a.UserAgentId, a.IpAddrHash, a.CreatedDate, a.LastAccessed))
            .ToListAsync(cancellationToken);

    public async Task<AnonymousUserDto?> GetAnonymousUserByIdAsync(int anonymousUserId, CancellationToken cancellationToken) =>
        await db.AnonymousUsers
            .AsNoTracking()
            .Where(a => a.AnonymousUserId == anonymousUserId)
            .Select(a => new AnonymousUserDto(
                a.AnonymousUserId, a.UserAgentId, a.IpAddrHash, a.CreatedDate, a.LastAccessed))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<AccessedRecordDto>> GetAccessRecordsAsync(CancellationToken cancellationToken) =>
        await db.AccessRecords
            .AsNoTracking()
            .Select(r => new AccessedRecordDto(
                r.AnonymousUserId, r.UserAgentId, r.LastAccessed, r.Site, r.URI))
            .ToListAsync(cancellationToken);

    public async Task<AccessedRecordDto?> GetAccessRecordByIdAsync(int anonymousUserId, int userAgentId, CancellationToken cancellationToken) =>
        await db.AccessRecords
            .AsNoTracking()
            .Where(r => r.AnonymousUserId == anonymousUserId && r.UserAgentId == userAgentId)
            .Select(r => new AccessedRecordDto(
                r.AnonymousUserId, r.UserAgentId, r.LastAccessed, r.Site, r.URI))
            .FirstOrDefaultAsync(cancellationToken);
}