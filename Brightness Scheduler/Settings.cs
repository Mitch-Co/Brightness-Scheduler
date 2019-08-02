using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auto_Dimmer
{
    class Setting
    {
        public String lineVal { get; private set; }

        public String name { get; private set; }

        public dynamic trueVal { get; private set; }

        public Type typeOf { get; private set; }

        public bool isValid { get; private set; }

        public Setting(String line, Dictionary<String, Type> verificationDic) //Input is the line of the "settings.txt" file
        {
            this.isValid = false;
            this.lineVal = line == null ? null : line.ToLower(); 
            this.trueVal = initializeFromLine(line, verificationDic);
        }

        private dynamic initializeFromLine(String line, Dictionary<String, Type> verificationDic)
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

                bool hasBeenSet = false;
                foreach (KeyValuePair<String, Type> kvp in verificationDic)
                {
                    if(kvp.Key == splitLine[0])
                    {
                        toReturn = Convert.ChangeType(splitLine[1], kvp.Value);
                        hasBeenSet = true;
                    }
                }
                
                if(!hasBeenSet)
                {
                    this.isValid = false;
                    return null;
                }
            }
            catch (Exception E)
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
