using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BACKnetLutron.DataModal;
using BACKnetLutron.BusinessEntities;

namespace BACKnetLutron.Repository
{
    public class LutronLightRepository : ILutronLightRepository
    {

        ESD_LutronEntities dbContext = new ESD_LutronEntities();

        /// <summary>
        /// Adds bacnet device details on start process.
        /// </summary>
        /// <param name="bacNetDeviceLst">Passes list of devices.</param>
        public void AddBacNetDeviceDetail(List<BACnetDevice> bacNetDeviceLst)
        {
            if (bacNetDeviceLst.Count() > 0)
            {
                foreach (var bacNetDevice in bacNetDeviceLst)
                {
                    dbContext.BACnetDevices.Add(bacNetDevice);
                }
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Adds bacnet mapping details on start process.
        /// </summary>
        /// <param name="bacNetDeviceMappingLst">Passes bacnet device mapping list.</param>
        public void AddBacNetMappingDetail(List<BACnetDeviceMapping> bacNetDeviceMappingLst)
        {
            if (bacNetDeviceMappingLst.Count() > 0)
            {
                foreach (var bacNetDeviceMap in bacNetDeviceMappingLst)
                {
                    dbContext.BACnetDeviceMappings.Add(bacNetDeviceMap);
                }
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets alaram enrollement value.
        /// </summary>
        /// <returns>Alaram enrollement value.</returns>
        public int? GetAlaramEnrollment()
        {
            var objName = Enum.GetName(typeof(LutronFloorObjectType), LutronFloorObjectType.OBJECT_ALERT_ENROLLMENT).ToString();
            var alaramEnrollment = dbContext.BACnetDevices.Where(x => x.device_id == 1
                                                && x.object_type.ToUpper() == objName)
                                           .Select(x => x.object_instance).FirstOrDefault();
            return alaramEnrollment;
        }

        /// <summary>
        /// Gets Floor Detail.
        /// </summary>
        /// <returns>Floor List.</returns>
        public List<FloorEntity> GetsFloor()
        {
            var floorLst = dbContext.MsFloorTypes.Select(x => new FloorEntity
            {
                FloorId = x.FloorId,
                FloorName = x.FloorName
            }).Where(y => y.isActive == true && y.isDeleted == false).ToList();
            return floorLst;
        }

        /// <summary>
        /// Gets Weeks name.
        /// </summary>
        /// <returns>List of Week name</returns>
        public List<WeeksEntity> GetsWeeks()
        {
            var weekLst = (from mw in dbContext.MsWeeks
                           where mw.isActive == true && mw.isDeleted == false
                           select new WeeksEntity
                           {
                               DayName = mw.DayName,
                               DayId = mw.DayId
                           }).ToList();
            return weekLst;
        }

        /// <summary>
        /// Gets bacnet device detail by floor id.
        /// </summary>
        /// <param name="floorId">Passes floor id.</param>
        /// <returns>List of BacNet device detail.</returns>
        public List<BackNetDeviceDetailEntity> GetBacNetDeviceDetailByFloor(int floorId)
        {
            var objTypeName = Enum.GetName(typeof(LutronFloorObjectType), LutronFloorObjectType.OBJECT_ANALOG_VALUE).ToString();
            var bacNetDevicelst = (from bdm in dbContext.BACnetDeviceMappings
                                   join bd in dbContext.BACnetDevices on
                                   bdm.device_id equals bd.device_id
                                   where bd.object_type.ToUpper() == objTypeName
                                   && bdm.floor_id == floorId && bd.object_instance != (int)ObjectInstanceType.AlarmEventAV7
                                   select new BackNetDeviceDetailEntity
                                   {
                                       device_id = bd.device_id,
                                       object_instance = bd.object_instance,
                                       network_id = bd.network_id,
                                       object_name = bd.object_name,
                                       routed_source = bd.routed_source,
                                       routed_net = bd.routed_net

                                   }).Distinct().ToList();

            return bacNetDevicelst;
        }

        /// <summary>
        /// Gets bacnet device detail base on floor id.
        /// </summary>
        /// <param name="flooId">Passes floor id.</param>
        /// <returns>devoce detail.</returns>
        public BackNetDeviceDetailEntity GetBackNetDeviceDetailToReadPropertyByFloor(int flooId)
        {

            var objTypeName = Enum.GetName(typeof(LutronFloorObjectType), LutronFloorObjectType.OBJECT_ANALOG_VALUE).ToString();
            var bacnetDeviceDetail = (from bcm in dbContext.BACnetDeviceMappings
                                      join bd in dbContext.BACnetDevices on bcm.device_id equals bd.device_id
                                      where bcm.floor_id == flooId
                                      && bd.object_type.ToUpper() == objTypeName
                                      select new BackNetDeviceDetailEntity
                                      {
                                          network_id = bd.network_id,
                                          device_id = bd.device_id,
                                          object_instance = bd.object_instance,
                                          object_name = bd.object_name,
                                          routed_source = bd.routed_source,
                                          routed_net = bd.routed_net
                                      }).Distinct().FirstOrDefault();
            return bacnetDeviceDetail;
        }

        /// <summary>
        /// Gets bacnet device detail base on device id.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <returns>Bacnet device detail.</returns>
        public BACnetDevice GetsCurrentBinaryValue(int deviceId)
        {
            var objName = Enum.GetName(typeof(LutronFloorObjectType), LutronFloorObjectType.OBJECT_BINARY_VALUE).ToString();
            var bacnetDeviceDetail = dbContext.BACnetDevices
                                        .Where(x => x.device_id == deviceId
                                              && x.object_type.ToUpper() == objName)
                                         .Select(x => x).FirstOrDefault();
            return bacnetDeviceDetail;
        }

        /// <summary>
        /// Gets bacnet device detail by light state.
        /// </summary>
        /// <param name="deviceId">Passes deviceid.</param>
        /// <returns>Device detail.</returns>
        public BACnetDevice GetDeviceDetailByLightState(int deviceId)
        {
            var bacnetDeviceDetail = dbContext.BACnetDevices
                                        .Where(x => x.device_id == deviceId
                                              && x.object_instance == (int?)LutronObjectType.Lighting_State)
                                         .Select(x => x).FirstOrDefault();
            return bacnetDeviceDetail;
        }


        /// <summary>
        /// Gets bacnet device detail base on device object type.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <returns>Bacnet device detail.</returns>
        public BACnetDevice GetBacNetDeviceObjDetail(int deviceId)
        {
            var objName = Enum.GetName(typeof(LutronFloorObjectType), LutronFloorObjectType.OBJECT_DEVICE).ToString();
            var bacnetDeviceDetail = dbContext.BACnetDevices
                           .Where(x => x.device_id == deviceId
                                 && x.object_type.ToUpper() == objName)
                           .Select(x => x).FirstOrDefault();
            return bacnetDeviceDetail;
        }

        /// <summary>
        /// Gets bacnet instance id base on schedule object type.
        /// </summary>
        /// <param name="deviceId">Passes deviceId</param>
        /// <returns>Instance id.</returns>
        public int? GetBacNetSceduleObjDetail(int deviceId)
        {
            var objName = Enum.GetName(typeof(LutronFloorObjectType), LutronFloorObjectType.OBJECT_SCHEDULE).ToString();
            var instanceId = dbContext.BACnetDevices
                           .Where(x => x.device_id == deviceId
                                 && x.object_type.ToUpper() == objName)
                           .Select(x => x.object_instance).OrderByDescending(y=>y.Value).FirstOrDefault();
            return instanceId;
        }


        /// <summary>
        /// Gets device list.
        /// </summary>
        /// <returns>Device List</returns>
        public List<DeviceEntity> GetsDevices()
        {
            var objName = Enum.GetName(typeof(LutronFloorObjectType), LutronFloorObjectType.OBJECT_DEVICE).ToString();
            var deviceLst = dbContext.BACnetDevices.Where(x => x.object_type == objName)
                               .Select(x => new DeviceEntity
                               {
                                   DeviceName = x.object_name,
                                   DeviceId = x.device_id
                               }).Distinct().ToList();

            return deviceLst;
        }

        /// <summary>
        /// Gets device object names base on device id.
        /// </summary>
        /// <param name="deviceId">Passes deviceId.</param>
        /// <returns>Schedule object.</returns>
        public List<ScheduleObjectListEntity> GetsScheduleObjectList(int deviceId)
        {
            var objLightLevel = Enum.GetName(typeof(LutronFloorObjectType), LutronFloorObjectType.OBJECT_ANALOG_VALUE).ToString();
            var objLightStateLevel = Enum.GetName(typeof(LutronFloorObjectType), LutronFloorObjectType.OBJECT_BINARY_VALUE).ToString();
            var scheduleObjList = (from bcd in dbContext.BACnetDevices
                                   where bcd.device_id == deviceId && (bcd.object_type == objLightLevel || bcd.object_type == objLightStateLevel)
                                   select new ScheduleObjectListEntity
                                   {
                                       ObjectInstance = bcd.object_instance,
                                       ObjectName = bcd.object_name,
                                       ObjectType = bcd.object_type
                                   }).Distinct().ToList();
            return scheduleObjList;
        }

        public List<BACnetDevice> GetsLstCurrentBinaryValue(int deviceId)
        {
            var objName = Enum.GetName(typeof(LutronFloorObjectType), LutronFloorObjectType.OBJECT_BINARY_VALUE).ToString();
            var bacnetDeviceDetail = dbContext.BACnetDevices
                                        .Where(x => x.device_id == deviceId
                                              && x.object_type.ToUpper() == objName)
                                         .Select(x => x).Distinct().ToList();
            return bacnetDeviceDetail;
        }


        public BACnetDevice GetsCurrentBinaryValueByInstance(int deviceId,int instanceId)
        {
            var objName = Enum.GetName(typeof(LutronFloorObjectType), LutronFloorObjectType.OBJECT_BINARY_VALUE).ToString();
            var bacnetDeviceDetail = dbContext.BACnetDevices
                                        .Where(x => x.device_id == deviceId && x.object_instance == instanceId
                                              && x.object_type.ToUpper() == objName)
                                         .Select(x => x).FirstOrDefault();
            return bacnetDeviceDetail;
        }


        public bool CheckIfExistNetworkAddress(string networkIp)
        {
            var isExist = dbContext.BACnetDevices.Where(x => x.network_id == networkIp).Any();
            return isExist;
        }
    }
}