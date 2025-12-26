using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using System;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.Services
{
    public static class AuditService
    {
        // Sửa tham số int adminId -> string adminId
        public static void LogChange(DataContext context, string tableName, string action, string recordId, string adminId, string details)
        {
            // Tự sinh LogID
            string logId = "LG" + DateTime.Now.Ticks.ToString().Substring(10, 3);

            var history = new ChangeHistory
            {
                LogID = logId,
                TableName = tableName,
                ActionType = action,
                RecordID = recordId,      // Đã sửa thành string
                ChangeByUserID = adminId, // Đã sửa thành string
                ChangeTime = DateTime.Now,
                Details = details
            };

            context.ChangeHistories.Add(history);
        }
    }
}