using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BACKnetLutron.BusinessEntities
{
    public class ScheduleEntity
    {
        public int SelectedDay { get; set; }

        public DateTime SelectedTime { get; set; }

        public int DeviceId { get; set; }

        public int ScheduleObjectid { get; set; }

        public float SchedulePresantValue { get; set; }

    }
}