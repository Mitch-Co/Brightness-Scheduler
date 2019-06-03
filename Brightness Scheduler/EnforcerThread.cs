using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Management;

namespace Auto_Dimmer
{
    class EnforcerThread
    {
        bool defaultIsEnforced; 
        private int defaultBrightness;

        private List<BrightnessRequest> toService;

        private Thread activeThread;

        private void serviceRequests()
        {
            if(toService == null)
            {
                return;
            }

            while(true)
            {
                System.Threading.Thread.Sleep(3000);
                DateTime localDT = DateTime.Now;
                Time24Hours localTime = Time24Hours.stringTo24HTime(localDT.ToString("HH:mm:ss"),':');
                if(localTime != null)
                {
                    bool defaultB = true;
                    foreach(BrightnessRequest BR in toService)
                    {
                        if (localTime.fallsInbetween(BR.getStartTime(), BR.getEndTime()))
                        {
                            //Console.WriteLine(localTime.toString() + " Falls Inbetween " + BR.getStartTime().toString() + " and " + BR.getEndTime().toString());
                            setBrightness(BR.getBrightness());
                            defaultB = false;
                        }
                    }
                    if(defaultB && defaultIsEnforced)
                    {
                        setBrightness(this.defaultBrightness);
                    }
                }
            }
        }

        public EnforcerThread(List<BrightnessRequest> requests)
        {
            this.toService = requests;
            activeThread = null;
            defaultBrightness = 100;
            defaultIsEnforced = false;
        }
        public EnforcerThread()
        {
            activeThread = null;
            defaultBrightness = 100;
            defaultIsEnforced = false;
        }


        private bool setBrightness(int brightness)
        {
            byte brightnessInBytes;
            if(brightness >= 0 && brightness <= 100)
            {
                brightnessInBytes = (byte)(brightness);
            }
            else
            {
                return false;
            }

            try
            {
                System.Management.ManagementScope scope = new System.Management.ManagementScope("root\\WMI");
                System.Management.SelectQuery query = new System.Management.SelectQuery("WmiMonitorBrightnessMethods");
                System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher(scope, query);
                System.Management.ManagementObjectCollection moc = mos.Get();

                foreach (System.Management.ManagementObject o in moc)
                {
                    o.InvokeMethod("WmiSetBrightness", new Object[] { UInt32.MaxValue, brightnessInBytes });
                    break; //only work on the first object (for an unknown reason)
                }

                moc.Dispose();
                mos.Dispose();
            }
            catch (Exception E)
            {
                Console.WriteLine(E.ToString());
                return false;
            }
            return true;
        }

        public void startThread()
        {
            if(activeThread == null)
            {
                ThreadStart childref = new ThreadStart(serviceRequests);
                activeThread = new Thread(childref);
                activeThread.Start();
            }
        }
        
        public void stopThread()
        {
            try
            {
                if (activeThread != null)
                {
                    activeThread.Abort();
                }
                activeThread = null;
            }
            catch(Exception E)
            {
                Console.WriteLine(E.ToString());
            }
        }

        public void updateDefault(int updateBrightness)
        {
            stopThread();
            this.defaultBrightness = updateBrightness;
            defaultIsEnforced = true;
            startThread();
        }

    }
}
