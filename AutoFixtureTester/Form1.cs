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

namespace AutoFixtureTester
{
    public partial class Form1 : Form
    {
        private SerialPort port;
        private int dutStartPin;
        private int dutEndPin;
        private readonly int startTestCmd;

        public Form1()
        {
            InitializeComponent();
            initSerialPort();
            initUIComponents();
            startTestCmd = 69;
        }

        private void initUIComponents()
        { 
            txtBoxTestLogs.Text += DateTime.Now.ToString("hh:mm:ss") + ">> program loaded\n";
        }

        private void initSerialPort()
        {
            string[] port_names = SerialPort.GetPortNames();
            port = new SerialPort();
            port.WriteTimeout = 1500;
            port.ReadTimeout = 1500;
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

        private void btnRunFullTest_Click(object sender, EventArgs e)
        {
            if (!checkUserInput())
                return;

            if (!checkPort())
                return;

            testComms();

            if (!runOpenTest())
                return;

            runShortTest();
        }

        private void testComms()
        {
            port.WriteLine(startTestCmd + "");
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
            
            return ret;
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
            
            return ret;
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
