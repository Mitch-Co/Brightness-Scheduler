using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Dimmer
{
    class AllSettings
    {
        private List<Setting> listOfSettings = new List<Setting>();

        private static Dictionary<String, Type> validSettings = new Dictionary<String, Type>();

        private String[] fileData;
        public AllSettings(String[] fromFile)
        {
            fillDictionary();
            this.fileData = fromFile;
            extractAllSettings(this.fileData, this.listOfSettings);
            verifyAllSettings(ref this.listOfSettings);
        }

        private void fillDictionary() //Fills a dictionary of all settings and their valid types
        {

            validSettings.Add("rrate", typeof(int)); //RefreshRate 
            validSettings.Add("userrate", typeof(bool)); //Use RefreshRate

            validSettings.Add("dbright", typeof(int)); //Default brightness
            validSettings.Add("usedbright", typeof(bool)); //Use default brightness

            validSettings.Add("skiplines", typeof(bool)); //Skip console lines
            validSettings.Add("startonlaunch", typeof(bool)); //Start on Windows launch
        }

        void extractAllSettings(String[] data, List<Setting> toFill) //Extracts Settings from
        {

        }

        private void verifyAllSettings(ref List<Setting> toFill)
        {

        }
    }
}
