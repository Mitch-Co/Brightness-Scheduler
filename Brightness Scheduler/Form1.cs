using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace Auto_Dimmer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static String fileNameR = "requests.dat";
        private static String fileNameS = "settings.dat";
        private static String fileLoc = @"./";

        private List<BrightnessRequest> requests = new List<BrightnessRequest>();
        private AllSettings globalSettings = null;

        EnforcerThread running;

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.AppendText("Data Log:\n");
            richTextBox2.AppendText("Program Log:\n");
            
            /* LOAD SETTINGS FROM FILE */
            FileManager fm1 = new FileManager(fileLoc, fileNameS);
            String[] rawData1 = null;
            consoleAppend(fm1.initialize(ref rawData1));

            //Pass off the raw data to globalSettings to be initialized
            globalSettings = new AllSettings(rawData1);

            applySettings(globalSettings);

            /* LOAD BRIGHTNESS REQUESTS FROM FILE */

            OHGODSHUTITDOWN("YEEET", false);

            //Get BrightnessRequest data from file 
            FileManager fm0 = new FileManager(fileLoc, fileNameR);
            String[] rawData0 = null;
            consoleAppend(fm0.initialize(ref rawData0));
            
            loadRequests(rawData0);
            displayEvents();

            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);

            running = new EnforcerThread(requests);

            running.startThread();
        }

         /*
          * Button1_Click:
          * When a new request is added, the changes are first reflected in the savefile, 
          * then the form is updated based on the savefile. There is likely a better way to do this,
          * but it ensures the save file is always up to date.
         */
        private void Button1_Click(object sender, EventArgs e) //As soon as the BrightnessRequest is added, changes are reflected in the file
        {
            running.stopThread();
            FileManager fm = new FileManager(fileLoc, fileNameR);
            String[] rawData = null;
            double inputBrightness = 1.0;

            //If the input is not valid
            if(!Time24Hours.isValid24HTime(textBox1.Text, ':') || !Time24Hours.isValid24HTime(textBox2.Text, ':') || !double.TryParse(textBox3.Text,out inputBrightness))
            {
                inputError1();
                return;
            }

            //Convert user input to time values
            Time24Hours startT = Time24Hours.stringTo24HTime(textBox1.Text + ":00", ':');
            Time24Hours endT = Time24Hours.stringTo24HTime(textBox2.Text + ":00", ':');

            //Assemble request
            BrightnessRequest newReq = BrightnessRequest.fromString(0, startT.toFileString() + ";" + endT.toFileString() + ";" + inputBrightness.ToString());

            if (newReq != null && !newReq.isValid()) //Check if request is valid
            {
                inputError1();
                return;
            }


            foreach(BrightnessRequest BR in requests)
            {
                if(newReq.getStartTime().fallsInbetween(BR.getStartTime(),BR.getEndTime()) || newReq.getEndTime().fallsInbetween(BR.getStartTime(), BR.getEndTime()))
                {
                    inputError2(BR.orderNum);
                    return;
                }
                if (newReq.getStartTime().equals(BR.getStartTime()) || newReq.getEndTime().equals(BR.getEndTime()))
                {
                    inputError2(BR.orderNum);
                    return;
                }
            }
            requests.Add(newReq); //Add request to list

            fm.saveAllBR(requests); //Save all requests
            fm.initialize(ref rawData); //Load all requests to string
            loadRequests(rawData); //Overwrite
            displayEvents();

            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();

            running.startThread();

        }
        private void Button2_Click(object sender, EventArgs e) //Remove request from requestlist, modify data file to reflect changes
        {
            running.stopThread();
            FileManager fm = new FileManager(fileLoc, fileNameR);
            String[] rawData = null;
            int toRemove = 1;

            if(!int.TryParse(textBox4.Text,out toRemove))
            {
                consoleAppend("Please enter a valid number to remove");
                return;
            }

            BrightnessRequest toRem = null;
            foreach(BrightnessRequest BR in requests)
            {
                if(BR.orderNum == toRemove)
                {
                    toRem = BR;
                }
            }

            if(toRem == null)
            {
                consoleAppend("The number you have entered is not a valid setting number");
                return;
            }

            requests.Remove(toRem);

            fm.saveAllBR(requests); //Save all requests
            fm.initialize(ref rawData); //Load all requests to string
            loadRequests(rawData); //Overwrite
            displayEvents();

            consoleAppend("Brightness setting removed succesfully");
            running.startThread();
        }
        public void consoleAppend(String toDisplay)
        {
            richTextBox2.AppendText("\n> " + toDisplay + "\n");
            richTextBox2.ScrollToCaret();
        }

        public void consoleClear()
        {
            richTextBox2.Clear();
        }

        private void loadRequests(string[] rawData) //Loads requests from a string array to the BR list
        {
            requests.Clear();
            if (rawData == null)
            {
                return;
            }

            int count = 1;
            foreach(string s in rawData)
            {
                BrightnessRequest temp = BrightnessRequest.fromString(count, s);
                if(temp != null && temp.isValid())
                {
                    requests.Add(temp);
                    count++;
                }

            }
        }

        private void applySettings(AllSettings toDisplay)
        {
            Setting temp; //Stores setting so global setting list does not have to be traversed more than once per lookup

            //TODO: These if statements could be shortened using '?' operator, but they might look more confusing
            temp = toDisplay.getSetting("usedbright");
            if (temp != null && temp.trueVal) //If setting exists and is true
            {
                radioButton1.Checked = true;

                temp = toDisplay.getSetting("dbright");
                if(temp == null) //If the user messes with the data file, this can happen
                {
                    label6.Text = "(Current: 100)";
                    if(!toDisplay.overrideSetting("dbright", "100")) //if something has gone wrong generating and adding a new setting
                    {
                        settingsCreationError();
                    }
                }
                else
                {
                    label6.Text = "(Current: " + temp.trueVal.ToString() + ")";
                }

            }
            else
            {
                radioButton1.Checked = false;
            }

            temp = toDisplay.getSetting("minwin");
            if (temp != null && temp.trueVal)
            {
                radioButton2.Checked = true;
            }
            else
            {
                radioButton2.Checked = false;
            }

            temp = toDisplay.getSetting("userrate");
            if (temp != null && temp.trueVal)
            {
                radioButton3.Checked = true;
            }
            else
            {
                radioButton3.Checked = false;
            }

            temp = toDisplay.getSetting("skiplines");
            if (temp != null && temp.trueVal)
            {
                radioButton3.Checked = true;
            }
            else
            {
                radioButton3.Checked = false;
            }



        }

        private void displayEvents()
        {
            richTextBox1.Clear();
            richTextBox1.AppendText("Current Brightness Settings:\n");
            foreach (BrightnessRequest BR in requests)
            {
                richTextBox1.AppendText("\n" + BR.toString());
            }
        }

        private void inputError1()
        {
            consoleAppend("Invalid Time/Brightness Format!");
            consoleAppend("Time is entered in format HH:MM");
            consoleAppend("Brightness is a value between 1 and 100");
        }
        private void inputError2(int err)
        {
            consoleAppend("Make sure you don't have any conflicting events");
            consoleAppend("Event " + err + " and the one you entered are conflicting");
        }

        public void settingsCreationError()
        {
            OHGODSHUTITDOWN("Unable to generate new settings!", false);
        }

        public void OHGODSHUTITDOWN(String Err, bool LIKESEAROUSLYWENEEDTOSHUTITDOWNNOW) //Last resort shutdown
        {
            if(LIKESEAROUSLYWENEEDTOSHUTITDOWNNOW)
            {
                Environment.Exit(-1);
            }

            consoleClear();
            consoleAppend("A CRITICAL ERROR HAS OCCURED, PLEASE REINSTALL");
            consoleAppend("ERROR: " + Err);
            lockAll();
        }

        private void lockAll()
        {
            lockRecursive(this);
            this.Enabled = true;
        }

        private void lockRecursive(Control toLock) // Locks elements 
        {
            foreach(Control C in toLock.Controls)
            {
                lockRecursive(C);
            }
            toLock.Enabled = false;
        }

        private void Label3_Click(object sender, EventArgs e)
        {

        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(running != null)
            {
                running.stopThread();
            }
            
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            int updated = -1; //Initialization does not really matter (if TryParse fails, it sets updated to zero)

            if(int.TryParse(textBox5.Text, out updated) && updated >= 0 && updated <= 100)
            {
                consoleAppend("Brightness default set to " + updated.ToString());
                running.updateDefault(updated);
            }
            else
            {
                consoleAppend("Incorrect format for default brightness, must be a number 0-100");
            }

        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RadioButton4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RadioButton3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
