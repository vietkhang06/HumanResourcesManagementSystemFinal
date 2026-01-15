using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.Models
{
    public static class UserSession
    {
        public static string CurrentEmployeeId { get; set; }
        public static string CurrentRole { get; set; }
    }
}