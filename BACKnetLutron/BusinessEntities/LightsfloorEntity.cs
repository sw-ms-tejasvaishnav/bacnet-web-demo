using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BACKnetLutron.BusinessEntities
{
    public class LightsfloorEntity
    {
        public int DeviceId { get; set; }

        public bool LightStatus { get; set; }

        public int InstanceId { get; set; }
    }
}