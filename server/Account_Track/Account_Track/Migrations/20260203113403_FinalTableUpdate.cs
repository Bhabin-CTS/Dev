using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Account_Track.Migrations
{
    /// <inheritdoc />
    public partial class FinalTableUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Logout_User_Date",
                table: "t_LoginLog");

            migrationBuilder.RenameColumn(
                name: "LogOutAt",
                table: "t_LoginLog",
                newName: "RefreshTokenExpiry");

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "t_LoginLog",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "t_LoginLog",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Login_RefreshToken",
                table: "t_LoginLog",
                column: "RefreshToken");

            migrationBuilder.CreateIndex(
                name: "IX_Login_UserId",
                table: "t_LoginLog",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Login_UserId_RefreshToken",
                table: "t_LoginLog",
                columns: new[] { "UserId", "RefreshToken" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Login_RefreshToken",
                table: "t_LoginLog");

            migrationBuilder.DropIndex(
                name: "IX_Login_UserId",
                table: "t_LoginLog");

            migrationBuilder.DropIndex(
                name: "IX_Login_UserId_RefreshToken",
                table: "t_LoginLog");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "t_LoginLog");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "t_LoginLog");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenExpiry",
                table: "t_LoginLog",
                newName: "LogOutAt");

            migrationBuilder.CreateIndex(
                name: "IX_Logout_User_Date",
                table: "t_LoginLog",
                columns: new[] { "UserId", "LogOutAt" });
        }
    }
}
