using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class Branch_usp_BranchUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
            CREATE OR ALTER PROCEDURE [dbo].[usp_Branch_Update]
                @BranchId INT,
                @BranchName NVARCHAR(100) = NULL,
                @IFSCCode NVARCHAR(50) = NULL,
                @City NVARCHAR(100) = NULL,
                @State NVARCHAR(100) = NULL,
                @Country NVARCHAR(100) = NULL,
                @Pincode NVARCHAR(20) = NULL,
                @UserId INT,
                @LoginId INT
            AS
            BEGIN
                SET NOCOUNT ON;

                IF NOT EXISTS (SELECT 1 FROM t_Branch WHERE BranchId = @BranchId)
                    THROW 50011, 'Branch not found', 1;

                -------------------------------------------------------
                -- CAPTURE BEFORE STATE FOR AUDIT
                -------------------------------------------------------
                DECLARE @BranchBeforeState NVARCHAR(MAX);
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
                DECLARE @BranchAfterState NVARCHAR(MAX);
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
            END
            ";

            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[usp_Branch_Update]");
        }
    }
}