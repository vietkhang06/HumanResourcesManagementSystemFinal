using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanResourcesManagementSystemFinal.Services;

public class AccountService
{
    private readonly DataContext _context;

    public AccountService(DataContext context)
    {
        _context = context;
    }

    public async Task<Account?> LoginAsync(string username, string password)
    {
        return await _context.Accounts
            .Include(a => a.Role)
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a =>
                a.UserName == username && a.Password == password
            );
    }

    public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        var account = await _context.Accounts.FindAsync(userId);
        if (account == null) return false;
        if (account.Password != oldPassword) return false;
        account.Password = newPassword;
        return await _context.SaveChangesAsync() > 0;
    }
}
