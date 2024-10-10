using STTech.BytesIO.Core;
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
    public partial class ClientPanel : UserControl
    {
        private BytesClient _client;
        public ClientPanel()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        public ClientPanel(BytesClient client):this()
        {
            _client = client;
            propertyGrid.SelectedObject = _client;

            _client.OnDataReceived += Client_OnDataReceived;
            _client.OnConnectedSuccessfully += Client_OnConnectedSuccessfully;
            _client.OnDisconnected += Client_OnDisconnected;
            _client.OnDataSent += Client_OnDataSent; ;
        }

        private void Client_OnDataSent(object sender, DataSentEventArgs e)
        {
            string msg = e.Data.EncodeToString("utf-8");
            Print($"发送数据：{msg}");
        }

        private void Client_OnDisconnected(object sender, STTech.BytesIO.Core.DisconnectedEventArgs e)
        {
            Print($"连接已断开（{e.ReasonCode}）");
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_OnConnectedSuccessfully(object sender, STTech.BytesIO.Core.ConnectedSuccessfullyEventArgs e)
        {
            Print("连接成功");
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
        {
            string msg = e.Data.EncodeToString("utf-8");
            Print($"收到数据：{msg}");
        }

        /// <summary>
        /// 打开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            _client.Connect();
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _client.Disconnect();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnSend_Click(object sender, EventArgs e)
        {
            _client.Send(tbSend.Text.GetBytes("UTF-8"));
        }

        private void Print(string msg)
        {
            tbRecv.AppendText($"[{DateTime.Now}] {msg}{Environment.NewLine}");
        }
    }
}
