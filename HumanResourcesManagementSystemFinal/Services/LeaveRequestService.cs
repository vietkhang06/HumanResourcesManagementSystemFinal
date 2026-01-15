using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace HumanResourcesManagementSystemFinal.Services
{
    public class LeaveRequestService
    {
        private readonly DataContext _context;

        public LeaveRequestService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<LeaveRequest>> GetRequestsByRoleAsync(string userId, string role)
        {
            var query = _context.LeaveRequests
                .Include(r => r.Requester)
                .AsQueryable();

            if (role != "Admin" && role != "Manager")
            {
                query = query.Where(r => r.EmployeeID == userId);
            }

            return await query
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }

        public async Task<bool> AddRequestAsync(LeaveRequest request)
        {
            _context.LeaveRequests.Add(request);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateRequestAsync(LeaveRequest request)
        {
            var existing = await _context.LeaveRequests.FindAsync(request.RequestID);
            if (existing == null) return false;

            existing.LeaveType = request.LeaveType;
            existing.StartDate = request.StartDate;
            existing.EndDate = request.EndDate;
            existing.Reason = request.Reason;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStatusAsync(string requestId, string status)
        {
            var existing = await _context.LeaveRequests.FindAsync(requestId);
            if (existing == null) return false;

            existing.Status = status;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteRequestAsync(string requestId)
        {
            var existing = await _context.LeaveRequests.FindAsync(requestId);
            if (existing == null) return false;

            _context.LeaveRequests.Remove(existing);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
