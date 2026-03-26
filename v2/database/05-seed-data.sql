/*
    Proposal Management System V2 — Seed Data
    Database: dmc-v2-ProposalMgmt
    
    Seeds: Palika (DMC), Zones, Prabhags, SiteConditions,
    default Lotus user, sample departments, designations,
    request sources, fund types, work execution methods.
    
    Run after 02-create-tables.sql and 03-create-indexes.sql
*/

USE [dmc-v2-ProposalMgmt];
GO

-- ============================================================
-- Fixed GUIDs for referential integrity in seed data
-- ============================================================
DECLARE @PalikaId_DMC       UNIQUEIDENTIFIER = 'A0000001-0000-0000-0000-000000000001';
DECLARE @Zone1Id            UNIQUEIDENTIFIER = 'B0000001-0000-0000-0000-000000000001';
DECLARE @Zone2Id            UNIQUEIDENTIFIER = 'B0000001-0000-0000-0000-000000000002';
DECLARE @Zone3Id            UNIQUEIDENTIFIER = 'B0000001-0000-0000-0000-000000000003';
DECLARE @Zone4Id            UNIQUEIDENTIFIER = 'B0000001-0000-0000-0000-000000000004';
DECLARE @Dept_Engineering   UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000001';
DECLARE @Dept_Water         UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000002';
DECLARE @Dept_Health        UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000003';
DECLARE @Dept_Electrical    UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000004';
DECLARE @Dept_Garden        UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000005';
DECLARE @Dept_Accounts      UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000006';
DECLARE @LotusUserId        UNIQUEIDENTIFIER = 'F0000001-0000-0000-0000-000000000001';
DECLARE @Now                DATETIME2        = SYSUTCDATETIME();

-- ============================================================
-- 0. PALIKA — Dhule Municipal Corporation
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Palikas] WHERE [Id] = @PalikaId_DMC)
BEGIN
    INSERT INTO [dbo].[Palikas] (
        [Id], [Name_En], [Name_Mr], [ShortCode], [Type],
        [Address_En], [Address_Mr], [ContactPhone], [Website],
        [PrimaryLanguage], [AlternateLanguage],
        [ProposalNumberPrefix], [CurrentFinancialYear],
        [OtpExpiryMinutes], [OtpMaxAttempts],
        [IsActive], [IsDeleted], [CreatedAt], [UpdatedAt]
    ) VALUES (
        @PalikaId_DMC,
        N'Dhule Municipal Corporation',
        N'धुळे महानगरपालिका',
        N'DMC',
        N'MahanagarPalika',
        N'Dhule Municipal Corporation, Dhule, Maharashtra 424001',
        N'धुळे महानगरपालिका, धुळे, महाराष्ट्र ४२४००१',
        N'02562-234567',
        N'https://dhulecorporation.org',
        N'en', N'mr',
        N'DMC',
        N'2025-2026',
        5, 3,
        1, 0, @Now, @Now
    );
END
GO

-- ============================================================
-- 1. ZONES — 4 Zone Offices of Dhule Municipal Corporation
-- ============================================================
DECLARE @PalikaId_DMC UNIQUEIDENTIFIER = 'A0000001-0000-0000-0000-000000000001';
DECLARE @Zone1Id UNIQUEIDENTIFIER = 'B0000001-0000-0000-0000-000000000001';
DECLARE @Zone2Id UNIQUEIDENTIFIER = 'B0000001-0000-0000-0000-000000000002';
DECLARE @Zone3Id UNIQUEIDENTIFIER = 'B0000001-0000-0000-0000-000000000003';
DECLARE @Zone4Id UNIQUEIDENTIFIER = 'B0000001-0000-0000-0000-000000000004';
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [dbo].[Zones] WHERE [Id] = @Zone1Id)
BEGIN
    INSERT INTO [dbo].[Zones] ([Id],[PalikaId],[Name_En],[Name_Mr],[Code],[OfficeName_En],[OfficeName_Mr],[IsActive],[IsDeleted],[CreatedAt],[UpdatedAt])
    VALUES
        (@Zone1Id, @PalikaId_DMC, N'Zone 1', N'क्षेत्र १',  N'Z1', N'Devpur (Navrang Tanki)',  N'देवपूर (नवरंग टाकी)',  1, 0, @Now, @Now),
        (@Zone2Id, @PalikaId_DMC, N'Zone 2', N'क्षेत्र २',  N'Z2', N'Subhash Putala',          N'सुभाष पुतळा',          1, 0, @Now, @Now),
        (@Zone3Id, @PalikaId_DMC, N'Zone 3', N'क्षेत्र ३',  N'Z3', N'Agra Road',               N'आग्रा रोड',            1, 0, @Now, @Now),
        (@Zone4Id, @PalikaId_DMC, N'Zone 4', N'क्षेत्र ४',  N'Z4', N'Lenin Chowk',             N'लेनिन चौक',            1, 0, @Now, @Now);
END
GO

-- ============================================================
-- 2. PRABHAGS — 19 Prabhags mapped to Zones
-- ============================================================
DECLARE @PalikaId_DMC UNIQUEIDENTIFIER = 'A0000001-0000-0000-0000-000000000001';
DECLARE @Zone1Id UNIQUEIDENTIFIER = 'B0000001-0000-0000-0000-000000000001';
DECLARE @Zone2Id UNIQUEIDENTIFIER = 'B0000001-0000-0000-0000-000000000002';
DECLARE @Zone3Id UNIQUEIDENTIFIER = 'B0000001-0000-0000-0000-000000000003';
DECLARE @Zone4Id UNIQUEIDENTIFIER = 'B0000001-0000-0000-0000-000000000004';
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [dbo].[Prabhags] WHERE [PalikaId] = @PalikaId_DMC)
BEGIN
    INSERT INTO [dbo].[Prabhags] ([Id],[PalikaId],[ZoneId],[Number],[Name_En],[Name_Mr],[CorporatorSeats],[IsActive],[IsDeleted],[CreatedAt],[UpdatedAt])
    VALUES
        -- Zone 1: Prabhags 1, 2, 4, 5, 6, 7
        (NEWID(), @PalikaId_DMC, @Zone1Id,  1, N'Prabhag 1',  N'प्रभाग १',  4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone1Id,  2, N'Prabhag 2',  N'प्रभाग २',  4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone1Id,  4, N'Prabhag 4',  N'प्रभाग ४',  4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone1Id,  5, N'Prabhag 5',  N'प्रभाग ५',  4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone1Id,  6, N'Prabhag 6',  N'प्रभाग ६',  4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone1Id,  7, N'Prabhag 7',  N'प्रभाग ७',  4, 1, 0, @Now, @Now),
        -- Zone 2: Prabhags 3, 11, 12, 15
        (NEWID(), @PalikaId_DMC, @Zone2Id,  3, N'Prabhag 3',  N'प्रभाग ३',  4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone2Id, 11, N'Prabhag 11', N'प्रभाग ११', 4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone2Id, 12, N'Prabhag 12', N'प्रभाग १२', 4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone2Id, 15, N'Prabhag 15', N'प्रभाग १५', 4, 1, 0, @Now, @Now),
        -- Zone 3: Prabhags 13, 14, 16, 18, 19
        (NEWID(), @PalikaId_DMC, @Zone3Id, 13, N'Prabhag 13', N'प्रभाग १३', 4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone3Id, 14, N'Prabhag 14', N'प्रभाग १४', 4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone3Id, 16, N'Prabhag 16', N'प्रभाग १६', 4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone3Id, 18, N'Prabhag 18', N'प्रभाग १८', 4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone3Id, 19, N'Prabhag 19', N'प्रभाग १९', 4, 1, 0, @Now, @Now),
        -- Zone 4: Prabhags 8, 9, 10, 17
        (NEWID(), @PalikaId_DMC, @Zone4Id,  8, N'Prabhag 8',  N'प्रभाग ८',  4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone4Id,  9, N'Prabhag 9',  N'प्रभाग ९',  4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone4Id, 10, N'Prabhag 10', N'प्रभाग १०', 4, 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, @Zone4Id, 17, N'Prabhag 17', N'प्रभाग १७', 4, 1, 0, @Now, @Now);
END
GO

-- ============================================================
-- 3. DEPARTMENTS
-- ============================================================
DECLARE @PalikaId_DMC UNIQUEIDENTIFIER = 'A0000001-0000-0000-0000-000000000001';
DECLARE @Dept_Engineering UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000001';
DECLARE @Dept_Water       UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000002';
DECLARE @Dept_Health      UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000003';
DECLARE @Dept_Electrical  UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000004';
DECLARE @Dept_Garden      UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000005';
DECLARE @Dept_Accounts    UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000006';
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [dbo].[Departments] WHERE [Id] = @Dept_Engineering)
BEGIN
    INSERT INTO [dbo].[Departments] ([Id],[PalikaId],[Name_En],[Name_Mr],[Code],[IsActive],[IsDeleted],[CreatedAt],[UpdatedAt])
    VALUES
        (@Dept_Engineering, @PalikaId_DMC, N'Engineering Department',   N'अभियांत्रिकी विभाग',    N'ENG',  1, 0, @Now, @Now),
        (@Dept_Water,       @PalikaId_DMC, N'Water Supply Department',  N'पाणी पुरवठा विभाग',     N'WSS',  1, 0, @Now, @Now),
        (@Dept_Health,      @PalikaId_DMC, N'Health Department',        N'आरोग्य विभाग',          N'HLT',  1, 0, @Now, @Now),
        (@Dept_Electrical,  @PalikaId_DMC, N'Electrical Department',    N'विद्युत विभाग',          N'ELEC', 1, 0, @Now, @Now),
        (@Dept_Garden,      @PalikaId_DMC, N'Garden Department',        N'उद्यान विभाग',          N'GDN',  1, 0, @Now, @Now),
        (@Dept_Accounts,    @PalikaId_DMC, N'Accounts Department',      N'लेखा विभाग',            N'ACC',  1, 0, @Now, @Now);
END
GO

-- ============================================================
-- 4. DEPT WORK CATEGORIES (sample for Engineering)
-- ============================================================
DECLARE @Dept_Engineering UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000001';
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [dbo].[DeptWorkCategories] WHERE [DepartmentId] = @Dept_Engineering)
BEGIN
    INSERT INTO [dbo].[DeptWorkCategories] ([Id],[DepartmentId],[Name_En],[Name_Mr],[IsActive],[IsDeleted],[CreatedAt],[UpdatedAt])
    VALUES
        (NEWID(), @Dept_Engineering, N'Road Construction',     N'रस्ता बांधकाम',       1, 0, @Now, @Now),
        (NEWID(), @Dept_Engineering, N'Road Repair',           N'रस्ता दुरुस्ती',      1, 0, @Now, @Now),
        (NEWID(), @Dept_Engineering, N'Bridge Construction',   N'पूल बांधकाम',         1, 0, @Now, @Now),
        (NEWID(), @Dept_Engineering, N'Drainage Work',         N'नाली / गटार कामे',    1, 0, @Now, @Now),
        (NEWID(), @Dept_Engineering, N'Building Construction', N'इमारत बांधकाम',       1, 0, @Now, @Now),
        (NEWID(), @Dept_Engineering, N'Building Repair',       N'इमारत दुरुस्ती',      1, 0, @Now, @Now),
        (NEWID(), NULL,              N'Other',                 N'इतर',                 1, 0, @Now, @Now);
END
GO

-- ============================================================
-- 5. DESIGNATIONS
-- ============================================================
DECLARE @PalikaId_DMC UNIQUEIDENTIFIER = 'A0000001-0000-0000-0000-000000000001';
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [dbo].[Designations] WHERE [PalikaId] = @PalikaId_DMC)
BEGIN
    INSERT INTO [dbo].[Designations] ([Id],[PalikaId],[Name_En],[Name_Mr],[IsActive],[IsDeleted],[CreatedAt],[UpdatedAt])
    VALUES
        (NEWID(), @PalikaId_DMC, N'Junior Engineer',          N'कनिष्ठ अभियंता',        1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Technical Sanctioner',     N'तांत्रिक मंजुरीकर्ता',  1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Assistant Engineer',       N'सहायक अभियंता',         1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Sub Engineer',             N'उप अभियंता',            1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'City Engineer',            N'शहर अभियंता',           1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Account Officer',          N'मुख्य लेखाधिकारी',      1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Deputy Commissioner',      N'उपायुक्त',              1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Commissioner',             N'आयुक्त',                1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Standing Committee Chair', N'स्थायी समिती सभापती',   1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'District Collector',       N'जिल्हाधिकारी',          1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Auditor',                  N'लेखा परीक्षक',          1, 0, @Now, @Now);
END
GO

-- ============================================================
-- 6. SITE CONDITIONS (global)
-- ============================================================
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [dbo].[SiteConditions])
BEGIN
    INSERT INTO [dbo].[SiteConditions] ([Id],[Name_En],[Name_Mr],[SortOrder],[IsActive],[IsDeleted],[CreatedAt],[UpdatedAt])
    VALUES
        (NEWID(), N'Worse',  N'अत्यंत खराब', 1, 1, 0, @Now, @Now),
        (NEWID(), N'Bad',    N'खराब',        2, 1, 0, @Now, @Now),
        (NEWID(), N'OK',     N'ठीक',         3, 1, 0, @Now, @Now),
        (NEWID(), N'Good',   N'चांगले',       4, 1, 0, @Now, @Now),
        (NEWID(), N'Better', N'उत्तम',       5, 1, 0, @Now, @Now);
END
GO

-- ============================================================
-- 7. REQUEST SOURCES
-- ============================================================
DECLARE @PalikaId_DMC UNIQUEIDENTIFIER = 'A0000001-0000-0000-0000-000000000001';
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [dbo].[RequestSources] WHERE [PalikaId] = @PalikaId_DMC)
BEGIN
    INSERT INTO [dbo].[RequestSources] ([Id],[PalikaId],[Name_En],[Name_Mr],[IsActive],[IsDeleted],[CreatedAt],[UpdatedAt])
    VALUES
        (NEWID(), @PalikaId_DMC, N'Citizen',            N'नागरिक',              1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'MLA',                N'आमदार',              1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'MP',                 N'खासदार',             1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Commissioner',       N'आयुक्त',             1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Corporator',         N'नगरसेवक',            1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Standing Committee', N'स्थायी समिती',       1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Department',         N'विभाग',              1, 0, @Now, @Now);
END
GO

-- ============================================================
-- 8. FUND TYPES
-- ============================================================
DECLARE @PalikaId_DMC UNIQUEIDENTIFIER = 'A0000001-0000-0000-0000-000000000001';
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [dbo].[FundTypes] WHERE [PalikaId] = @PalikaId_DMC)
BEGIN
    INSERT INTO [dbo].[FundTypes] ([Id],[PalikaId],[Name_En],[Name_Mr],[IsActive],[IsDeleted],[CreatedAt],[UpdatedAt])
    VALUES
        (NEWID(), @PalikaId_DMC, N'MNP (Municipal)',         N'महापालिका निधी',                 1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'State Fund',              N'राज्य निधी',                    1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Central Fund',            N'केंद्र निधी',                   1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'DPDC',                    N'जिल्हा नियोजन व विकास समिती',   1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'15th Finance Commission', N'१५ वा वित्त आयोग',              1, 0, @Now, @Now);
END
GO

-- ============================================================
-- 9. WORK EXECUTION METHODS
-- ============================================================
DECLARE @PalikaId_DMC UNIQUEIDENTIFIER = 'A0000001-0000-0000-0000-000000000001';
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [dbo].[WorkExecutionMethods] WHERE [PalikaId] = @PalikaId_DMC)
BEGIN
    INSERT INTO [dbo].[WorkExecutionMethods] ([Id],[PalikaId],[Name_En],[Name_Mr],[IsActive],[IsDeleted],[CreatedAt],[UpdatedAt])
    VALUES
        (NEWID(), @PalikaId_DMC, N'Open Tender (e-Tendering)',     N'खुली निविदा (ई-निविदा)',           1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Limited Tender',                N'मर्यादित निविदा',                  1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Quotation',                     N'दरपत्रक',                          1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Direct Purchase / Rate Contract', N'थेट खरेदी / दरकरार',             1, 0, @Now, @Now),
        (NEWID(), @PalikaId_DMC, N'Departmental Work',             N'खातेनिहाय (विभागीय कामकाज)',       1, 0, @Now, @Now);
END
GO

-- ============================================================
-- 10. DEFAULT LOTUS USER
-- ============================================================
DECLARE @PalikaId_DMC UNIQUEIDENTIFIER = 'A0000001-0000-0000-0000-000000000001';
DECLARE @LotusUserId  UNIQUEIDENTIFIER = 'F0000001-0000-0000-0000-000000000001';
DECLARE @Now DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Id] = @LotusUserId)
BEGIN
    INSERT INTO [dbo].[Users] (
        [Id], [FullName_En], [FullName_Mr], [MobileNumber], [Email],
        [PasswordHash], [Role], [PalikaId],
        [IsActive], [IsDeleted], [CreatedAt], [UpdatedAt]
    ) VALUES (
        @LotusUserId,
        N'System Administrator',
        N'प्रणाली प्रशासक',
        N'9999999999',
        N'admin@dhulecorporation.org',
        -- BCrypt hash of 'Admin@123' (change in production!)
        N'$2a$11$K3fNqXVqfBrVR4P6.Qm0OOdVYgwL.Q0YmDK6xEzyOcX5sS3Dza2a',
        N'Lotus',
        @PalikaId_DMC,
        1, 0, @Now, @Now
    );
END
GO

PRINT 'Seed data inserted successfully.';
GO
