using HumanResourcesManagementSystemFinal.Data; 
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.Services
{
    public class LeaveRequestService
    {
        private readonly DataContext _context;
        public LeaveRequestService(DataContext context)
        {
            _context = context;
        }
        /// <summary>
        /// </summary>
        public async Task<bool> AddRequestAsync(LeaveRequest request)
        {
            try
            {
                if (request == null) return false;
                _context.LeaveRequests.Add(request);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="userId">ID của người đang đăng nhập</param>
        /// <param name="roleName">Role của người dùng ("Admin", "Manager", "Employee")</param>
        public async Task<List<LeaveRequest>> GetRequestsByRoleAsync(int userId, string roleName)
        {
            int currentUserId = userId;
            var query = _context.LeaveRequests
                                .Include(x => x.Employee)
                                .AsQueryable();
            if (roleName == "Admin")
            {
                return await query.OrderByDescending(x => x.StartDate).ToListAsync();
            }
            else if (roleName == "Manager")
            {
                return await query.Where(x => x.EmployeeId == currentUserId || x.Employee.ManagerId == currentUserId).OrderByDescending(x => x.StartDate).ToListAsync();
            }
            else
            {
                return await query.Where(x => x.EmployeeId == currentUserId).OrderByDescending(x => x.StartDate).ToListAsync();
            }
        }
        /// <summary>
        /// </summary>
        public async Task<bool> UpdateStatusAsync(int requestId, string newStatus)
        {
            try
            {
                var request = await _context.LeaveRequests.FindAsync(requestId);
                if (request == null) return false;
                request.Status = newStatus;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// </summary>
        public async Task<bool> DeletePendingRequestAsync(int requestId)
        {
            try
            {
                var request = await _context.LeaveRequests.FindAsync(requestId);
                if (request != null && request.Status == "Pending")
                {
                    _context.LeaveRequests.Remove(request);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}