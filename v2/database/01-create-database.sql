/*
    Proposal Management System V2
    Database: dmc-v2-ProposalMgmt
    Server:   .\SQLEXPRESS
    
    Run this script first to create the database.
*/

USE [master];
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'dmc-v2-ProposalMgmt')
BEGIN
    CREATE DATABASE [dmc-v2-ProposalMgmt];
END
GO

USE [dmc-v2-ProposalMgmt];
GO

PRINT 'Database [dmc-v2-ProposalMgmt] ready.';
GO
