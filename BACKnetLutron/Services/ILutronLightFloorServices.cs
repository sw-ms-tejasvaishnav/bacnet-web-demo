using BACKnetLutron.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKnetLutron.Services
{
    public interface ILutronLightFloorServices
    {

        void StartBacnetServerActivity();

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
        /// Saves bacnet presant value.
        /// </summary>
        /// <param name="bacnetValue">Passses bacnet presant value.</param>
        /// <param name="floorId">Passes selected floor id.</param>
        void SaveBackNetPresantValue(int bacnetValue,int floorId);

        /// <summary>
        /// Gets Light level base on floor id.
        /// </summary>
        /// <param name="floorId">Passes floor id.</param>
        /// <returns>Current light level.</returns>
        string GetsLutronLightLevelByFloor(int floorId);

        /// <summary>
        /// Sets binary value in simulator.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <returns>Status of binary presunt value.</returns>
        bool SetLightSimulator(int deviceId,int instanceId);

        /// <summary>
        /// Gets current binary presant value.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <returns>Status</returns>
        bool CurrentBinaryPresantValue(int deviceId);

        /// <summary>
        /// Saves schedule detail.
        /// </summary>
        /// <param name="scheduleDetail">Passes schedule detail.</param>
        void SaveSchedule(ScheduleEntity scheduleDetail);

        /// <summary>
        /// Gets device list.
        /// </summary>
        /// <returns>Device List</returns>
        List<DeviceEntity> GetDevices();

        /// <summary>
        /// Gets device object names base on device id.
        /// </summary>
        /// <param name="deviceId">Passes deviceId.</param>
        /// <returns>Schedule object.</returns>
        List<ScheduleObjectListEntity> GetsScheduleObjectList(int deviceId);


    }
}
