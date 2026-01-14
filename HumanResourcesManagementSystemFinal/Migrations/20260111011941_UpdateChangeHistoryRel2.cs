using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanResourcesManagementSystemFinal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChangeHistoryRel2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChangeHistory_Accounts_ChangeByUserID",
                table: "ChangeHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_ChangeHistory_Employees_EmployeeID",
                table: "ChangeHistory");

            migrationBuilder.RenameColumn(
                name: "EmployeeID",
                table: "ChangeHistory",
                newName: "AccountUserID");

            migrationBuilder.RenameIndex(
                name: "IX_ChangeHistory_EmployeeID",
                table: "ChangeHistory",
                newName: "IX_ChangeHistory_AccountUserID");

            migrationBuilder.AlterColumn<string>(
                name: "LogID",
                table: "ChangeHistory",
                type: "char(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(5)",
                oldMaxLength: 5);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeHistory_Accounts_AccountUserID",
                table: "ChangeHistory",
                column: "AccountUserID",
                principalTable: "Accounts",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeHistory_Employees_ChangeByUserID",
                table: "ChangeHistory",
                column: "ChangeByUserID",
                principalTable: "Employees",
                principalColumn: "EmployeeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChangeHistory_Accounts_AccountUserID",
                table: "ChangeHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_ChangeHistory_Employees_ChangeByUserID",
                table: "ChangeHistory");

            migrationBuilder.RenameColumn(
                name: "AccountUserID",
                table: "ChangeHistory",
                newName: "EmployeeID");

            migrationBuilder.RenameIndex(
                name: "IX_ChangeHistory_AccountUserID",
                table: "ChangeHistory",
                newName: "IX_ChangeHistory_EmployeeID");

            migrationBuilder.AlterColumn<string>(
                name: "LogID",
                table: "ChangeHistory",
                type: "char(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(8)",
                oldMaxLength: 8);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeHistory_Accounts_ChangeByUserID",
                table: "ChangeHistory",
                column: "ChangeByUserID",
                principalTable: "Accounts",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeHistory_Employees_EmployeeID",
                table: "ChangeHistory",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID");
        }
    }
}
