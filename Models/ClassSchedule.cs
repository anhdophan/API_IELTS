using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class ClassSchedule
    {
        public string DayOfWeek { get; set; }     // e.g., "Monday"
        public string StartTime { get; set; }     // e.g., "14:00"
        public string EndTime { get; set; }       // e.g., "16:00"
    }
}