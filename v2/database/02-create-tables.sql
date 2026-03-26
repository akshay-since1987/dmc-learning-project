/*
    Proposal Management System V2 — All Tables
    Database: dmc-v2-ProposalMgmt
    
    Tables are created in dependency order (parents before children).
    Run 01-create-database.sql first.
*/

USE [dmc-v2-ProposalMgmt];
GO

-- ============================================================
-- 0. PALIKAS (Municipal Corporations / Nagar Palikas)
--    Root tenant entity. All scoped tables FK here.
-- ============================================================
CREATE TABLE [dbo].[Palikas] (
    [Id]                    UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [Name_En]               NVARCHAR(300)       NOT NULL,
    [Name_Mr]               NVARCHAR(300)       NULL,
    [ShortCode]             NVARCHAR(20)        NOT NULL,
    [Type]                  NVARCHAR(50)        NOT NULL,           -- MahanagarPalika | NagarPalika | NagarPanchayat
    [LogoUrl]               NVARCHAR(500)       NULL,
    [Address_En]            NVARCHAR(500)       NULL,
    [Address_Mr]            NVARCHAR(500)       NULL,
    [ContactPhone]          NVARCHAR(20)        NULL,
    [Website]               NVARCHAR(300)       NULL,
    [PrimaryLanguage]       NVARCHAR(5)         NOT NULL DEFAULT 'en',
    [AlternateLanguage]     NVARCHAR(5)         NOT NULL DEFAULT 'mr',
    [ProposalNumberPrefix]  NVARCHAR(20)        NOT NULL,
    [CurrentFinancialYear]  NVARCHAR(20)        NOT NULL,
    [SmsGatewayProvider]    NVARCHAR(100)       NULL,
    [SmsGatewayApiKey]      NVARCHAR(500)       NULL,
    [OtpExpiryMinutes]      INT                 NOT NULL DEFAULT 5,
    [OtpMaxAttempts]        INT                 NOT NULL DEFAULT 3,
    [IsActive]              BIT                 NOT NULL DEFAULT 1,
    [IsDeleted]             BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_Palikas] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_Palikas_ShortCode] UNIQUE ([ShortCode])
);
GO

-- ============================================================
-- 1A. DEPARTMENTS
-- ============================================================
CREATE TABLE [dbo].[Departments] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [PalikaId]      UNIQUEIDENTIFIER    NOT NULL,
    [Name_En]       NVARCHAR(200)       NOT NULL,
    [Name_Mr]       NVARCHAR(200)       NULL,
    [Code]          NVARCHAR(20)        NULL,
    [IsActive]      BIT                 NOT NULL DEFAULT 1,
    [IsDeleted]     BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_Departments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Departments_Palikas] FOREIGN KEY ([PalikaId]) REFERENCES [dbo].[Palikas]([Id])
);
GO

-- ============================================================
-- 1B. DEPT WORK CATEGORIES
-- ============================================================
CREATE TABLE [dbo].[DeptWorkCategories] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [DepartmentId]  UNIQUEIDENTIFIER    NULL,
    [Name_En]       NVARCHAR(200)       NOT NULL,
    [Name_Mr]       NVARCHAR(200)       NULL,
    [IsActive]      BIT                 NOT NULL DEFAULT 1,
    [IsDeleted]     BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_DeptWorkCategories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DeptWorkCategories_Departments] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([Id])
);
GO

-- ============================================================
-- 1C. DESIGNATIONS
-- ============================================================
CREATE TABLE [dbo].[Designations] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [PalikaId]      UNIQUEIDENTIFIER    NOT NULL,
    [Name_En]       NVARCHAR(200)       NOT NULL,
    [Name_Mr]       NVARCHAR(200)       NULL,
    [IsActive]      BIT                 NOT NULL DEFAULT 1,
    [IsDeleted]     BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_Designations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Designations_Palikas] FOREIGN KEY ([PalikaId]) REFERENCES [dbo].[Palikas]([Id])
);
GO

-- ============================================================
-- 1D. ZONES
-- ============================================================
CREATE TABLE [dbo].[Zones] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [PalikaId]      UNIQUEIDENTIFIER    NOT NULL,
    [Name_En]       NVARCHAR(200)       NOT NULL,
    [Name_Mr]       NVARCHAR(200)       NULL,
    [Code]          NVARCHAR(20)        NULL,
    [OfficeName_En] NVARCHAR(300)       NULL,
    [OfficeName_Mr] NVARCHAR(300)       NULL,
    [IsActive]      BIT                 NOT NULL DEFAULT 1,
    [IsDeleted]     BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_Zones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Zones_Palikas] FOREIGN KEY ([PalikaId]) REFERENCES [dbo].[Palikas]([Id])
);
GO

-- ============================================================
-- 1E. PRABHAGS (child of Zone)
-- ============================================================
CREATE TABLE [dbo].[Prabhags] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [PalikaId]          UNIQUEIDENTIFIER    NOT NULL,
    [ZoneId]            UNIQUEIDENTIFIER    NOT NULL,
    [Number]            INT                 NOT NULL,
    [Name_En]           NVARCHAR(200)       NOT NULL,
    [Name_Mr]           NVARCHAR(200)       NULL,
    [CorporatorSeats]   INT                 NOT NULL DEFAULT 4,
    [Population]        INT                 NULL,
    [IsActive]          BIT                 NOT NULL DEFAULT 1,
    [IsDeleted]         BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]         DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_Prabhags] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Prabhags_Palikas] FOREIGN KEY ([PalikaId]) REFERENCES [dbo].[Palikas]([Id]),
    CONSTRAINT [FK_Prabhags_Zones]   FOREIGN KEY ([ZoneId])   REFERENCES [dbo].[Zones]([Id])
);
GO

-- ============================================================
-- 1F. REQUEST SOURCES
-- ============================================================
CREATE TABLE [dbo].[RequestSources] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [PalikaId]      UNIQUEIDENTIFIER    NOT NULL,
    [Name_En]       NVARCHAR(200)       NOT NULL,
    [Name_Mr]       NVARCHAR(200)       NULL,
    [IsActive]      BIT                 NOT NULL DEFAULT 1,
    [IsDeleted]     BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_RequestSources] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RequestSources_Palikas] FOREIGN KEY ([PalikaId]) REFERENCES [dbo].[Palikas]([Id])
);
GO

-- ============================================================
-- 1G. SITE CONDITIONS (global — no PalikaId)
-- ============================================================
CREATE TABLE [dbo].[SiteConditions] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [Name_En]       NVARCHAR(100)       NOT NULL,
    [Name_Mr]       NVARCHAR(100)       NULL,
    [SortOrder]     INT                 NOT NULL DEFAULT 0,
    [IsActive]      BIT                 NOT NULL DEFAULT 1,
    [IsDeleted]     BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_SiteConditions] PRIMARY KEY ([Id])
);
GO

-- ============================================================
-- 1H. WORK EXECUTION METHODS
-- ============================================================
CREATE TABLE [dbo].[WorkExecutionMethods] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [PalikaId]      UNIQUEIDENTIFIER    NOT NULL,
    [Name_En]       NVARCHAR(200)       NOT NULL,
    [Name_Mr]       NVARCHAR(200)       NULL,
    [IsActive]      BIT                 NOT NULL DEFAULT 1,
    [IsDeleted]     BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_WorkExecutionMethods] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkExecutionMethods_Palikas] FOREIGN KEY ([PalikaId]) REFERENCES [dbo].[Palikas]([Id])
);
GO

-- ============================================================
-- 1I. FUND TYPES
-- ============================================================
CREATE TABLE [dbo].[FundTypes] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [PalikaId]      UNIQUEIDENTIFIER    NOT NULL,
    [Name_En]       NVARCHAR(200)       NOT NULL,
    [Name_Mr]       NVARCHAR(200)       NULL,
    [IsActive]      BIT                 NOT NULL DEFAULT 1,
    [IsDeleted]     BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_FundTypes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FundTypes_Palikas] FOREIGN KEY ([PalikaId]) REFERENCES [dbo].[Palikas]([Id])
);
GO

-- ============================================================
-- 1J. BUDGET HEADS
-- ============================================================
CREATE TABLE [dbo].[BudgetHeads] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [PalikaId]          UNIQUEIDENTIFIER    NOT NULL,
    [DepartmentId]      UNIQUEIDENTIFIER    NOT NULL,
    [FundTypeId]        UNIQUEIDENTIFIER    NOT NULL,
    [Code]              NVARCHAR(50)        NOT NULL,
    [Name_En]           NVARCHAR(300)       NOT NULL,
    [Name_Mr]           NVARCHAR(300)       NULL,
    [FinancialYear]     NVARCHAR(20)        NOT NULL,
    [AllocatedAmount]   DECIMAL(18,2)       NOT NULL,
    [CurrentAvailable]  DECIMAL(18,2)       NOT NULL,
    [IsActive]          BIT                 NOT NULL DEFAULT 1,
    [IsDeleted]         BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]         DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_BudgetHeads] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BudgetHeads_Palikas]     FOREIGN KEY ([PalikaId])     REFERENCES [dbo].[Palikas]([Id]),
    CONSTRAINT [FK_BudgetHeads_Departments] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Departments]([Id]),
    CONSTRAINT [FK_BudgetHeads_FundTypes]   FOREIGN KEY ([FundTypeId])   REFERENCES [dbo].[FundTypes]([Id])
);
GO

-- ============================================================
-- 2. USERS
-- ============================================================
CREATE TABLE [dbo].[Users] (
    [Id]                UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [FullName_En]       NVARCHAR(200)       NOT NULL,
    [FullName_Mr]       NVARCHAR(200)       NULL,
    [MobileNumber]      NVARCHAR(15)        NOT NULL,
    [Email]             NVARCHAR(200)       NULL,
    [PasswordHash]      NVARCHAR(500)       NULL,
    [Role]              NVARCHAR(50)        NOT NULL,
    [DepartmentId]      UNIQUEIDENTIFIER    NULL,
    [DesignationId]     UNIQUEIDENTIFIER    NULL,
    [PalikaId]          UNIQUEIDENTIFIER    NOT NULL,
    [SignaturePath]     NVARCHAR(500)       NULL,
    [IsActive]          BIT                 NOT NULL DEFAULT 1,
    [IsDeleted]         BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]         DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]         DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_Users_MobileNumber] UNIQUE ([MobileNumber]),
    CONSTRAINT [FK_Users_Palikas]      FOREIGN KEY ([PalikaId])      REFERENCES [dbo].[Palikas]([Id]),
    CONSTRAINT [FK_Users_Departments]  FOREIGN KEY ([DepartmentId])  REFERENCES [dbo].[Departments]([Id]),
    CONSTRAINT [FK_Users_Designations] FOREIGN KEY ([DesignationId]) REFERENCES [dbo].[Designations]([Id])
);
GO

-- ============================================================
-- 3A. OTP REQUESTS
-- ============================================================
CREATE TABLE [dbo].[OtpRequests] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [MobileNumber]  NVARCHAR(15)        NOT NULL,
    [OtpHash]       NVARCHAR(500)       NOT NULL,
    [Purpose]       NVARCHAR(50)        NOT NULL,
    [ExpiresAt]     DATETIME2           NOT NULL,
    [IsUsed]        BIT                 NOT NULL DEFAULT 0,
    [AttemptCount]  INT                 NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_OtpRequests] PRIMARY KEY ([Id])
);
GO

-- ============================================================
-- 3B. REFRESH TOKENS
-- ============================================================
CREATE TABLE [dbo].[RefreshTokens] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [UserId]        UNIQUEIDENTIFIER    NOT NULL,
    [Token]         NVARCHAR(500)       NOT NULL,
    [ExpiresAt]     DATETIME2           NOT NULL,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [RevokedAt]     DATETIME2           NULL,

    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
);
GO

-- ============================================================
-- 4. PROPOSALS (Tab 1)
-- ============================================================
CREATE TABLE [dbo].[Proposals] (
    [Id]                        UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [ProposalNumber]            NVARCHAR(50)        NOT NULL,
    [PalikaId]                  UNIQUEIDENTIFIER    NOT NULL,
    [ProposalDate]              DATE                NOT NULL DEFAULT GETDATE(),
    [DepartmentId]              UNIQUEIDENTIFIER    NOT NULL,
    [DeptWorkCategoryId]        UNIQUEIDENTIFIER    NOT NULL,
    [CreatedById]               UNIQUEIDENTIFIER    NOT NULL,

    -- Location
    [ZoneId]                    UNIQUEIDENTIFIER    NOT NULL,
    [PrabhagId]                 UNIQUEIDENTIFIER    NOT NULL,
    [Area]                      NVARCHAR(300)       NULL,
    [LocationAddress_En]        NVARCHAR(500)       NULL,
    [LocationAddress_Mr]        NVARCHAR(500)       NULL,
    [LocationMapPath]           NVARCHAR(500)       NULL,

    -- Work Details
    [WorkTitle_En]              NVARCHAR(500)       NOT NULL,
    [WorkTitle_Mr]              NVARCHAR(500)       NULL,
    [WorkDescription_En]        NVARCHAR(MAX)       NOT NULL,
    [WorkDescription_Mr]        NVARCHAR(MAX)       NULL,

    -- Request Source
    [RequestSourceId]           UNIQUEIDENTIFIER    NULL,
    [RequestorName]             NVARCHAR(200)       NULL,
    [RequestorMobile]           NVARCHAR(15)        NULL,
    [RequestorAddress]          NVARCHAR(500)       NULL,
    [RequestorDesignation]      NVARCHAR(200)       NULL,
    [RequestorOrganisation]     NVARCHAR(200)       NULL,

    [Priority]                  NVARCHAR(20)        NOT NULL DEFAULT 'Medium',

    -- Workflow State
    [CurrentStage]              NVARCHAR(50)        NOT NULL DEFAULT 'Draft',
    [CurrentOwnerId]            UNIQUEIDENTIFIER    NULL,
    [PushBackCount]             INT                 NOT NULL DEFAULT 0,
    [ParkedAt]                  DATETIME2           NULL,
    [ParkedAtStage]             NVARCHAR(50)        NULL,

    -- Completion Tracking
    [CompletedTab]              INT                 NOT NULL DEFAULT 1,

    -- System
    [IsDeleted]                 BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]                 DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]                 DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_Proposals] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_Proposals_Number] UNIQUE ([ProposalNumber]),
    CONSTRAINT [FK_Proposals_Palikas]           FOREIGN KEY ([PalikaId])           REFERENCES [dbo].[Palikas]([Id]),
    CONSTRAINT [FK_Proposals_Departments]       FOREIGN KEY ([DepartmentId])       REFERENCES [dbo].[Departments]([Id]),
    CONSTRAINT [FK_Proposals_DeptWorkCategories] FOREIGN KEY ([DeptWorkCategoryId]) REFERENCES [dbo].[DeptWorkCategories]([Id]),
    CONSTRAINT [FK_Proposals_CreatedBy]         FOREIGN KEY ([CreatedById])        REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_Proposals_Zones]             FOREIGN KEY ([ZoneId])             REFERENCES [dbo].[Zones]([Id]),
    CONSTRAINT [FK_Proposals_Prabhags]          FOREIGN KEY ([PrabhagId])          REFERENCES [dbo].[Prabhags]([Id]),
    CONSTRAINT [FK_Proposals_RequestSources]    FOREIGN KEY ([RequestSourceId])    REFERENCES [dbo].[RequestSources]([Id]),
    CONSTRAINT [FK_Proposals_CurrentOwner]      FOREIGN KEY ([CurrentOwnerId])     REFERENCES [dbo].[Users]([Id])
);
GO

-- ============================================================
-- 5. PROPOSAL DOCUMENTS
-- ============================================================
CREATE TABLE [dbo].[ProposalDocuments] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [ProposalId]    UNIQUEIDENTIFIER    NOT NULL,
    [TabNumber]     INT                 NOT NULL,
    [DocumentType]  NVARCHAR(50)        NOT NULL,
    [DocName]       NVARCHAR(300)       NULL,
    [FileName]      NVARCHAR(300)       NOT NULL,
    [FileSize]      BIGINT              NOT NULL,
    [ContentType]   NVARCHAR(100)       NOT NULL,
    [StoragePath]   NVARCHAR(500)       NOT NULL,
    [UploadedById]  UNIQUEIDENTIFIER    NOT NULL,
    [IsDeleted]     BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_ProposalDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProposalDocuments_Proposals] FOREIGN KEY ([ProposalId]) REFERENCES [dbo].[Proposals]([Id]),
    CONSTRAINT [FK_ProposalDocuments_Users]     FOREIGN KEY ([UploadedById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- ============================================================
-- 6. FIELD VISITS (Tab 2)
-- ============================================================
CREATE TABLE [dbo].[FieldVisits] (
    [Id]                    UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [ProposalId]            UNIQUEIDENTIFIER    NOT NULL,
    [VisitNumber]           INT                 NOT NULL,
    [AssignedToId]          UNIQUEIDENTIFIER    NOT NULL,
    [AssignedById]          UNIQUEIDENTIFIER    NOT NULL,

    [InspectionById]        UNIQUEIDENTIFIER    NULL,
    [InspectionDate]        DATE                NULL,
    [SiteConditionId]       UNIQUEIDENTIFIER    NULL,
    [ProblemDescription_En] NVARCHAR(MAX)       NULL,
    [ProblemDescription_Mr] NVARCHAR(MAX)       NULL,
    [Measurements_En]       NVARCHAR(MAX)       NULL,
    [Measurements_Mr]       NVARCHAR(MAX)       NULL,
    [GpsLatitude]           DECIMAL(10,7)       NULL,
    [GpsLongitude]          DECIMAL(10,7)       NULL,
    [Remark_En]             NVARCHAR(MAX)       NULL,
    [Remark_Mr]             NVARCHAR(MAX)       NULL,
    [Recommendation_En]     NVARCHAR(MAX)       NULL,
    [Recommendation_Mr]     NVARCHAR(MAX)       NULL,
    [UploadedPdfPath]       NVARCHAR(500)       NULL,

    [SignaturePath]         NVARCHAR(500)       NULL,
    [Status]                NVARCHAR(20)        NOT NULL DEFAULT 'Assigned',
    [CompletedAt]           DATETIME2           NULL,

    [IsDeleted]             BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_FieldVisits] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FieldVisits_Proposals]       FOREIGN KEY ([ProposalId])      REFERENCES [dbo].[Proposals]([Id]),
    CONSTRAINT [FK_FieldVisits_AssignedTo]      FOREIGN KEY ([AssignedToId])    REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_FieldVisits_AssignedBy]      FOREIGN KEY ([AssignedById])    REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_FieldVisits_InspectionBy]    FOREIGN KEY ([InspectionById])  REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_FieldVisits_SiteConditions]  FOREIGN KEY ([SiteConditionId]) REFERENCES [dbo].[SiteConditions]([Id])
);
GO

-- ============================================================
-- 6B. FIELD VISIT PHOTOS
-- ============================================================
CREATE TABLE [dbo].[FieldVisitPhotos] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [FieldVisitId]  UNIQUEIDENTIFIER    NOT NULL,
    [FileName]      NVARCHAR(300)       NOT NULL,
    [FileSize]      BIGINT              NOT NULL,
    [ContentType]   NVARCHAR(100)       NOT NULL,
    [StoragePath]   NVARCHAR(500)       NOT NULL,
    [Caption]       NVARCHAR(300)       NULL,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_FieldVisitPhotos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FieldVisitPhotos_FieldVisits] FOREIGN KEY ([FieldVisitId]) REFERENCES [dbo].[FieldVisits]([Id])
);
GO

-- ============================================================
-- 7. ESTIMATES (Tab 3)
-- ============================================================
CREATE TABLE [dbo].[Estimates] (
    [Id]                        UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [ProposalId]                UNIQUEIDENTIFIER    NOT NULL,
    [EstimatePdfPath]           NVARCHAR(500)       NULL,
    [EstimatedCost]             DECIMAL(18,2)       NULL,
    [PreparedById]              UNIQUEIDENTIFIER    NOT NULL,
    [PreparedSignaturePath]     NVARCHAR(500)       NULL,

    -- Approval
    [SentToRole]                NVARCHAR(50)        NULL,
    [SentToId]                  UNIQUEIDENTIFIER    NULL,
    [ApprovedById]              UNIQUEIDENTIFIER    NULL,
    [ApproverSignaturePath]     NVARCHAR(500)       NULL,
    [ApproverDisclaimerAccepted] BIT                NOT NULL DEFAULT 0,
    [ApproverOpinion_En]        NVARCHAR(MAX)       NULL,
    [ApproverOpinion_Mr]        NVARCHAR(MAX)       NULL,
    [Status]                    NVARCHAR(30)        NOT NULL DEFAULT 'Draft',
    [ReturnQueryNote_En]        NVARCHAR(MAX)       NULL,
    [ReturnQueryNote_Mr]        NVARCHAR(MAX)       NULL,

    [ApprovedAt]                DATETIME2           NULL,
    [IsDeleted]                 BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]                 DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]                 DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_Estimates] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_Estimates_Proposal] UNIQUE ([ProposalId]),
    CONSTRAINT [FK_Estimates_Proposals]   FOREIGN KEY ([ProposalId])   REFERENCES [dbo].[Proposals]([Id]),
    CONSTRAINT [FK_Estimates_PreparedBy]  FOREIGN KEY ([PreparedById]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_Estimates_SentTo]      FOREIGN KEY ([SentToId])     REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_Estimates_ApprovedBy]  FOREIGN KEY ([ApprovedById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- ============================================================
-- 8. TECHNICAL SANCTIONS (Tab 4)
-- ============================================================
CREATE TABLE [dbo].[TechnicalSanctions] (
    [Id]                            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [ProposalId]                    UNIQUEIDENTIFIER    NOT NULL,
    [TsNumber]                      NVARCHAR(100)       NULL,
    [TsDate]                        DATE                NULL,
    [TsAmount]                      DECIMAL(18,2)       NULL,
    [Description_En]                NVARCHAR(MAX)       NULL,
    [Description_Mr]                NVARCHAR(MAX)       NULL,

    -- Documents
    [TsPdfPath]                     NVARCHAR(500)       NULL,
    [OutsideApprovalLetterPath]     NVARCHAR(500)       NULL,

    -- Sanctioned By
    [SanctionedByName]              NVARCHAR(200)       NULL,
    [SanctionedByDept]              NVARCHAR(200)       NULL,
    [SanctionedByDesignation]       NVARCHAR(200)       NULL,

    -- Prepared / Signed
    [PreparedById]                  UNIQUEIDENTIFIER    NULL,
    [SignedById]                    UNIQUEIDENTIFIER    NULL,
    [SignerSignaturePath]           NVARCHAR(500)       NULL,
    [SignedAt]                      DATETIME2           NULL,

    [Status]                        NVARCHAR(30)        NOT NULL DEFAULT 'Draft',
    [IsDeleted]                     BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]                     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]                     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_TechnicalSanctions] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_TechnicalSanctions_Proposal] UNIQUE ([ProposalId]),
    CONSTRAINT [FK_TechnicalSanctions_Proposals]  FOREIGN KEY ([ProposalId])   REFERENCES [dbo].[Proposals]([Id]),
    CONSTRAINT [FK_TechnicalSanctions_PreparedBy] FOREIGN KEY ([PreparedById]) REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_TechnicalSanctions_SignedBy]   FOREIGN KEY ([SignedById])   REFERENCES [dbo].[Users]([Id])
);
GO

-- ============================================================
-- 9. PRAMA DETAILS (Tab 5)
-- ============================================================
CREATE TABLE [dbo].[PramaDetails] (
    [Id]                    UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [ProposalId]            UNIQUEIDENTIFIER    NOT NULL,
    [FundTypeId]            UNIQUEIDENTIFIER    NULL,
    [BudgetHeadId]          UNIQUEIDENTIFIER    NULL,
    [FundApprovalYear]      NVARCHAR(20)        NULL,
    [DeptUserName_En]       NVARCHAR(200)       NULL,
    [DeptUserName_Mr]       NVARCHAR(200)       NULL,
    [References_En]         NVARCHAR(MAX)       NULL,
    [References_Mr]         NVARCHAR(MAX)       NULL,
    [AdditionalDetails_En]  NVARCHAR(MAX)       NULL,
    [AdditionalDetails_Mr]  NVARCHAR(MAX)       NULL,

    [IsDeleted]             BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]             DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]             DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_PramaDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_PramaDetails_Proposal] UNIQUE ([ProposalId]),
    CONSTRAINT [FK_PramaDetails_Proposals]   FOREIGN KEY ([ProposalId])   REFERENCES [dbo].[Proposals]([Id]),
    CONSTRAINT [FK_PramaDetails_FundTypes]   FOREIGN KEY ([FundTypeId])   REFERENCES [dbo].[FundTypes]([Id]),
    CONSTRAINT [FK_PramaDetails_BudgetHeads] FOREIGN KEY ([BudgetHeadId]) REFERENCES [dbo].[BudgetHeads]([Id])
);
GO

-- ============================================================
-- 10. BUDGET DETAILS (Tab 6)
-- ============================================================
CREATE TABLE [dbo].[BudgetDetails] (
    [Id]                        UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [ProposalId]                UNIQUEIDENTIFIER    NOT NULL,
    [WorkExecutionMethodId]     UNIQUEIDENTIFIER    NULL,
    [WorkDurationDays]          INT                 NULL,
    [TenderVerificationDone]    BIT                 NOT NULL DEFAULT 0,
    [BudgetHeadId]              UNIQUEIDENTIFIER    NULL,
    [AllocatedFund]             DECIMAL(18,2)       NULL,
    [CurrentAvailableFund]      DECIMAL(18,2)       NULL,
    [OldExpenditure]            DECIMAL(18,2)       NULL,
    [EstimatedCost]             DECIMAL(18,2)       NULL,
    [BalanceAmount]             DECIMAL(18,2)       NULL,
    [AccountSerialNo]           NVARCHAR(100)       NULL,

    -- Compliance
    [ComplianceNotes_En]        NVARCHAR(MAX)       NULL,
    [ComplianceNotes_Mr]        NVARCHAR(MAX)       NULL,

    -- Approval authority
    [DeterminedApprovalSlab]    NVARCHAR(50)        NULL,
    [FinalAuthorityRole]        NVARCHAR(50)        NULL,

    [IsDeleted]                 BIT                 NOT NULL DEFAULT 0,
    [CreatedAt]                 DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [UpdatedAt]                 DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_BudgetDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [UQ_BudgetDetails_Proposal] UNIQUE ([ProposalId]),
    CONSTRAINT [FK_BudgetDetails_Proposals]            FOREIGN KEY ([ProposalId])            REFERENCES [dbo].[Proposals]([Id]),
    CONSTRAINT [FK_BudgetDetails_WorkExecutionMethods] FOREIGN KEY ([WorkExecutionMethodId]) REFERENCES [dbo].[WorkExecutionMethods]([Id]),
    CONSTRAINT [FK_BudgetDetails_BudgetHeads]          FOREIGN KEY ([BudgetHeadId])          REFERENCES [dbo].[BudgetHeads]([Id])
);
GO

-- ============================================================
-- 11. PROPOSAL APPROVALS (Formal Approval Chain)
-- ============================================================
CREATE TABLE [dbo].[ProposalApprovals] (
    [Id]                    BIGINT IDENTITY(1,1) NOT NULL,
    [ProposalId]            UNIQUEIDENTIFIER    NOT NULL,
    [StageRole]             NVARCHAR(50)        NOT NULL,
    [Action]                NVARCHAR(30)        NOT NULL,

    -- Actor info
    [ActorId]               UNIQUEIDENTIFIER    NOT NULL,
    [ActorName_En]          NVARCHAR(200)       NULL,
    [ActorName_Mr]          NVARCHAR(200)       NULL,
    [ActorDesignation_En]   NVARCHAR(200)       NULL,
    [ActorDesignation_Mr]   NVARCHAR(200)       NULL,

    -- Disclaimer
    [DisclaimerText]        NVARCHAR(MAX)       NOT NULL,
    [DisclaimerAccepted]    BIT                 NOT NULL,

    -- Opinion & Signature
    [Opinion_En]            NVARCHAR(MAX)       NULL,
    [Opinion_Mr]            NVARCHAR(MAX)       NULL,
    [SignaturePath]         NVARCHAR(500)       NULL,

    -- Push-back
    [PushBackNote_En]       NVARCHAR(MAX)       NULL,
    [PushBackNote_Mr]       NVARCHAR(MAX)       NULL,

    -- PDF
    [ConsolidatedPdfPath]   NVARCHAR(500)       NULL,

    [CreatedAt]             DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_ProposalApprovals] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProposalApprovals_Proposals] FOREIGN KEY ([ProposalId]) REFERENCES [dbo].[Proposals]([Id]),
    CONSTRAINT [FK_ProposalApprovals_Users]     FOREIGN KEY ([ActorId])    REFERENCES [dbo].[Users]([Id])
);
GO

-- ============================================================
-- 12. GENERATED PDFs
-- ============================================================
CREATE TABLE [dbo].[GeneratedPdfs] (
    [Id]            UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWID(),
    [ProposalId]    UNIQUEIDENTIFIER    NOT NULL,
    [PdfType]       NVARCHAR(50)        NOT NULL,
    [TabNumber]     INT                 NULL,
    [StageRole]     NVARCHAR(50)        NULL,
    [Title_En]      NVARCHAR(300)       NULL,
    [Title_Mr]      NVARCHAR(300)       NULL,
    [StoragePath]   NVARCHAR(500)       NOT NULL,
    [GeneratedById] UNIQUEIDENTIFIER    NOT NULL,
    [FileSize]      BIGINT              NULL,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_GeneratedPdfs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GeneratedPdfs_Proposals] FOREIGN KEY ([ProposalId])   REFERENCES [dbo].[Proposals]([Id]),
    CONSTRAINT [FK_GeneratedPdfs_Users]     FOREIGN KEY ([GeneratedById]) REFERENCES [dbo].[Users]([Id])
);
GO

-- ============================================================
-- 13. NOTIFICATIONS
-- ============================================================
CREATE TABLE [dbo].[Notifications] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [UserId]        UNIQUEIDENTIFIER    NOT NULL,
    [PalikaId]      UNIQUEIDENTIFIER    NOT NULL,
    [ProposalId]    UNIQUEIDENTIFIER    NULL,
    [Type]          NVARCHAR(50)        NOT NULL,
    [Title_En]      NVARCHAR(300)       NOT NULL,
    [Title_Mr]      NVARCHAR(300)       NULL,
    [Message_En]    NVARCHAR(1000)      NOT NULL,
    [Message_Mr]    NVARCHAR(1000)      NULL,
    [IsRead]        BIT                 NOT NULL DEFAULT 0,
    [ReadAt]        DATETIME2           NULL,
    [CreatedAt]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Users]     FOREIGN KEY ([UserId])     REFERENCES [dbo].[Users]([Id]),
    CONSTRAINT [FK_Notifications_Palikas]   FOREIGN KEY ([PalikaId])   REFERENCES [dbo].[Palikas]([Id]),
    CONSTRAINT [FK_Notifications_Proposals] FOREIGN KEY ([ProposalId]) REFERENCES [dbo].[Proposals]([Id])
);
GO

-- ============================================================
-- 14. AUDIT TRAIL (Append-Only, Immutable)
-- ============================================================
CREATE TABLE [dbo].[AuditTrail] (
    [Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [Timestamp]     DATETIME2           NOT NULL DEFAULT SYSUTCDATETIME(),
    [PalikaId]      UNIQUEIDENTIFIER    NULL,
    [UserId]        UNIQUEIDENTIFIER    NULL,
    [UserName]      NVARCHAR(200)       NULL,
    [UserRole]      NVARCHAR(50)        NULL,
    [IpAddress]     NVARCHAR(45)        NULL,
    [UserAgent]     NVARCHAR(500)       NULL,
    [Action]        NVARCHAR(50)        NOT NULL,
    [EntityType]    NVARCHAR(100)       NOT NULL,
    [EntityId]      NVARCHAR(100)       NULL,
    [Description]   NVARCHAR(1000)      NULL,
    [OldValues]     NVARCHAR(MAX)       NULL,
    [NewValues]     NVARCHAR(MAX)       NULL,
    [Module]        NVARCHAR(50)        NOT NULL,
    [Severity]      NVARCHAR(20)        NOT NULL DEFAULT 'Info',

    CONSTRAINT [PK_AuditTrail] PRIMARY KEY ([Id])
);
GO

PRINT 'All tables created successfully.';
GO
