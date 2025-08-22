/**
*
*       Automatic Cable Tester
*       User Interface
*       
*       Author: John Glatts
*       Date:   8/21/2025
*/
using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

/*
 *  
 *  Nice working PoC System
 *  Refactor :)
 *  
 */

namespace AutoFixtureTester
{
    public partial class Form1 : Form
    {
        private SerialPort port;
        private Result testResult;
        private int dutStartPin;
        private int dutEndPin;
        private readonly int startOpenTestCmd;
        private readonly int startShortTestCmd;
        private readonly int cmdStart;

        public delegate void checkData(string line);

        public Form1()
        {
            InitializeComponent();
            initSerialPort();
            initUIComponents();
            startOpenTestCmd = 68;
            startShortTestCmd = 69;
            cmdStart = 71;
        }

        private void initUIComponents()
        { 
            txtBoxTestLogs.Text += DateTime.Now.ToString("hh:mm:ss") + ">> program loaded\n";
        }

        private void initSerialPort()
        {
            string[] port_names = SerialPort.GetPortNames();
            port = new SerialPort();
            port.WriteTimeout = 5000;
            port.ReadTimeout = 5000;
            port.BaudRate = 115200;
            if (port_names.Length != 0)
            {
                port.PortName = port_names[0];
                try
                {
                    port.Open();
                    txtBoxTestLogs.Text += DateTime.Now.ToString("hh:mm:ss") + ">> device opened\n";
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.ToString());
                }
            }
            else
            {
                txtBoxTestLogs.Text += DateTime.Now.ToString("hh:mm:ss") + ">> device not found\n";
            }
        }

        private bool checkPort()
        {
            if (!port.IsOpen)
            {
                MessageBox.Show("Error!\nDevice Not Found!", "Z-Axis Connector");
                return false;
            }

            return true;
        }

        private void sendCmdInfo(int cmd) 
        {
            port.Write($"{cmdStart} {dutStartPin} {dutEndPin} {cmd}\n");
        }

        private void btnRunFullTest_Click(object sender, EventArgs e)
        {
            testResult = new Result();

            if (!checkUserInput())
                return;

            if (!checkPort())
                return;

            MessageBox.Show("Running Open Test");
            setUIForTest();
            sendCmdInfo(startOpenTestCmd);
            waitForSerialData(checkOpenData);

            MessageBox.Show("Running Short Test");
            setUIForTest();
            sendCmdInfo(startShortTestCmd);
            waitForSerialData(checkShortData);

            printTestFailures();
        }

        private void testComms(int cmd)
        {
            port.WriteLine(cmd + "");
            string ret = port.ReadLine();
            MessageBox.Show(ret);
        }

        private void btnRunShortTest_Click(object sender, EventArgs e)
        {
            runShortTest();
        }

        private bool runShortTest() 
        {
            bool ret = false;
            testResult = new Result();

            if (!checkUserInput())
                return false;

            if (!checkPort())
                return false;

            MessageBox.Show("Running Short Test");
            setUIForTest();
            sendCmdInfo(startShortTestCmd);
            waitForSerialData(checkShortData);
            printTestFailures();

            return ret;
        }

        private void handleNoShort(int pin)
        {
            Control[] matches = this.Controls.Find("textBoxPin" + pin, true);
            if (matches.Length > 0 && matches[0] is TextBox tb)
            {
                tb.BackColor = Color.Green;
            }
            else
            {
                MessageBox.Show($"No control named {"textBoxPin" + pin} found.");
            }
        }

        private void handleShort(string[] data, int pin)
        {
            try
            {
                int short_pin = Int32.Parse(data[3].Trim());
                Control[] testPin = this.Controls.Find("textBoxPin" + pin, true);
                Control[] shortPin = this.Controls.Find("textBoxPin" + short_pin, true);
                if (testPin.Length > 0 && testPin[0] is TextBox tb)
                    tb.BackColor = Color.Red;
                if (shortPin.Length > 0 && shortPin[0] is TextBox t)
                    t.BackColor = Color.Red;
                testResult.hasFailure = true;
                testResult.failures.Add("pin " + pin + " shorted with pin " + short_pin);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void checkShortData(string line)
        {
            string[] data = line.Trim().Split(',');
            int pin = -1;
            if (data.Length >= 3)
            {
                Int32.TryParse(data[1], out pin);
                if (pin == -1)
                {
                    MessageBox.Show("err in checkshortdata");
                    return;
                }

                if (data[2].Trim() == "noshort")
                {
                    handleNoShort(pin);
                }
                else
                {
                    handleShort(data, pin);
                }
            }
        }

        private void setUIForTest() {
            for (int i = 1; i < 51; i++)
            {
                Control[] matches = this.Controls.Find("textBoxPin" + i, true);
                if (matches.Length > 0 && matches[0] is TextBox tb)
                {
                    tb.BackColor = Color.Gray;
                }
                else
                {
                    MessageBox.Show($"No control named {"textBoxPin" + i} found.");
                }
            }

            for (int i = dutStartPin; i < dutEndPin+1; i++)
            {
                Control[] matches = this.Controls.Find("textBoxPin" + i, true);
                if (matches.Length > 0 && matches[0] is TextBox tb)
                {
                    tb.BackColor = Color.Green; 
                }
                else
                {
                    MessageBox.Show($"No control named {"textBoxPin" + i} found.");
                }
            }

            // check for failures here and set UI

        }

        private void btnRunOpenTest_Click(object sender, EventArgs e)
        {
            runOpenTest(); 
        }

        private bool runOpenTest()
        {
            bool ret = false;
            testResult = new Result();

            if (!checkUserInput())
                return false;

            if (!checkPort())
                return false;

            MessageBox.Show("Running Open Test");
            setUIForTest();
            sendCmdInfo(startOpenTestCmd);
            waitForSerialData(checkOpenData);
            printTestFailures();

            return ret;
        }


        private void handleOpenData(string[] data)
        {
            int pin = -1;

            Int32.TryParse(data[1], out pin);
            if (pin == -1)
            {
                MessageBox.Show("err in checkopendata");
                return;
            }

            Control[] matches = this.Controls.Find("textBoxPin" + pin, true);
            if (matches.Length > 0 && matches[0] is TextBox tb)
            {
                if (data[2].Trim() == "passed")
                    tb.BackColor = Color.Green;
                else
                {
                    testResult.hasFailure = true;
                    testResult.failures.Add("pin " + pin + " open-circuit");
                    tb.BackColor = Color.DarkRed;
                }
            }
            else
            {
                MessageBox.Show($"No control named {"textBoxPin" + pin} found.");
            }
        }

        private void updateUIForOpenFailure(int pin) 
        {
            Control[] matches = this.Controls.Find("textBoxPin" + pin, true);
            if (matches.Length > 0 && matches[0] is TextBox tb)
                tb.BackColor = Color.DarkRed;
        }

        private void updateUIForShortFailure(int pin, int short_pin)
        {
            try
            {
                Control[] testPin = this.Controls.Find("textBoxPin" + pin, true);
                Control[] shortPin = this.Controls.Find("textBoxPin" + short_pin, true);
                if (testPin.Length > 0 && testPin[0] is TextBox tb)
                    tb.BackColor = Color.Red;
                if (shortPin.Length > 0 && shortPin[0] is TextBox t)
                    t.BackColor = Color.Red;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void checkOpenData(string line) 
        {
            string[] data = line.Split(',');
            if (data.Length >= 3)
            {
                handleOpenData(data);    
            }
            else
            {
                MessageBox.Show($"err with data\n{line}");
            }
        }

        private void waitForSerialData(checkData dataCallback) {
            while (true)
            {
                try
                {
                    string dataLine = port.ReadLine();
                    if (dataLine.Trim() == "done")
                    {
                        // can put result processing here 
                        txtBoxTestLogs.Text += "\n";
                        return;
                    }
                    else
                    {
                        dataCallback(dataLine);
                        txtBoxTestLogs.Text += DateTime.Now.ToString("hh:mm:ss") + ">> " + dataLine + "\n";
                    }
                }
                catch (TimeoutException)
                {
                    txtBoxTestLogs.Text += DateTime.Now.ToString("hh:mm:ss") + ">> timeout exp\n";
                    continue;
                }

                txtBoxTestLogs.SelectionStart = txtBoxTestLogs.Text.Length;
                txtBoxTestLogs.ScrollToCaret();
            }
        }

        private void printTestFailures()
        {
            if (testResult.hasFailure)
            {
                string msg = "";
                foreach (string failure in testResult.failures)
                {
                    string[] failed_pins = failure.Split(' ');
                    int failed_pin = Int32.Parse(failed_pins[1]);
                    msg += failure + "\n";
                    if (failed_pins[2] == "open-circuit")
                        updateUIForOpenFailure(failed_pin);
                    else
                        updateUIForShortFailure(failed_pin, Int32.Parse(failed_pins[5]));  
                }
                MessageBox.Show(msg);
            }
        }

        private bool checkUserInput()
        {
            if (!Int32.TryParse(txtBoxPinStart.Text, out dutStartPin))
            {
                MessageBox.Show("Error!\nDUT Start Pin Must Be An Integer", "Z-Axis Connector Company");
                return false;
            }

            if (dutStartPin < 1 || dutStartPin > 40)
            {
                MessageBox.Show("Error!\nDUT Start Pin Must Be 1<X<40", "Z-Axis Connector Company");
                return false;
            }

            if (!Int32.TryParse(txtBoxPinEnd.Text, out dutEndPin))
            {
                MessageBox.Show("Error!\nDUT End Pin Must Be An Integer", "Z-Axis Connector Company");
                return false;
            }

            if (dutStartPin < 1 || dutEndPin > 40 || dutEndPin <= dutStartPin)
            {
                MessageBox.Show("Error!\nDUT Start End Pin Must Be 40>Y>X>1", "Z-Axis Connector Company");
                return false;
            }

            return true;
        }
    }
}
