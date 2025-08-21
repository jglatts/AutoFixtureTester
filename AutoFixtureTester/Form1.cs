/**
*
*       Automatic Cable Tester
*       User Interface
*       
*       Author: John Glatts
*       Date:   8/21/2025
*/
using System;
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
 *  get nice working system in single thread
 *  then branch of test sequences in sep. thread
 */

namespace AutoFixtureTester
{
    public partial class Form1 : Form
    {
        private SerialPort port;
        private int dutStartPin;
        private int dutEndPin;
        private readonly int startOpenTestCmd;
        private readonly int startShortTestCmd;
        private readonly int startFullTestCmd;
        private readonly int cmdStart;

        public delegate void checkData(string line);

        public Form1()
        {
            InitializeComponent();
            initSerialPort();
            initUIComponents();
            startOpenTestCmd = 68;
            startShortTestCmd = 69;
            startFullTestCmd = 70;
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
            if (!checkUserInput())
                return;

            if (!checkPort())
                return;

            sendCmdInfo(startFullTestCmd);
            //runFullTest();
        }

        private void runFullTest()
        { 
            // will need data processing here
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

            if (!checkUserInput())
                return false;

            if (!checkPort())
                return false;

            setUIForTest();
            sendCmdInfo(startShortTestCmd);
            waitForSerialData(checkShortData);

            return ret;
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
                else
                {
                    // better indexing here
                    int short_pin = Int32.Parse(data[3].Trim());
                    Control[] testPin = this.Controls.Find("textBoxPin" + pin, true);
                    Control[] shortPin = this.Controls.Find("textBoxPin" + short_pin, true);
                    if (testPin.Length > 0 && testPin[0] is TextBox tb)
                    {
                        tb.BackColor = Color.Red;
                    }
                    if (shortPin.Length > 0 && shortPin[0] is TextBox t)
                    {
                        t.BackColor = Color.Red;
                    }
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
        }

        private void btnRunOpenTest_Click(object sender, EventArgs e)
        {
            runOpenTest(); 
        }

        private bool runOpenTest()
        {
            bool ret = false;

            if (!checkUserInput())
                return false;

            if (!checkPort())
                return false;

            setUIForTest();
            sendCmdInfo(startOpenTestCmd);
            waitForSerialData(checkOpenData);

            return ret;
        }

        private void checkOpenData(string line) 
        {
            string[] data = line.Split(',');
            int pin = -1;
            if (data.Length >= 3)
            {
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
                        tb.BackColor = Color.Red;
                }
                else
                {
                    MessageBox.Show($"No control named {"textBoxPin" + pin} found.");
                }
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
                    string check = port.ReadLine();
                    if (check.Trim() == "done")
                        return;
                    dataCallback(check);
                    txtBoxTestLogs.Text += DateTime.Now.ToString("hh:mm:ss") + ">> " + check + "\n";
                }
                catch (TimeoutException)
                {
                    txtBoxTestLogs.Text += DateTime.Now.ToString("hh:mm:ss") + ">> timeout exp\n";
                    continue;
                }
            }
        }

        private bool checkUserInput()
        {
            if (!Int32.TryParse(txtBoxPinStart.Text, out dutStartPin))
            {
                MessageBox.Show("Error!\nDUT Start Pin Must Be An Integer", "Z-Axis Connector Company");
                return false;
            }

            if (dutStartPin < 1 || dutStartPin > 50)
            {
                MessageBox.Show("Error!\nDUT Start Pin Must Be 1<X<50", "Z-Axis Connector Company");
                return false;
            }

            if (!Int32.TryParse(txtBoxPinEnd.Text, out dutEndPin))
            {
                MessageBox.Show("Error!\nDUT End Pin Must Be An Integer", "Z-Axis Connector Company");
                return false;
            }

            if (dutStartPin < 1 || dutEndPin > 50 || dutEndPin <= dutStartPin)
            {
                MessageBox.Show("Error!\nDUT Start End Pin Must Be 50>Y>X>1", "Z-Axis Connector Company");
                return false;
            }

            return true;
        }
    }
}
