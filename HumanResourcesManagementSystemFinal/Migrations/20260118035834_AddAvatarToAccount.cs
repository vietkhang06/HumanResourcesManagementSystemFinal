using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanResourcesManagementSystemFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CHỈ GIỮ LẠI LỆNH THÊM CỘT AvatarData VÀO BẢNG ACCOUNTS
            // Các lệnh CreateTable đã được comment lại để tránh lỗi "Already Exists"

            /* migrationBuilder.CreateTable(
                name: "Departments",
                ...
            );
            ... (Các lệnh tạo bảng khác cũng bị comment) ...
            */

            // --- LỆNH QUAN TRỌNG CẦN THỰC THI ---
            // Kiểm tra xem bảng Accounts đã có cột AvatarData chưa, nếu chưa thì thêm vào
            // Lưu ý: EF Core Migration thuần túy không hỗ trợ IF NOT EXISTS trực tiếp trong code C#,
            // nhưng vì bạn đang gặp lỗi bảng đã tồn tại, ta giả định cấu trúc cũ chưa có cột này.

            migrationBuilder.AddColumn<byte[]>(
                name: "AvatarData",
                table: "Accounts",
                type: "BLOB", // SQLite dùng BLOB, SQL Server dùng varbinary(max)
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Khi rollback, chỉ xóa cột AvatarData đi
            migrationBuilder.DropColumn(
                name: "AvatarData",
                table: "Accounts");

            /*
            migrationBuilder.DropTable(name: "ChangeHistory");
            ... (Các lệnh xóa bảng khác cũng bị comment) ...
            */
        }
    }
}