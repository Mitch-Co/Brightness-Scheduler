using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Auto_Dimmer
{
    class FileManager //A class for controlling brightnessRequests file management
    {
        String fileName { get; set; }

        String filePath { get; set; }

        public FileManager(String fileN, String FileP)
        {
            this.fileName = fileN;
            this.filePath = FileP;
        }
        public FileManager(String fileN)
        {
            this.fileName = fileN;
            this.filePath = @"./" + this.fileName;
        }

        public String initialize(ref String[] output) //Output can be edited without ref, but to initialize a new string array over output, a reference to the reference is needed
        {
            String toReturn = "ERROR - INITIALIZE FUNCTION HAS FAILED!!!";
            if(this.fileName == null || this.filePath == null)
            {
                output = null;
                return "ERROR - FILE NAME/PATH NOT SET!!!";
            }
            try
            {
                if (File.Exists(this.filePath))
                {
                    output = System.IO.File.ReadAllLines(this.filePath);
                    toReturn = "File " + this.fileName + " found and loaded";
                }
                else
                {
                    File.Create(this.filePath).Close(); //Closes file, similar to fclose() in c
                    toReturn = "Warning - File " + this.fileName + " not found; a new save file will be created";
                }
            }
            catch (Exception E)
            {
                toReturn = "File Initialization Failed! - " + E.ToString();
            }

            return toReturn;
        }
        public String saveAllBR(List<BrightnessRequest> requests)
        {
            String toReturn = "FILE SAVING ERROR!!!";
            List<BrightnessRequest> hitList = new List<BrightnessRequest>(); //TODO: Remove this list and use an iterator
            foreach (BrightnessRequest BR in requests)
            {
                if (!BR.isValid())
                {
                    hitList.Add(BR);
                }
            }
            foreach (BrightnessRequest toKill in hitList)
            {
                requests.Remove(toKill);
            }

            String[] reqsString = reqsToStrings(requests).ToArray();

            try
            {
                System.IO.File.WriteAllLines(this.filePath, reqsString);
                toReturn = "File Saved Successfully";
            }
            catch (Exception E)
            {
                toReturn = "FILE SAVING ERROR - " + E + "!!!";
            }
            return toReturn;

        }

        private List<String> reqsToStrings(List<BrightnessRequest> requests)
        {
            List<String> toReturn = new List<String>();
            foreach (BrightnessRequest req in requests)
            {
                String temp = "";
                temp = req.toFileString();
                if (temp != null)
                {
                    toReturn.Add(temp);
                }
            }
            return toReturn;
        }
    }


}
