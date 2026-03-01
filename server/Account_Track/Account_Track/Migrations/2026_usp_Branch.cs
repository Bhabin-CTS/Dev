using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    public partial class usp_Branch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
                CREATE OR ALTER PROCEDURE [dbo].[usp_Branch]
                (
                    @Action VARCHAR(30),

                    -- Common
                    @BranchId INT = NULL,
                    @UserId INT = NULL,
                    @LoginId INT = NULL,

                    -- Branch Fields
                    @BranchName NVARCHAR(100) = NULL,
                    @IFSCCode NVARCHAR(50) = NULL,
                    @City NVARCHAR(100) = NULL,
                    @State NVARCHAR(100) = NULL,
                    @Country NVARCHAR(100) = NULL,
                    @Pincode NVARCHAR(20) = NULL,

                    -- Filters
                    @SearchText NVARCHAR(100) = NULL,
                    @CreatedFrom DATETIME = NULL,
                    @CreatedTo DATETIME = NULL,
                    @UpdatedFrom DATETIME = NULL,
                    @UpdatedTo DATETIME = NULL,

                    -- Sorting & Paging
                    @SortBy NVARCHAR(50) = 'BranchName',
                    @SortOrder NVARCHAR(10) = 'ASC',
                    @Limit INT = 20,
                    @Offset INT = 0
                )
                AS
                BEGIN
                    SET NOCOUNT ON;
                    DECLARE @BranchAfterState NVARCHAR(MAX);
                    DECLARE @BranchBeforeState NVARCHAR(MAX);
                    ------------------------------------------------------------
                    -- CREATE
                    ------------------------------------------------------------
                    IF @Action = 'CREATE'
                    BEGIN
                        SET NOCOUNT ON;

                        IF EXISTS (SELECT 1 FROM t_Branch WHERE IFSCCode = @IFSCCode)
                            THROW 50010, 'IFSCCode already exists', 1;

                        -------------------------------------------------------
                        -- CREATE BRANCH
                        -------------------------------------------------------
                        INSERT INTO t_Branch
                        (BranchName, IFSCCode, City, State, Country, Pincode, CreatedAt)
                        VALUES
                        (@BranchName, @IFSCCode, @City, @State, @Country, @Pincode, GETUTCDATE());

                        DECLARE @NewBranchId INT = SCOPE_IDENTITY();

                        -------------------------------------------------------
                        -- CAPTURE AFTER STATE FOR AUDIT
                        -------------------------------------------------------
                        
                        SELECT @BranchAfterState = (
                            SELECT BranchId, BranchName, IFSCCode, City, State, Country, Pincode, CreatedAt, UpdatedAt
                            FROM t_Branch
                            WHERE BranchId = @NewBranchId
                            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                        );

                        -------------------------------------------------------
                        -- AUDIT LOG (CREATE)
                        -------------------------------------------------------
                        INSERT INTO t_AuditLog
                        (
                            UserId,
                            LoginId,
                            EntityType,
                            EntityId,
                            Action,
                            beforeState,
                            afterState,
                            CreatedAt
                        )
                        VALUES
                        (
                            @UserId,
                            @LoginId,
                            'Branch',
                            @NewBranchId,
                            'CREATE',
                            NULL,
                            @BranchAfterState,
                            GETUTCDATE()
                        );

                        -------------------------------------------------------
                        -- RETURN CREATED BRANCH
                        -------------------------------------------------------
                        SELECT *
                        FROM t_Branch
                        WHERE BranchId = @NewBranchId;
                        RETURN;
                    END

                    ------------------------------------------------------------
                    -- UPDATE
                    ------------------------------------------------------------
                    IF @Action = 'UPDATE'
                    BEGIN
                        SET NOCOUNT ON;

                        IF NOT EXISTS (SELECT 1 FROM t_Branch WHERE BranchId = @BranchId)
                            THROW 50011, 'Branch not found', 1;

                        -------------------------------------------------------
                        -- CAPTURE BEFORE STATE FOR AUDIT
                        -------------------------------------------------------
                        
                        SELECT @BranchBeforeState = (
                            SELECT BranchId, BranchName, IFSCCode, City, State, Country, Pincode, CreatedAt, UpdatedAt
                            FROM t_Branch
                            WHERE BranchId = @BranchId
                            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                        );

                        -------------------------------------------------------
                        -- FETCH OLD DETAILS FOR NOTIFICATIONS
                        -------------------------------------------------------
                        DECLARE @OldBranchName NVARCHAR(100);
                        DECLARE @OldCity NVARCHAR(100);
                        DECLARE @OldState NVARCHAR(100);
                        DECLARE @OldCountry NVARCHAR(100);

                        SELECT 
                            @OldBranchName = BranchName,
                            @OldCity = City,
                            @OldState = State,
                            @OldCountry = Country
                        FROM t_Branch
                        WHERE BranchId = @BranchId;

                        -------------------------------------------------------
                        -- UPDATE BRANCH
                        -------------------------------------------------------
                        UPDATE t_Branch
                        SET
                            BranchName = ISNULL(@BranchName, BranchName),
                            IFSCCode = ISNULL(@IFSCCode, IFSCCode),
                            City = ISNULL(@City, City),
                            State = ISNULL(@State, State),
                            Country = ISNULL(@Country, Country),
                            Pincode = ISNULL(@Pincode, Pincode),
                            UpdatedAt = GETUTCDATE()
                        WHERE BranchId = @BranchId;

                        -------------------------------------------------------
                        -- CAPTURE AFTER STATE FOR AUDIT
                        -------------------------------------------------------

                        SELECT @BranchAfterState = (
                            SELECT BranchId, BranchName, IFSCCode, City, State, Country, Pincode, CreatedAt, UpdatedAt
                            FROM t_Branch
                            WHERE BranchId = @BranchId
                            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
                        );

                        -------------------------------------------------------
                        -- NOTIFICATION LOGIC
                        -------------------------------------------------------
                        DECLARE @NOTIF_UNREAD INT = 1;
                        DECLARE @NOTIF_SYSTEM INT = 3;
                        DECLARE @NewBranchName NVARCHAR(100);
                        DECLARE @NewCity NVARCHAR(100);
                        DECLARE @NewState NVARCHAR(100);
                        DECLARE @NewCountry NVARCHAR(100);

                        SELECT 
                            @NewBranchName = BranchName,
                            @NewCity = City,
                            @NewState = State,
                            @NewCountry = Country
                        FROM t_Branch
                        WHERE BranchId = @BranchId;

                        -------------------------------------------------------
                        -- SCENARIO 1: BRANCH NAME UPDATED
                        -------------------------------------------------------
                        IF @BranchName IS NOT NULL AND @BranchName <> @OldBranchName
                        BEGIN
                            INSERT INTO t_Notification
                            (
                                UserId,
                                Message,
                                Status,
                                Type,
                                CreatedDate
                            )
                            SELECT
                                UserId,
                                CONCAT('Your branch name has been updated from ', @OldBranchName, ' to ', @NewBranchName, '.'),
                                @NOTIF_UNREAD,
                                @NOTIF_SYSTEM,
                                GETUTCDATE()
                            FROM t_User
                            WHERE BranchId = @BranchId;
                        END

                        -------------------------------------------------------
                        -- SCENARIO 2: LOCATION DETAILS UPDATED (City, State, Country, Pincode)
                        -------------------------------------------------------
                        IF (@City IS NOT NULL AND @City <> @OldCity) 
                           OR (@State IS NOT NULL AND @State <> @OldState)
                           OR (@Country IS NOT NULL AND @Country <> @OldCountry)
                        BEGIN
                            DECLARE @LocationMessage NVARCHAR(500) = 'Branch location details have been updated. ';
                    
                            IF @City IS NOT NULL AND @City <> @OldCity
                                SET @LocationMessage = CONCAT(@LocationMessage, 'City: ', @OldCity, ' → ', @NewCity, '. ');
                    
                            IF @State IS NOT NULL AND @State <> @OldState
                                SET @LocationMessage = CONCAT(@LocationMessage, 'State: ', @OldState, ' → ', @NewState, '. ');
                    
                            IF @Country IS NOT NULL AND @Country <> @OldCountry
                                SET @LocationMessage = CONCAT(@LocationMessage, 'Country: ', @OldCountry, ' → ', @NewCountry, '.');

                            INSERT INTO t_Notification
                            (
                                UserId,
                                Message,
                                Status,
                                Type,
                                CreatedDate
                            )
                            SELECT
                                UserId,
                                @LocationMessage,
                                @NOTIF_UNREAD,
                                @NOTIF_SYSTEM,
                                GETUTCDATE()
                            FROM t_User
                            WHERE BranchId = @BranchId;
                        END

                        -------------------------------------------------------
                        -- SCENARIO 3: IFSC CODE UPDATED
                        -------------------------------------------------------
                        IF @IFSCCode IS NOT NULL
                        BEGIN
                            DECLARE @OldIFSCCode NVARCHAR(50);
                            SELECT @OldIFSCCode = IFSCCode FROM t_Branch WHERE BranchId = @BranchId;

                            IF @IFSCCode <> @OldIFSCCode
                            BEGIN
                                INSERT INTO t_Notification
                                (
                                    UserId,
                                    Message,
                                    Status,
                                    Type,
                                    CreatedDate
                                )
                                SELECT
                                    UserId,
                                    CONCAT('Branch IFSC Code has been updated from ', @OldIFSCCode, ' to ', @IFSCCode, '.'),
                                    @NOTIF_UNREAD,
                                    @NOTIF_SYSTEM,
                                    GETUTCDATE()
                                FROM t_User
                                WHERE BranchId = @BranchId;
                            END
                        END

                        -------------------------------------------------------
                        -- AUDIT LOG (UPDATE)
                        -------------------------------------------------------
                        INSERT INTO t_AuditLog
                        (
                            UserId,
                            LoginId,
                            EntityType,
                            EntityId,
                            Action,
                            beforeState,
                            afterState,
                            CreatedAt
                        )
                        VALUES
                        (
                            @UserId,
                            @LoginId,
                            'Branch',
                            @BranchId,
                            'UPDATE',
                            @BranchBeforeState,
                            @BranchAfterState,
                            GETUTCDATE()
                        );

                        -------------------------------------------------------
                        -- RETURN UPDATED BRANCH
                        -------------------------------------------------------
                        SELECT * FROM t_Branch WHERE BranchId = @BranchId;
                        RETURN;
                    END

                    ------------------------------------------------------------
                    -- GET BY ID
                    ------------------------------------------------------------
                    IF @Action = 'GET_BY_ID'
                    BEGIN
                        SET NOCOUNT ON;

                        SELECT *
                        FROM t_Branch
                        WHERE BranchId = @BranchId;
                        RETURN;
                    END

                    ------------------------------------------------------------
                    -- GET LIST (Filters + Paging)
                    ------------------------------------------------------------
                    IF @Action = 'GET_LIST'
                    BEGIN
                        SET NOCOUNT ON;

                        SELECT
                            BranchId,
                            BranchName,
                            IFSCCode,
                            City,
                            State,
                            Country,
                            Pincode,
                            CreatedAt,
                            UpdatedAt,
                            COUNT(*) OVER() AS TotalCount
                        FROM t_Branch
                        WHERE
                            (@BranchId IS NULL OR BranchId = @BranchId)
                            AND (@BranchName IS NULL OR BranchName LIKE '%' + @BranchName + '%')
                            AND (@IFSCCode IS NULL OR IFSCCode LIKE '%' + @IFSCCode + '%')
                            AND (@City IS NULL OR City = @City)
                            AND (@State IS NULL OR State = @State)
                            AND (@Country IS NULL OR Country = @Country)
                            AND (@Pincode IS NULL OR Pincode = @Pincode)

                            AND (@SearchText IS NULL OR
                                    BranchName LIKE '%' + @SearchText + '%' OR
                                    IFSCCode LIKE '%' + @SearchText + '%' OR
                                    City LIKE '%' + @SearchText + '%')

                            AND (@CreatedFrom IS NULL OR CreatedAt >= @CreatedFrom)
                            AND (@CreatedTo IS NULL OR CreatedAt <= @CreatedTo)

                            AND (@UpdatedFrom IS NULL OR UpdatedAt >= @UpdatedFrom)
                            AND (@UpdatedTo IS NULL OR UpdatedAt <= @UpdatedTo)

                        ORDER BY
                            CASE WHEN @SortBy = 'BranchName' AND @SortOrder = 'ASC' THEN BranchName END ASC,
                            CASE WHEN @SortBy = 'BranchName' AND @SortOrder = 'DESC' THEN BranchName END DESC,

                            CASE WHEN @SortBy = 'IFSCCode' AND @SortOrder = 'ASC' THEN IFSCCode END ASC,
                            CASE WHEN @SortBy = 'IFSCCode' AND @SortOrder = 'DESC' THEN IFSCCode END DESC,

                            CASE WHEN @SortBy = 'City' AND @SortOrder = 'ASC' THEN City END ASC,
                            CASE WHEN @SortBy = 'City' AND @SortOrder = 'DESC' THEN City END DESC,

                            CASE WHEN @SortBy = 'State' AND @SortOrder = 'ASC' THEN State END ASC,
                            CASE WHEN @SortBy = 'State' AND @SortOrder = 'DESC' THEN State END DESC,

                            CASE WHEN @SortBy = 'Country' AND @SortOrder = 'ASC' THEN Country END ASC,
                            CASE WHEN @SortBy = 'Country' AND @SortOrder = 'DESC' THEN Country END DESC,

                            CASE WHEN @SortBy = 'Pincode' AND @SortOrder = 'ASC' THEN Pincode END ASC,
                            CASE WHEN @SortBy = 'Pincode' AND @SortOrder = 'DESC' THEN Pincode END DESC,

                            CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'ASC' THEN CreatedAt END ASC,
                            CASE WHEN @SortBy = 'CreatedAt' AND @SortOrder = 'DESC' THEN CreatedAt END DESC,

                            CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'ASC' THEN UpdatedAt END ASC,
                            CASE WHEN @SortBy = 'UpdatedAt' AND @SortOrder = 'DESC' THEN UpdatedAt END DESC

                        OFFSET @Offset ROWS
                        FETCH NEXT @Limit ROWS ONLY;
        

                        RETURN;
                    END
                END
                ";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_Branch]");
        }
    }
}