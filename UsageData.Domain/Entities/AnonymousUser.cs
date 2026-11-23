using System.ComponentModel.DataAnnotations;

namespace UsageData.Domain.Entities;

public class AnonymousUser
{
    [Key]
    public int AnonymousUserId { get; private set; }
    public int UserAgentId { get; set; }

    public required string IpAddrHash { get; set; }
    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    public DateTime? LastAccessed { get; set; } = DateTime.Now;
}
