IF NOT EXISTS (SELECT 1 FROM Users WHERE Role = 'Submitter')
INSERT INTO Users (Id, FullName_En, FullName_Alt, MobileNumber, Email, PasswordHash, Role, DepartmentId, DesignationId, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES (NEWID(), 'Rajesh Patil', N'राजेश पाटील', '9876543220', NULL, NULL, 'Submitter', '140674FE-5D53-4355-B57C-19143100009A', '874AE55E-1451-4886-AB44-BA5BEE027810', 1, 0, GETUTCDATE(), GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Users WHERE Role = 'ChiefAccountant')
INSERT INTO Users (Id, FullName_En, FullName_Alt, MobileNumber, Email, PasswordHash, Role, DepartmentId, DesignationId, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES (NEWID(), 'Priya Deshmukh', N'प्रिया देशमुख', '9876543212', NULL, NULL, 'ChiefAccountant', '93CFDB79-7C93-4962-92CA-E5C962DF908F', '1D8CB4D0-B39E-4EE7-8971-13B4DBE1F20E', 1, 0, GETUTCDATE(), GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Users WHERE Role = 'DeputyCommissioner')
INSERT INTO Users (Id, FullName_En, FullName_Alt, MobileNumber, Email, PasswordHash, Role, DepartmentId, DesignationId, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES (NEWID(), 'Amit Kulkarni', N'अमित कुलकर्णी', '9876543213', NULL, NULL, 'DeputyCommissioner', '140674FE-5D53-4355-B57C-19143100009A', '4AA191B4-A414-4585-9928-4A846846A740', 1, 0, GETUTCDATE(), GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Users WHERE Role = 'Commissioner')
INSERT INTO Users (Id, FullName_En, FullName_Alt, MobileNumber, Email, PasswordHash, Role, DepartmentId, DesignationId, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES (NEWID(), 'Dr. Mahesh Sharma', N'डॉ. महेश शर्मा', '9876543214', NULL, NULL, 'Commissioner', '140674FE-5D53-4355-B57C-19143100009A', 'AD9F38DC-0806-4B57-8000-7506B740FCCD', 1, 0, GETUTCDATE(), GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Users WHERE Role = 'Auditor')
INSERT INTO Users (Id, FullName_En, FullName_Alt, MobileNumber, Email, PasswordHash, Role, DepartmentId, DesignationId, IsActive, IsDeleted, CreatedAt, UpdatedAt)
VALUES (NEWID(), 'Sandeep Gaikwad', N'संदीप गायकवाड', '9876543215', NULL, NULL, 'Auditor', '93CFDB79-7C93-4962-92CA-E5C962DF908F', '1D8CB4D0-B39E-4EE7-8971-13B4DBE1F20E', 1, 0, GETUTCDATE(), GETUTCDATE());