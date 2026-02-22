INSERT INTO t_Branch (BranchName, IFSCCode, City, State, Country, Pincode, CreatedAt)
VALUES
('Chennai Main', 'IFSC001', 'Chennai', 'TN', 'India', '600001', GETDATE()),
('Mumbai Central', 'IFSC002', 'Mumbai', 'MH', 'India', '400001', GETDATE()),
('Bangalore City', 'IFSC003', 'Bangalore', 'KA', 'India', '560001', GETDATE()),
('Delhi Metro', 'IFSC004', 'Delhi', 'DL', 'India', '110001', GETDATE()),
('Hyderabad Prime', 'IFSC005', 'Hyderabad', 'TS', 'India', '500001', GETDATE())

INSERT INTO t_User (Name, Role, Email, BranchId, PasswordHash, FalseAttempt, IsLocked, Status, CreatedAt)
VALUES
('Admin Chennai', 1, 'chennai@test.com', 1, 'hash', 0, 0, 1, GETDATE()),
('Admin Mumbai', 1, 'mumbai@test.com', 2, 'hash', 0, 0, 1, GETDATE()),
('Admin Bangalore', 1, 'blr@test.com', 3, 'hash', 0, 0, 1, GETDATE()),
('Admin Delhi', 1, 'delhi@test.com', 4, 'hash', 0, 0, 1, GETDATE()),
('Admin Hyderabad', 1, 'hyd@test.com', 5, 'hash', 0, 0, 1, GETDATE())


INSERT INTO t_Account
(CustomerName, AccountNumber, AccountType, Balance, Status, BranchId, CreatedByUserId, CreatedAt)
VALUES
-- Previous Year
('Cust1', 2001, 1, 50000, 1, 1, 1, DATEADD(YEAR,-1,GETDATE())),
('Cust2', 2002, 1, 60000, 1, 2, 2, DATEADD(YEAR,-1,GETDATE())),

-- 4 months ago
('Cust3', 2003, 1, 45000, 1, 3, 3, DATEADD(MONTH,-4,GETDATE())),
('Cust4', 2004, 1, 55000, 1, 4, 4, DATEADD(MONTH,-4,GETDATE())),

-- Last Month
('Cust5', 2005, 1, 70000, 1, 1, 1, DATEADD(MONTH,-1,GETDATE())),
('Cust6', 2006, 1, 80000, 1, 2, 2, DATEADD(MONTH,-1,GETDATE())),

-- This Month
('Cust7', 2007, 1, 90000, 1, 3, 3, GETDATE()),
('Cust8', 2008, 1, 100000, 1, 4, 4, GETDATE()),
('Cust9', 2009, 1, 120000, 1, 5, 5, GETDATE())

DECLARE @i INT = 1
WHILE @i <= 50
BEGIN
    INSERT INTO t_Transaction
    (BranchId, CreatedByUserId, FromAccountId, ToAccountId, Type, Amount, Status, IsHighValue, BalanceBefore, BalanceAfterTxn, CreatedAt)
    VALUES
    (
        (ABS(CHECKSUM(NEWID())) % 5) + 1,
        (ABS(CHECKSUM(NEWID())) % 5) + 1,
        (ABS(CHECKSUM(NEWID())) % 9) + 1,
        NULL,
        (ABS(CHECKSUM(NEWID())) % 3) + 1,
        (ABS(CHECKSUM(NEWID())) % 200000) + 1000,
        1,
        CASE WHEN (ABS(CHECKSUM(NEWID())) % 5) = 0 THEN 1 ELSE 0 END,
        50000,
        60000,
        DATEADD(DAY, - (ABS(CHECKSUM(NEWID())) % 120), GETDATE())
    )

    SET @i = @i + 1
END

INSERT INTO t_Approval (TransactionId, ReviewerId, Decision, CreatedAt)
SELECT 
    TransactionID,
    (ABS(CHECKSUM(NEWID())) % 5) + 1,
    (ABS(CHECKSUM(NEWID())) % 3) + 1,
    DATEADD(DAY, - (ABS(CHECKSUM(NEWID())) % 60), GETDATE())
FROM t_Transaction
WHERE IsHighValue = 1


