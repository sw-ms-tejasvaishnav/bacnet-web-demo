using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO.BACnet;
using BACKnetLutron.BusinessEntities;
using System.Threading;
using BACKnetLutron.DataModal;
using BACKnetLutron.Repository;
using System.IO.BACnet.Serialize;
using BACKnetLutron.BusinessEntities.Common_Constant;

namespace BACKnetLutron.Services
{
    public class LutronLightServices : ILutronLightFloorServices
    {
        #region Global Objects

        static BacnetClient bacNetClient;
        static BackNetDeviceModel bacNetDeviceModel = new BackNetDeviceModel();
        private ILutronLightRepository _LutronLightRepository;
        #endregion

        #region Counstructor
        public LutronLightServices(ILutronLightRepository lutronLightRepository)
        {
            this._LutronLightRepository = lutronLightRepository;
        }

        #endregion

        #region Services

        public void StartBacnetServerActivity()
        {
            //Start bacnet 
            StartBackNetService();
            Thread.Sleep(1000);
            AddBackNetDeviceDetail();
            bacNetClient.Dispose();

            //// Bacnet on UDP/IP/Ethernet
            bacNetClient = new BacnetClient(new BacnetIpUdpProtocolTransport(47808, false));// (0xBAC0, false));
            //  bacNetClient.OnWhoIs += new BacnetClient.WhoIsHandler(handler_OnWhoIs);
            bacNetClient.Start();    // go
            bacNetClient.WhoIs();

        }

        /// <summary>
        /// Gets Floor Detail.
        /// </summary>
        /// <returns>Floor List.</returns>
        public List<FloorEntity> GetsFloor()
        {
            var floorLst = GetsFloorWithInstance();// _LutronLightRepository.GetsFloor();

            return floorLst;
        }

        /// <summary>
        /// Gets Weeks name.
        /// </summary>
        /// <returns>List of Week name</returns>
        public List<WeeksEntity> GetsWeeks()
        {
            var weekLst = _LutronLightRepository.GetsWeeks();
            return weekLst;
        }

        /// <summary>
        /// Saves bacnet presant value.
        /// </summary>
        /// <param name="bacnetValue">Passses bacnet presant value.</param>
        /// <param name="floorId">Passes selected floor id.</param>
        public void SaveBackNetPresantValue(int bacnetValue, int floorId)
        {
            var bacnetdeviceLst = _LutronLightRepository.GetBacNetDeviceDetailByFloor(floorId);
            if (bacnetdeviceLst != null)
            {
                foreach (var deviceDetail in bacnetdeviceLst)
                {
                    BacnetAddress bacnetAddress;
                    bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, deviceDetail.network_id);
                    bacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, deviceDetail.routed_source, 
                        (ushort)deviceDetail.routed_net);

                    BacnetValue newBacnetValue = new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL,
                        Convert.ToSingle(bacnetValue));
                    BacnetValue[] writeNewBacnetValueValue = { newBacnetValue };

                    //Use to write property in simulator.
                    bacNetClient.WritePropertyRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE,
                        (uint)deviceDetail.object_instance), BacnetPropertyIds.PROP_PRESENT_VALUE, writeNewBacnetValueValue);
                }
            }
        }

        /// <summary>
        /// Gets Light level base on floor id.
        /// </summary>
        /// <param name="floorId">Passes floor id.</param>
        /// <returns>Current light level.</returns>
        public string GetsLutronLightLevelByFloor(int floorId)
        {
            var bacNetDeviceDetail = _LutronLightRepository.GetBackNetDeviceDetailToReadPropertyByFloor(floorId);
            IList<BacnetValue> loBacnetValueList;
            BacnetAddress loBacnetAddress;
            loBacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, bacNetDeviceDetail.network_id);

            loBacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, bacNetDeviceDetail.routed_source,
                (ushort)bacNetDeviceDetail.routed_net);

            bacNetClient.ReadPropertyRequest(loBacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE,
                (uint)bacNetDeviceDetail.object_instance), BacnetPropertyIds.PROP_PRESENT_VALUE, out loBacnetValueList);



            if (loBacnetValueList != null && loBacnetValueList.Count > 0)
            {
                return loBacnetValueList.FirstOrDefault().Value.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Sets binary value in simulator.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <returns>Status of binary presunt value.</returns>
        public bool SetLightSimulator(int deviceId, int instanceId)
        {

            var bacnetDeviceDetail = _LutronLightRepository.GetsCurrentBinaryValueByInstance(deviceId, instanceId);
          
            bool currentBinaryValue = ReadCurrentBinaryPresantValue(bacnetDeviceDetail);

            bool binaryValueStatus = currentBinaryValue == true ? false : true;
        
            TurnOnLight(deviceId, binaryValueStatus);

            return binaryValueStatus;
        }

        /// <summary>
        /// Gets current binary presant value.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <returns>Status</returns>
        public bool CurrentBinaryPresantValue(int deviceId)
        {
            var bacnetDeviceDetail = _LutronLightRepository.GetsCurrentBinaryValue(deviceId);
            bool currentBinaryValue = ReadCurrentBinaryPresantValue(bacnetDeviceDetail);
            return currentBinaryValue;
        }

        /// <summary>
        /// Gets device list.
        /// </summary>
        /// <returns>Device List</returns>
        public List<DeviceEntity> GetDevices()
        {
            var deviceLst = _LutronLightRepository.GetsDevices();
            return deviceLst;
        }

        /// <summary>
        /// Gets device object names base on device id.
        /// </summary>
        /// <param name="deviceId">Passes deviceId.</param>
        /// <returns>Schedule object.</returns>
        public List<ScheduleObjectListEntity> GetsScheduleObjectList(int deviceId)
        {
            var scheduleObjLst = _LutronLightRepository.GetsScheduleObjectList(deviceId);
            return scheduleObjLst;
        }

        /// <summary>
        /// Saves schedule detail.
        /// </summary>
        /// <param name="scheduleDetail">Passes schedule detail.</param>
        public void SaveSchedule(ScheduleEntity scheduleDetail)
        {
            if (scheduleDetail != null)
            {
                var bacnetDeviceobjDetail = _LutronLightRepository.GetBacNetDeviceObjDetail(scheduleDetail.DeviceId);
                var firstInstanceId = _LutronLightRepository.GetBacNetSceduleObjDetail(scheduleDetail.DeviceId);
                BacnetAddress bacnetAddress;
                bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceobjDetail.network_id);
                bacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceobjDetail.routed_source,
                    (ushort)bacnetDeviceobjDetail.routed_net);


                IList<BacnetValue> loScheduleValues;
                bacNetClient.ReadPropertyRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_SCHEDULE,
                    (uint)1), BacnetPropertyIds.PROP_EXCEPTION_SCHEDULE, out loScheduleValues);


                ICollection<BacnetPropertyValue> loBacnetPropertyValueList = new List<BacnetPropertyValue>();

                BacnetPropertyValue loNewPropertyValue = new BacnetPropertyValue();
                List<BacnetValue> loBacnetValue = new List<BacnetValue>();

                #region Set Schedule
                ////    Create new instance id based on largest available
                //if(liTopInstanceID)
                firstInstanceId = firstInstanceId != null ? firstInstanceId + 1 : 1;

                loBacnetPropertyValueList = SetScheduleDetail(scheduleDetail, loBacnetPropertyValueList, firstInstanceId,
                    loBacnetValue, loNewPropertyValue);
                #endregion

                #region Update Schedule Object
                //// Set object reference to update it's value with schedule object
                var updateScheduleObjDetail = UpdateScheduleObject(scheduleDetail);
                loBacnetPropertyValueList.Add(updateScheduleObjDetail);
                #endregion

                #region Adds Weekly Schedule                              
                //// Add weekly schedule for object
                var scheduleWeekalyValue = AddScheduleWeeklyDetail(scheduleDetail);
                loBacnetPropertyValueList.Add(scheduleWeekalyValue);
                #endregion

                #region Adds WeeklyExpection Detail.
                var weeklyExpScheduleValue = AddWeeklyExpectionSchedule(scheduleDetail);
                loBacnetPropertyValueList.Add(weeklyExpScheduleValue);
                #endregion


                bacNetClient.CreateObjectRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_SCHEDULE,
                    (uint)firstInstanceId), loBacnetPropertyValueList);
            }
        }

        #endregion

        #region BacnetMethods

        /// <summary>
        /// Start Bacnet protocol.
        /// </summary>
        private void StartBackNetService()
        {
            
            if (bacNetClient == null)
            {
                bacNetClient = new BacnetClient(new BacnetIpUdpProtocolTransport(0xBAC0, false));

                bacNetClient.Dispose();
                bacNetClient.Start();
            }
            bacNetClient.OnIam += new BacnetClient.IamHandler(Handler_OmIam);
            bacNetClient.WhoIs();
        }

        /// <summary>
        /// Gets available device and create global list
        /// </summary>
        /// <param name="bacNetClient">Passes bacNetClient detail</param>
        /// <param name="bacNetAddress">Passes Address</param>
        /// <param name="deviceId">Passes device id.</param>
        /// <param name="maxPLoad">Passes maxpayload.</param>
        /// <param name="segmentation"></param>
        /// <param name="vendorId"></param>
        static void Handler_OmIam(BacnetClient bacNetClient, BacnetAddress bacNetAddress, uint deviceId, uint maxPLoad,
            BacnetSegmentations segmentation, ushort vendorId)
        {
            if (bacNetDeviceModel != null)
            {
                //// OnIam get current device and add into list to process bunch of device in DBs
                lock (bacNetDeviceModel.BACnetDeviceList)
                {

                    if (!bacNetDeviceModel.BACnetDeviceList.Any(x => x.DeviceId == deviceId))
                    {
                        //// Not already in the list

                        bacNetDeviceModel.BACnetDeviceList.Add(new BackNetDeviceEntity
                        {
                            BacNetAddress = bacNetAddress,
                            DeviceId = deviceId,
                            InstanceId = 0
                        });   //// add it
                    }
                }
            }
        }

        /// <summary>
        /// Adds bacnet device details.
        /// </summary>
        private void AddBackNetDeviceDetail()
        {
            if (bacNetDeviceModel != null && bacNetDeviceModel.BACnetDeviceList != null)
            {
                List<BACnetDevice> bACnetDeviceLst = new List<BACnetDevice>();
                List<BACnetDeviceMapping> bACnetDeviceMappingLst = new List<BACnetDeviceMapping>();
                foreach (var deviceDetail in bacNetDeviceModel.BACnetDeviceList)
                {
                    IList<BacnetValue> objValueLst;
                    bacNetClient.ReadPropertyRequest(deviceDetail.BacNetAddress,
                        new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, deviceDetail.DeviceId),
                        BacnetPropertyIds.PROP_OBJECT_LIST, out objValueLst);
                    foreach (var objValue in objValueLst)
                    {
                        var isExistNetworkId = _LutronLightRepository.CheckIfExistNetworkAddress(deviceDetail.BacNetAddress.ToString());
                        if(isExistNetworkId == true)
                        {
                            continue;
                        }
                        IList<BacnetValue> objNameList;
                        bacNetClient.ReadPropertyRequest(deviceDetail.BacNetAddress,
                            new BacnetObjectId((BacnetObjectTypes)((BacnetObjectId)objValue.Value).Type, 
                            ((BacnetObjectId)objValue.Value).Instance),
                            BacnetPropertyIds.PROP_OBJECT_NAME, out objNameList);
                        var bacNetdevice = new BACnetDevice
                        {
                            device_id = Convert.ToInt32(deviceDetail.DeviceId),
                            network_id = deviceDetail.BacNetAddress.ToString(),
                            object_type = ((BacnetObjectId)objValue.Value).type.ToString(),
                            object_instance = Convert.ToInt32(((BacnetObjectId)objValue.Value).Instance.ToString()),
                            object_name = objNameList != null && objNameList.Count > 0 ? objNameList[0].Value.ToString() : null,
                            routed_source = deviceDetail.BacNetAddress.RoutedSource.ToString(),
                            routed_net = deviceDetail.BacNetAddress.RoutedSource.net
                        };
                        bACnetDeviceLst.Add(bacNetdevice);

                        int? suiteID = null, roomID = null;
                        var objName = Enum.GetName(typeof(LutronFloorObjectType), LutronFloorObjectType.OBJECT_ANALOG_VALUE).ToString();
                        if (((BacnetObjectId)objValue.Value).type.ToString().ToUpper() == objName)
                        {
                            if (Convert.ToInt32(((BacnetObjectId)objValue.Value).Instance.ToString()) < 4)
                            {
                                suiteID = 1;
                            }
                            else
                            {
                                suiteID = 2;
                            }

                            roomID = Convert.ToInt32(((BacnetObjectId)objValue.Value).Instance.ToString());
                        }
                        var bACnetDeviceMapping = new BACnetDeviceMapping
                        {
                            device_id = Convert.ToInt32(deviceDetail.DeviceId),
                            floor_id = Convert.ToInt32(deviceDetail.DeviceId),
                            suite_id = suiteID,
                            room_id = roomID,
                            object_instance = Convert.ToInt32(((BacnetObjectId)objValue.Value).Instance.ToString())
                        };
                        bACnetDeviceMappingLst.Add(bACnetDeviceMapping);
                    }

                }
            
                if (bACnetDeviceLst.Count() > 0)
                {
                    _LutronLightRepository.AddBacNetDeviceDetail(bACnetDeviceLst);
                }
                if (bACnetDeviceMappingLst.Count() > 0)
                {
                    _LutronLightRepository.AddBacNetMappingDetail(bACnetDeviceMappingLst);
                }
            }
        }

        /// <summary>
        /// handler to assign alarm enrolment instance on event notification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="adr"></param>
        /// <param name="low_limit"></param>
        /// <param name="high_limit"></param>
        public void handler_OnWhoIs(BacnetClient sender, BacnetAddress adr, int low_limit, int high_limit)
        {
            if (low_limit != -1 && 1 < low_limit)
                return;
            else if (high_limit != -1 && 1 > high_limit)
                return;

            int? alaramEnrollmentVal = _LutronLightRepository.GetAlaramEnrollment();
            if (alaramEnrollmentVal != null)
            {
                sender.Iam((uint)alaramEnrollmentVal, new BacnetSegmentations());
            }
        }

        /// <summary>
        /// Sets binary value according to device.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <param name="binaryValueStatus">Passes Binary value status.</param>
        public void TurnOnLight(int deviceId, bool binaryValueStatus)
        {
            var bacNetDeviceDetail = _LutronLightRepository.GetDeviceDetailByLightState(deviceId);

            if (bacNetDeviceDetail != null && bacNetDeviceDetail.object_instance != null)
            {
                BacnetAddress bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, bacNetDeviceDetail.network_id);
                bacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, bacNetDeviceDetail.routed_source,
                    (ushort)bacNetDeviceDetail.routed_net);

                //IList<BacnetValue> loBacnetValueList;
                //moBacnetClient.ReadPropertyRequest(loBacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_BINARY_VALUE, (uint)loBACnetDeviceDetail.object_instance), BacnetPropertyIds.PROP_PRESENT_VALUE, out loBacnetValueList);

                BacnetValue bacNetValue = new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED,
                    binaryValueStatus == true ? 1 : 0);
                BacnetValue[] newValue = { bacNetValue };

                bacNetClient.WritePropertyRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_BINARY_VALUE,
                    (uint)bacNetDeviceDetail.object_instance), BacnetPropertyIds.PROP_PRESENT_VALUE, newValue);
            }
        }


        /// <summary>
        /// Sets schedule detail.
        /// </summary>
        /// <param name="scheduleDetail">Schedule info.</param>
        /// <param name="bacnetPropertyValueList">Bacnet property list.</param>
        /// <param name="firstInstanceId">Current instance id.</param>
        /// <param name="bacnetValue">List of backnet value.</param>
        /// <param name="newPropertyValue">Set propert value.</param>
        /// <returns>Updated schedule backnet property list.</returns>
        private ICollection<BacnetPropertyValue> SetScheduleDetail(ScheduleEntity scheduleDetail,
                ICollection<BacnetPropertyValue> bacnetPropertyValueList, int? firstInstanceId,
                List<BacnetValue> bacnetValue, BacnetPropertyValue newPropertyValue)
        {
           

            //// Set schedule object name
            bacnetValue = new List<BacnetValue>();
            newPropertyValue = new BacnetPropertyValue();
            bacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_CHARACTER_STRING, "Schedule" + " " + firstInstanceId));

            newPropertyValue.value = bacnetValue;
            newPropertyValue.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_OBJECT_NAME, ASN1.BACNET_ARRAY_ALL);

            bacnetPropertyValueList.Add(newPropertyValue);


            //// Set effective period for schedule object
            bacnetValue = new List<BacnetValue>();
            newPropertyValue = new BacnetPropertyValue();
            bacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE, DateTime.Today.AddDays(-1)));
            bacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE, DateTime.Today.AddMonths(1)));

            newPropertyValue.value = bacnetValue;
            newPropertyValue.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_EFFECTIVE_PERIOD, ASN1.BACNET_ARRAY_ALL);
            bacnetPropertyValueList.Add(newPropertyValue);

            return bacnetPropertyValueList;
        }

        /// <summary>
        /// Updated schedule object detail.
        /// </summary>
        /// <param name="scheduleDetail">Passe schedule info.</param>
        /// <returns>Updated schedule backnet property list.</returns>
        private BacnetPropertyValue UpdateScheduleObject(ScheduleEntity scheduleDetail)
        {

            var loBacnetValue = new List<BacnetValue>();
            var loNewPropertyValue = new BacnetPropertyValue();

            BacnetDeviceObjectPropertyReference loPropertyReference = new BacnetDeviceObjectPropertyReference();


            //BACnetCalendarEntry loBACnetCalendarEntry = new BACnetCalendarEntry();


            //// Add AV 1 present value change
            loPropertyReference.ArrayIndex = -1;
            loPropertyReference.DeviceId = new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, (uint)scheduleDetail.DeviceId);
            loPropertyReference.ObjectId = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, (uint)scheduleDetail.ScheduleObjectid);
            loPropertyReference.PropertyId = BacnetPropertyIds.PROP_PRESENT_VALUE;

            loBacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_PROPERTY_REFERENCE,
                              loPropertyReference));

            // Add AV 2 present value change
            loPropertyReference = new BacnetDeviceObjectPropertyReference();
            loPropertyReference.ArrayIndex = -1;
            loPropertyReference.DeviceId = new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, (uint)scheduleDetail.DeviceId);
            loPropertyReference.ObjectId = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, (uint)2);
            loPropertyReference.PropertyId = BacnetPropertyIds.PROP_PRESENT_VALUE;

            loBacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_PROPERTY_REFERENCE,
                              loPropertyReference));

            loNewPropertyValue.value = loBacnetValue;
            loNewPropertyValue.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_LIST_OF_OBJECT_PROPERTY_REFERENCES, ASN1.BACNET_ARRAY_ALL);
            return loNewPropertyValue;
        }

        /// <summary>
        /// Updated schedule weekly object detail.
        /// </summary>
        /// <param name="scheduleDetail">Passes schedule info.</param>
        /// <returns>Updated schedule weekly backnet property list.</returns>
        private BacnetPropertyValue AddScheduleWeeklyDetail(ScheduleEntity scheduleDetail)
        {
            var loBacnetValue = new List<BacnetValue>();
            var loNewPropertyValue = new BacnetPropertyValue();

            //// setup weekday to add schedule (0 = Monday, 1 = Tuesday ....., 6 = Sunday)
            BacnetWeeklySchedule loBacnetWeeklySchedule = new BacnetWeeklySchedule();
            loBacnetWeeklySchedule.days[scheduleDetail.SelectedDay] = new List<DaySchedule>();

            //// Schedule to update AV 1,2 at specified time
            loBacnetWeeklySchedule.days[scheduleDetail.SelectedDay].Add(
                new DaySchedule(new DateTime(1, 1, 1, Convert.ToInt32(scheduleDetail.SelectedTime.Hour),
                Convert.ToInt32(scheduleDetail.SelectedTime.Minute), Convert.ToInt32(scheduleDetail.SelectedTime.Second)),
                  Convert.ToSingle((new Random()).Next(100, 999))));

            //// Schedule to update AV 1,2 after 5 mins of schedule
            loBacnetWeeklySchedule.days[scheduleDetail.SelectedDay].Add(new DaySchedule(DateTime.Now.AddMinutes(1),
                Convert.ToSingle(scheduleDetail.SchedulePresantValue)));


            loBacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_WEEKLY_SCHEDULE, loBacnetWeeklySchedule));

            loNewPropertyValue.value = loBacnetValue;
            loNewPropertyValue.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_WEEKLY_SCHEDULE, ASN1.BACNET_ARRAY_ALL);

            return loNewPropertyValue;
        }

        /// <summary>
        /// Updated schedule weekly expection object detail.
        /// </summary>
        /// <param name="scheduleDetail">Passe schedule info.</param>
        /// <returns>Updated schedule weekly expection backnet property list.</returns>
        private BacnetPropertyValue AddWeeklyExpectionSchedule(ScheduleEntity scheduleDetail)
        {
            var loBacnetValue = new List<BacnetValue>();
            var loNewPropertyValue = new BacnetPropertyValue();

            BacnetWeeklyExceptionSchedule loBacnetWeeklyExceptionSchedule = new BacnetWeeklyExceptionSchedule();

            List<ExceptionScheduleTimeValue>[] loExceptionSchedulTimeValue = new List<ExceptionScheduleTimeValue>[1];
            loExceptionSchedulTimeValue[0] = new List<ExceptionScheduleTimeValue>();
            loExceptionSchedulTimeValue[0].Add(new ExceptionScheduleTimeValue(DateTime.Now.AddMinutes(2), Convert.ToSingle(111)));


            loBacnetWeeklyExceptionSchedule.loExceptionScheduleArray = new List<ExceptionScheduleArray>[1];
            loBacnetWeeklyExceptionSchedule.loExceptionScheduleArray[0] = new List<ExceptionScheduleArray>();
            loBacnetWeeklyExceptionSchedule.loExceptionScheduleArray[0].Add(new ExceptionScheduleArray(new DateTime(1, 1, 1, 18, 30, 1),
                loExceptionSchedulTimeValue));

            loBacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_SPECIAL_EVENT, loBacnetWeeklyExceptionSchedule));

            loNewPropertyValue.value = loBacnetValue;
            loNewPropertyValue.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_EXCEPTION_SCHEDULE, ASN1.BACNET_ARRAY_ALL);

            return loNewPropertyValue;
        }


        #endregion

        #region Private Functions

        /// <summary>
        /// Gets floor device list with instance count.
        /// </summary>
        /// <returns></returns>
        private List<FloorEntity> GetsFloorWithInstance()
        {
            var floorLst = new List<FloorEntity>();
            foreach (var deviceDetail in bacNetDeviceModel.BACnetDeviceList)
            {

                IList<BacnetValue> objValueLst;
                IList<BacnetValue> objBinaryLst;
                bacNetClient.ReadPropertyRequest(deviceDetail.BacNetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE,
                    deviceDetail.DeviceId),
                    BacnetPropertyIds.PROP_OBJECT_LIST, out objValueLst);
                var binaryDeviceDetail = _LutronLightRepository.GetsCurrentBinaryValue((int)deviceDetail.DeviceId);
                bacNetClient.ReadPropertyRequest(deviceDetail.BacNetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_BINARY_VALUE,
                    (uint)binaryDeviceDetail.object_instance),
               BacnetPropertyIds.PROP_PRESENT_VALUE, out objBinaryLst);

                var bacnetBinaryValuesByDevice = _LutronLightRepository.GetsLstCurrentBinaryValue((int)deviceDetail.DeviceId);

                var binaryDetaillst = new List<LightsfloorEntity>();
                foreach (var value in bacnetBinaryValuesByDevice)
                {
                    bool currentBinaryValue = ReadCurrentBinaryPresantValue(value);
                    var binaryPresantValuesLst = new LightsfloorEntity
                    {
                        DeviceId = (int)deviceDetail.DeviceId,
                        InstanceId = (int)value.object_instance,
                        LightStatus = currentBinaryValue
                    };
                    binaryDetaillst.Add(binaryPresantValuesLst);
                }
          
                var floorDetail = new FloorEntity
                {
                    FloorId = (int)deviceDetail.DeviceId,
                    FloorName = (CommonConstant.FloorName + deviceDetail.DeviceId).ToString(),
                    NoOfInstance = objValueLst.Count(),
                    BinaryDetails = binaryDetaillst
                };
                floorLst.Add(floorDetail);
            }
            return floorLst;
        }

        /// <summary>
        /// Read current binary presant value.
        /// </summary>
        /// <param name="bacnetDeviceDetail">Passses bacnet device detail.</param>
        /// <returns>Current binary presant value.</returns>
        private bool ReadCurrentBinaryPresantValue(BACnetDevice bacnetDeviceDetail)
        {
            bool currentBinaryValue;
            IList<BacnetValue> bacnetValueList;
            BacnetAddress bacnetAddress;
            bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceDetail.network_id);
            bacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceDetail.routed_source, (ushort)bacnetDeviceDetail.routed_net);
     
            bacNetClient.ReadPropertyRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_BINARY_VALUE, (uint)bacnetDeviceDetail.object_instance),
                BacnetPropertyIds.PROP_PRESENT_VALUE, out bacnetValueList);
            if (bacnetValueList != null && bacnetValueList.Count > 0)
            {
                return currentBinaryValue = Convert.ToBoolean(Convert.ToInt32(bacnetValueList.FirstOrDefault().Value));
            }
            return false;
        }

        #endregion

    }
}