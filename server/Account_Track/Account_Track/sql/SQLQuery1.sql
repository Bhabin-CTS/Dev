SELECT TOP (1000) [UserId]
      ,[Name]
      ,[Role]
      ,[Email]
      ,[BranchId]
      ,[PasswordHash]
      ,[FalseAttempt]
      ,[IsLocked]
      ,[Status]
      ,[CreatedAt]
      ,[UpdatedAt]
  FROM [AccountTrackDevDb].[dbo].[t_User]

  select MAX(UserId) AS SecondLargest from t_user
  where UserId < ( select max(UserId) from t_user );