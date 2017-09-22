using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BACKnetLutron.BusinessEntities
{
    public class BackNetDeviceDetailEntity
    {
        public string network_id { get; set; }
        public Nullable<int> device_id { get; set; }
        public string object_type { get; set; }
        public Nullable<int> object_instance { get; set; }
        public string object_name { get; set; }
        public Nullable<int> routed_net { get; set; }
        public string routed_source { get; set; }

    }
}