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
using AutoMapper;
using System.Configuration;
using NetBIX.oBIX.Client.Framework;
using System.Xml.Linq;
using BACKnetLutron.Init.Obix;
using System.Xml.Serialization;
using BACKnetLutron.BusinessEntities.Obix;


namespace BACKnetLutron.Services
{
    public class LutronLightServices : ILutronLightFloorServices
    {
        #region Global Objects

        static BacnetClient bacNetClient;
        static BackNetDeviceModel bacNetDeviceModel = new BackNetDeviceModel();
        private ILutronLightRepository _LutronLightRepository;
        public string obixHubURL = ConfigurationManager.AppSettings["ObixHubURL"];
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
            Thread.Sleep(5000);
            //AddBackNetDeviceDetail();
            // bacNetClient.Dispose();
            // bacNetClient.Transport.Dispose();
            //// Bacnet on UDP/IP/Ethernet
            // bacNetClient = new BacnetClient(new BacnetIpUdpProtocolTransport(47808, false));// (0xBAC0, false));

            bacNetClient.OnReinitializedDevice += new BacnetClient.ReinitializedRequestHandler(OnReinitializedDevice);

            // bacNetClient.Start();    // go
            //  bacNetClient.WhoIs();

        }

        private void OnReinitializedDevice(BacnetClient sender, BacnetAddress adr, byte invoke_id, BacnetReinitializedStates state, string password, BacnetMaxSegments max_segments)
        {
            sender.SimpleAckResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_REINITIALIZE_DEVICE, invoke_id);
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
        public void SaveSchedule(WeeklyScheduleEntity scheduleDetail)
        {
            if (scheduleDetail != null && scheduleDetail.ScheduleDetailId == 0)
            {
                var bacnetDeviceobjDetail = _LutronLightRepository.GetBacNetDeviceObjDetail(scheduleDetail.DeviceId);
                var firstInstanceId = _LutronLightRepository.GetBacNetSceduleObjDetail(scheduleDetail.DeviceId);
                BacnetAddress bacnetAddress;
                bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceobjDetail.network_id);
                bacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceobjDetail.routed_source,
                    (ushort)bacnetDeviceobjDetail.routed_net);


                //IList<BacnetValue> loScheduleValues;
                //bacNetClient.ReadPropertyRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_SCHEDULE,
                //    (uint)1), BacnetPropertyIds.PROP_EXCEPTION_SCHEDULE, out loScheduleValues);


                ICollection<BacnetPropertyValue> loBacnetPropertyValueList = new List<BacnetPropertyValue>();

                BacnetPropertyValue loNewPropertyValue = new BacnetPropertyValue();
                List<BacnetValue> loBacnetValue = new List<BacnetValue>();
                //IList<BacnetValue> objValueLst;
                #region Set Schedule
                ////    Create new instance id based on largest available
                //if(liTopInstanceID)
                firstInstanceId = firstInstanceId != null ? firstInstanceId + 1 : 1;

                loBacnetPropertyValueList = SetScheduleDetail(scheduleDetail, loBacnetPropertyValueList, firstInstanceId,
                    loBacnetValue, loNewPropertyValue);

                // bacNetClient.ReadPropertyRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_SCHEDULE,
                //(uint)bacnetDeviceobjDetail.object_instance),
                //         BacnetPropertyIds.PROP_LIST_OF_OBJECT_PROPERTY_REFERENCES, out objValueLst);

                var totalArrayInPropertyLst = 1;
                #endregion

                #region Update Schedule Object
                //// Set object reference to update it's value with schedule object

                var updateScheduleObjDetail = UpdateScheduleObject(scheduleDetail, totalArrayInPropertyLst);
                loBacnetPropertyValueList.Add(updateScheduleObjDetail);
                #endregion

                #region Adds Weekly Schedule
                //// Add weekly schedule for object
                var scheduleWeekalyValue = AddScheduleWeeklyDetail(scheduleDetail);
                loBacnetPropertyValueList.Add(scheduleWeekalyValue);
                #endregion

                //#region Adds WeeklyExpection Detail.
                //var weeklyExpScheduleValue = AddWeeklyExpectionSchedule(scheduleDetail);
                //loBacnetPropertyValueList.Add(weeklyExpScheduleValue);
                //#endregion


                bacNetClient.CreateObjectRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_SCHEDULE,
                    (uint)firstInstanceId), loBacnetPropertyValueList);

                //Adds schedule detail in data base.
                SaveNewSchedule(scheduleDetail, firstInstanceId, totalArrayInPropertyLst);
                AddBackNetDeviceDetail();

            }
            else
            {
                UpdateSchedule(scheduleDetail);
            }

        }

        public List<ScheduleEntity> ScheduleList()
        {
            var scheduleLst = _LutronLightRepository.ScheduleList();
            return scheduleLst;
        }

        public ScheduleEntity GetsScheduleInfo(int deviceId, int instanceId)
        {
            var scheduleDetail = _LutronLightRepository.GetsScheduleDetail(deviceId, instanceId);

            return scheduleDetail;
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
                BacnetIpUdpProtocolTransport newPort = new BacnetIpUdpProtocolTransport(0xBAC0, false);
                bacNetClient = new BacnetClient(newPort);

                //bacNetClient.Dispose();
                //   Thread.Sleep(1000);
                bacNetClient.Start();
                bacNetClient.OnIam += new BacnetClient.IamHandler(Handler_OmIam);
                bacNetClient.OnWhoIs += new BacnetClient.WhoIsHandler(handler_OnWhoIs);

                bacNetClient.WhoIs();
            }
            else
            {
                bacNetClient.OnIam -= new BacnetClient.IamHandler(Handler_OmIam);
                bacNetClient.OnWhoIs -= new BacnetClient.WhoIsHandler(handler_OnWhoIs);
            }
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

                if(bacNetDeviceModel.BACnetDeviceList.Count == 48)
                {
                    AddBackNetDeviceDetail();
                }
            }
        }

        /// <summary>
        /// Adds bacnet device details.
        /// </summary>
        private static void AddBackNetDeviceDetail()
        {
            if (bacNetDeviceModel != null && bacNetDeviceModel.BACnetDeviceList != null)
            {
                LutronLightRepository lutronLightRepository = new LutronLightRepository();
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

                        var isExistNetworkId = lutronLightRepository.CheckIfExistNetworkAddress(deviceDetail.BacNetAddress.ToString(),
                            (int)((BacnetObjectId)objValue.Value).Instance, (int)deviceDetail.DeviceId
                            , ((BacnetObjectId)objValue.Value).Type.ToString());
                        if (isExistNetworkId == true)
                        {
                            continue;
                        }
                        IList<BacnetValue> objNameList;
                        bacNetClient.ReadPropertyRequest(deviceDetail.BacNetAddress,
                            new BacnetObjectId((BacnetObjectTypes)((BacnetObjectId)objValue.Value).Type,
                            ((BacnetObjectId)objValue.Value).Instance),
                            BacnetPropertyIds.PROP_OBJECT_NAME, out objNameList);
                        if (deviceDetail.BacNetAddress.RoutedSource != null && deviceDetail.BacNetAddress.RoutedSource.net != null)
                        {
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
                        }
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
                    lutronLightRepository.AddBacNetDeviceDetail(bACnetDeviceLst);
                }
                if (bACnetDeviceMappingLst.Count() > 0)
                {
                    lutronLightRepository.AddBacNetMappingDetail(bACnetDeviceMappingLst);
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

        #region Floor lights on/off.
        /// <summary>
        /// Sets binary value according to device.
        /// </summary>
        /// <param name="deviceId">Passes device id.</param>
        /// <param name="binaryValueStatus">Passes Binary value status.</param>
        /// 
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

        #endregion


        #region Create New Schedule Methods.

        /// <summary>
        /// Sets schedule detail.
        /// </summary>
        /// <param name="scheduleDetail">Schedule info.</param>
        /// <param name="bacnetPropertyValueList">Bacnet property list.</param>
        /// <param name="firstInstanceId">Current instance id.</param>
        /// <param name="bacnetValue">List of backnet value.</param>
        /// <param name="newPropertyValue">Set propert value.</param>
        /// <returns>Updated schedule backnet property list.</returns>
        private ICollection<BacnetPropertyValue> SetScheduleDetail(WeeklyScheduleEntity scheduleDetail,
                ICollection<BacnetPropertyValue> bacnetPropertyValueList, int? firstInstanceId,
                List<BacnetValue> bacnetValue, BacnetPropertyValue newPropertyValue)
        {


            //// Set schedule object name
            bacnetValue = new List<BacnetValue>();
            newPropertyValue = new BacnetPropertyValue();
            bacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_CHARACTER_STRING, "Schedule" +
                " " + firstInstanceId));

            newPropertyValue.value = bacnetValue;
            newPropertyValue.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_OBJECT_NAME, ASN1.BACNET_ARRAY_ALL);

            bacnetPropertyValueList.Add(newPropertyValue);


            //// Set effective period for schedule object
            bacnetValue = new List<BacnetValue>();
            newPropertyValue = new BacnetPropertyValue();
            bacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE,
                scheduleDetail.ScheduleStartDate));
            bacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE,
                    scheduleDetail.ScheduleEndDate));

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
        private BacnetPropertyValue UpdateScheduleObject(WeeklyScheduleEntity scheduleDetail, int totalArrayInPropertyLst)
        {

            var loBacnetValue = new List<BacnetValue>();
            var loNewPropertyValue = new BacnetPropertyValue();

            BacnetDeviceObjectPropertyReference loPropertyReference = new BacnetDeviceObjectPropertyReference();


            //BACnetCalendarEntry loBACnetCalendarEntry = new BACnetCalendarEntry();


            //// Add AV 1 present value change
            loPropertyReference.ArrayIndex = totalArrayInPropertyLst;
            loPropertyReference.DeviceId = new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, (uint)scheduleDetail.DeviceId);
            loPropertyReference.ObjectId = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, (uint)scheduleDetail.ScheduleId);
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
        private BacnetPropertyValue AddScheduleWeeklyDetail(WeeklyScheduleEntity scheduleDetail)
        {
            var loBacnetValue = new List<BacnetValue>();
            var loNewPropertyValue = new BacnetPropertyValue();

            //// setup weekday to add schedule (0 = Monday, 1 = Tuesday ....., 6 = Sunday)
            BacnetWeeklySchedule loBacnetWeeklySchedule = new BacnetWeeklySchedule();
            loBacnetWeeklySchedule.days[scheduleDetail.SelectedDayId] = new List<DaySchedule>();

            //// Schedule to update AV 1,2 at specified time
            loBacnetWeeklySchedule.days[scheduleDetail.SelectedDayId].Add(
                new DaySchedule(new DateTime(1, 1, 1, Convert.ToInt32(scheduleDetail.SelectedTime.Hour),
                Convert.ToInt32(scheduleDetail.SelectedTime.Minute), Convert.ToInt32(scheduleDetail.SelectedTime.Second)),
                  Convert.ToSingle(scheduleDetail.PresentValue)));


            loBacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_WEEKLY_SCHEDULE, loBacnetWeeklySchedule));


            loNewPropertyValue.value = loBacnetValue;

            loNewPropertyValue.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_WEEKLY_SCHEDULE,
                ASN1.BACNET_ARRAY_ALL);

            return loNewPropertyValue;
        }

        /// <summary>
        /// Updated schedule weekly expection object detail.
        /// </summary>
        /// <param name="scheduleDetail">Passe schedule info.</param>
        /// <returns>Updated schedule weekly expection backnet property list.</returns>
        private BacnetPropertyValue AddWeeklyExpectionSchedule(WeeklyScheduleEntity scheduleDetail)
        {
            var loBacnetValue = new List<BacnetValue>();
            var loNewPropertyValue = new BacnetPropertyValue();

            BacnetWeeklyExceptionSchedule loBacnetWeeklyExceptionSchedule = new BacnetWeeklyExceptionSchedule();

            List<ExceptionScheduleTimeValue>[] loExceptionSchedulTimeValue = new List<ExceptionScheduleTimeValue>[1];
            loExceptionSchedulTimeValue[0] = new List<ExceptionScheduleTimeValue>();
            loExceptionSchedulTimeValue[0].Add(new ExceptionScheduleTimeValue(DateTime.Now.AddMinutes(2), Convert.ToSingle(111)));


            loBacnetWeeklyExceptionSchedule.loExceptionScheduleArray = new List<ExceptionScheduleArray>[1];
            loBacnetWeeklyExceptionSchedule.loExceptionScheduleArray[0] = new List<ExceptionScheduleArray>();
            loBacnetWeeklyExceptionSchedule.loExceptionScheduleArray[0].Add(
                new ExceptionScheduleArray(new DateTime(1, 1, 1, 18, 30, 1),
                loExceptionSchedulTimeValue));

            loBacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_SPECIAL_EVENT,
                loBacnetWeeklyExceptionSchedule));

            loNewPropertyValue.value = loBacnetValue;
            loNewPropertyValue.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_EXCEPTION_SCHEDULE,
                ASN1.BACNET_ARRAY_ALL);

            return loNewPropertyValue;
        }

        #endregion


        #region Update Schedule

        /// <summary>
        /// Update weekly schedule in simulator.
        /// </summary>
        /// <param name="scheduleDetail">Passes new schedule info.</param>
        /// <param name="bacnetAddress">Passes bacnet address.</param>
        /// <param name="existingScheduleDetail">Passses existing schedule info.</param>
        /// <param name="deviceDetail">Passes device info</param>
        private void UpdateWeeklySchedule(WeeklyScheduleEntity scheduleDetail, BacnetAddress bacnetAddress
            , ScheduleDetail existingScheduleDetail, BACnetDevice deviceDetail)
        {
            IList<BacnetValue> existingWeekalySchedule;
            bacNetClient.ReadPropertyRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_SCHEDULE,
               (uint)deviceDetail.object_instance), BacnetPropertyIds.PROP_WEEKLY_SCHEDULE, out existingWeekalySchedule);

            ICollection<BacnetPropertyValue> newBacnetPropertyList = new List<BacnetPropertyValue>();
            BacnetWeeklySchedule weeklySchaduleData = new BacnetWeeklySchedule();
            var weeklySchedulePropery = new BacnetPropertyValue();
            var updatebacnetValue = new List<BacnetValue>();
            var existingSchedule = _LutronLightRepository.GetWeeklyScheduleDetailById(existingScheduleDetail.ScheduleDetailId);
            if (existingSchedule != null)
            {
                foreach (var schedule in existingSchedule)
                {
                    if (weeklySchaduleData.days[schedule.SelectedDayId] == null)
                    {
                        weeklySchaduleData.days[schedule.SelectedDayId] = new List<DaySchedule>();
                    }
                    // Schedule days add.
                    weeklySchaduleData.days[schedule.SelectedDayId].Add(
                        new DaySchedule(Convert.ToDateTime(schedule.SelectedTime), Convert.ToSingle(schedule.PresentValue)));
                }
            }
            if (weeklySchaduleData.days[scheduleDetail.SelectedDayId] == null)
            {
                weeklySchaduleData.days[scheduleDetail.SelectedDayId] = new List<DaySchedule>();
            }
            weeklySchaduleData.days[scheduleDetail.SelectedDayId].Add(
           new DaySchedule(new DateTime(1, 1, 1, Convert.ToInt32(scheduleDetail.SelectedTime.Hour),
           Convert.ToInt32(scheduleDetail.SelectedTime.Minute), Convert.ToInt32(scheduleDetail.SelectedTime.Second)),
             Convert.ToSingle(scheduleDetail.PresentValue)));
            updatebacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_WEEKLY_SCHEDULE, weeklySchaduleData));


            weeklySchedulePropery.value = updatebacnetValue;
            weeklySchedulePropery.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_WEEKLY_SCHEDULE,
                ASN1.BACNET_ARRAY_ALL);

            var updatedProperty = AddScheduleWeeklyDetail(scheduleDetail);
            newBacnetPropertyList.Add(weeklySchedulePropery);

            //Write multiple request in one list in to the simulator.
            bacNetClient.WritePropertyMultipleRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_SCHEDULE,
                (uint)scheduleDetail.InstanceId), newBacnetPropertyList, (byte)newBacnetPropertyList.Count);
        }

        /// <summary>
        /// Update Schedule device property reference.
        /// </summary>
        /// <param name="scheduleDetail">Passes new schedule info.</param>
        /// <param name="bacnetAddress">Passes bacnet address.</param>
        /// <param name="existingScheduleDetail">Passses existing schedule info.</param>
        /// <param name="deviceDetail">Passes device info</param>
        private int UpdateScheduleObjectPropertyReference(WeeklyScheduleEntity scheduleDetail, BacnetAddress bacnetAddress,
            ScheduleDetail existingScheduleDetail, BACnetDevice deviceDetail)
        {
            IList<BacnetValue> existingListofPropertyReferenceList;
            ICollection<BacnetPropertyValue> newBacnetPropertyList = new List<BacnetPropertyValue>();
            bacNetClient.ReadPropertyRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_SCHEDULE,
               (uint)deviceDetail.object_instance), BacnetPropertyIds.PROP_LIST_OF_OBJECT_PROPERTY_REFERENCES,
               out existingListofPropertyReferenceList);

            var bacnetValue = new List<BacnetValue>();
            var updatedropertyValue = new BacnetPropertyValue();
            int currentArrayIndex = 0;
            BacnetDeviceObjectPropertyReference bacnetDevicePReference = new BacnetDeviceObjectPropertyReference();
            if (existingScheduleDetail != null && existingListofPropertyReferenceList.Count() > 0)
            {

                foreach (var existingProperty in existingListofPropertyReferenceList)
                {
                    bacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_PROPERTY_REFERENCE,
                               existingProperty.Value));
                }
                currentArrayIndex = existingListofPropertyReferenceList.Count() + 1;
                bacnetDevicePReference.ArrayIndex = currentArrayIndex;
                bacnetDevicePReference.DeviceId = new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE,
                    (uint)scheduleDetail.DeviceId);
                bacnetDevicePReference.ObjectId = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE,
                    (uint)scheduleDetail.ScheduleId);
                bacnetDevicePReference.PropertyId = BacnetPropertyIds.PROP_PRESENT_VALUE;


                bacnetValue.Add(new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_OBJECT_PROPERTY_REFERENCE,
                                  bacnetDevicePReference));

                updatedropertyValue.value = bacnetValue;
                updatedropertyValue.property = new BacnetPropertyReference((uint)BacnetPropertyIds.PROP_LIST_OF_OBJECT_PROPERTY_REFERENCES,
                    ASN1.BACNET_ARRAY_ALL);

                newBacnetPropertyList.Add(updatedropertyValue);
                bacNetClient.WritePropertyMultipleRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_SCHEDULE,
               (uint)scheduleDetail.InstanceId), newBacnetPropertyList, (byte)bacnetValue.Count);



            }
            return currentArrayIndex;
        }

        #endregion


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
            bacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceDetail.routed_source,
                (ushort)bacnetDeviceDetail.routed_net);

            bacNetClient.ReadPropertyRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_BINARY_VALUE,
                (uint)bacnetDeviceDetail.object_instance), BacnetPropertyIds.PROP_PRESENT_VALUE, out bacnetValueList);
            if (bacnetValueList != null && bacnetValueList.Count > 0)
            {
                return currentBinaryValue = Convert.ToBoolean(Convert.ToInt32(bacnetValueList.FirstOrDefault().Value));
            }
            return false;
        }

        /// <summary>
        /// Update schedule
        /// </summary>
        /// <param name="scheduleDetail">Passes schedule info.</param>
        private void UpdateSchedule(WeeklyScheduleEntity scheduleDetail)
        {
            var existScheduleDetail = _LutronLightRepository.GetScheduleDetailById(scheduleDetail.ScheduleDetailId);
            var deviceDetail = _LutronLightRepository.GetScheduleDeviceDetail(scheduleDetail.DeviceId, scheduleDetail.InstanceId);

            BacnetAddress bacnetAddress;
            bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, deviceDetail.network_id);
            bacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, deviceDetail.routed_source,
                (ushort)deviceDetail.routed_net);

            //Update weeekly schedule detail.
            UpdateWeeklySchedule(scheduleDetail, bacnetAddress, existScheduleDetail, deviceDetail);

            //Update list of object property references.
            var currentArrayIndex = UpdateScheduleObjectPropertyReference(scheduleDetail, bacnetAddress, existScheduleDetail
                 , deviceDetail);
            //Update weekly schedule in data base.
            UpdateScheduleWeekly(scheduleDetail, scheduleDetail.InstanceId, currentArrayIndex, existScheduleDetail);


        }
        /// <summary>
        /// Adds new schedule.
        /// </summary>
        /// <param name="scheduleDetail">Passes schedule detail.</param>
        /// <param name="firstInstanceId">Passes current instance of device. </param>
        /// <param name="totalPropertyLst">Passes total array of list.</param>
        private void SaveNewSchedule(WeeklyScheduleEntity scheduleDetail, int? firstInstanceId, int totalPropertyLst)
        {
            // var scheduleInfo= Mapper.Map<ScheduleEntity, ScheduleDetail>(scheduleDetail);
            var scheduleInfo = new ScheduleDetail();
            scheduleInfo.ScheduleId = scheduleDetail.ScheduleId;
            scheduleInfo.ScheduleName = "Instance " + firstInstanceId;
            scheduleInfo.InstanceId = firstInstanceId.Value;
            scheduleInfo.PropertyArrayIndex = totalPropertyLst;
            scheduleInfo.DeviceId = scheduleDetail.DeviceId;
            scheduleInfo.ScheduleStartDate = scheduleDetail.ScheduleStartDate;
            scheduleInfo.ScheduleEndDate = scheduleDetail.ScheduleEndDate;
            scheduleInfo.IsActive = true;
            scheduleInfo.IsDeleted = false;
            scheduleInfo.DateOfEntry = DateTime.UtcNow;
            scheduleInfo.DateOfModified = DateTime.UtcNow;
            scheduleInfo.UserID = 1;
            scheduleInfo.UserModified = 1;
            var weeklySchedule = new WeeklySchedule
            {
                SelectedDayId = scheduleDetail.SelectedDayId,
                SelectedTime = scheduleDetail.SelectedTime,
                PresentValue = scheduleDetail.PresentValue,
                IsActive = true,
                IsDeleted = false,
                DateOfEntry = DateTime.UtcNow,
                DateOfModified = DateTime.UtcNow,
                UserID = 1,
                UserModified = 1,
            };
            scheduleInfo.WeeklySchedules.Add(weeklySchedule);
            _LutronLightRepository.SaveScheduleDetail(scheduleInfo);
        }


        /// <summary>
        /// Update weekly schedule detail.
        /// </summary>
        /// <param name="scheduleDetail">Passes schedule detail.</param>
        /// <param name="firstInstanceId">Passes instance id.</param>
        /// <param name="totalPropertyLst">Passes total array of property reference.</param>
        /// <param name="existingScheduleDetail">Passes existing schedule id.</param>
        private void UpdateScheduleWeekly(WeeklyScheduleEntity scheduleDetail, int? firstInstanceId, int totalPropertyLst,
            ScheduleDetail existingScheduleDetail)
        {
            // var scheduleInfo= Mapper.Map<ScheduleEntity, ScheduleDetail>(scheduleDetail);

            existingScheduleDetail.ScheduleId = scheduleDetail.ScheduleId;
            existingScheduleDetail.ScheduleName = "Instance " + firstInstanceId;
            existingScheduleDetail.InstanceId = firstInstanceId.Value;
            existingScheduleDetail.PropertyArrayIndex = totalPropertyLst;
            existingScheduleDetail.DeviceId = scheduleDetail.DeviceId;
            existingScheduleDetail.IsActive = true;
            existingScheduleDetail.IsDeleted = false;
            existingScheduleDetail.DateOfModified = DateTime.UtcNow;
            existingScheduleDetail.UserModified = 1;
            var weeklySchedule = new WeeklySchedule
            {
                SelectedDayId = scheduleDetail.SelectedDayId,
                SelectedTime = scheduleDetail.SelectedTime,
                PresentValue = scheduleDetail.PresentValue,
                ScheduleDetailId = scheduleDetail.ScheduleDetailId,
                IsActive = true,
                IsDeleted = false,
                DateOfEntry = DateTime.UtcNow,
                DateOfModified = DateTime.UtcNow,
                UserID = 1,
                UserModified = 1,
            };
            existingScheduleDetail.WeeklySchedules.Add(weeklySchedule);
            _LutronLightRepository.UpdateScheduleDetail(existingScheduleDetail);
        }
        #endregion



        public LightLevelEntity GetConfLightLevel(int? deviceID)
        {
            ESD_LutronEntities lutronEntities = new ESD_LutronEntities();
            var bacnetDeviceFromDB = lutronEntities.BACnetDevices
                                       .Where(x => x.device_id == deviceID
                                           && x.object_instance == (int?)LutronObjectType.Lighting_Level)
                                       .Select(x => x).FirstOrDefault();


            IList<BacnetValue> bacnetValueList;
            BacnetAddress bacnetAddress;
            bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceFromDB.network_id);

            bacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceFromDB.routed_source,
                (ushort)bacnetDeviceFromDB.routed_net);

            bacNetClient.ReadPropertyRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE,
                (uint)LutronObjectType.Lighting_Level), BacnetPropertyIds.PROP_PRESENT_VALUE, out bacnetValueList);

            if (bacnetValueList != null && bacnetValueList.Count > 0)
            {
                return new LightLevelEntity { DeviceID = (Int32)deviceID, LightLevel = Convert.ToString(bacnetValueList.FirstOrDefault().Value)};
            }
            else
            {
                return new LightLevelEntity { DeviceID = (Int32)deviceID, LightLevel = string.Empty };
            }
        }

        public LightStateEntity GetConfLightState(int? deviceID)
        {
            ESD_LutronEntities lutronEntities = new ESD_LutronEntities();
            var bacnetDeviceFromDB = lutronEntities.BACnetDevices
                                       .Where(x => x.device_id == deviceID
                                            && x.object_instance == (int?)LutronObjectType.Lighting_State)
                                       .Select(x => x).FirstOrDefault();


            IList<BacnetValue> loBacnetValueList;
            BacnetAddress loBacnetAddress;
            loBacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceFromDB.network_id);

            loBacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceFromDB.routed_source,
                (ushort)bacnetDeviceFromDB.routed_net);

            bacNetClient.ReadPropertyRequest(loBacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_BINARY_VALUE,
                (uint)LutronObjectType.Lighting_State), BacnetPropertyIds.PROP_PRESENT_VALUE, out loBacnetValueList);

            if (loBacnetValueList != null && loBacnetValueList.Count > 0)
            {
                return new LightStateEntity { DeviceID = (Int32)deviceID, LightState = Convert.ToBoolean(loBacnetValueList.FirstOrDefault().Value) };
            }
            else
            {
                return new LightStateEntity { DeviceID = (Int32)deviceID, LightState = null };
            }
        }


        public LightSceneEntity GetConfLightingScene(int? deviceID)
        {
            LightSceneEntity lightSceneEntity = new LightSceneEntity();
            ESD_LutronEntities lutronEntities = new ESD_LutronEntities();
            var bacnetDeviceFromDB = lutronEntities.BACnetDevices
                                       .Where(x => x.device_id == deviceID
                                           && x.object_instance == (int?)LutronObjectType.Lighting_Scene)
                                       .Select(x => x).FirstOrDefault();


            IList<BacnetValue> bacnetValueList;
            BacnetAddress bacnetAddress;
            bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceFromDB.network_id);

            bacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceFromDB.routed_source,
                (ushort)bacnetDeviceFromDB.routed_net);

            bacNetClient.ReadPropertyRequest(bacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_MULTI_STATE_VALUE,
                (uint)LutronObjectType.Lighting_Scene), BacnetPropertyIds.PROP_PRESENT_VALUE, out bacnetValueList);





            ObixClientInit obixClientInit = new ObixClientInit();
            ObixResult<XElement> xmlLevelResult = obixClientInit.oBixClient.ReadUriXml(new Uri(obixHubURL + "config/Drivers/BacnetNetwork/Lutron_Quantum/Conference%24204628A%24201761035/points/Basic/Scene/"));

            IEnumerable<XNode> lighLevelLst = xmlLevelResult.Result.Document.DescendantNodes();
            XElement lightLevelelement = lighLevelLst.LastOrDefault() as XElement;
            if (lightLevelelement != null)
            {
                lightSceneEntity.LightScene = lightLevelelement.Attribute("val").Value;
            }


            if (bacnetValueList != null && bacnetValueList.Count > 0)
            {
                lightSceneEntity.Value = Convert.ToString(bacnetValueList.FirstOrDefault().Value);
            }

            lightSceneEntity.DeviceID = (Int32)deviceID;

            return lightSceneEntity;
        }

        public LightLevelEntity SetConfLightLevel(LightLevelEntity lightLevelEntity)
        {
            ESD_LutronEntities lutronEntities = new ESD_LutronEntities();
            var bacnetDeviceFromDB = lutronEntities.BACnetDevices
                                       .Where(x => x.device_id == lightLevelEntity.DeviceID
                                           && x.object_instance == (int?)LutronObjectType.Lighting_Level)
                                       .Select(x => x).FirstOrDefault();

            if(bacnetDeviceFromDB != null && bacnetDeviceFromDB.bacnet_device_id > 0)
            {
                BacnetAddress bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceFromDB.network_id);
                bacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceFromDB.routed_source,
                    (ushort)bacnetDeviceFromDB.routed_net);


                BacnetValue newLightLevel = new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL, Convert.ToSingle(lightLevelEntity.LightLevel));
                BacnetValue[] writeNewLightLevel = { newLightLevel };

                bacNetClient.WritePropertyRequest(bacnetAddress,
                    new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, (uint)LutronObjectType.Lighting_Level),
                    BacnetPropertyIds.PROP_PRESENT_VALUE, writeNewLightLevel);

                //if (bacnetValueList != null && bacnetValueList.Count > 0)
                //{
                //    return new LightLevelEntity { DeviceID = (Int32)deviceID, LightLevel = Convert.ToString(bacnetValueList.FirstOrDefault().Value) };
                //}
                //else
                //{
                //    return new LightLevelEntity { DeviceID = (Int32)deviceID, LightLevel = string.Empty };
                //}



                //BacnetAddress loBacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, loBACnetDeviceDetail.network_id);
                //loBacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, loBACnetDeviceDetail.routed_source, (ushort)loBACnetDeviceDetail.routed_net);

                //IList<BacnetValue> loBacnetValueList;
                //moBacnetClient.ReadPropertyRequest(loBacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_BINARY_VALUE, (uint)loBACnetDeviceDetail.object_instance), BacnetPropertyIds.PROP_PRESENT_VALUE, out loBacnetValueList);


                //BacnetValue newLightLevel = new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED, fbToggleStatus == true ? 1 : 0);
                //BacnetValue[] writeNewLightLevel = { newLightLevel };

                //moBacnetClient.WritePropertyRequest(loBacnetAddress, new BacnetObjectId(BacnetObjectTypes.OBJECT_BINARY_VALUE, (uint)loBACnetDeviceDetail.object_instance), BacnetPropertyIds.PROP_PRESENT_VALUE, writeNewLightLevel);


            }



            return GetConfLightLevel(lightLevelEntity.DeviceID);
        }


        public LightSceneEntity SetConfLightScene(LightSceneEntity lightSceneEntity)
        {
            ESD_LutronEntities lutronEntities = new ESD_LutronEntities();
            var bacnetDeviceFromDB = lutronEntities.BACnetDevices
                                       .Where(x => x.device_id == lightSceneEntity.DeviceID
                                           && x.object_instance == (int?)LutronObjectType.Lighting_Scene)
                                       .Select(x => x).FirstOrDefault();

            if (bacnetDeviceFromDB != null && bacnetDeviceFromDB.bacnet_device_id > 0)
            {
                BacnetAddress bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceFromDB.network_id);
                bacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceFromDB.routed_source,
                    (ushort)bacnetDeviceFromDB.routed_net);


                BacnetValue newLightScene = new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT, Convert.ToUInt32(lightSceneEntity.Value));
                BacnetValue[] writeNewLightScene = { newLightScene };

                bacNetClient.WritePropertyRequest(bacnetAddress,
                    new BacnetObjectId(BacnetObjectTypes.OBJECT_MULTI_STATE_VALUE, (uint)LutronObjectType.Lighting_Scene),
                    BacnetPropertyIds.PROP_PRESENT_VALUE, writeNewLightScene);

            }

            Thread.Sleep(1000);

            return GetConfLightingScene(lightSceneEntity.DeviceID);
        }

        public LightStateEntity SetConfLightState(LightStateEntity lightStateEntity)
        {
            ESD_LutronEntities lutronEntities = new ESD_LutronEntities();
            var bacnetDeviceFromDB = lutronEntities.BACnetDevices
                                       .Where(x => x.device_id == lightStateEntity.DeviceID
                                           && x.object_instance == (int?)LutronObjectType.Lighting_State)
                                       .Select(x => x).FirstOrDefault();

            if (bacnetDeviceFromDB != null && bacnetDeviceFromDB.bacnet_device_id > 0)
            {
                BacnetAddress bacnetAddress = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceFromDB.network_id);
                bacnetAddress.RoutedSource = new BacnetAddress(BacnetAddressTypes.IP, bacnetDeviceFromDB.routed_source,
                    (ushort)bacnetDeviceFromDB.routed_net);


                BacnetValue newLightState = new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED, lightStateEntity.LightState == true ? 1 : 0);
                BacnetValue[] writeNewLightState = { newLightState };

                bacNetClient.WritePropertyRequest(bacnetAddress,
                    new BacnetObjectId(BacnetObjectTypes.OBJECT_BINARY_VALUE, (uint)LutronObjectType.Lighting_State),
                    BacnetPropertyIds.PROP_PRESENT_VALUE, writeNewLightState);

            }

            Thread.Sleep(1000);

            return GetConfLightState(lightStateEntity.DeviceID);
        }
    }

}