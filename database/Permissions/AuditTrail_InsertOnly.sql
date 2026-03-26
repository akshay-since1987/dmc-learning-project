-- AuditTrail INSERT-Only Permissions
-- This script ensures the application database user can only INSERT into AuditTrails table.
-- No role (including Lotus) can UPDATE or DELETE audit records.
-- Run this AFTER the initial migration has created the tables.

-- Create a database role for audit restrictions
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'AppAuditRole' AND type = 'R')
BEGIN
    CREATE ROLE AppAuditRole;
END

-- Grant INSERT only on AuditTrails (deny UPDATE and DELETE)
GRANT INSERT ON [dbo].[AuditTrails] TO AppAuditRole;
DENY UPDATE ON [dbo].[AuditTrails] TO AppAuditRole;
DENY DELETE ON [dbo].[AuditTrails] TO AppAuditRole;

-- Note: When using Windows Integrated Security, the Windows user has dbo access.
-- For production with a dedicated SQL login, assign that login to AppAuditRole instead of dbo.
-- Example:
-- ALTER ROLE AppAuditRole ADD MEMBER [YourAppLoginUser];

PRINT 'AuditTrail INSERT-only permissions applied successfully.';
