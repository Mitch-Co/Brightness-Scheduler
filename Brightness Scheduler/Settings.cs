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

        private String valueStr { get; }

        private dynamic trueVal { get; }

        private bool isValid { get; }

        public Setting(String line) //Input is the line of the "settings.txt" file
        {
            this.lineVal = line;
            this.trueVal = getTrueValue(line); 
            
        }

        private dynamic getTrueValue(String line)
        {
            try
            {
                String[] splitLine = line.Split(':');
            }
            catch(Exception E)
            {

                Console.Out.WriteLine(E);
            }
            return false;
        }

    }
}
