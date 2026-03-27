namespace ProposalManagement.Application.Proposals.Dtos;

// List view DTO — minimal columns for table
public record ProposalListDto(
    Guid Id,
    string ProposalNumber,
    DateTime ProposalDate,
    string WorkTitle_En,
    string? WorkTitle_Mr,
    string DepartmentName,
    string? DepartmentName_Mr,
    string? WorkCategoryName,
    string? WorkCategoryName_Mr,
    string CurrentStage,
    string Priority,
    string CreatedByName,
    string? CreatedByName_Mr,
    int CompletedTab,
    DateTime CreatedAt);

// Detail DTO — full Tab 1 data
public record ProposalDetailDto
{
    public Guid Id { get; init; }
    public string ProposalNumber { get; init; } = default!;
    public DateTime ProposalDate { get; init; }
    public Guid DepartmentId { get; init; }
    public string? DepartmentName { get; init; }
    public string? DepartmentName_Mr { get; init; }
    public Guid DeptWorkCategoryId { get; init; }
    public string? WorkCategoryName { get; init; }
    public string? WorkCategoryName_Mr { get; init; }
    public Guid ZoneId { get; init; }
    public string? ZoneName { get; init; }
    public string? ZoneName_Mr { get; init; }
    public Guid PrabhagId { get; init; }
    public string? PrabhagName { get; init; }
    public string? PrabhagName_Mr { get; init; }
    public string? Area { get; init; }
    public string? Area_Mr { get; init; }
    public string? LocationAddress_En { get; init; }
    public string? LocationAddress_Mr { get; init; }
    public string? LocationMapPath { get; init; }

    public string WorkTitle_En { get; init; } = default!;
    public string? WorkTitle_Mr { get; init; }
    public string WorkDescription_En { get; init; } = default!;
    public string? WorkDescription_Mr { get; init; }

    public Guid? RequestSourceId { get; init; }
    public string? RequestSourceName { get; init; }
    public string? RequestSourceName_Mr { get; init; }
    public string? RequestorName { get; init; }
    public string? RequestorName_Mr { get; init; }
    public string? RequestorMobile { get; init; }
    public string? RequestorAddress { get; init; }
    public string? RequestorAddress_Mr { get; init; }
    public string? RequestorDesignation { get; init; }
    public string? RequestorDesignation_Mr { get; init; }
    public string? RequestorOrganisation { get; init; }
    public string? RequestorOrganisation_Mr { get; init; }

    public string Priority { get; init; } = default!;
    public string CurrentStage { get; init; } = default!;
    public Guid? CurrentOwnerId { get; init; }
    public string? CurrentOwnerName { get; init; }
    public string? CurrentOwnerName_Mr { get; init; }
    public int PushBackCount { get; init; }
    public int CompletedTab { get; init; }
    public Guid CreatedById { get; init; }
    public string? CreatedByName { get; init; }
    public string? CreatedByName_Mr { get; init; }
    public DateTime CreatedAt { get; init; }

    public List<ProposalDocumentDto> Documents { get; init; } = [];
}

public record ProposalDocumentDto(
    Guid Id,
    int TabNumber,
    string DocumentType,
    string? DocName,
    string FileName,
    long FileSize,
    string ContentType,
    DateTime CreatedAt);

// Dashboard stats
public record ProposalStatsDto(
    int Draft,
    int InProgress,
    int PushedBack,
    int Parked,
    int Approved,
    int Total);

// Approval timeline entry
public record ApprovalHistoryDto(
    long Id,
    string StageRole,
    string Action,
    string? ActorName_En,
    string? ActorName_Mr,
    string? ActorDesignation_En,
    string? ActorDesignation_Mr,
    string? Opinion_En,
    string? Opinion_Mr,
    string? PushBackNote_En,
    string? PushBackNote_Mr,
    bool DisclaimerAccepted,
    DateTime CreatedAt);
