using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Dimmer
{
    class BrightnessRequest
    {
        public int orderNum { get; set; }
        private Time24Hours startTime { get; set; }
        private Time24Hours endTime { get; set; }
        private int brightness { get; set; }

        public BrightnessRequest()
        {
            this.orderNum = 0;
            this.startTime = new Time24Hours();
            this.endTime = new Time24Hours();
            this.brightness = 1; //We never want the light level to default to 0%
        }

        public String toFileString()
        {
            return startTime.toFileString() + ";" + endTime.toFileString() + ";" + brightness.ToString();
        }

        public String toString()
        {
            return orderNum + ". From " + startTime.toString() + " to " + endTime.toString() + " - " + brightness.ToString() + "%";
        }

        public bool isValid()
        {
            if(this.startTime == null || this.endTime == null)
            {
                return false;
            }

            if(this.orderNum >= 0 && this.startTime.isValid() && this.endTime.isValid() && this.brightness >= 0 && this.brightness <= 100)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static BrightnessRequest fromString(int orderN, String toConvert)
        {
            if(toConvert == null)
            {
                return null;
            }

            BrightnessRequest toReturn = new BrightnessRequest();

            toReturn.orderNum = orderN;

            String[] split = toConvert.Split(';');

            if(split.Length != 3)
            {
                return null;
            }

            Time24Hours startT = Time24Hours.stringTo24HTime(split[0],',');
            Time24Hours endT = Time24Hours.stringTo24HTime(split[1], ',');
            int lightL = 1; //We never want the light level to default to 0%

            if(!int.TryParse(split[2],out lightL))
            {
                return null;
            }
            if (lightL < 0 || lightL > 100)
            {
                return null;
            }
            if(startT == null || endT == null)
            {
                return null;
            }
            if(!startT.isValid() || !endT.isValid())
            {
                return null;
            }

            toReturn.startTime = startT;
            toReturn.endTime = endT;
            toReturn.brightness = lightL;

            return toReturn;
        }
        public Time24Hours getStartTime()
        {
            return this.startTime;
        }
        public Time24Hours getEndTime()
        {
            return this.endTime;
        }

        public int getBrightness()
        {
            return this.brightness;
        }
    }


}
