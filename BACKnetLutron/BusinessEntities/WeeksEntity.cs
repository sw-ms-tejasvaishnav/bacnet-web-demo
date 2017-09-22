using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BACKnetLutron.BusinessEntities
{
    public class WeeksEntity
    {

        public int DayId { get; set; }
        public string DayName { get; set; }
        public bool isActive { get; set; }
        public bool isDeleted { get; set; }
    }
}