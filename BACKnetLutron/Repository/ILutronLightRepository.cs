﻿using BACKnetLutron.BusinessEntities;
using BACKnetLutron.DataModal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKnetLutron.Repository
{
    public interface ILutronLightRepository
    {


        /// <summary>
        /// Adds bacnet device details on start process.
        /// </summary>
        /// <param name="bacNetDeviceLst">Passes list of devices.</param>
        void AddBacNetDeviceDetail(List<BACnetDevice> bacNetDeviceLst);

        /// <summary>
        /// Adds bacnet device mappling details on start process.
        /// </summary>
        /// <param name="bacNetDeviceMappingLst">Passes bacnet device mapping list.</param>
        void AddBacNetMappingDetail(List<BACnetDeviceMapping> bacNetDeviceMappingLst);


        /// <summary>
        /// Gets alaram enrollement value.
        /// </summary>
        /// <returns>Alaram enrollement value.</returns>
        int? GetAlaramEnrollment();

        /// <summary>
        /// Gets Floor Detail.
        /// </summary>
        /// <returns>Floor List.</returns>
        List<FloorEntity> GetsFloor();

        /// <summary>
        /// Gets Weeks name.
        /// </summary>
        /// <returns>List of Week name</returns>
        List<WeeksEntity> GetsWeeks();

        /// <summary>
        /// Gets bacnet device detail by floor id.
        /// </summary>
        /// <param name="floorId">Passes floor id.</param>
        /// <returns>List of BacNet device detail.</returns>
        List<BackNetDeviceDetailEntity> GetBacNetDeviceDetailByFloor(int floorId);

        /// <summary>
        /// Gets bacnet device detail base on floor id.
        /// </summary>
        /// <param name="flooId">Passes floor id.</param>
        /// <returns>devoce detail.</returns>
        BackNetDeviceDetailEntity GetBackNetDeviceDetailToReadPropertyByFloor(int flooId);

        /// <summary>
        /// Gets bacnet device detail base on device id.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <returns>Bacnet device detail.</returns>
        BACnetDevice GetsCurrentBinaryValue(int deviceId);

        /// <summary>
        /// Gets bacnet device detail by light state.
        /// </summary>
        /// <param name="deviceId">Passes deviceid.</param>
        /// <returns>Device detail.</returns>
        BACnetDevice GetDeviceDetailByLightState(int deviceId);

        /// <summary>
        /// Gets bacnet device detail base on device object type.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <returns>Bacnet device detail.</returns>
        BACnetDevice GetBacNetDeviceObjDetail(int deviceId);

        /// <summary>
        /// Gets bacnet instance id base on schedule object type.
        /// </summary>
        /// <param name="deviceId">Passes deviceId</param>
        /// <returns>Instance id.</returns>
        int? GetBacNetSceduleObjDetail(int deviceId);

        /// <summary>
        /// Gets device list.
        /// </summary>
        /// <returns>Device List</returns>
        List<DeviceEntity> GetsDevices();

        /// <summary>
        /// Gets device object names base on device id.
        /// </summary>
        /// <param name="deviceId">Passes deviceId.</param>
        /// <returns>Schedule object.</returns>
        List<ScheduleObjectListEntity> GetsScheduleObjectList(int deviceId);


        /// <summary>
        /// Gets current binary value by device id.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <returns></returns>
        List<BACnetDevice> GetsLstCurrentBinaryValue(int deviceId);

        /// <summary>
        /// Gets cuurent binary present value bi instance.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <param name="instanceId">Passes instance id.</param>
        /// <returns></returns>
        BACnetDevice GetsCurrentBinaryValueByInstance(int deviceId, int instanceId);

        /// <summary>
        /// Chedk if current network address is exist or not in database.
        /// </summary>
        /// <param name="networkIp">Paasses network Id.</param>
        /// <param name="instanceId">Passes Instance Id.</param>
        /// <param name="deviceId">Passes device id.</param>
        /// <param name="objType">Passes object type.</param>
        /// <returns></returns>
        bool CheckIfExistNetworkAddress(string networkIp, int instanceId, int deviceId, string objType);

        /// <summary>
        /// Save new schedule detail.
        /// </summary>
        /// <param name="scheduleInfo">Passes schedule detail.</param>
        void SaveScheduleDetail(ScheduleDetail scheduleInfo);

        /// <summary>
        /// Gets all schedule list and related weekly schedule.
        /// </summary>
        /// <returns>Schedule list.</returns>
        List<ScheduleEntity> ScheduleList();

        /// <summary>
        /// Gets schedule detail.
        /// </summary>
        /// <param name="deviceId">Passes deviceId.</param>
        /// <param name="instanceId">Passes instance id.</param>
        /// <returns></returns>
        BACnetDevice GetScheduleDeviceDetail(int deviceId, int instanceId);

        /// <summary>
        /// Gets schedule detail by device and instance id.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <param name="instanceId">Passes instance id.</param>
        /// <returns>Schedule detail</returns>
        ScheduleEntity GetsScheduleDetail(int deviceId, int instanceId);

        /// <summary>
        /// Gets schedule detail by schedule id.
        /// </summary>
        /// <param name="scheduleDetailId">Passes schedule detail id.</param>
        /// <returns>Schedule detail.</returns>
        ScheduleDetail GetScheduleDetailById(int scheduleDetailId);


        /// <summary>
        /// Update weekly schedule detail.
        /// </summary>
        /// <param name="scheduleInfo">Passes weekly schedule detail.</param>
        void UpdateScheduleDetail(ScheduleDetail scheduleInfo);

        /// <summary>
        /// Gets all weekly schedule detail by schedule detail id.
        /// </summary>
        /// <param name="scheduleDetailId">Passes schedule detail.</param>
        /// <returns></returns>
        List<WeeklyScheduleEntity> GetWeeklyScheduleDetailById(int scheduleDetailId);
    }

}
