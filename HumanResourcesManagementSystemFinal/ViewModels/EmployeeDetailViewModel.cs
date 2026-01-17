using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HumanResourcesManagementSystemFinal.Models;
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

        private string GetImageBase64(string employeeId)
        {
            try
            {
                // Giả sử ảnh nhân viên lưu trong thư mục "Images" nằm cùng chỗ với file .exe
                // Tên ảnh là ID nhân viên (ví dụ: NV001.png hoặc NV001.jpg)
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string imagePath = Path.Combine(baseDir, "Images", $"{employeeId}.png");

                // Nếu không thấy đuôi .png thì tìm .jpg
                if (!File.Exists(imagePath))
                    imagePath = Path.Combine(baseDir, "Images", $"{employeeId}.jpg");

                // Nếu vẫn không thấy thì lấy ảnh mặc định
                if (!File.Exists(imagePath))
                    imagePath = Path.Combine(baseDir, "Images", "default_user.png");

                if (File.Exists(imagePath))
                {
                    byte[] imageBytes = File.ReadAllBytes(imagePath);
                    return "data:image/png;base64," + Convert.ToBase64String(imageBytes);
                }
            }
            catch
            {
                // Lỗi thì trả về rỗng (không hiện ảnh)
            }
            return "";
        }

        // 2. CẬP NHẬT HÀM TẠO HTML: Thêm cột ảnh vào bố cục
        private string GenerateHtmlContent()
        {
            // Lấy ngày vào làm an toàn
            string hireDateStr = "N/A";
            if (Employee.WorkContracts != null && Employee.WorkContracts.Any())
            {
                var firstContract = Employee.WorkContracts.OrderBy(c => c.StartDate).FirstOrDefault();
                if (firstContract != null) hireDateStr = string.Format("{0:dd/MM/yyyy}", firstContract.StartDate);
            }

            // --- LẤY ẢNH ---
            string imgBase64 = GetImageBase64(Employee.EmployeeID);
            string imgTag = string.IsNullOrEmpty(imgBase64)
                ? ""
                : $"<img src='{imgBase64}' width='110' height='140' style='border: 1px solid #333; object-fit: cover;' />";

            string css = @"
        <style>
            body { font-family: 'Times New Roman', serif; line-height: 1.4; font-size: 13pt; }
            h1 { text-align: center; color: #1A3D64; text-transform: uppercase; margin-bottom: 20px; font-size: 18pt; }
            .section { margin-top: 20px; border-bottom: 2px solid #333; padding-bottom: 5px; font-weight: bold; font-size: 14pt; color: #333; }
            /* Table layout cho phần thông tin */
            .info-table { width: 100%; border-collapse: collapse; margin-top: 10px; }
            .info-table td { padding: 5px; vertical-align: top; }
            .label { font-weight: bold; width: 160px; color: #444; }
            .footer { margin-top: 50px; text-align: right; font-style: italic; }
        </style>";

            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'>" + css + "</head><body>");

            sb.Append($"<h1>SƠ YẾU LÝ LỊCH NHÂN VIÊN</h1>");

            // --- PHẦN 1: THÔNG TIN CHUNG (Chia 2 cột: Trái là chữ, Phải là ảnh) ---
            sb.Append("<div class='section'>I. THÔNG TIN CHUNG</div>");

            sb.Append("<table class='info-table'>");
            sb.Append("<tr>");

            // Cột 1: Thông tin chữ
            sb.Append("<td>");
            sb.Append("<table class='info-table'>"); // Bảng con chứa thông tin
            sb.Append($"<tr><td class='label'>Họ và tên:</td><td><b>{Employee.FullName?.ToUpper()}</b></td></tr>");
            sb.Append($"<tr><td class='label'>Mã nhân viên:</td><td>{Employee.EmployeeID}</td></tr>");
            sb.Append($"<tr><td class='label'>Ngày sinh:</td><td>{string.Format("{0:dd/MM/yyyy}", Employee.DateOfBirth)}</td></tr>");
            sb.Append($"<tr><td class='label'>Giới tính:</td><td>{Employee.Gender}</td></tr>");
            sb.Append($"<tr><td class='label'>CMND/CCCD:</td><td>{Employee.CCCD}</td></tr>");
            sb.Append($"<tr><td class='label'>Địa chỉ:</td><td>{Employee.Address}</td></tr>");
            sb.Append("</table>");
            sb.Append("</td>");

            // Cột 2: Ảnh thẻ (Căn phải)
            sb.Append($"<td style='width: 130px; text-align: center; vertical-align: top;'>{imgTag}</td>");

            sb.Append("</tr>");
            sb.Append("</table>");

            // --- PHẦN 2: LIÊN HỆ ---
            sb.Append("<div class='section'>II. THÔNG TIN LIÊN HỆ</div>");
            sb.Append("<table class='info-table'>");
            sb.Append($"<tr><td class='label'>Điện thoại:</td><td>{Employee.PhoneNumber}</td></tr>");
            sb.Append($"<tr><td class='label'>Email:</td><td>{Employee.Email}</td></tr>");
            sb.Append("</table>");

            // --- PHẦN 3: CÔNG TÁC ---
            sb.Append("<div class='section'>III. THÔNG TIN CÔNG TÁC</div>");
            sb.Append("<table class='info-table'>");
            sb.Append($"<tr><td class='label'>Phòng ban:</td><td>{Employee.Department?.DepartmentName ?? "N/A"}</td></tr>");
            sb.Append($"<tr><td class='label'>Chức vụ:</td><td>{Employee.Position?.PositionName ?? "N/A"}</td></tr>");
            sb.Append($"<tr><td class='label'>Trạng thái:</td><td>{Employee.Status}</td></tr>");
            sb.Append($"<tr><td class='label'>Ngày vào làm:</td><td>{hireDateStr}</td></tr>");
            sb.Append("</table>");

            // Footer
            sb.Append("<div class='footer'>");
            sb.Append($"<p>Tp.HCM, ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}</p>");
            sb.Append("<p>Người lập phiếu</p><br><br><br><br>");
            sb.Append($"<p><b>{Employee.FullName}</b></p>");
            sb.Append("</div>");

            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
}