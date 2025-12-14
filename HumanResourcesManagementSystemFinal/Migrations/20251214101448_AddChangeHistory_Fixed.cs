using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HumanResourcesManagementSystemFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddChangeHistory_Fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- XÓA HẾT MẤY CÁI Department, Employee, Account Ở TRÊN ĐI ---

            // --- CHỈ GIỮ LẠI ĐÚNG CÁI NÀY ---
            migrationBuilder.CreateTable(
                name: "ChangeHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TableName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ActionType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RecordId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangeTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    Details = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeHistories_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeHistories_AccountId",
                table: "ChangeHistories",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChangeHistories");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "TimeSheets");

            migrationBuilder.DropTable(
                name: "WorkContracts");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Positions");
        }
    }
}
