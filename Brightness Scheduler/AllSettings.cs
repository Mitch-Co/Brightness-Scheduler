using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Dimmer
{
    /* 
     * ALLSETTINGS INSTANCES SHOULD ONLY BE MODIFIED BY ADDING/REMOVING SETTINGS FROM THE LIST
     *
     * SETTINGS ARE RIGID, IF A SETTING NEEDS TO BE MODIFIED, CREATE A NEW ONE AND DELETE THE OLD ONE
     *
     * BOTH ALLSETTINGS AND SETTINGS ARE INITIALIZED UPON CREATION, NO INCOMPLETE ALLSETTINGS OR SETTINGS SHOULD EXIST
     * HOWEVER AN ALLSETTINGS INSTANCE WITH NO SETTINGS IS FINE 
     * 
     * IF A SETTINGS VARIABLE NEEDS TO BE ADDED, HARD CODE ITS NAME AND TYPE INTO THE FILLDICTIONARY FUNCTION
     * 
    */

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
            verifyAllSettings(this.listOfSettings);
        }

        private void fillDictionary() //Fills a dictionary of all settings and their valid types
        {
            validSettings.Add("rrate", typeof(int)); //RefreshRate 
            validSettings.Add("userrate", typeof(bool)); //Use RefreshRate

            validSettings.Add("dbright", typeof(int)); //Default brightness
            validSettings.Add("usedbright", typeof(bool)); //Use default brightness

            validSettings.Add("skiplines", typeof(bool)); //Skip console lines
            validSettings.Add("minwin", typeof(bool)); //Minimize window on launch
            validSettings.Add("startonlaunch", typeof(bool)); //Start on Windows launch
        }

        private void extractAllSettings(String[] data, List<Setting> toFill) //Extracts Settings from data file
        {
            foreach (String S in data)
            {
                toFill.Add(extractSetting(S));
            }
        }

        private Setting extractSetting(String line)
        {
            Setting toReturn = new Setting(line, AllSettings.validSettings);
            return toReturn;
        }

        private void verifyAllSettings(List<Setting> settingsList)
        {
            List<Setting> hitList = new List<Setting>();

            foreach(Setting S in settingsList)
            {
                if(!verifySetting(S))
                {
                    hitList.Add(S);
                }
            }

            foreach(Setting S in hitList)
            {
                settingsList.Remove(S);
            }
        }

        private bool verifySetting(Setting toCheck)
        {
            if(toCheck.isValid) //If valid return true
            {
                return true;
            }
            return false;
        }

        public Setting getSetting(String toFind)
        {
            foreach(Setting S in listOfSettings)
            {
                if (toFind == S.name)
                {
                    return S;
                }
            }

            return null;
        }

        public override String ToString() //ONLY USE FOR TESTING, MAY CRASH WHEN PRESENTED WITH UNUSUAL TYPES
        {
            String toReturn = null;
            foreach (Setting S in listOfSettings)
            {
                if(S.isValid)
                {
                    toReturn += S.name + " = " + S.trueVal.ToString() + "\n";
                }
            }
            return toReturn;
        }

        public bool overrideSetting(String name, String value)
        {
            return true; //TODO: THIS FUNCTION
        }
    }
}
