using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResourcesManagementSystemFinal.Models
{
    public class TimeSheetDTO
    {
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public DateTime WorkDate { get; set; }
        public string CheckInText { get; set; }
        public string CheckOutText { get; set; }
        public string TotalHoursText { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
    }
}
