using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProposalManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithSignature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountHeads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name_En = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name_Alt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountHeads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditTrails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CorporationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CorporationName_En = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CorporationName_Alt = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    PrimaryLanguage = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false, defaultValue: "en"),
                    AlternateLanguage = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    AlternateLanguageLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DefaultDisplayLanguage = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    AutoTranslateEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SmsGatewayProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SmsGatewayApiKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OtpExpiryMinutes = table.Column<int>(type: "int", nullable: false),
                    OtpMaxAttempts = table.Column<int>(type: "int", nullable: false),
                    LotusSessionTimeoutMinutes = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporationSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name_En = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name_Alt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Designations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name_En = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name_Alt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FundTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name_En = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name_Alt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtpRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    OtpHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name_En = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name_Alt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description_En = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description_Alt = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenderPublicationPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    Description_En = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description_Alt = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenderPublicationPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Name_En = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name_Alt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName_En = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FullName_Alt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DesignationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SignaturePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Designations_DesignationId",
                        column: x => x.DesignationId,
                        principalTable: "Designations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Proposals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmitterDesignationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subject_En = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Subject_Alt = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FundTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FundYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WardId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BriefInfo_En = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BriefInfo_Alt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountHeadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousExpenditure = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProposedWorkCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SiteInspectionDone = table.Column<bool>(type: "bit", nullable: false),
                    TechnicalApprovalDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TechnicalApprovalNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TechnicalApprovalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CompetentAuthorityTADone = table.Column<bool>(type: "bit", nullable: false),
                    ProcurementMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenderPublicationPeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenderPeriodVerified = table.Column<bool>(type: "bit", nullable: false),
                    SiteOwnershipVerified = table.Column<bool>(type: "bit", nullable: false),
                    NocObtained = table.Column<bool>(type: "bit", nullable: false),
                    LegalObstacleExists = table.Column<bool>(type: "bit", nullable: false),
                    CourtCasePending = table.Column<bool>(type: "bit", nullable: false),
                    CourtCaseDetails_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CourtCaseDetails_Alt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditObjectionExists = table.Column<bool>(type: "bit", nullable: false),
                    AuditObjectionDetails_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuditObjectionDetails_Alt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuplicateFundCheckDone = table.Column<bool>(type: "bit", nullable: false),
                    OtherWorkInProgress = table.Column<bool>(type: "bit", nullable: false),
                    OtherWorkDetails_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherWorkDetails_Alt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DlpCheckDone = table.Column<bool>(type: "bit", nullable: false),
                    OverallComplianceConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    CompetentAuthorityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentStage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PushBackCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proposals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Proposals_AccountHeads_AccountHeadId",
                        column: x => x.AccountHeadId,
                        principalTable: "AccountHeads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proposals_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proposals_Designations_SubmitterDesignationId",
                        column: x => x.SubmitterDesignationId,
                        principalTable: "Designations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proposals_FundTypes_FundTypeId",
                        column: x => x.FundTypeId,
                        principalTable: "FundTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proposals_ProcurementMethods_ProcurementMethodId",
                        column: x => x.ProcurementMethodId,
                        principalTable: "ProcurementMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Proposals_TenderPublicationPeriods_TenderPublicationPeriodId",
                        column: x => x.TenderPublicationPeriodId,
                        principalTable: "TenderPublicationPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Proposals_Users_CompetentAuthorityId",
                        column: x => x.CompetentAuthorityId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Proposals_Users_SubmittedById",
                        column: x => x.SubmittedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Proposals_Wards_WardId",
                        column: x => x.WardId,
                        principalTable: "Wards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneratedDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentKind = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title_En = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Title_Alt = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GeneratedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GeneratedDocuments_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GeneratedDocuments_Users_GeneratedById",
                        column: x => x.GeneratedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProposalDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UploadedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalDocuments_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProposalDocuments_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProposalStageHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToStage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionByName_En = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ActionByName_Alt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ActionByDesignation_En = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ActionByDesignation_Alt = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Reason_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason_Alt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Opinion_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Opinion_Alt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks_En = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks_Alt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DscSignatureRef = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DscSignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PushedBackToStage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalStageHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalStageHistory_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProposalStageHistory_Users_ActionById",
                        column: x => x.ActionById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountHeads_Code",
                table: "AccountHeads",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_EntityType",
                table: "AuditTrails",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_Module",
                table: "AuditTrails",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_Timestamp",
                table: "AuditTrails",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_UserId",
                table: "AuditTrails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_GeneratedById",
                table: "GeneratedDocuments",
                column: "GeneratedById");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_ProposalId",
                table: "GeneratedDocuments",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_CreatedAt",
                table: "NotificationLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_UserId",
                table: "NotificationLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpRequests_MobileNumber_CreatedAt",
                table: "OtpRequests",
                columns: new[] { "MobileNumber", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProposalDocuments_ProposalId",
                table: "ProposalDocuments",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalDocuments_UploadedById",
                table: "ProposalDocuments",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_AccountHeadId",
                table: "Proposals",
                column: "AccountHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_CompetentAuthorityId",
                table: "Proposals",
                column: "CompetentAuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_CreatedAt",
                table: "Proposals",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_CurrentStage",
                table: "Proposals",
                column: "CurrentStage");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_DepartmentId",
                table: "Proposals",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_FundTypeId",
                table: "Proposals",
                column: "FundTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ProcurementMethodId",
                table: "Proposals",
                column: "ProcurementMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_ProposalNumber",
                table: "Proposals",
                column: "ProposalNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_SubmittedById",
                table: "Proposals",
                column: "SubmittedById");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_SubmitterDesignationId",
                table: "Proposals",
                column: "SubmitterDesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_TenderPublicationPeriodId",
                table: "Proposals",
                column: "TenderPublicationPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_WardId",
                table: "Proposals",
                column: "WardId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalStageHistory_ActionById",
                table: "ProposalStageHistory",
                column: "ActionById");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalStageHistory_CreatedAt",
                table: "ProposalStageHistory",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalStageHistory_ProposalId",
                table: "ProposalStageHistory",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DesignationId",
                table: "Users",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_MobileNumber",
                table: "Users",
                column: "MobileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wards_Number",
                table: "Wards",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditTrails");

            migrationBuilder.DropTable(
                name: "CorporationSettings");

            migrationBuilder.DropTable(
                name: "GeneratedDocuments");

            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DropTable(
                name: "OtpRequests");

            migrationBuilder.DropTable(
                name: "ProposalDocuments");

            migrationBuilder.DropTable(
                name: "ProposalStageHistory");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Proposals");

            migrationBuilder.DropTable(
                name: "AccountHeads");

            migrationBuilder.DropTable(
                name: "FundTypes");

            migrationBuilder.DropTable(
                name: "ProcurementMethods");

            migrationBuilder.DropTable(
                name: "TenderPublicationPeriods");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Wards");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Designations");
        }
    }
}
