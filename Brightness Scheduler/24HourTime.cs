using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Dimmer
{
    class Time24Hours
    {
        private int Hours { get; set; }
        private int Minutes { get; set; }
        private int Seconds { get; set; }

        public Time24Hours()
        {
            Hours = 0;
            Minutes = 0;
            Seconds = 0;
        }
        public Time24Hours(int HH, int MM, int SS)
        {
            Hours = HH;
            Minutes = MM;
            Seconds = SS;
        }
        public Time24Hours(int HH, int MM)
        {
            Hours = HH;
            Minutes = MM;
            Seconds = 0;
        }
        public Time24Hours(int HH)
        {
            Hours = HH;
            Minutes = 0;
            Seconds = 0;
        }

        public static Time24Hours null24Time() //For when you don't want to throw null around, but need some form of null
        {
            Time24Hours nTime = new Time24Hours();
            nTime.Hours = -1;
            nTime.Minutes = -1;
            nTime.Seconds = -1;

            return nTime;
        }
        public bool isNull24() //For when you don't want to throw null around, but need some form of null
        {
            if(this.Hours == -1 || this.Minutes == -1 || this.Seconds == -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool isValid() //Checks if the current object is a valid 24 hour time.
        {
            if (this.Hours >= 0 && this.Hours < 24
              && this.Minutes >= 0 && this.Minutes <= 60
              && this.Seconds >= 0 && this.Seconds <= 60)
            {
                return true;
            }
            else return false;
        }

        public static bool isValid24HTime(String toTest, char splitBy)
        {

            if(toTest == null)
            {
                return false;
            }

            String[] split = toTest.Split(splitBy);

            int value = -1;

            foreach(string S in split)
            {
                if(!int.TryParse(S,out value))
                {
                    return false;
                }
                if(value < 0 || value > 60)
                {
                    return false;
                }
            }

            int.TryParse(split[0], out value);
            if (value > 24)
            {
                return false;
            }

            return true;
        }

        public static Time24Hours stringTo24HTime(String toConvert, char splitBy)
        {
            if (toConvert == null)
            {
                return null;
            }

            String[] split = toConvert.Split(splitBy);
            int[] data = new int[3]{0, 0, 0};

            if(split.Length != 3)
            {
                return null;
            }

            if(!int.TryParse(split[0], out data[0]) || !int.TryParse(split[1], out data[1]) || !int.TryParse(split[2], out data[2]))
            {
                return null;
            }

            Time24Hours toReturn = new Time24Hours();

            toReturn.Hours = data[0];
            toReturn.Minutes = data[1];
            toReturn.Seconds = data[2];

            if(!toReturn.isValid())
            {
                return null;
            }

            return toReturn;

        }

        public String toFileString()
        {
            return this.Hours + "," + this.Minutes + "," + this.Seconds;
        }
        public String toStringFull()
        {
            return this.Hours.ToString("00") + ":" + this.Minutes.ToString("00") + ":" + this.Seconds.ToString("00");
        }
        public String toString()
        {
            return this.Hours.ToString("00") + ":" + this.Minutes.ToString("00");
        }

        public bool fallsInbetween(Time24Hours A, Time24Hours B)
        {
            int AtoInt = (10000 * A.Hours) + (100 * A.Minutes) + (A.Seconds);
            int BtoInt = (10000 * B.Hours) + (100 * B.Minutes) + (B.Seconds);
            int thistoInt = (10000 * this.Hours) + (100 * this.Minutes) + (this.Seconds);

            if(AtoInt == BtoInt) //Should never happen
            {
                return false;
            }

            if (AtoInt < BtoInt) //Takes place in the same day cycle
            {
                if(thistoInt > AtoInt && thistoInt < BtoInt)
                {
                    return true;
                }
            }
            else //B takes place tomorrow 
            {
                if (thistoInt > AtoInt || thistoInt < BtoInt)
                {
                    return true;
                }
            }

            return false;
        }

        public bool equals(Time24Hours A)
        {
            int thistoInt = (10000 * this.Hours) + (100 * this.Minutes) + (this.Seconds);
            int AtoInt = (10000 * A.Hours) + (100 * A.Minutes) + (A.Seconds);

            if(thistoInt == AtoInt)
            {
                return true;
            }
            return false;
        }
    }
}
