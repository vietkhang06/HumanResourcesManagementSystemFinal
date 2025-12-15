using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.Services
{
    public static class AuditService
    {
        public static void LogChange(DataContext context, string tableName, string actionType, int recordId, int? accountId, string details)
        {
            try
            {
                var history = new ChangeHistory
                {
                    TableName = tableName,
                    ActionType = actionType,
                    RecordId = recordId,
                    AccountId = accountId,
                    ChangeTime = DateTime.Now,
                    Details = details
                };
                context.ChangeHistories.Add(history);
            }
            catch { }
        }
    }
}
