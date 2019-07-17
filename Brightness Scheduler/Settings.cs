using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Dimmer
{
    class Setting
    {
        private String lineVal { get; set; }

        private String name { get; set; }

        private String valueStr { get; set; }

        private dynamic trueVal { get; }

        private bool isValid { get; set; }

        public Setting(String line, Dictionary<String, Type> verificationDic) //Input is the line of the "settings.txt" file
        {

            this.lineVal = line == null ? null : line.ToLower(); 
            this.trueVal = initialize(line, verificationDic);
            
        }

        private dynamic initialize(String line, Dictionary<String, Type> verificationDic)
        {
            if(verificationDic == null || line == null)
            {
                return null;
            }

            dynamic toReturn = null;
            try
            {
                String[] splitLine = line.Split(':');
                if(splitLine.Length != 2)
                {
                    return null;
                }

                this.name = splitLine[0];
                this.valueStr = splitLine[1];

                foreach(KeyValuePair<String, Type> kvp in verificationDic)
                {
                    if(kvp.Key.Equals(splitLine[0]))
                    {
                        toReturn = Convert.ChangeType(splitLine[0], kvp.Value);
                    }
                }
                
            }
            catch(Exception E)
            {
                Console.Out.WriteLine(E);
                this.isValid = false;
                return null;
            }

            this.isValid = true;
            return toReturn;
            
        }

    }
}
