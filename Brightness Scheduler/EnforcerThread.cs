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
        bool useDefBright; 
        private int defaultBrightness;

        private bool useRR;
        private int refreshRate;

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
                        if (localTime.fallsInbetween(BR.getStartTime(), BR.getEndTime())) //If the current time is in any request in toService
                        {
                            setBrightness(BR.getBrightness());
                            defaultB = false;
                        }
                    }
                    if(defaultB && useDefBright)
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
            useDefBright = false;
            refreshRate = 0;
        }
        public EnforcerThread()
        {
            activeThread = null;
            defaultBrightness = 100;
            useDefBright = false;
            refreshRate = 0;
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
                /* DANGER ZONE CODE - Only about 80% sure what this does */

                System.Management.ManagementScope scope = new System.Management.ManagementScope("root\\WMI");
                System.Management.SelectQuery query = new System.Management.SelectQuery("WmiMonitorBrightnessMethods");
                System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher(scope, query);
                System.Management.ManagementObjectCollection moc = mos.Get();

                foreach (System.Management.ManagementObject o in moc) //Don't ask questions you don't want to know the answer to
                {
                    o.InvokeMethod("WmiSetBrightness", new Object[] { UInt32.MaxValue, brightnessInBytes });
                    break; //Searously
                }

                moc.Dispose();
                mos.Dispose();

                /* END OF DANGER ZONE CODE */
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

        public bool isValidBrightness(int toCheck)
        {
            if(toCheck < 0 || toCheck > 100)
            {
                return false;
            }

            return true;
        }

        public bool isValidRefreshRate(int toCheck)
        {
            if (refreshRate > 0 && refreshRate < 600000) //Must be in between 1ms and 10min
            {
                return true;
            }

            return false;
        }

        public void updateDefault(int updateBrightness)
        {
            stopThread(); //ZA WARUDO 

            this.defaultBrightness = updateBrightness;
            if(isValidBrightness(defaultBrightness))
            {
                this.useDefBright = true;
            }
            else
            {
                this.useDefBright = false;
            }
            
            startThread(); //Time has begun to move again.
        }
        public bool updateSettings(AllSettings update)
        {
            stopThread();

            if (isValidRefreshRate(this.refreshRate) && update.getSetting("userrate").trueVal)
            {
                this.refreshRate = update.getSetting("rrate").trueVal;
            }
            else
            {
                this.refreshRate = 1000;
                this.useRR = false;
            }

            if (isValidBrightness(defaultBrightness))
            {
                try
                {
                    this.refreshRate = update.getSetting("usedbright").trueVal;
                }
                catch(Exception E)
                {
                    Console.WriteLine(E.ToString());
                    return false;
                }
            }
            else
            {
                useDefBright = false;
                this.defaultBrightness = 100;
            }
            startThread();

            return true;
        }

    }
}
