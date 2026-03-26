/*
    Seed Test Users for all key workflow roles
    Password hash for "123456" via BCrypt (not normally used, OTP-based auth)
    
    Mobile numbers:
    8888000001 - Junior Engineer (JE)
    8888000002 - Technical Sanctioner (TS)  
    8888000003 - City Engineer
    8888000004 - Account Officer
    8888000005 - Deputy Commissioner
    8888000006 - Commissioner
    8888000007 - Auditor
*/

USE [dmc-v2-ProposalMgmt];
GO

DECLARE @PalikaId  UNIQUEIDENTIFIER = 'A0000001-0000-0000-0000-000000000001';
DECLARE @DeptEng   UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000001';
DECLARE @DeptAcct  UNIQUEIDENTIFIER = 'C0000001-0000-0000-0000-000000000006';
DECLARE @Now       DATETIME2        = SYSUTCDATETIME();

-- Designation IDs (from seed data)
DECLARE @DesigJE     UNIQUEIDENTIFIER = '870986AB-F32A-41D3-A787-6A78D4FBAF6C';
DECLARE @DesigTS     UNIQUEIDENTIFIER = 'CBB8C7E2-27A5-4EC7-8491-61D8946F9A33';
DECLARE @DesigCE     UNIQUEIDENTIFIER = '86E5D20D-65D4-4421-B24F-04E4605CB8A6';
DECLARE @DesigAO     UNIQUEIDENTIFIER = '9D1BCFA5-716E-4195-9BB2-D5228C0CE763';
DECLARE @DesigDyCom  UNIQUEIDENTIFIER = '84B25D4A-17A1-44BB-8497-C1DE3B0FA7C5';
DECLARE @DesigCom    UNIQUEIDENTIFIER = 'D65ACD4C-3422-4C65-8E29-99DEB3566AAB';
DECLARE @DesigAudit  UNIQUEIDENTIFIER = 'F2701CA2-24D1-4994-B16B-12AC10A75957';

-- JE (Junior Engineer) — Proposal creator
IF NOT EXISTS (SELECT 1 FROM Users WHERE MobileNumber = '8888000001')
INSERT INTO Users (Id, FullName_En, FullName_Mr, MobileNumber, Email, Role, DepartmentId, DesignationId, PalikaId, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES (NEWID(), N'Rajesh Kumar', N'राजेश कुमार', '8888000001', 'je@test.com', 'JE', @DeptEng, @DesigJE, @PalikaId, 1, 0, @Now, @Now);

-- TS (Technical Sanctioner)
IF NOT EXISTS (SELECT 1 FROM Users WHERE MobileNumber = '8888000002')
INSERT INTO Users (Id, FullName_En, FullName_Mr, MobileNumber, Email, Role, DepartmentId, DesignationId, PalikaId, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES (NEWID(), N'Suresh Patil', N'सुरेश पाटील', '8888000002', 'ts@test.com', 'TS', @DeptEng, @DesigTS, @PalikaId, 1, 0, @Now, @Now);

-- City Engineer
IF NOT EXISTS (SELECT 1 FROM Users WHERE MobileNumber = '8888000003')
INSERT INTO Users (Id, FullName_En, FullName_Mr, MobileNumber, Email, Role, DepartmentId, DesignationId, PalikaId, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES (NEWID(), N'Anil Deshmukh', N'अनिल देशमुख', '8888000003', 'ce@test.com', 'CityEngineer', @DeptEng, @DesigCE, @PalikaId, 1, 0, @Now, @Now);

-- Account Officer
IF NOT EXISTS (SELECT 1 FROM Users WHERE MobileNumber = '8888000004')
INSERT INTO Users (Id, FullName_En, FullName_Mr, MobileNumber, Email, Role, DepartmentId, DesignationId, PalikaId, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES (NEWID(), N'Priya Sharma', N'प्रिया शर्मा', '8888000004', 'ao@test.com', 'AccountOfficer', @DeptAcct, @DesigAO, @PalikaId, 1, 0, @Now, @Now);

-- Deputy Commissioner
IF NOT EXISTS (SELECT 1 FROM Users WHERE MobileNumber = '8888000005')
INSERT INTO Users (Id, FullName_En, FullName_Mr, MobileNumber, Email, Role, DepartmentId, DesignationId, PalikaId, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES (NEWID(), N'Vikram Jadhav', N'विक्रम जाधव', '8888000005', 'dycom@test.com', 'DyCommissioner', NULL, @DesigDyCom, @PalikaId, 1, 0, @Now, @Now);

-- Commissioner
IF NOT EXISTS (SELECT 1 FROM Users WHERE MobileNumber = '8888000006')
INSERT INTO Users (Id, FullName_En, FullName_Mr, MobileNumber, Email, Role, DepartmentId, DesignationId, PalikaId, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES (NEWID(), N'Ashok Kulkarni', N'अशोक कुलकर्णी', '8888000006', 'commissioner@test.com', 'Commissioner', NULL, @DesigCom, @PalikaId, 1, 0, @Now, @Now);

-- Auditor
IF NOT EXISTS (SELECT 1 FROM Users WHERE MobileNumber = '8888000007')
INSERT INTO Users (Id, FullName_En, FullName_Mr, MobileNumber, Email, Role, DepartmentId, DesignationId, PalikaId, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES (NEWID(), N'Meera Joshi', N'मीरा जोशी', '8888000007', 'auditor@test.com', 'Auditor', NULL, @DesigAudit, @PalikaId, 1, 0, @Now, @Now);

PRINT 'Test users seeded successfully.';
SELECT FullName_En, MobileNumber, Role FROM Users WHERE IsDeleted = 0 ORDER BY CreatedAt;
GO
