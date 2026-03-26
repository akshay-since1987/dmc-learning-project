/*
    Proposal Management System V2 — Indexes
    Database: dmc-v2-ProposalMgmt
    
    Run after 02-create-tables.sql
*/

USE [dmc-v2-ProposalMgmt];
GO

SET QUOTED_IDENTIFIER ON;
GO

-- ============================================================
-- PALIKAS
-- ============================================================
-- ShortCode already has a unique constraint (acts as a unique index)

-- ============================================================
-- DEPARTMENTS
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_Departments_PalikaId]
    ON [dbo].[Departments] ([PalikaId]);
GO

-- ============================================================
-- DESIGNATIONS
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_Designations_PalikaId]
    ON [dbo].[Designations] ([PalikaId]);
GO

-- ============================================================
-- ZONES
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_Zones_PalikaId]
    ON [dbo].[Zones] ([PalikaId]);
GO

-- ============================================================
-- PRABHAGS
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_Prabhags_PalikaId]
    ON [dbo].[Prabhags] ([PalikaId]);
GO

CREATE NONCLUSTERED INDEX [IX_Prabhags_ZoneId]
    ON [dbo].[Prabhags] ([ZoneId]);
GO

CREATE NONCLUSTERED INDEX [IX_Prabhags_Number]
    ON [dbo].[Prabhags] ([Number]);
GO

-- ============================================================
-- USERS
-- ============================================================
-- MobileNumber already has a unique constraint
CREATE NONCLUSTERED INDEX [IX_Users_PalikaId]
    ON [dbo].[Users] ([PalikaId]);
GO

CREATE NONCLUSTERED INDEX [IX_Users_Role]
    ON [dbo].[Users] ([Role]);
GO

CREATE NONCLUSTERED INDEX [IX_Users_DepartmentId]
    ON [dbo].[Users] ([DepartmentId])
    WHERE [DepartmentId] IS NOT NULL;
GO

-- ============================================================
-- REFRESH TOKENS
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId]
    ON [dbo].[RefreshTokens] ([UserId]);
GO

CREATE NONCLUSTERED INDEX [IX_RefreshTokens_Token]
    ON [dbo].[RefreshTokens] ([Token]);
GO

-- ============================================================
-- BUDGET HEADS
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_BudgetHeads_PalikaId_DeptId_FundTypeId_Year]
    ON [dbo].[BudgetHeads] ([PalikaId], [DepartmentId], [FundTypeId], [FinancialYear]);
GO

-- ============================================================
-- PROPOSALS
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_Proposals_PalikaId]
    ON [dbo].[Proposals] ([PalikaId]);
GO

CREATE NONCLUSTERED INDEX [IX_Proposals_CurrentStage]
    ON [dbo].[Proposals] ([CurrentStage])
    INCLUDE ([ProposalNumber], [PalikaId], [CurrentOwnerId]);
GO

CREATE NONCLUSTERED INDEX [IX_Proposals_CurrentOwnerId]
    ON [dbo].[Proposals] ([CurrentOwnerId])
    WHERE [CurrentOwnerId] IS NOT NULL;
GO

CREATE NONCLUSTERED INDEX [IX_Proposals_CreatedById]
    ON [dbo].[Proposals] ([CreatedById]);
GO

CREATE NONCLUSTERED INDEX [IX_Proposals_DepartmentId]
    ON [dbo].[Proposals] ([DepartmentId]);
GO

CREATE NONCLUSTERED INDEX [IX_Proposals_CreatedAt]
    ON [dbo].[Proposals] ([CreatedAt] DESC);
GO

-- ============================================================
-- PROPOSAL DOCUMENTS
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_ProposalDocuments_ProposalId]
    ON [dbo].[ProposalDocuments] ([ProposalId]);
GO

-- ============================================================
-- FIELD VISITS
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_FieldVisits_ProposalId]
    ON [dbo].[FieldVisits] ([ProposalId]);
GO

CREATE NONCLUSTERED INDEX [IX_FieldVisits_AssignedToId]
    ON [dbo].[FieldVisits] ([AssignedToId]);
GO

CREATE NONCLUSTERED INDEX [IX_FieldVisits_Status]
    ON [dbo].[FieldVisits] ([Status]);
GO

-- ============================================================
-- FIELD VISIT PHOTOS
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_FieldVisitPhotos_FieldVisitId]
    ON [dbo].[FieldVisitPhotos] ([FieldVisitId]);
GO

-- ============================================================
-- ESTIMATES (ProposalId has unique constraint already)
-- ============================================================

-- ============================================================
-- TECHNICAL SANCTIONS (ProposalId has unique constraint already)
-- ============================================================

-- ============================================================
-- PROPOSAL APPROVALS
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_ProposalApprovals_ProposalId]
    ON [dbo].[ProposalApprovals] ([ProposalId]);
GO

CREATE NONCLUSTERED INDEX [IX_ProposalApprovals_StageRole]
    ON [dbo].[ProposalApprovals] ([StageRole]);
GO

CREATE NONCLUSTERED INDEX [IX_ProposalApprovals_CreatedAt]
    ON [dbo].[ProposalApprovals] ([CreatedAt] DESC);
GO

-- ============================================================
-- GENERATED PDFs
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_GeneratedPdfs_ProposalId]
    ON [dbo].[GeneratedPdfs] ([ProposalId]);
GO

-- ============================================================
-- NOTIFICATIONS
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_Notifications_UserId_IsRead]
    ON [dbo].[Notifications] ([UserId], [IsRead])
    INCLUDE ([Title_En], [CreatedAt]);
GO

CREATE NONCLUSTERED INDEX [IX_Notifications_PalikaId]
    ON [dbo].[Notifications] ([PalikaId]);
GO

CREATE NONCLUSTERED INDEX [IX_Notifications_ProposalId]
    ON [dbo].[Notifications] ([ProposalId])
    WHERE [ProposalId] IS NOT NULL;
GO

CREATE NONCLUSTERED INDEX [IX_Notifications_CreatedAt]
    ON [dbo].[Notifications] ([CreatedAt] DESC);
GO

-- ============================================================
-- AUDIT TRAIL
-- ============================================================
CREATE NONCLUSTERED INDEX [IX_AuditTrail_Timestamp]
    ON [dbo].[AuditTrail] ([Timestamp] DESC);
GO

CREATE NONCLUSTERED INDEX [IX_AuditTrail_PalikaId]
    ON [dbo].[AuditTrail] ([PalikaId])
    WHERE [PalikaId] IS NOT NULL;
GO

CREATE NONCLUSTERED INDEX [IX_AuditTrail_EntityType_EntityId]
    ON [dbo].[AuditTrail] ([EntityType], [EntityId]);
GO

CREATE NONCLUSTERED INDEX [IX_AuditTrail_UserId]
    ON [dbo].[AuditTrail] ([UserId])
    WHERE [UserId] IS NOT NULL;
GO

CREATE NONCLUSTERED INDEX [IX_AuditTrail_Module]
    ON [dbo].[AuditTrail] ([Module]);
GO

PRINT 'All indexes created successfully.';
GO
