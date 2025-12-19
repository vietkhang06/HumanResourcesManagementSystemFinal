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
        public LeaveRequestService()
        {
            _context = new DataContext();
        }
        public async Task<List<LeaveRequest>> GetRequestsByRoleAsync(int userId, string role)
        {
            if (role == "Admin" || role == "Manager")
            {
                return await _context.LeaveRequests
                                     .Include(l => l.Employee)
                                     .OrderByDescending(l => l.StartDate)
                                     .ToListAsync();
            }
            else
            {
                return await _context.LeaveRequests
                                     .Include(l => l.Employee)
                                     .Where(l => l.EmployeeId == userId)
                                     .OrderByDescending(l => l.StartDate)
                                     .ToListAsync();
            }
        }
        public async Task<bool> AddRequestAsync(LeaveRequest request)
        {
            _context.LeaveRequests.Add(request);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> UpdateRequestAsync(LeaveRequest request)
        {
            var item = await _context.LeaveRequests.FindAsync(request.Id);
            if (item == null) return false;

            item.LeaveType = request.LeaveType;
            item.StartDate = request.StartDate;
            item.EndDate = request.EndDate;
            item.Reason = request.Reason;
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var item = await _context.LeaveRequests.FindAsync(id);
            if (item == null) return false;

            item.Status = status;
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteRequestAsync(int id)
        {
            var item = await _context.LeaveRequests.FindAsync(id);
            if (item == null) return false;

            _context.LeaveRequests.Remove(item);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}