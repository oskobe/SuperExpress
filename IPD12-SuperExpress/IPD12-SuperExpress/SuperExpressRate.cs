using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPD12_SuperExpress
{
    public class SuperExpressRate
    {
        public bool IsSelected { get; set; }
        public string ServiceType { get; set; }
        public bool GuaranteedService { get; set; }
        public int DeliveryDays { get; set; }
        public DateTime EstimatedDeliveryDateTime { get; set; }
        public double Amount { get; set; }

        public string Guaranteed
        {
            get
            {
                return GuaranteedService ? Globals.YES : Globals.NO;
            }
        }

        public string DeliverDaysStr
        {
            get
            {
                if (DeliveryDays > 0)
                {
                    return DeliveryDays.ToString();
                }
                else
                {
                    return string.Empty;
                }

            }
        }

        public string EstimatedDeliveryDateTimeStr
        {
            get
            {
                if (EstimatedDeliveryDateTime <= DateTime.Now)
                {
                    return string.Empty;
                } else
                {
                    return EstimatedDeliveryDateTime.ToString("MM/dd/yyyy HH:mm");
                }
                
            }
        }

        public string AmountStr
        {
            get
            {
                return string.Format("{0:0.00}", Amount);
            }
        }
    }
}
