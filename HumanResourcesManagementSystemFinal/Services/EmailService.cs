using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.Services;

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
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(SenderEmail, "HRMS Pro Support"),
                Subject = "Yêu cầu cấp lại mật khẩu - HRMS Pro",
                Body = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <style>
                                body {{ font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                                .email-container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                                .header {{ background-color: #1a3d64; padding: 20px; text-align: center; color: #ffffff; }}
                                .content {{ padding: 30px; color: #333333; line-height: 1.6; }}
                                .password-box {{ background-color: #f0f9ff; border: 2px dashed #1a3d64; border-radius: 6px; padding: 15px; text-align: center; margin: 20px 0; }}
                                .password-text {{ font-size: 24px; font-weight: bold; color: #d32f2f; letter-spacing: 2px; }}
                                .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #888888; border-top: 1px solid #eeeeee; }}
                                .contact-info {{ margin-top: 10px; font-style: italic; }}
                            </style>
                        </head>
                        <body>
                            <div class='email-container'>
                                <div class='header'>
                                    <h1 style='margin:0; font-size: 24px;'>HRMS PRO</h1>
                                    <p style='margin:5px 0 0 0; font-size: 14px; opacity: 0.8;'>Hệ thống Quản lý Nhân sự</p>
                                </div>

                                <div class='content'>
                                    <h3 style='color: #1a3d64; margin-top: 0;'>Xin chào,</h3>
                                    <p>Chúng tôi đã nhận được yêu cầu khôi phục mật khẩu cho tài khoản của bạn.</p>
                                    <p>Dưới đây là mật khẩu tạm thời mới của bạn:</p>
                
                                    <div class='password-box'>
                                        <span class='password-text'>{newPass}</span>
                                    </div>

                                    <p><strong>Lưu ý quan trọng:</strong></p>
                                    <ul style='margin-bottom: 0;'>
                                        <li>Vui lòng đăng nhập và <strong>đổi mật khẩu ngay lập tức</strong> để đảm bảo bảo mật.</li>
                                        <li>Không chia sẻ mật khẩu này cho bất kỳ ai.</li>
                                    </ul>
                
                                    <br/>
                                    <p>Trân trọng,</p>
                                    <p><strong>Đội ngũ hỗ trợ HRMS Pro</strong></p>
                                </div>

                                <div class='footer'>
                                    <p>Bạn nhận được email này vì có yêu cầu thay đổi mật khẩu cho tài khoản của bạn.</p>
                                    <div class='contact-info'>
                                        <p style='margin: 5px 0;'>Hotline: <a href='tel:0762654245' style='color: #1a3d64; text-decoration: none;'>0762654245</a></p>
                                        <p style='margin: 5px 0;'>Email: <a href='mailto:24520730@gm.uit.edu.vn' style='color: #1a3d64; text-decoration: none;'>24520730@gm.uit.edu.vn</a></p>
                                        <p style='margin: 5px 0;'><strong>UIT - IT008.Q14 Project Team</strong></p>
                                    </div>
                                </div>
                            </div>
                        </body>
                        </html>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(recipientEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Không thể gửi email. Vui lòng kiểm tra kết nối internet hoặc cấu hình SMTP.",
                ex
            );
        }
    }
}
