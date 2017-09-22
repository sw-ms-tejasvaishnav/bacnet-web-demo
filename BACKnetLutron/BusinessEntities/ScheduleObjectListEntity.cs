using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BACKnetLutron.BusinessEntities
{
    public class ScheduleObjectListEntity
    {

        public string ObjectType { get; set; }

        public string  ObjectName { get; set; }

        public Nullable<int> ObjectInstance { get; set; }

    }
}