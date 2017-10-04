using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BACKnetLutron.BusinessEntities
{
    public class ScheduleEntity
    {
        public int ScheduleDetailId { get; set; }
        public int ScheduleId { get; set; }
        public int DeviceId { get; set; }  
        public bool IsActive { get; set; }
        public int InstanceId { get; set; }
        public bool IsDeleted { get; set; }
        public int UserID { get; set; }
        public int UserModified { get; set; }
        public List<WeeklyScheduleEntity> WeeklySchedule { get; set; }
        public System.DateTime DateOfEntry { get; set; }
        public System.DateTime DateOfModified { get; set; }     
        public string DeviceName { get; set; }
        public string ScheduleName { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }



    }

}
