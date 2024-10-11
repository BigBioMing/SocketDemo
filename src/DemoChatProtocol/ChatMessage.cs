using STTech.BytesIO.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoChatProtocol
{
    public class ChatMessageRequest : IRequest
    {
        public ChatMessageType Type { get; set; }
        public ushort DataLength => (ushort)Data.Length;
        public ushort ArgsLength => (ushort)Args.Length;
        public byte[] Data { get; set; } = new byte[0];
        public byte[] Args { get; set; } = new byte[0];

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)Type);
            bytes.AddRange(BitConverter.GetBytes(DataLength));
            bytes.AddRange(BitConverter.GetBytes(ArgsLength));
            bytes.AddRange(Data);
            bytes.AddRange(Args);
            return bytes.ToArray();
        }
    }
    public class ChatMessageResponse : Response
    {
        public ChatMessageResponse(byte[] bytes) : base(bytes)
        {
            var array = bytes.ToArray();
            Type = (ChatMessageType)array[0];
            DataLength = BitConverter.ToUInt16(array, 1);
            ArgsLength = BitConverter.ToUInt16(array, 3);
            Data = array.Skip(5).Take(DataLength).ToArray();
            Args = array.Skip(5 + DataLength).Take(ArgsLength).ToArray();
        }

        public ChatMessageType Type { get; }
        public ushort DataLength { get; }
        public ushort ArgsLength { get; }
        public byte[] Data { get; }
        public byte[] Args { get; }
    }

    public enum ChatMessageType
    {
        Text = 0,
        FileInfo = 1,
        FileContent = 2,
        FileEnd = 3,
        Shake = 4
    }
}
