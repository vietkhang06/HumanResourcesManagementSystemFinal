using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.Models
{
    public static class UserSession
    {
        public static int CurrentEmployeeId { get; set; } = 5;
        public static string CurrentRole { get; set; }
    }
}
