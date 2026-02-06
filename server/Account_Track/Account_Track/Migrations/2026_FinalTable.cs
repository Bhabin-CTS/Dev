using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class FinalTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_Branch",
                columns: table => new
                {
                    BranchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IFSCCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    City = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Branch", x => x.BranchId);
                });

            migrationBuilder.CreateTable(
                name: "t_Report",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Metrics = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Report", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_t_Report_t_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "t_Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_User",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FalseAttempt = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_User", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_t_User_t_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "t_Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "t_Account",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<int>(type: "int", nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Account", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_t_Account_t_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "t_Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_Account_t_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "t_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "t_LoginLog",
                columns: table => new
                {
                    LoginId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_LoginLog", x => x.LoginId);
                    table.ForeignKey(
                        name: "FK_t_LoginLog_t_User_UserId",
                        column: x => x.UserId,
                        principalTable: "t_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_Notification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Notification", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_t_Notification_t_User_UserId",
                        column: x => x.UserId,
                        principalTable: "t_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_Transaction",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    FromAccountId = table.Column<int>(type: "int", nullable: false),
                    ToAccountId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsHighValue = table.Column<bool>(type: "bit", nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfterTxn = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    flagReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Transaction", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK_t_Transaction_t_Account_FromAccountId",
                        column: x => x.FromAccountId,
                        principalTable: "t_Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_Transaction_t_Account_ToAccountId",
                        column: x => x.ToAccountId,
                        principalTable: "t_Account",
                        principalColumn: "AccountId");
                    table.ForeignKey(
                        name: "FK_t_Transaction_t_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "t_Branch",
                        principalColumn: "BranchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_Transaction_t_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "t_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "t_AuditLog",
                columns: table => new
                {
                    AuditLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    beforeState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    afterState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_AuditLog", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_t_AuditLog_t_LoginLog_LoginId",
                        column: x => x.LoginId,
                        principalTable: "t_LoginLog",
                        principalColumn: "LoginId");
                    table.ForeignKey(
                        name: "FK_t_AuditLog_t_User_UserId",
                        column: x => x.UserId,
                        principalTable: "t_User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "t_Approval",
                columns: table => new
                {
                    ApprovalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Approval", x => x.ApprovalId);
                    table.ForeignKey(
                        name: "FK_t_Approval_t_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "t_Transaction",
                        principalColumn: "TransactionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_Approval_t_User_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "t_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_Branch_Status_Type",
                table: "t_Account",
                columns: new[] { "BranchId", "Status", "AccountType" });

            migrationBuilder.CreateIndex(
                name: "IX_Account_Number",
                table: "t_Account",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_User_Created",
                table: "t_Account",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_User_Created_Date",
                table: "t_Account",
                columns: new[] { "CreatedByUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Approval_Decision",
                table: "t_Approval",
                column: "Decision");

            migrationBuilder.CreateIndex(
                name: "IX_Approval_Decision_Date",
                table: "t_Approval",
                columns: new[] { "Decision", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Approval_Reviewer",
                table: "t_Approval",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_Approval_Reviewer_Date",
                table: "t_Approval",
                columns: new[] { "ReviewerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Approval_TransactionId_ReviewerId",
                table: "t_Approval",
                columns: new[] { "TransactionId", "ReviewerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Approval_Txn",
                table: "t_Approval",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Audit_Entity_Date",
                table: "t_AuditLog",
                columns: new[] { "EntityType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Audit_User_Date",
                table: "t_AuditLog",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_t_AuditLog_LoginId",
                table: "t_AuditLog",
                column: "LoginId");

            migrationBuilder.CreateIndex(
                name: "IX_Branch_City_State",
                table: "t_Branch",
                columns: new[] { "City", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Branch_IFSC",
                table: "t_Branch",
                column: "IFSCCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Login_RefreshToken",
                table: "t_LoginLog",
                column: "RefreshToken");

            migrationBuilder.CreateIndex(
                name: "IX_Login_User_Date",
                table: "t_LoginLog",
                columns: new[] { "UserId", "LoginAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Login_UserId",
                table: "t_LoginLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Login_UserId_RefreshToken",
                table: "t_LoginLog",
                columns: new[] { "UserId", "RefreshToken" });

            migrationBuilder.CreateIndex(
                name: "IX_Notif",
                table: "t_Notification",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notif_User_Status_NotificationId",
                table: "t_Notification",
                columns: new[] { "UserId", "Status", "NotificationId" });

            migrationBuilder.CreateIndex(
                name: "IX_Report_Branch",
                table: "t_Report",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Report_Branch_Date",
                table: "t_Report",
                columns: new[] { "BranchId", "GeneratedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Report_Date",
                table: "t_Report",
                column: "GeneratedDate");

            migrationBuilder.CreateIndex(
                name: "IX_t_Transaction_BranchId",
                table: "t_Transaction",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Txn_Branch",
                table: "t_Transaction",
                columns: new[] { "Status", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_Txn_FromAcc",
                table: "t_Transaction",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Txn_HighValue",
                table: "t_Transaction",
                column: "IsHighValue");

            migrationBuilder.CreateIndex(
                name: "IX_Txn_HighValue_Status",
                table: "t_Transaction",
                columns: new[] { "IsHighValue", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Txn_Status_Date",
                table: "t_Transaction",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Txn_ToAcc",
                table: "t_Transaction",
                column: "ToAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Txn_Type_Date",
                table: "t_Transaction",
                columns: new[] { "Type", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Txn_User_Date",
                table: "t_Transaction",
                columns: new[] { "CreatedByUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_User_Branch_Role_Status",
                table: "t_User",
                columns: new[] { "BranchId", "Role", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "t_User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email_Status",
                table: "t_User",
                columns: new[] { "Email", "Status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Lock_Attempt",
                table: "t_User",
                columns: new[] { "IsLocked", "FalseAttempt" });

            migrationBuilder.CreateIndex(
                name: "IX_User_Locked",
                table: "t_User",
                column: "IsLocked");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_Approval");

            migrationBuilder.DropTable(
                name: "t_AuditLog");

            migrationBuilder.DropTable(
                name: "t_Notification");

            migrationBuilder.DropTable(
                name: "t_Report");

            migrationBuilder.DropTable(
                name: "t_Transaction");

            migrationBuilder.DropTable(
                name: "t_LoginLog");

            migrationBuilder.DropTable(
                name: "t_Account");

            migrationBuilder.DropTable(
                name: "t_User");

            migrationBuilder.DropTable(
                name: "t_Branch");
        }
    }
}
