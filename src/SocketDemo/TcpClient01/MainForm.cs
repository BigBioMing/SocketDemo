using STTech.BytesIO.Serial;
using STTech.BytesIO.Tcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpClient01
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void tsmiCreateTcpClient_Click(object sender, EventArgs e)
        {
            tab.AddPage("TCP客户端", new ClientPanel(new TcpClient() { Port = 60000 }));
        }

        private void tsmiCreateSerial_Click(object sender, EventArgs e)
        {
            tab.AddPage("串口客户端", new ClientPanel(new SerialClient()));
        }
    }
}
