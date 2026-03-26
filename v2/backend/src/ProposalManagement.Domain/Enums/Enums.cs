namespace ProposalManagement.Domain.Enums;

public enum UserRole
{
    JE,
    TS,
    AE,
    SE,
    CityEngineer,
    AccountOfficer,
    DyCommissioner,
    Commissioner,
    StandingCommittee,
    Collector,
    Auditor,
    Lotus
}

public enum PalikaType
{
    MahanagarPalika,
    NagarPalika,
    NagarPanchayat
}

public enum OtpPurpose
{
    Login,
    PasswordReset
}

public enum Priority
{
    High,
    Medium,
    Low
}

public enum ProposalStage
{
    Draft,
    Submitted,
    FieldVisitPending,
    FieldVisitCompleted,
    EstimatePending,
    EstimateSentForApproval,
    EstimateApproved,
    TechnicalSanctionPending,
    TechnicalSanctionComplete,
    PramaPending,
    PramaComplete,
    BudgetPending,
    BudgetComplete,
    AtCityEngineer,
    AtAccountOfficer,
    AtDyCommissioner,
    AtCommissioner,
    AtStandingCommittee,
    AtCollector,
    Approved,
    PushedBack,
    Parked,
    Cancelled
}

public enum FieldVisitStatus
{
    Assigned,
    InProgress,
    Completed
}

public enum EstimateStatus
{
    Draft,
    SentForApproval,
    ReturnedWithQuery,
    Approved
}

public enum TechnicalSanctionStatus
{
    Draft,
    Pending,
    Signed
}

public enum DocumentType
{
    LocationMap,
    SitePhoto,
    EstimateCopy,
    TechnicalSanctionDoc,
    OutsideApprovalLetter,
    FieldVisitReport,
    GeoTaggedPhoto,
    SupportingDoc,
    Other
}

public enum PdfType
{
    Tab1,
    Tab2,
    Tab3,
    Tab4,
    Tab5,
    Tab6,
    Consolidated,
    StageApproval,
    FinalCombined
}

public enum ApprovalAction
{
    Approve,
    PushBack
}

public enum NotificationType
{
    Assignment,
    Approval,
    PushBack,
    Parked,
    Unparked,
    FieldVisitAssigned,
    EstimateReturned,
    TSCompleted,
    General
}

public enum ApprovalSlab
{
    Slab0to3L,
    Slab3to24L,
    Slab24to25L,
    Slab25LPlus
}

public enum AuditAction
{
    Create,
    Update,
    Delete,
    Login,
    Logout,
    Approve,
    PushBack,
    Submit,
    Upload,
    Download,
    Generate,
    Assign,
    Park,
    Unpark,
    FailedAuth
}

public enum AuditModule
{
    Auth,
    Proposal,
    FieldVisit,
    Estimate,
    TS,
    Prama,
    Budget,
    Workflow,
    Lotus,
    Master,
    Document,
    System
}

public enum AuditSeverity
{
    Info,
    Warning,
    Critical
}
