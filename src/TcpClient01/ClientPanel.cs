using DemoChatProtocol;
using Newtonsoft.Json;
using STTech.BytesIO.Core;
using STTech.BytesIO.Tcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpClient01
{
    public partial class ClientPanel : UserControl
    {
        private BytesClient _client;
        private Dictionary<string, FileStream> dictFileStreams = new Dictionary<string, FileStream>();
        public ClientPanel()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        public ClientPanel(BytesClient client) : this()
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
            //string msg = e.Data.EncodeToString("utf-8");
            //Print($"发送数据：{msg}");
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
            //string msg = e.Data.EncodeToString("utf-8");
            //Print($"收到数据：{msg}");

            string jsonString = e.Data.EncodeToString();
            ChatMessageResponse res = new ChatMessageResponse(e.Data);
            switch (res.Type)
            {
                case ChatMessageType.Text:
                    Print($"收到消息：{res.Data.EncodeToString()}");
                    break;
                case ChatMessageType.FileInfo:
                case ChatMessageType.FileContent:
                case ChatMessageType.FileEnd:
                    var fileName = res.Args.EncodeToString();
                    var filePath = Path.Combine(tbSavePath.Text + "/" + fileName);

                    Directory.CreateDirectory(tbSavePath.Text);
                    if (res.Type == ChatMessageType.FileInfo)
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        Print($"正在接收文件：{fileName}");
                        dictFileStreams[filePath]= new FileStream(filePath, FileMode.Append, FileAccess.Write);
                    }
                    else if (res.Type == ChatMessageType.FileContent)
                    {
                        FileStream fileStream = dictFileStreams[filePath];
                        lock (fileStream)
                        {
                            fileStream.Write(res.Data, 0, res.Data.Length);
                        }
                        Print($"接收到文件，存放在：{filePath}");
                    }
                    else if (res.Type == ChatMessageType.FileEnd)
                    {
                        FileStream fileStream = dictFileStreams[filePath];
                        fileStream.Close();
                        fileStream.Dispose();

                        Process.Start("explorer.exe", $"/select,{filePath}");
                        Print($"文件接收完毕：{filePath}");
                    }
                    break;
                case ChatMessageType.Shake:
                    this.ParentForm.Shake();
                    break;
            }
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
            string msg = tbSend.Text;
            ChatMessageRequest message = new ChatMessageRequest()
            {
                Type = ChatMessageType.Text,
                Data = msg.GetBytes("UTF-8")
            };
            _client.SendAsync(message.GetBytes());
            Print($"已发送消息：{msg}");
        }

        private void Print(string msg)
        {
            tbRecv.AppendText($"[{DateTime.Now}] {msg}{Environment.NewLine}");
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "All files (*.*)|*.*";
                //openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;
                    Task.Run(() => SendFile(filePath));
                }
            }
        }

        private void btnSendShake_Click(object sender, EventArgs e)
        {
            ChatMessageRequest message = new ChatMessageRequest()
            {
                Type = ChatMessageType.Shake
            };
            _client.SendAsync(message.GetBytes());

            Print($"已发送一个窗口抖动");
        }

        private void SendFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            long fileSize = new FileInfo(filePath).Length;

            ChatMessageRequest message = new ChatMessageRequest()
            {
                Type = ChatMessageType.FileInfo,
                Args = fileName.GetBytes()
            };
            _client.SendAsync(message.GetBytes());
            Print($"开始发送文件：{fileName}，总大小：{fileSize}字节");


            long sentCount = 0;
            using (FileStream fs = File.OpenRead(filePath))
            {
                byte[] buffer = new byte[10240];
                int readLen = 0;
                UTF8Encoding temp = new UTF8Encoding(true);
                while ((readLen = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Console.WriteLine(temp.GetString(buffer, 0, readLen));
                    Thread.Sleep(50);

                    sentCount += readLen;
                    ChatMessageRequest req2 = new ChatMessageRequest()
                    {
                        Type = ChatMessageType.FileContent,
                        Data = readLen == buffer.Length ? buffer : buffer.Take(readLen).ToArray(),
                        Args = fileName.GetBytes()
                    };
                    _client.SendAsync(req2.GetBytes());
                    Print($"正在发送文件：{fileName}，{sentCount * 100.0 / fileSize}%");
                }
            }

            Thread.Sleep(50);
            ChatMessageRequest req3 = new ChatMessageRequest()
            {
                Type = ChatMessageType.FileEnd,
                Args = fileName.GetBytes()
            };
            _client.SendAsync(req3.GetBytes());

            Print($"文件发送完毕：{Path.GetFileName(filePath)}");
        }
    }
}
