using BACKnetLutron.BusinessEntities;
using BACKnetLutron.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace BACKnetLutron.Controllers
{
    [RoutePrefix("api/LutronLightFloor")]
    public class LutronLightFloorController : ApiController
    {
        private ILutronLightFloorServices _LutronLightFloorServices;

        #region Coustructor
        public LutronLightFloorController(ILutronLightFloorServices LutronLightFloorServices)
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

        /// <summary>
        /// Gets floors.
        /// </summary>
        /// <returns>Floor list.</returns>
        [HttpGet]
        [Route("Floor")]
        public IHttpActionResult Floor()
        {
            var floorLst = _LutronLightFloorServices.GetsFloor();
            return Ok(floorLst);
        }

        /// <summary>
        /// Gets Weeks name.
        /// </summary>
        /// <returns>List of weeks.</returns>
        [HttpGet]
        [Route("Weeks")]
        public IHttpActionResult Weeks()
        {
            var weeksLst = _LutronLightFloorServices.GetsWeeks();
            return Ok(weeksLst);
        }

        /// <summary>
        /// Gets device list.
        /// </summary>
        /// <returns>Device List</returns>
        [HttpGet]
        [Route("Devices")]
        public IHttpActionResult Devices()
        {
            var deviceLst = _LutronLightFloorServices.GetDevices();
            return Ok(deviceLst);
        }

        /// <summary>
        /// Gets device object names base on device id.
        /// </summary>
        /// <param name="deviceId">Passes deviceId.</param>
        /// <returns>Schedule object.</returns>
        [HttpGet]
        [Route("ScheduleObjects/{deviceId:int}")]
        public IHttpActionResult ScheduleObjects(int deviceId)
        {
            var scheduleObjLst = _LutronLightFloorServices.GetsScheduleObjectList(deviceId);
            return Ok(scheduleObjLst);
        }

        /// <summary>
        /// Saves bacnet presant value.
        /// </summary>
        /// <param name="bacnetvalue">Passes value.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SaveBackNetPresantValue/{bacnetvalue:int}/{floorId:int}")]
        public IHttpActionResult SaveBackNetValue(int bacnetvalue,int floorId)
        {
            _LutronLightFloorServices.SaveBackNetPresantValue(bacnetvalue,floorId);
            return Ok();
        }

        /// <summary>
        /// Gets current light level according to device.
        /// </summary>
        /// <param name="floorId">Passes floor id.</param>
        /// <returns>Current Light level.</returns>
        [HttpGet]
        [Route("GetsLutronLightLevelByFloor/{floorId:int}")]
        public IHttpActionResult GetsLutronLightLevelByFloor(int floorId)
        {
            var currentLightLevel = _LutronLightFloorServices.GetsLutronLightLevelByFloor(floorId);
            return Ok(currentLightLevel);
        }

        /// <summary>
        /// Sets simulator binary presant value.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("SetLightsSimulator/{deviceId:int}")]
        public IHttpActionResult SetLightsSimulator(int deviceId)
        {
            var currentStatus=_LutronLightFloorServices.SetLightSimulator(deviceId);
            return Ok(currentStatus);
        }

        /// <summary>
        /// Gets current binary presant value.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <returns>Status</returns>
        [HttpGet]
        [Route("CurrentBinaryPresantValue/{deviceId:int}")]
        public IHttpActionResult CurrentBinaryPresantValue(int deviceId)
        {
            bool istrue = _LutronLightFloorServices.CurrentBinaryPresantValue(deviceId);
            return Ok(istrue);
        }

        /// <summary>
        /// Saves schedule detail.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("SaveSchedule")]
        public IHttpActionResult SaveSchedule(ScheduleEntity scheduleDetail)
        {
            _LutronLightFloorServices.SaveSchedule(scheduleDetail);
            return Ok();
        }

        #endregion

    }
}
