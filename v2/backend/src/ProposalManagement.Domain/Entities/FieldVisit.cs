using ProposalManagement.Domain.Common;

namespace ProposalManagement.Domain.Entities;

public class FieldVisit : BaseEntity
{
    public Guid ProposalId { get; set; }
    public int VisitNumber { get; set; }
    public Guid AssignedToId { get; set; }
    public Guid AssignedById { get; set; }

    public Guid? InspectionById { get; set; }
    public DateTime? InspectionDate { get; set; }
    public Guid? SiteConditionId { get; set; }
    public string? ProblemDescription_En { get; set; }
    public string? ProblemDescription_Mr { get; set; }
    public string? Measurements_En { get; set; }
    public string? Measurements_Mr { get; set; }
    public decimal? GpsLatitude { get; set; }
    public decimal? GpsLongitude { get; set; }
    public string? Remark_En { get; set; }
    public string? Remark_Mr { get; set; }
    public string? Recommendation_En { get; set; }
    public string? Recommendation_Mr { get; set; }
    public string? UploadedPdfPath { get; set; }

    public string? SignaturePath { get; set; }
    public string Status { get; set; } = Enums.FieldVisitStatus.Assigned.ToString();
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public Proposal Proposal { get; set; } = default!;
    public User AssignedTo { get; set; } = default!;
    public User AssignedBy { get; set; } = default!;
    public User? InspectionBy { get; set; }
    public SiteCondition? SiteCondition { get; set; }
    public ICollection<FieldVisitPhoto> Photos { get; set; } = [];
}

public class FieldVisitPhoto
{
    public Guid Id { get; set; }
    public Guid FieldVisitId { get; set; }
    public string FileName { get; set; } = default!;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = default!;
    public string StoragePath { get; set; } = default!;
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public FieldVisit FieldVisit { get; set; } = default!;
}
