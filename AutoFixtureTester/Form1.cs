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

        /// <summary>
        /// Constructor: Initializes the form, serial port, and UI components
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            initSerialPort();
            initUIComponents();
            startOpenTestCmd = 68;
            startShortTestCmd = 69;
            cmdStart = 71;
        }

        /// <summary>
        /// Initializes the UI components and writes initial log message
        /// </summary>
        private void initUIComponents()
        {
            txtBoxTestLogs.Text += DateTime.Now.ToString("hh:mm:ss") + ">> program loaded\n";
        }

        /// <summary>
        /// Initializes the serial port and opens the first available port
        /// </summary>
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

        /// <summary>
        /// Checks whether the serial port is open
        /// </summary>
        /// <returns>True if port is open, false if not</returns>
        private bool checkPort()
        {
            if (!port.IsOpen)
            {
                MessageBox.Show("Error!\nDevice Not Found!", "Z-Axis Connector");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sends a command to the device including start/end pins
        /// </summary>
        /// <param name="cmd">Command code to send</param>
        private void sendCmdInfo(int cmd)
        {
            port.Write($"{cmdStart} {dutStartPin} {dutEndPin} {cmd}\n");
        }

        /// <summary>
        /// Event handler to run a full test (open + short)
        /// </summary>
        private void btnRunFullTest_Click(object sender, EventArgs e)
        {
            testResult = new Result();

            if (!checkUserInput())
                return;

            if (!checkPort())
                return;

            setUIForTest();
            sendCmdInfo(startOpenTestCmd);
            waitForSerialData(checkOpenData);

            setUIForTest();
            sendCmdInfo(startShortTestCmd);
            waitForSerialData(checkShortData);

            printTestFailures();
        }

        /// <summary>
        /// Sends a test command and shows the returned response (used for communication tests)
        /// </summary>
        /// <param name="cmd">Command to send</param>
        private void testComms(int cmd)
        {
            port.WriteLine(cmd + "");
            string ret = port.ReadLine();
            MessageBox.Show(ret);
        }

        /// <summary>
        /// Event handler to run the short test only
        /// </summary>
        private void btnRunShortTest_Click(object sender, EventArgs e)
        {
            runShortTest();
        }

        /// <summary>
        /// Executes a short-circuit test on the DUT pins
        /// </summary>
        /// <returns>True if test executed, false if input/port error</returns>
        private bool runShortTest()
        {
            bool ret = false;
            testResult = new Result();

            if (!checkUserInput())
                return false;

            if (!checkPort())
                return false;

            setUIForTest();
            sendCmdInfo(startShortTestCmd);
            waitForSerialData(checkShortData);
            printTestFailures();

            return ret;
        }

        /// <summary>
        /// Finds the TextBox control corresponding to a DUT pin
        /// </summary>
        /// <param name="pin">Pin number</param>
        /// <returns>TextBox for that pin</returns>
        private TextBox getPinTextBox(int pin)
        {
            TextBox ret = null;
            Control[] matches = this.Controls.Find("textBoxPin" + pin, true);

            if (matches.Length > 0 && matches[0] is TextBox)
            {
                ret = (TextBox)matches[0];
            }
            else
            {
                MessageBox.Show($"No control named {"textBoxPin" + pin} found.");
            }

            return ret;
        }

        /// <summary>
        /// Handles a detected short between pins and updates UI/logs
        /// </summary>
        /// <param name="data">Serial data array</param>
        /// <param name="pin">Pin number being checked</param>
        private void handleShort(string[] data, int pin)
        {
            try
            {
                int short_pin = Int32.Parse(data[3].Trim());
                getPinTextBox(pin).BackColor = Color.Red;
                getPinTextBox(short_pin).BackColor = Color.Red;
                testResult.hasFailure = true;
                testResult.failures.Add("pin " + pin + " shorted with pin " + short_pin);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Processes a single line of serial data for short-circuit test
        /// </summary>
        /// <param name="line">CSV string from serial port</param>
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
                    getPinTextBox(pin).BackColor = Color.Green;
                }
                else
                {
                    handleShort(data, pin);
                }
            }
        }

        /// <summary>
        /// Sets the UI pins to default colors before a test
        /// </summary>
        private void setUIForTest()
        {
            // rest tp gray
            for (int i = 1; i < 51; i++)
                getPinTextBox(i).BackColor = Color.Gray;

            // set the user-supplied test pins to green
            for (int i = dutStartPin; i < dutEndPin + 1; i++)
                getPinTextBox(i).BackColor = Color.Green;
        }

        /// <summary>
        /// Event handler to run open-circuit test only
        /// </summary>
        private void btnRunOpenTest_Click(object sender, EventArgs e)
        {
            runOpenTest();
        }

        /// <summary>
        /// Executes an open-circuit test on the DUT pins
        /// </summary>
        /// <returns>True if test executed, false if input/port error</returns>
        private bool runOpenTest()
        {
            bool ret = false;
            testResult = new Result();

            if (!checkUserInput())
                return false;

            if (!checkPort())
                return false;

            setUIForTest();
            sendCmdInfo(startOpenTestCmd);
            waitForSerialData(checkOpenData);
            printTestFailures();

            return ret;
        }

        /// <summary>
        /// Updates result and UI for a single open-circuit pin
        /// </summary>
        /// <param name="data">CSV data array from serial</param>
        private void handleOpenData(string[] data)
        {
            int pin = -1;

            Int32.TryParse(data[1], out pin);
            if (pin == -1)
            {
                MessageBox.Show("err in checkopendata");
                return;
            }

            TextBox tb = getPinTextBox(pin);
            if (data[2].Trim() == "passed")
            {
                tb.BackColor = Color.Green;
            }
            else
            {
                testResult.hasFailure = true;
                testResult.failures.Add("pin " + pin + " open-circuit");
                tb.BackColor = Color.DarkRed;
            }
        }

        /// <summary>
        /// Updates UI for short-circuit failure
        /// </summary>
        /// <param name="pin">First pin</param>
        /// <param name="short_pin">Shorted pin</param>
        private void updateUIForShortFailure(int pin, int short_pin)
        {
            try
            {
                getPinTextBox(pin).BackColor = Color.Red;
                getPinTextBox(short_pin).BackColor = Color.Red;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Processes a single line of serial data for open-circuit test
        /// </summary>
        /// <param name="line">CSV string from serial port</param>
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

        /// <summary>
        /// Waits for serial data and calls a callback for each line
        /// </summary>
        /// <param name="dataCallback">Delegate to handle incoming data lines</param>
        private void waitForSerialData(checkData dataCallback)
        {
            while (true)
            {
                try
                {
                    string dataLine = port.ReadLine();
                    if (dataLine.Trim() == "done")
                    {
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

        /// <summary>
        /// Prints test failures or success to UI
        /// </summary>
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
                        getPinTextBox(failed_pin).BackColor = Color.DarkRed;
                    else
                        updateUIForShortFailure(failed_pin, Int32.Parse(failed_pins[5]));
                }
                MessageBox.Show(msg);
            }
            else
            {
                MessageBox.Show("Test Passed");
            }
        }

        /// <summary>
        /// Validates user input for DUT start/end pins
        /// </summary>
        /// <returns>True if valid, false if invalid</returns>
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
