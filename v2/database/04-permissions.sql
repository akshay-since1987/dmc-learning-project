/*
    Proposal Management System V2 — Permissions
    Database: dmc-v2-ProposalMgmt
    
    AuditTrail table: INSERT-only (no UPDATE, no DELETE) for the app user.
    Run after 02-create-tables.sql
    
    NOTE: Adjust the app login/user name below to match your deployment.
    For local dev with Windows Integrated auth, this script creates a
    deny rule on the table itself as a safety net.
*/

USE [dmc-v2-ProposalMgmt];
GO

-- ============================================================
-- AUDIT TRAIL: Deny UPDATE and DELETE for all database users
-- except db_owner (sysadmin). This ensures immutability.
-- ============================================================

-- Create a database role for normal app access
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'AppRole' AND type = 'R')
BEGIN
    CREATE ROLE [AppRole];
END
GO

-- Grant INSERT only on AuditTrail to AppRole
GRANT INSERT ON [dbo].[AuditTrail] TO [AppRole];
GO

-- Deny UPDATE and DELETE on AuditTrail to AppRole
DENY UPDATE ON [dbo].[AuditTrail] TO [AppRole];
DENY DELETE ON [dbo].[AuditTrail] TO [AppRole];
GO

-- Grant SELECT on AuditTrail to AppRole (for Lotus/Commissioner/Auditor queries)
GRANT SELECT ON [dbo].[AuditTrail] TO [AppRole];
GO

-- ============================================================
-- For the trigger-based approach (belt-and-suspenders):
-- Block UPDATE and DELETE at trigger level regardless of user.
-- Only db_owner / sysadmin can disable triggers if needed.
-- ============================================================
CREATE OR ALTER TRIGGER [dbo].[TR_AuditTrail_PreventModify]
ON [dbo].[AuditTrail]
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    RAISERROR ('AuditTrail records are immutable. UPDATE and DELETE are not allowed.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END
GO

PRINT 'AuditTrail permissions and immutability trigger created.';
GO
