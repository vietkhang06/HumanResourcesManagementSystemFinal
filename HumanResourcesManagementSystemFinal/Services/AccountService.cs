using HumanResourcesManagementSystemFinal.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.Services
{
    public class AccountService
    {
        private readonly DataContext _context;

        public AccountService(DataContext context)
        {
            _context = context;
        }
        public async Task<bool> ChangePasswordAsync(int accountId, string oldPassword, string newPassword)
        {
            try
            {
                var account = await _context.Accounts.FindAsync(accountId);
                if (account == null) return false;
                if (account.PasswordHash != oldPassword)
                {
                    throw new Exception("Mật khẩu cũ không chính xác!");
                }
                account.PasswordHash = newPassword;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw; 
            }
        }
    }
}