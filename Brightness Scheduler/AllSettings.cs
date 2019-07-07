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
            extractAllSettings();
            verifyAllSettings();
        }

        private void fillDictionary()
        {
            validSettings.Add("rrate", typeof(String));
            validSettings.Add("dbright", typeof(bool));
            validSettings.Add("usedbright",typeof(bool));
        }
    }
}
