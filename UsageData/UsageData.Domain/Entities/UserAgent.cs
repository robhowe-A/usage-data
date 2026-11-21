using System.ComponentModel.DataAnnotations;

namespace UsageData.Domain.Entities;

public class UserAgent
{
    [Key]
    public int AgentId { get; private set; }
    public string UserAgentFull { get; set; } = string.Empty;
    public int? Count { get; set; }
    public string? Browser { get; set; }
    public DateTime? CreatedDate { get; set; } = DateTime.Now;
    public string? BrowserVersion { get; set; }
};

