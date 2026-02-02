INSERT INTO t_Branch
(BranchName, IFSCCode, City, State, Country, PinCode, CreatedAt)
VALUES
('Main Branch', 'IFSC0001', 'Chennai', 'TN', 'India', '600001', GETUTCDATE());

INSERT INTO t_Account
(CustomerName, AccountNumber, BranchId, AccountType, Balance, Status, CreatedByUserId, CreatedAt)
VALUES
('John Doe', 100001, 1, 1, 100000, 0, 2, GETUTCDATE()),
('Jane Doe', 100002, 1, 2, 200000, 0, 2, GETUTCDATE());

INSERT INTO t_User
(Name, Role, Email, BranchId, Status, PasswordHash, CreatedAt,AccessToken,FalseAttempt,IsLocked)
VALUES
('Officer One', 1, 'officer@test.com', 1, 1, 'dummy', GETUTCDATE(),'',0,0),
('Manager One', 2, 'manager@test.com', 1, 1, 'dummy', GETUTCDATE(),'',0,0);