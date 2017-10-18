using BACKnetLutron.BusinessEntities;
using BACKnetLutron.BusinessEntities.Obix;
using BACKnetLutron.DataModal;
using BACKnetLutron.Services;
using System;
using System.Collections.Generic;
using System.IO.BACnet;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BACKnetLutron.Controllers
{
     [RoutePrefix("api/obixjacebacne")]
    public class ObixJACEBacnetController : System.Web.Http.ApiController
    {
        private ILutronLightFloorServices _LutronLightFloorServices;

         #region Coustructor
        public ObixJACEBacnetController(ILutronLightFloorServices LutronLightFloorServices)
        {
            this._LutronLightFloorServices = LutronLightFloorServices;
        }
        #endregion

        #region Controller Methods
        /// <summary>
        /// Start BacNet process.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("StartBackNetProtocol")]
        public IHttpActionResult StartBackNetProtocol()
        {
            _LutronLightFloorServices.StartBacnetServerActivity();
            return Ok();
        }

        [HttpGet]
        [Route("GetConfLightState")]
        public IHttpActionResult GetConfLightState(Int32? deviceID)
        {
            var lightState = _LutronLightFloorServices.GetConfLightState(deviceID);
            return Ok(lightState);
        }


        [HttpGet]
        [Route("GetConfLightLevel")]
        public IHttpActionResult GetConfLightLevel(Int32? deviceID)
        {
            var lightLevel = _LutronLightFloorServices.GetConfLightLevel(deviceID);
            return Ok(lightLevel);
        }


        [HttpGet]
        [Route("GetConfLightingScene")]
        public IHttpActionResult GetConfLightingScene(Int32? deviceID)
        {
            var lightLevel = _LutronLightFloorServices.GetConfLightingScene(deviceID);
            return Ok(lightLevel);
        }

        [HttpPost]
        [Route("SetConfLightLevel")]
        public IHttpActionResult SetConfLightLevel(LightLevelEntity lightLevelEntity)
        {
            var lightLevel = _LutronLightFloorServices.SetConfLightLevel(lightLevelEntity);
            return Ok(lightLevel);
        }

        [HttpPost]
        [Route("SetConfLightScene")]
        public IHttpActionResult SetConfLightScene(LightSceneEntity lightSceneEntity)
        {
            var lightLevel = _LutronLightFloorServices.SetConfLightScene(lightSceneEntity);
            return Ok(lightLevel);
        }


        [HttpPost]
        [Route("SetConfLightState")]
        public IHttpActionResult SetConfLightState(LightStateEntity lightStateEntity)
        {
            var lightState = _LutronLightFloorServices.SetConfLightState(lightStateEntity);
            return Ok(lightState);
        }
        #endregion
    }
}