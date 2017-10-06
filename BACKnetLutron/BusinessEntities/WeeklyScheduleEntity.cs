using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BACKnetLutron.BusinessEntities
{
    public class WeeklyScheduleEntity
    {
        public double PresentValue { get; set; }
        public bool IsDeleted { get; set; }
        public int UserID { get; set; }
        public int ScheduleId { get; set; }
        public int ScheduleDetailId { get; set; }
        public int InstanceId { get; set; }
        public int DeviceId { get; set; }
        public int UserModified { get; set; }
        public System.DateTime DateOfEntry { get; set; }
        public System.DateTime DateOfModified { get; set; }
        public int SelectedDayId { get; set; }
        public string DayName { get; set; }
        public System.DateTime SelectedTime { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public string  d1 { get; set; }
        public string d2 { get; set; }
        public int SelectedDay { get; set; }
        public bool IsActive { get; set; }
    }
}