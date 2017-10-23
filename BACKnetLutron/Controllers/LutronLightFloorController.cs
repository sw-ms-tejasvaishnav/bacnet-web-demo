using BACKnetLutron.BusinessEntities;
using BACKnetLutron.BusinessEntities.Common_Constant;
using BACKnetLutron.BusinessEntities.Obix;
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

        #region LutronLightFloorNew

        [HttpGet]
        [Route("GetDeviceDetails/{deviceId:int}")]
        public IHttpActionResult GetDeviceDetails(int deviceId)
        {
            var lightScene = _LutronLightFloorServices.GetConfLightingScene(deviceId);
            var lightState = _LutronLightFloorServices.GetConfLightState(deviceId);
            var lightLevel = _LutronLightFloorServices.GetConfLightLevel(deviceId);

            var deviceDetail = new DeviceDetailEnity
            {
                DeviceID = lightScene.DeviceID,
                LightScene = lightScene.LightScene,
                LightSceneValue = lightScene.Value,
                LightLevel = lightLevel.LightLevel,
                LightState = lightState.LightState
            };
            return Ok(deviceDetail);
        }

        [HttpPost]
        [Route("SetLightingScene")]
        public IHttpActionResult SetLightingScene(LightSceneEntity lightscene)
        {
            LightSceneEntity lightScenetemp = new LightSceneEntity();
            lightscene.Value = EnumConstants.GetEnumValueFromDescription<LightSceneEnum>(lightscene.LightScene).ToString();
            var lightScene = _LutronLightFloorServices.SetConfLightScene(lightscene);
            var lightLevel = _LutronLightFloorServices.GetConfLightLevel(lightscene.DeviceID);
            var deviceDetail = new DeviceDetailEnity
            {
                DeviceID = lightScene.DeviceID,
                LightScene = lightScene.LightScene,
                LightSceneValue = lightScene.Value,
                LightLevel = lightLevel.LightLevel,
                //LightState = lightState.LightState
            };
            return Ok(deviceDetail);
        }

        [HttpPost]
        [Route("SetLightingLevel")]
        public IHttpActionResult SetLightingLevel(LightLevelEntity lightLevel)
        {
            LightSceneEntity lightScenetemp = new LightSceneEntity();
            var deviceLightLevel = _LutronLightFloorServices.SetConfLightLevel(lightLevel);
            var lightScene = _LutronLightFloorServices.GetConfLightingScene(lightLevel.DeviceID);
            var deviceDetail = new DeviceDetailEnity
            {
                DeviceID = lightScene.DeviceID,
                LightScene = lightScene.LightScene,
                LightSceneValue = lightScene.Value,
                LightLevel = deviceLightLevel.LightLevel,
                //LightState = lightState.LightState
            };
            return Ok(deviceDetail);
        }



        #endregion

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
        public IHttpActionResult SaveBackNetValue(int bacnetvalue, int floorId)
        {
            _LutronLightFloorServices.SaveBackNetPresantValue(bacnetvalue, floorId);
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
        [Route("SetLightsSimulator/{deviceId:int}/{instanceId:int}")]
        public IHttpActionResult SetLightsSimulator(int deviceId, int instanceId)
        {
            var currentStatus = _LutronLightFloorServices.SetLightSimulator(deviceId, instanceId);
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
        public IHttpActionResult SaveSchedule(WeeklyScheduleEntity scheduleDetail)
        {
            if (!string.IsNullOrEmpty(scheduleDetail.d1))
            {
                scheduleDetail.ScheduleStartDate = Convert.ToDateTime(scheduleDetail.d1);
            }
            else
            {
                scheduleDetail.ScheduleStartDate = DateTime.Today.AddDays(-1);
            }
            if (!string.IsNullOrEmpty(scheduleDetail.d2))
            {
                scheduleDetail.ScheduleEndDate = Convert.ToDateTime(scheduleDetail.d2);
            }
            else
            {
                scheduleDetail.ScheduleEndDate = DateTime.Today.AddMonths(1);
            }
            _LutronLightFloorServices.SaveSchedule(scheduleDetail);

            return Ok();
        }

        /// <summary>
        /// Gets schedule list.
        /// </summary>
        /// <returns>Schedule list.</returns>
        [HttpGet]
        [Route("GetScheduleList")]
        public IHttpActionResult GetScheduleList()
        {
            var scheduleLst = _LutronLightFloorServices.ScheduleList();
            return Ok(scheduleLst);
        }

        /// <summary>
        /// Gets schedule info by device id and instance id.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <param name="instanceId">Passes instance id.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetsScheduleInfo/{deviceId:int}/{instanceId:int}")]
        public IHttpActionResult GetsScheduleInfo(int deviceId, int instanceId)
        {
            var scheduleInfo = _LutronLightFloorServices.GetsScheduleInfo(deviceId, instanceId);
            return Ok(scheduleInfo);
        }



        #endregion

    }
}
