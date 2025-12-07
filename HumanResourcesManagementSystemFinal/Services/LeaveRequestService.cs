using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> AddRequestAsync(LeaveRequest request)
        {
            try
            {
                _context.LeaveRequests.Add(request);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<List<LeaveRequest>> GetRequestsByRoleAsync(int employeeId, string role)
        {
            int currentId = employeeId;

            var query = _context.LeaveRequests
                .Include(l => l.Employee)
                .AsQueryable();

            if (role == "Admin" || role == "Manager")
            {
                return await query.OrderByDescending(x => x.StartDate).ToListAsync();
            }
            else
            {
                return await query
                    .Where(l => l.EmployeeId == currentId)
                    .OrderByDescending(x => x.StartDate)
                    .ToListAsync();
            }
        }

        public async Task<bool> UpdateStatusAsync(int requestId, string newStatus)
        {
            var request = await _context.LeaveRequests.FindAsync(requestId);
            if (request == null) return false;

            request.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}