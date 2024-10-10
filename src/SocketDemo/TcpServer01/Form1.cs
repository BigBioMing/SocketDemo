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

namespace TcpServer01
{
    public partial class Form1 : Form
    {
        private TcpServer _server;
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            _server = new TcpServer();
            _server.Port = int.Parse(tsmiPort.Text);

            _server.Started += Server_Started;
            _server.Closed += Server_Closed;
            _server.ClientConnected += Server_ClientConnected;
            _server.ClientDisconnected += Server_ClientDisconnected;
            _server.ClientConnectionAcceptedHandle = (s, e) =>
              {
                  if (_server.Clients.Count() < 3)
                  {
                      return true;
                  }
                  else
                  {
                      Print($"服务器已满，拒绝客户端[{e.ClientSocket.RemoteEndPoint}]的连接请求");
                      return false;
                  }
              };
        }

        private void Server_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Print($"客户端[{e.Client.Host}:{e.Client.Port}]断开连接");
        }

        private void Server_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            Print($"客户端[{e.Client.Host}:{e.Client.Port}]连接成功");
            e.Client.OnDataReceived += Client_OnDataReceived;
            e.Client.UseHeartbeatTimeout(3000);//心跳检测
        }

        private void Client_OnDataReceived(object sender, STTech.BytesIO.Core.DataReceivedEventArgs e)
        {
            TcpClient tcpClient = (TcpClient)sender;
            Print($"来自客户端[{tcpClient.RemoteEndPoint}]的消息：{e.Data.EncodeToString("utf-8")}");

            foreach (TcpClient client in _server.Clients)
            {
                if (client != tcpClient)
                {
                    client.SendAsync(e.Data);
                }
            }
        }

        private void Server_Closed(object sender, EventArgs e)
        {
            Print("停止监听");
        }

        private void Server_Started(object sender, EventArgs e)
        {
            Print("开始监听");
        }

        private void tsmiStart_Click(object sender, EventArgs e)
        {
            _server.StartAsync();
        }

        private void tsmiClose_Click(object sender, EventArgs e)
        {
            _server.CloseAsync();
        }

        private void Print(string msg)
        {
            tbLog.AppendText($"[{DateTime.Now}] {msg}{Environment.NewLine}");
        }
    }
}
