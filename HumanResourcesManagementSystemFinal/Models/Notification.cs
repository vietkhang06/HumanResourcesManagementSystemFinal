using System;

namespace HumanResourcesManagementSystemFinal.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } 
        public string Department { get; set; }
        public string SenderID { get; set; } 
    }
}