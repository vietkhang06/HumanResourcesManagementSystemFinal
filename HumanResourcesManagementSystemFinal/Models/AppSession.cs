using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HumanResourcesManagementSystemFinal.Models;

namespace HumanResourcesManagementSystemFinal
{
    public static class AppSession
    {
        public static Employee CurrentUser { get; set; }
        public static string CurrentRole { get; set; }
    }
}