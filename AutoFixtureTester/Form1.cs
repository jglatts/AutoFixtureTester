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
        
        public Form1()
        {
            InitializeComponent();
            init();
        }

        private void init()
        {
            string[] port_names = SerialPort.GetPortNames();
            port = new SerialPort();
            port.WriteTimeout = 500;
            port.ReadTimeout = 500;
            port.BaudRate = 115200;
            txtBoxTestLogs.Text += DateTime.Now.ToString("hh:mm:ss") + ">> program loaded\n";
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
                MessageBox.Show("Device Not Found!");
                txtBoxTestLogs.Text += DateTime.Now.ToString("hh:mm:ss") + ">> device not found\n";
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
