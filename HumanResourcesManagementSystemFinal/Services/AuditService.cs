using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;

namespace HumanResourcesManagementSystemFinal.Services;

public static class AuditService
{
    public static void LogChange(DataContext context, string tableName, string action, string recordId, string adminId, string details)
    {
        string logId = "LG" + DateTime.Now.Ticks.ToString().Substring(10, 3);
        var history = new ChangeHistory
        {
            LogID = logId,
            TableName = tableName,
            ActionType = action,
            RecordID = recordId,
            ChangeByUserID = adminId,
            ChangeTime = DateTime.Now,
            Details = details
        };
        context.ChangeHistories.Add(history);
    }
}
