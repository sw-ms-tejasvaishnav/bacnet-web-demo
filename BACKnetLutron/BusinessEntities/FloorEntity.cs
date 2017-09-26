using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BACKnetLutron.BusinessEntities
{
    public class FloorEntity
    {
        public int FloorId { get; set; }

        public string FloorName { get; set; }

        public bool isActive { get; set; }
        public bool isDeleted { get; set; }

        public int NoOfInstance { get; set; }

        public List<LightsfloorEntity> BinaryDetails { get; set; }


    }
}