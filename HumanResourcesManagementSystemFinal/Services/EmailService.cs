using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.Services
{
    public class EmailService
    {
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "doanvietkhang06@gmail.com";
        private const string SenderPassword = "ahjb ihwr bnkw axbq";
        public async Task SendPassResetEmailAsync(string recipientEmail, string newPass)
        {
            try
            {
                var smtpClient = new SmtpClient(SmtpServer)
                {
                    Port = SmtpPort,
                    Credentials = new NetworkCredential(SenderEmail, SenderPassword),
                    EnableSsl = true,
                };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(SenderEmail, "HRMS Pro Support"),
                    Subject = "Yêu cầu cấp lại mật khẩu - HRMS",
                    Body = $@"
                        <h3>Xin chào,</h3>
                        <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu của bạn.</p>
                        <p>Mật khẩu tạm thời mới của bạn là: <strong>{newPass}</strong></p>
                        <p>Vui lòng đăng nhập và đổi mật khẩu ngay lập tức.</p>
                        <br/>
                        <p>Trân trọng,<br/>HRMS Pro Team - UIT - IT008.Q14</p>",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(recipientEmail);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch(Exception e)
            {
                throw new Exception("Không thể gửi email. Vui lòng kiểm tra kết nối internet hoặc cấu hình SMTP.", e);
            }
        }
    }
}
