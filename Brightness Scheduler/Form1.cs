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
        private bool skipslines = false;
        EnforcerThread running = null;

        private void Form1_Load(object sender, EventArgs e)
        {

            richTextBox1.AppendText("Data Log:\n\n");
            richTextBox2.AppendText("Program Log:\n\n");
            
            /* LOAD SETTINGS FROM FILE */
            FileManager fm1 = new FileManager(fileLoc, fileNameS);
            String[] rawData1 = null;
            consoleAppend(fm1.initialize(ref rawData1));

            //Pass off the raw data to globalSettings to be initialized
            globalSettings = new AllSettings(rawData1);
            applySettings(globalSettings);

            /* LOAD BRIGHTNESS REQUESTS FROM FILE */

            //Get BrightnessRequest data from file 
            FileManager fm0 = new FileManager(fileLoc, fileNameR);
            String[] rawData0 = null;
            consoleAppend(fm0.initialize(ref rawData0));
            
            loadRequests(rawData0);
            displayEvents();

            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);

            running = new EnforcerThread(requests);
            running.updateSettings(globalSettings);
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

            if(newReq != null && !newReq.isValid()) //Check if request is valid
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
                if(newReq.getStartTime().equals(BR.getStartTime()) || newReq.getEndTime().equals(BR.getEndTime()))
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
            if(skipslines == true)
            {
                richTextBox2.AppendText("\n");
            }
            richTextBox2.AppendText("> " + toDisplay + "\n");
            richTextBox2.ScrollToCaret();
        }

        public void consoleClear()
        {
            richTextBox2.Clear();
        }

        private void loadRequests(string[] rawData) //Loads requests from a string array to the BR list
        {
            requests.Clear();
            if(rawData == null)
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

            temp = toDisplay.getSetting("usedbright");
            if(temp != null && temp.trueVal) //If setting exists and is true
            {
                checkBox1.Checked = true;
                checkBox1.CheckState = CheckState.Indeterminate;

                temp = toDisplay.getSetting("dbright");
                if(temp == null) //If the user messes with the data file, this can happen
                {
                    label6.Text = "(Current: 100)";
                    updateSetting("dbright", "100");
                }
                else
                {
                    label6.Text = "(Current: " + temp.trueVal.ToString() + ")";
                }

            }
            else
            {
                checkBox1.Checked = false;
            }

            temp = toDisplay.getSetting("minwin");
            if(temp != null && temp.trueVal)
            {
                checkBox2.Checked = true;
                checkBox2.CheckState = CheckState.Indeterminate;
                this.WindowState = FormWindowState.Minimized;
            }
            else
            {
                checkBox2.Checked = false;
            }

            temp = toDisplay.getSetting("userrate");
            if(temp != null && temp.trueVal)
            {
                checkBox3.Checked = true;
                checkBox3.CheckState = CheckState.Indeterminate;
            }
            else
            {
                checkBox3.Checked = false;
            }

            temp = toDisplay.getSetting("skiplines");
            if(temp != null && temp.trueVal)
            {
                checkBox4.Checked = true;
                checkBox4.CheckState = CheckState.Indeterminate;
                skipslines = true;
            }
            else
            {
                checkBox4.Checked = false;
                skipslines = false;
            }

        }

        private void updateSetting(String name, String value)
        {
            //Add Setting to global list
            if(!globalSettings.overrideSetting(name, value))
            {
                settingsCreationError();
            }

            //Update EnforcerThread
            if(running != null)
            {
                running.updateSettings(globalSettings);
            }

            Setting temp = globalSettings.getSetting("skiplines");
            if (temp != null && temp.trueVal)
            {
                skipslines = true;
            }
            else
            {
                skipslines = false;
            }

            //Save Settings To file
            FileManager fm = new FileManager(fileLoc, fileNameS);
            fm.saveAllSettings(globalSettings);

        }

        private void displayEvents()
        {
            richTextBox1.Clear();
            richTextBox1.AppendText("Current Brightness Settings:\n");
            foreach(BrightnessRequest BR in requests)
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

        private void lockRecursive(Control toLock) // Locks elements under and including the element inputted
        {
            foreach(Control C in toLock.Controls)
            {
                lockRecursive(C);
            }
            toLock.Enabled = false;
        }

        private void clearConsole()
        {
            richTextBox2.Clear();
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

        private void CheckBox2_Click(object sender, EventArgs e)
        {
            if(checkBox2.Checked)
            {
                checkBox2.Checked = false;
                updateSetting("minwin", "false");
            }
            else
            {
                checkBox2.Checked = true;
                checkBox2.CheckState = CheckState.Indeterminate;
                updateSetting("minwin", "true");
            }
        }

        private void CheckBox1_Click(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                checkBox1.Checked = false;
                updateSetting("usedbright", "false");
            }
            else
            {
                checkBox1.Checked = true;
                checkBox1.CheckState = CheckState.Indeterminate;
                updateSetting("usedbright", "true");
            }
        }

        private void TextBox6_TextChanged(object sender, EventArgs e)
        {
            checkBox3.Enabled = false;
            checkBox3.Checked = false;
            updateSetting("userrate", "false");
            //TODO: CONTINUE
        }

        private void CheckBox3_Click(object sender, EventArgs e)
        {
            if(checkBox3.Checked)
            {
                checkBox3.Checked = false;
                updateSetting("userrate", "false");
            }
            else
            {
                checkBox3.Checked = true;
                checkBox3.CheckState = CheckState.Indeterminate;
                updateSetting("userrate", "true");
            }
        }

        private void CheckBox4_Click(object sender, EventArgs e)
        {
            if(checkBox4.Checked)
            {
                checkBox4.Checked = false;
                updateSetting("skiplines", "false");
            }
            else
            {
                checkBox4.Checked = true;
                checkBox4.CheckState = CheckState.Indeterminate;
                updateSetting("skiplines", "true");
            }
        }

        private void Form1_FormClosed_1(object sender, FormClosedEventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            running.stopThread();
        }
    }

}
