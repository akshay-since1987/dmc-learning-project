using ProposalManagement.Domain.Common;
using ProposalManagement.Domain.Enums;

namespace ProposalManagement.Domain.Entities;

public class Palika : BaseAuditableEntity
{
    public string Name_En { get; set; } = default!;
    public string? Name_Mr { get; set; }
    public string ShortCode { get; set; } = default!;
    public string Type { get; set; } = PalikaType.MahanagarPalika.ToString();
    public string? LogoUrl { get; set; }
    public string? Address_En { get; set; }
    public string? Address_Mr { get; set; }
    public string? ContactPhone { get; set; }
    public string? Website { get; set; }
    public string PrimaryLanguage { get; set; } = "en";
    public string AlternateLanguage { get; set; } = "mr";
    public string ProposalNumberPrefix { get; set; } = default!;
    public string CurrentFinancialYear { get; set; } = default!;
    public string? SmsGatewayProvider { get; set; }
    public string? SmsGatewayApiKey { get; set; }
    public int OtpExpiryMinutes { get; set; } = 5;
    public int OtpMaxAttempts { get; set; } = 3;

    // Navigation
    public ICollection<Zone> Zones { get; set; } = [];
    public ICollection<Prabhag> Prabhags { get; set; } = [];
    public ICollection<Department> Departments { get; set; } = [];
    public ICollection<User> Users { get; set; } = [];
    public ICollection<Proposal> Proposals { get; set; } = [];
}
