using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Data; // Cần thêm để query nếu cần
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class EmployeeDetailViewModel : ObservableObject
    {
        [ObservableProperty]
        private Employee _employee;

        public EmployeeDetailViewModel(Employee emp)
        {
            Employee = emp;
        }

        [RelayCommand]
        private void ExportWord()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Word Document (*.doc)|*.doc",
                    FileName = $"SoYeuLyLich_{Employee.FullName}_{DateTime.Now:yyyyMMdd}.doc"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string content = GenerateHtmlContent();
                    File.WriteAllText(saveFileDialog.FileName, content, Encoding.UTF8);

                    MessageBox.Show("Xuất file Word thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    var p = new System.Diagnostics.Process();
                    p.StartInfo = new System.Diagnostics.ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true };
                    p.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file: " + ex.Message);
            }
        }

        [RelayCommand]
        private void ExportPdf(Window window)
        {
            if (window == null) return;
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(window, $"Hồ sơ {Employee.FullName}");
            }
        }

        // Lấy ảnh Base64 từ Byte Array trong Account
        private string GetImageBase64()
        {
            try
            {
                // Nếu Model Employee đã có Account và AvatarData
                if (Employee.Account != null && Employee.Account.AvatarData != null && Employee.Account.AvatarData.Length > 0)
                {
                    return "data:image/png;base64," + Convert.ToBase64String(Employee.Account.AvatarData);
                }

                // Nếu chưa có, thử query lại từ DB cho chắc chắn
                using var context = new DataContext();
                var acc = context.Accounts.FirstOrDefault(a => a.EmployeeID == Employee.EmployeeID);
                if (acc != null && acc.AvatarData != null && acc.AvatarData.Length > 0)
                {
                    return "data:image/png;base64," + Convert.ToBase64String(acc.AvatarData);
                }
            }
            catch { }
            return ""; // Trả về rỗng để hiện khung "No Photo"
        }

        private string GenerateHtmlContent()
        {
            string hireDateStr = "N/A";
            if (Employee.WorkContracts != null && Employee.WorkContracts.Any())
            {
                var firstContract = Employee.WorkContracts.OrderBy(c => c.StartDate).FirstOrDefault();
                if (firstContract != null) hireDateStr = string.Format("{0:dd/MM/yyyy}", firstContract.StartDate);
            }

            string imgBase64 = GetImageBase64();
            string imgTag = string.IsNullOrEmpty(imgBase64)
                ? "<div style='height: 180px; background: #eee; text-align: center; line-height: 180px; color: #999;'>No Photo</div>"
                : $"<img src='{imgBase64}' style='width: 100%; max-width: 200px; height: auto; border-radius: 8px; border: 2px solid #eee;' />";

            // (Phần CSS và HTML phía dưới GIỮ NGUYÊN không đổi)
            string css = @"
                <style>
                    body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.5; color: #333; font-size: 11pt; }
                    h1 { text-align: center; color: #1E3A8A; text-transform: uppercase; margin-bottom: 30px; font-size: 18pt; }
                    .layout-table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }
                    .layout-table td { vertical-align: top; }
                    .left-col { width: 32%; padding-right: 20px; }
                    .right-col { width: 68%; }
                    .content-box { background: #fff; padding: 20px; border: 1px solid #e2e8f0; border-radius: 10px; box-shadow: 0 2px 5px rgba(0,0,0,0.05); }
                    .photo-box { text-align: center; margin-bottom: 20px; background: #f8fafc; }
                    .contact-box { background: #f1f5f9; }
                    .section-header { color: #1E3A8A; font-weight: bold; font-size: 13pt; margin-bottom: 15px; border-bottom: 2px solid #1E3A8A; padding-bottom: 8px; text-transform: uppercase; }
                    .detail-table { width: 100%; border-collapse: collapse; }
                    .detail-table td { padding: 6px 0; border-bottom: 1px solid #eee; }
                    .detail-table tr:last-child td { border-bottom: none; }
                    .label { font-weight: bold; color: #555; width: 140px; }
                    .value { color: #000; font-weight: 500; }
                    .footer { margin-top: 40px; text-align: right; font-style: italic; color: #666; }
                </style>";

            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'>" + css + "</head><body>");
            sb.Append($"<h1>HỒ SƠ NHÂN VIÊN</h1>");

            sb.Append("<table class='layout-table'>");
            sb.Append("<tr>");

            sb.Append("<td class='left-col'>");
            sb.Append("<div class='content-box photo-box'>");
            sb.Append(imgTag);
            sb.Append($"<h2 style='margin: 15px 0 5px 0; color: #1E3A8A; font-size: 16pt;'>{Employee.FullName}</h2>");
            sb.Append($"<p style='margin: 0; color: #64748B; font-weight: bold;'>{Employee.Position?.PositionName ?? "N/A"}</p>");
            sb.Append($"<p style='margin: 5px 0 0 0; color: #64748B; font-size: 10pt;'>ID: {Employee.EmployeeID}</p>");
            sb.Append("</div>");

            sb.Append("<div class='content-box contact-box'>");
            sb.Append("<div class='section-header'>LIÊN HỆ</div>");
            sb.Append("<table class='detail-table'>");
            sb.Append($"<tr><td class='label'>Điện thoại:</td><td class='value'>{Employee.PhoneNumber}</td></tr>");
            sb.Append($"<tr><td class='label'>Email:</td><td class='value'>{Employee.Email}</td></tr>");
            sb.Append($"<tr><td class='label' style='vertical-align:top;'>Địa chỉ:</td><td class='value' style='line-height: 1.4;'>{Employee.Address}</td></tr>");
            sb.Append("</table>");
            sb.Append("</div>");
            sb.Append("</td>");

            sb.Append("<td class='right-col'>");
            sb.Append("<div class='content-box' style='height: 96%;'>");

            sb.Append("<div class='section-header'>THÔNG TIN CÁ NHÂN</div>");
            sb.Append("<table class='detail-table'>");
            sb.Append($"<tr><td class='label'>Ngày sinh:</td><td class='value'>{string.Format("{0:dd/MM/yyyy}", Employee.DateOfBirth)}</td></tr>");
            sb.Append($"<tr><td class='label'>Giới tính:</td><td class='value'>{Employee.Gender}</td></tr>");
            sb.Append($"<tr><td class='label'>CMND/CCCD:</td><td class='value'>{Employee.CCCD}</td></tr>");
            sb.Append("</table>");

            sb.Append("<br><br>");

            sb.Append("<div class='section-header'>THÔNG TIN CÔNG TÁC</div>");
            sb.Append("<table class='detail-table'>");
            sb.Append($"<tr><td class='label'>Phòng ban:</td><td class='value'>{Employee.Department?.DepartmentName ?? "N/A"}</td></tr>");
            sb.Append($"<tr><td class='label'>Chức vụ hiện tại:</td><td class='value'>{Employee.Position?.PositionName ?? "N/A"}</td></tr>");
            sb.Append($"<tr><td class='label'>Quản lý trực tiếp:</td><td class='value'>{Employee.ManagerID ?? "N/A"}</td></tr>");
            sb.Append($"<tr><td class='label'>Ngày vào làm:</td><td class='value'>{hireDateStr}</td></tr>");
            sb.Append("</table>");

            sb.Append("</div>");
            sb.Append("</td>");

            sb.Append("</tr>");
            sb.Append("</table>");

            sb.Append("<div class='footer'>");
            sb.Append($"<p>Tp.HCM, ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}</p>");
            sb.Append("<p style='margin-bottom: 60px;'>Người lập phiếu</p>");
            sb.Append($"<p style='font-weight: bold; font-size: 12pt;'>{Employee.FullName}</p>");
            sb.Append("</div>");

            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
}