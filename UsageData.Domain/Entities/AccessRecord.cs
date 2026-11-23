namespace UsageData.Domain.Entities;

public class AccessRecord
{
    public int AnonymousUserId { get; set; }
    public int UserAgentId { get; set; }

    public DateTime? LastAccessed { get; set; } = DateTime.Now;
    public string? Site { get; set; }
    public string? URI { get; set; }
}
