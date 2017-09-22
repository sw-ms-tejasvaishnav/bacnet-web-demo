using System;
using System.Collections.Generic;
using System.IO.BACnet;
using System.IO.BACnet.Serialize;
using System.Linq;
using System.Web;

namespace BACKnetLutron.BusinessEntities
{
    [Serializable]
    class DaySchedule
    {
        public DateTime dt;
        public object Value;

        public DaySchedule(DateTime dt, object Value)
        {
            this.dt = dt;
            this.Value = Value;
        }
    }


    [Serializable]
    class BacnetWeeklySchedule : ASN1.IEncode
    {
        public List<DaySchedule>[] days = new List<DaySchedule>[7];

        public void Encode(EncodeBuffer buffer)
        {
            for (int i = 0; i < 7; i++)
            {
                ASN1.encode_opening_tag(buffer, 0);

                if (days[i] != null)
                {
                    List<DaySchedule> dsl = days[i];
                    foreach (DaySchedule ds in dsl)
                    {
                        ASN1.bacapp_encode_application_data(buffer, new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME, ds.dt));
                        ASN1.bacapp_encode_application_data(buffer, new BacnetValue(ds.Value));

                    }
                }

                ASN1.encode_closing_tag(buffer, 0);
            }
        }
    }

        
    [Serializable]
    class ExceptionScheduleTimeValue
    {
        public DateTime dt;
        public object Value;
        public ExceptionScheduleTimeValue(DateTime dt, object Value)
        {
            this.dt = dt;
            this.Value = Value;
        }
    }


    [Serializable]
    class ExceptionSchedule
    {
        public List<ExceptionScheduleTimeValue>[] loExceptionScheduleTimeValue = new List<ExceptionScheduleTimeValue>[1];
        public ExceptionSchedule(List<ExceptionScheduleTimeValue>[] loExceptionScheduleTimeValue)
        {
            this.loExceptionScheduleTimeValue = loExceptionScheduleTimeValue;
        }
    }

    [Serializable]
    class ExceptionScheduleArray
    {
        public List<ExceptionScheduleTimeValue>[] loExceptionScheduleTimeValue = new List<ExceptionScheduleTimeValue>[1];
        public DateTime period;
        public ExceptionScheduleArray(DateTime dt, List<ExceptionScheduleTimeValue>[] loExceptionScheduleTimeValue)
        {
            this.loExceptionScheduleTimeValue = loExceptionScheduleTimeValue;
            this.period = dt;
        }
    }


    [Serializable]
    class BacnetWeeklyExceptionSchedule : ASN1.IEncode
    {
        public List<ExceptionScheduleArray>[] loExceptionScheduleArray = new List<ExceptionScheduleArray>[1];

        public void Encode(EncodeBuffer buffer)
        {
            for (int i = 0; i < 1; i++)
            {
                ASN1.encode_opening_tag(buffer, 0);

                List<ExceptionScheduleArray> loArray = loExceptionScheduleArray[i];

                for (int j = 0; j < 1; j++)
                {
                    var loExceptionScheduleTimeValue = loArray[j];
                    ASN1.bacapp_encode_application_data(buffer, new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_DATE, loArray[j].period));

                    foreach (var ds in loExceptionScheduleTimeValue.loExceptionScheduleTimeValue)
                    {
                        var loTime = ds[0].dt;
                        var loValue = ds[0].Value;

                        ASN1.bacapp_encode_application_data(buffer, new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME, loTime));
                        ASN1.bacapp_encode_application_data(buffer, new BacnetValue(loValue));
                    }
                }

                ASN1.encode_closing_tag(buffer, 0);
            }
        }
    }

}