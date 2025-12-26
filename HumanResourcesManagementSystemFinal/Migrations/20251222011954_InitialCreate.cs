using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanResourcesManagementSystemFinal.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    PositionID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    PositionName = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    JobDescription = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    RoleName = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    UserID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    IsActive = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EmployeeID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: true),
                    RoleID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Accounts_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID");
                });

            migrationBuilder.CreateTable(
                name: "ChangeHistory",
                columns: table => new
                {
                    LogID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    TableName = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    RecordID = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    ActionType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ChangeTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChangeByUserID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: true),
                    Details = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    EmployeeID = table.Column<string>(type: "char(5)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeHistory", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_ChangeHistory_Accounts_ChangeByUserID",
                        column: x => x.ChangeByUserID,
                        principalTable: "Accounts",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    DepartmentName = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    ManagerID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentID);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CCCD = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DepartmentID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: true),
                    PositionID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: true),
                    ManagerID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Employees_Employees_ManagerID",
                        column: x => x.ManagerID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Employees_Positions_PositionID",
                        column: x => x.PositionID,
                        principalTable: "Positions",
                        principalColumn: "PositionID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    RequestID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    EmployeeID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: true),
                    LeaveType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    StartDate = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    EndDate = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    TotalDays = table.Column<int>(type: "INTEGER", nullable: true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ApproverID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: true),
                    ManagerComment = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.RequestID);
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_ApproverID",
                        column: x => x.ApproverID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_LeaveRequests_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeSheets",
                columns: table => new
                {
                    TimeSheetID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    EmployeeID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: true),
                    WorkDate = table.Column<DateTime>(type: "smalldatetime", nullable: false),
                    TimeIn = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    TimeOut = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    ActualHours = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSheets", x => x.TimeSheetID);
                    table.ForeignKey(
                        name: "FK_TimeSheets_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkContracts",
                columns: table => new
                {
                    ContractID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: false),
                    EmployeeID = table.Column<string>(type: "char(5)", maxLength: 5, nullable: true),
                    ContractType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    StartDate = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    EndDate = table.Column<DateTime>(type: "smalldatetime", nullable: true),
                    Salary = table.Column<decimal>(type: "money", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkContracts", x => x.ContractID);
                    table.ForeignKey(
                        name: "FK_WorkContracts_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_EmployeeID",
                table: "Accounts",
                column: "EmployeeID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleID",
                table: "Accounts",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeHistory_ChangeByUserID",
                table: "ChangeHistory",
                column: "ChangeByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeHistory_EmployeeID",
                table: "ChangeHistory",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerID",
                table: "Departments",
                column: "ManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentID",
                table: "Employees",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ManagerID",
                table: "Employees",
                column: "ManagerID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PositionID",
                table: "Employees",
                column: "PositionID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_ApproverID",
                table: "LeaveRequests",
                column: "ApproverID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_EmployeeID",
                table: "LeaveRequests",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSheets_EmployeeID",
                table: "TimeSheets",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_WorkContracts_EmployeeID",
                table: "WorkContracts",
                column: "EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Employees_EmployeeID",
                table: "Accounts",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeHistory_Employees_EmployeeID",
                table: "ChangeHistory",
                column: "EmployeeID",
                principalTable: "Employees",
                principalColumn: "EmployeeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_ManagerID",
                table: "Departments",
                column: "ManagerID",
                principalTable: "Employees",
                principalColumn: "EmployeeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_ManagerID",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "ChangeHistory");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "TimeSheets");

            migrationBuilder.DropTable(
                name: "WorkContracts");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Positions");
        }
    }
}
