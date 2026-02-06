using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    public partial class _2026_Account_CreateTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_Account",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),

                    AccountNumber = table.Column<int>(type: "int", nullable: false),

                    BranchId = table.Column<int>(type: "int", nullable: false),

                    AccountType = table.Column<int>(type: "int", nullable: false),

                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0.00m),

                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),

                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),

                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),

                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),

                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Account_Number",
                table: "t_Account",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_Branch_Status_Type",
                table: "t_Account",
                columns: new[] { "BranchId", "Status", "AccountType" });

            migrationBuilder.CreateIndex(
                name: "IX_Account_User_Created_Date",
                table: "t_Account",
                columns: new[] { "CreatedByUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Account_User_Created",
                table: "t_Account",
                column: "CreatedByUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "t_Account");
        }
    }
}