IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [AccountHeads] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Name_En] nvarchar(200) NOT NULL,
        [Name_Alt] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_AccountHeads] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [AuditTrails] (
        [Id] bigint NOT NULL IDENTITY,
        [Timestamp] datetime2 NOT NULL,
        [UserId] uniqueidentifier NULL,
        [UserName] nvarchar(200) NOT NULL,
        [UserRole] nvarchar(50) NOT NULL,
        [IpAddress] nvarchar(45) NOT NULL,
        [UserAgent] nvarchar(500) NOT NULL,
        [Action] nvarchar(50) NOT NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [Metadata] nvarchar(max) NULL,
        [Module] nvarchar(50) NOT NULL,
        [Severity] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_AuditTrails] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [CorporationSettings] (
        [Id] int NOT NULL,
        [CorporationName_En] nvarchar(300) NOT NULL,
        [CorporationName_Alt] nvarchar(300) NOT NULL,
        [PrimaryLanguage] nvarchar(5) NOT NULL DEFAULT N'en',
        [AlternateLanguage] nvarchar(5) NOT NULL,
        [AlternateLanguageLabel] nvarchar(50) NOT NULL,
        [DefaultDisplayLanguage] nvarchar(5) NOT NULL,
        [AutoTranslateEnabled] bit NOT NULL,
        [LogoUrl] nvarchar(500) NULL,
        [SmsGatewayProvider] nvarchar(100) NOT NULL,
        [SmsGatewayApiKey] nvarchar(500) NOT NULL,
        [OtpExpiryMinutes] int NOT NULL,
        [OtpMaxAttempts] int NOT NULL,
        [LotusSessionTimeoutMinutes] int NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CorporationSettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [Departments] (
        [Id] uniqueidentifier NOT NULL,
        [Name_En] nvarchar(200) NOT NULL,
        [Name_Alt] nvarchar(200) NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [Designations] (
        [Id] uniqueidentifier NOT NULL,
        [Name_En] nvarchar(200) NOT NULL,
        [Name_Alt] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Designations] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [FundTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Name_En] nvarchar(200) NOT NULL,
        [Name_Alt] nvarchar(200) NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FundTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [OtpRequests] (
        [Id] bigint NOT NULL IDENTITY,
        [MobileNumber] nvarchar(15) NOT NULL,
        [OtpHash] nvarchar(500) NOT NULL,
        [Purpose] nvarchar(50) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [IsUsed] bit NOT NULL,
        [AttemptCount] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_OtpRequests] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [ProcurementMethods] (
        [Id] uniqueidentifier NOT NULL,
        [Name_En] nvarchar(200) NOT NULL,
        [Name_Alt] nvarchar(200) NOT NULL,
        [Description_En] nvarchar(1000) NOT NULL,
        [Description_Alt] nvarchar(1000) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProcurementMethods] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [TenderPublicationPeriods] (
        [Id] uniqueidentifier NOT NULL,
        [MinAmount] decimal(18,2) NOT NULL,
        [MaxAmount] decimal(18,2) NOT NULL,
        [DurationDays] int NOT NULL,
        [Description_En] nvarchar(500) NOT NULL,
        [Description_Alt] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_TenderPublicationPeriods] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [Wards] (
        [Id] uniqueidentifier NOT NULL,
        [Number] int NOT NULL,
        [Name_En] nvarchar(200) NOT NULL,
        [Name_Alt] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Wards] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [FullName_En] nvarchar(200) NOT NULL,
        [FullName_Alt] nvarchar(200) NOT NULL,
        [MobileNumber] nvarchar(15) NOT NULL,
        [Email] nvarchar(200) NULL,
        [PasswordHash] nvarchar(500) NULL,
        [Role] nvarchar(50) NOT NULL,
        [DepartmentId] uniqueidentifier NULL,
        [DesignationId] uniqueidentifier NULL,
        [SignaturePath] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Users_Designations_DesignationId] FOREIGN KEY ([DesignationId]) REFERENCES [Designations] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [NotificationLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] uniqueidentifier NULL,
        [MobileNumber] nvarchar(15) NOT NULL,
        [Channel] nvarchar(20) NOT NULL,
        [TemplateName] nvarchar(100) NOT NULL,
        [Content] nvarchar(1000) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [ErrorMessage] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_NotificationLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NotificationLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [Proposals] (
        [Id] uniqueidentifier NOT NULL,
        [ProposalNumber] nvarchar(50) NOT NULL,
        [Date] date NOT NULL,
        [DepartmentId] uniqueidentifier NOT NULL,
        [SubmittedById] uniqueidentifier NOT NULL,
        [SubmitterDesignationId] uniqueidentifier NOT NULL,
        [Subject_En] nvarchar(500) NOT NULL,
        [Subject_Alt] nvarchar(500) NOT NULL,
        [FundTypeId] uniqueidentifier NOT NULL,
        [FundYear] nvarchar(20) NOT NULL,
        [ReferenceNumber] nvarchar(200) NOT NULL,
        [WardId] uniqueidentifier NULL,
        [BriefInfo_En] nvarchar(max) NOT NULL,
        [BriefInfo_Alt] nvarchar(max) NOT NULL,
        [EstimatedCost] decimal(18,2) NOT NULL,
        [AccountHeadId] uniqueidentifier NOT NULL,
        [ApprovedBudget] decimal(18,2) NOT NULL,
        [PreviousExpenditure] decimal(18,2) NOT NULL,
        [ProposedWorkCost] decimal(18,2) NOT NULL,
        [RemainingBalance] decimal(18,2) NOT NULL,
        [SiteInspectionDone] bit NOT NULL,
        [TechnicalApprovalDate] date NULL,
        [TechnicalApprovalNumber] nvarchar(100) NULL,
        [TechnicalApprovalCost] decimal(18,2) NULL,
        [CompetentAuthorityTADone] bit NOT NULL,
        [ProcurementMethodId] uniqueidentifier NULL,
        [TenderPublicationPeriodId] uniqueidentifier NULL,
        [TenderPeriodVerified] bit NOT NULL,
        [SiteOwnershipVerified] bit NOT NULL,
        [NocObtained] bit NOT NULL,
        [LegalObstacleExists] bit NOT NULL,
        [CourtCasePending] bit NOT NULL,
        [CourtCaseDetails_En] nvarchar(max) NULL,
        [CourtCaseDetails_Alt] nvarchar(max) NULL,
        [AuditObjectionExists] bit NOT NULL,
        [AuditObjectionDetails_En] nvarchar(max) NULL,
        [AuditObjectionDetails_Alt] nvarchar(max) NULL,
        [DuplicateFundCheckDone] bit NOT NULL,
        [OtherWorkInProgress] bit NOT NULL,
        [OtherWorkDetails_En] nvarchar(max) NULL,
        [OtherWorkDetails_Alt] nvarchar(max) NULL,
        [DlpCheckDone] bit NOT NULL,
        [OverallComplianceConfirmed] bit NOT NULL,
        [CompetentAuthorityId] uniqueidentifier NULL,
        [CurrentStage] nvarchar(50) NOT NULL,
        [PushBackCount] int NOT NULL DEFAULT 0,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Proposals] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Proposals_AccountHeads_AccountHeadId] FOREIGN KEY ([AccountHeadId]) REFERENCES [AccountHeads] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Proposals_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Proposals_Designations_SubmitterDesignationId] FOREIGN KEY ([SubmitterDesignationId]) REFERENCES [Designations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Proposals_FundTypes_FundTypeId] FOREIGN KEY ([FundTypeId]) REFERENCES [FundTypes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Proposals_ProcurementMethods_ProcurementMethodId] FOREIGN KEY ([ProcurementMethodId]) REFERENCES [ProcurementMethods] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Proposals_TenderPublicationPeriods_TenderPublicationPeriodId] FOREIGN KEY ([TenderPublicationPeriodId]) REFERENCES [TenderPublicationPeriods] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Proposals_Users_CompetentAuthorityId] FOREIGN KEY ([CompetentAuthorityId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Proposals_Users_SubmittedById] FOREIGN KEY ([SubmittedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Proposals_Wards_WardId] FOREIGN KEY ([WardId]) REFERENCES [Wards] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Token] nvarchar(500) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [RevokedAt] datetime2 NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [GeneratedDocuments] (
        [Id] uniqueidentifier NOT NULL,
        [ProposalId] uniqueidentifier NOT NULL,
        [DocumentKind] nvarchar(50) NOT NULL,
        [Title_En] nvarchar(300) NOT NULL,
        [Title_Alt] nvarchar(300) NOT NULL,
        [StoragePath] nvarchar(500) NOT NULL,
        [GeneratedById] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_GeneratedDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GeneratedDocuments_Proposals_ProposalId] FOREIGN KEY ([ProposalId]) REFERENCES [Proposals] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_GeneratedDocuments_Users_GeneratedById] FOREIGN KEY ([GeneratedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [ProposalDocuments] (
        [Id] uniqueidentifier NOT NULL,
        [ProposalId] uniqueidentifier NOT NULL,
        [DocumentType] nvarchar(50) NOT NULL,
        [FileName] nvarchar(300) NOT NULL,
        [FileSize] bigint NOT NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [StoragePath] nvarchar(500) NOT NULL,
        [UploadedById] uniqueidentifier NOT NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProposalDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProposalDocuments_Proposals_ProposalId] FOREIGN KEY ([ProposalId]) REFERENCES [Proposals] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProposalDocuments_Users_UploadedById] FOREIGN KEY ([UploadedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE TABLE [ProposalStageHistory] (
        [Id] bigint NOT NULL IDENTITY,
        [ProposalId] uniqueidentifier NOT NULL,
        [FromStage] nvarchar(50) NOT NULL,
        [ToStage] nvarchar(50) NOT NULL,
        [Action] nvarchar(50) NOT NULL,
        [ActionById] uniqueidentifier NOT NULL,
        [ActionByName_En] nvarchar(200) NOT NULL,
        [ActionByName_Alt] nvarchar(200) NOT NULL,
        [ActionByDesignation_En] nvarchar(200) NOT NULL,
        [ActionByDesignation_Alt] nvarchar(200) NOT NULL,
        [Reason_En] nvarchar(max) NULL,
        [Reason_Alt] nvarchar(max) NULL,
        [Opinion_En] nvarchar(max) NULL,
        [Opinion_Alt] nvarchar(max) NULL,
        [Remarks_En] nvarchar(max) NULL,
        [Remarks_Alt] nvarchar(max) NULL,
        [DscSignatureRef] nvarchar(500) NULL,
        [DscSignedAt] datetime2 NULL,
        [PushedBackToStage] nvarchar(50) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProposalStageHistory] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProposalStageHistory_Proposals_ProposalId] FOREIGN KEY ([ProposalId]) REFERENCES [Proposals] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProposalStageHistory_Users_ActionById] FOREIGN KEY ([ActionById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AccountHeads_Code] ON [AccountHeads] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_AuditTrails_EntityType] ON [AuditTrails] ([EntityType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_AuditTrails_Module] ON [AuditTrails] ([Module]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_AuditTrails_Timestamp] ON [AuditTrails] ([Timestamp]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_AuditTrails_UserId] ON [AuditTrails] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Departments_Code] ON [Departments] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_GeneratedDocuments_GeneratedById] ON [GeneratedDocuments] ([GeneratedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_GeneratedDocuments_ProposalId] ON [GeneratedDocuments] ([ProposalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_NotificationLogs_CreatedAt] ON [NotificationLogs] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_NotificationLogs_UserId] ON [NotificationLogs] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_OtpRequests_MobileNumber_CreatedAt] ON [OtpRequests] ([MobileNumber], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_ProposalDocuments_ProposalId] ON [ProposalDocuments] ([ProposalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_ProposalDocuments_UploadedById] ON [ProposalDocuments] ([UploadedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Proposals_AccountHeadId] ON [Proposals] ([AccountHeadId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Proposals_CompetentAuthorityId] ON [Proposals] ([CompetentAuthorityId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Proposals_CreatedAt] ON [Proposals] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Proposals_CurrentStage] ON [Proposals] ([CurrentStage]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Proposals_DepartmentId] ON [Proposals] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Proposals_FundTypeId] ON [Proposals] ([FundTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Proposals_ProcurementMethodId] ON [Proposals] ([ProcurementMethodId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Proposals_ProposalNumber] ON [Proposals] ([ProposalNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Proposals_SubmittedById] ON [Proposals] ([SubmittedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Proposals_SubmitterDesignationId] ON [Proposals] ([SubmitterDesignationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Proposals_TenderPublicationPeriodId] ON [Proposals] ([TenderPublicationPeriodId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Proposals_WardId] ON [Proposals] ([WardId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_ProposalStageHistory_ActionById] ON [ProposalStageHistory] ([ActionById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_ProposalStageHistory_CreatedAt] ON [ProposalStageHistory] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_ProposalStageHistory_ProposalId] ON [ProposalStageHistory] ([ProposalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Users_DepartmentId] ON [Users] ([DepartmentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE INDEX [IX_Users_DesignationId] ON [Users] ([DesignationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_MobileNumber] ON [Users] ([MobileNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Wards_Number] ON [Wards] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324110759_InitialCreateWithSignature'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260324110759_InitialCreateWithSignature', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [AccountantWillingToProcess] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [AccountingNumber] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [BalanceAmount] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [CompletedStep] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [FirstApproverRole] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [HasPreviousExpenditure] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [HomeId] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [LegalSurveyDone] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [PreviousExpenditureAmount] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [PublicationDays] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [Reason_Alt] nvarchar(2000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [Reason_En] nvarchar(2000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [SameWorkProposedInOtherFund] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [VendorTenureCompleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    ALTER TABLE [Proposals] ADD [WorkPlaceWithinPalika] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    CREATE TABLE [InAppNotifications] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [Title_En] nvarchar(300) NOT NULL,
        [Title_Alt] nvarchar(300) NOT NULL,
        [Message_En] nvarchar(1000) NOT NULL,
        [Message_Alt] nvarchar(1000) NOT NULL,
        [LinkUrl] nvarchar(500) NULL,
        [IsRead] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_InAppNotifications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InAppNotifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    CREATE TABLE [ProposalSignatures] (
        [Id] uniqueidentifier NOT NULL,
        [ProposalId] uniqueidentifier NOT NULL,
        [StageHistoryId] bigint NOT NULL,
        [SignedById] uniqueidentifier NOT NULL,
        [PageNumber] int NOT NULL,
        [PositionX] decimal(10,4) NOT NULL,
        [PositionY] decimal(10,4) NOT NULL,
        [Width] decimal(10,4) NOT NULL,
        [Height] decimal(10,4) NOT NULL,
        [Rotation] decimal(10,4) NOT NULL,
        [GeneratedPdfPath] nvarchar(500) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProposalSignatures] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProposalSignatures_ProposalStageHistory_StageHistoryId] FOREIGN KEY ([StageHistoryId]) REFERENCES [ProposalStageHistory] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProposalSignatures_Proposals_ProposalId] FOREIGN KEY ([ProposalId]) REFERENCES [Proposals] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProposalSignatures_Users_SignedById] FOREIGN KEY ([SignedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    CREATE TABLE [ProposalStepLocks] (
        [Id] uniqueidentifier NOT NULL,
        [ProposalId] uniqueidentifier NOT NULL,
        [StepNumber] int NOT NULL,
        [LockedById] uniqueidentifier NOT NULL,
        [LockedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [IsReleased] bit NOT NULL,
        CONSTRAINT [PK_ProposalStepLocks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProposalStepLocks_Proposals_ProposalId] FOREIGN KEY ([ProposalId]) REFERENCES [Proposals] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProposalStepLocks_Users_LockedById] FOREIGN KEY ([LockedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    CREATE INDEX [IX_InAppNotifications_CreatedAt] ON [InAppNotifications] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    CREATE INDEX [IX_InAppNotifications_UserId_IsRead] ON [InAppNotifications] ([UserId], [IsRead]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    CREATE INDEX [IX_ProposalSignatures_ProposalId] ON [ProposalSignatures] ([ProposalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    CREATE INDEX [IX_ProposalSignatures_SignedById] ON [ProposalSignatures] ([SignedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    CREATE INDEX [IX_ProposalSignatures_StageHistoryId] ON [ProposalSignatures] ([StageHistoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    CREATE INDEX [IX_ProposalStepLocks_ExpiresAt] ON [ProposalStepLocks] ([ExpiresAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    CREATE INDEX [IX_ProposalStepLocks_LockedById] ON [ProposalStepLocks] ([LockedById]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_ProposalStepLocks_ProposalId_StepNumber] ON [ProposalStepLocks] ([ProposalId], [StepNumber]) WHERE [IsReleased] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324131935_V1_WizardSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260324131935_V1_WizardSchema', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324150024_V1_AccountHeadNullable'
)
BEGIN
    ALTER TABLE [Proposals] DROP CONSTRAINT [FK_Proposals_AccountHeads_AccountHeadId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324150024_V1_AccountHeadNullable'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Proposals]') AND [c].[name] = N'AccountHeadId');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Proposals] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Proposals] ALTER COLUMN [AccountHeadId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324150024_V1_AccountHeadNullable'
)
BEGIN
    ALTER TABLE [Proposals] ADD CONSTRAINT [FK_Proposals_AccountHeads_AccountHeadId] FOREIGN KEY ([AccountHeadId]) REFERENCES [AccountHeads] ([Id]) ON DELETE SET NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324150024_V1_AccountHeadNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260324150024_V1_AccountHeadNullable', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324152402_V1_FundTypeOwners'
)
BEGIN
    ALTER TABLE [Proposals] ADD [FundOwner] nvarchar(50) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324152402_V1_FundTypeOwners'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FundTypes]') AND [c].[name] = N'Code');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [FundTypes] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [FundTypes] ALTER COLUMN [Code] nvarchar(50) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324152402_V1_FundTypeOwners'
)
BEGIN
    ALTER TABLE [FundTypes] ADD [IsCentral] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324152402_V1_FundTypeOwners'
)
BEGIN
    ALTER TABLE [FundTypes] ADD [IsDpdc] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324152402_V1_FundTypeOwners'
)
BEGIN
    ALTER TABLE [FundTypes] ADD [IsMnp] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324152402_V1_FundTypeOwners'
)
BEGIN
    ALTER TABLE [FundTypes] ADD [IsState] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324152402_V1_FundTypeOwners'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260324152402_V1_FundTypeOwners', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324164411_V1_AccountingOfficer'
)
BEGIN
    ALTER TABLE [Proposals] ADD [AccountingOfficerId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324164411_V1_AccountingOfficer'
)
BEGIN
    CREATE INDEX [IX_Proposals_AccountingOfficerId] ON [Proposals] ([AccountingOfficerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324164411_V1_AccountingOfficer'
)
BEGIN
    ALTER TABLE [Proposals] ADD CONSTRAINT [FK_Proposals_Users_AccountingOfficerId] FOREIGN KEY ([AccountingOfficerId]) REFERENCES [Users] ([Id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324164411_V1_AccountingOfficer'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260324164411_V1_AccountingOfficer', N'10.0.5');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324184535_V1_SubmitterDeclarationAndRemarks'
)
BEGIN
    ALTER TABLE [Proposals] ADD [SubmitterDeclarationAccepted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324184535_V1_SubmitterDeclarationAndRemarks'
)
BEGIN
    ALTER TABLE [Proposals] ADD [SubmitterDeclarationText_Alt] nvarchar(4000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324184535_V1_SubmitterDeclarationAndRemarks'
)
BEGIN
    ALTER TABLE [Proposals] ADD [SubmitterDeclarationText_En] nvarchar(4000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324184535_V1_SubmitterDeclarationAndRemarks'
)
BEGIN
    ALTER TABLE [Proposals] ADD [SubmitterRemarks_Alt] nvarchar(2000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324184535_V1_SubmitterDeclarationAndRemarks'
)
BEGIN
    ALTER TABLE [Proposals] ADD [SubmitterRemarks_En] nvarchar(2000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260324184535_V1_SubmitterDeclarationAndRemarks'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260324184535_V1_SubmitterDeclarationAndRemarks', N'10.0.5');
END;

COMMIT;
GO

