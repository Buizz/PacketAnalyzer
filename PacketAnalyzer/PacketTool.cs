using SharpPcap;
using SharpPcap.WinDivert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PacketAnalyzer
{
    public class PacketTool
    {
        WinDivertDevice recvdevice = null;
        //WinDivertDevice senddevice = null;


        private List<string> ipbanlist = new List<string>();
        public void ClearIPBanList()
        {
            ipbanlist.Clear();
        }
        public void AddIPBanList(string ip)
        {
            ipbanlist.Add(ip);
        }



        public event EventHandler PacketReceive;




        private int datastart = 28;
        public string datafliter = "";

        public enum OrderID : ulong
        {
            LOBBY = 286392584,
            INGAME = 387055880,
            JOIN = 15671099656
        }

        public string Client_IP
        {
            get
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                string ClientIP = string.Empty;
                for (int i = 0; i < host.AddressList.Length; i++)
                {
                    if (host.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ClientIP = host.AddressList[i].ToString();
                    }
                }
                return ClientIP;
            }
        }


        string ip;
        public PacketTool()
        {
            recvdevice = new WinDivertDevice();
            //senddevice = new WinDivertDevice();

            ip = Client_IP;
            var ipuint32 = BitConverter.ToUInt32(IPAddress.Parse(ip).GetAddressBytes(), 0);



            int readTimeoutMilliseconds = 1000;

            //string filter = "(udp.DstPort == 6112 or udp.SrcPort == 6112) and ip.SrcAddr == " + ip;
            string filter = "(udp.DstPort == 6112 or udp.SrcPort == 6112)";
            recvdevice.Filter = filter;
            recvdevice.Flags = 0x01;
            /*
             * 0x01     WINDIVERT_FLAG_SNIFF
             * 0x02     WINDIVERT_FLAG_DROP
             * 0x04     WINDIVERT_FLAG_RECV_ONLY
             * 0x08     WINDIVERT_FLAG_READ_ONLY
             * 0x16     WINDIVERT_FLAG_SEND_ONLY
             * 0x32     WINDIVERT_FLAG_WRITE_ONLY
             */
            //senddevice.Filter = filter;
            //senddevice.Flags = 0x16;


            //senddevice.Open(mode: DeviceModes.Promiscuous | DeviceModes.DataTransferUdp | DeviceModes.NoCaptureLocal, read_timeout: readTimeoutMilliseconds);
            //senddevice.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
            //senddevice.StartCapture();

            recvdevice.Open(mode: DeviceModes.Promiscuous | DeviceModes.DataTransferUdp | DeviceModes.NoCaptureLocal, read_timeout: readTimeoutMilliseconds);
            //device.Open(mode: DeviceModes.DataTransferUdp, read_timeout: readTimeoutMilliseconds);
            recvdevice.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
            recvdevice.StartCapture();

            //Global.Program.log.AppendLog("WinDivert CaptureStart");
        }



        private struct packet
        {
            public OrderID orderID;

            public string sourceIP;
            public string destIP;

            public ulong tranID;

            public int datalen;

            const int datastart = 28;
            public packet(byte[] bytes)
            {
                datalen = bytes.Length;
                sourceIP = bytes[12] + "." + bytes[13]
                    + "." + bytes[14] + "." + bytes[15];
                destIP = bytes[16] + "." + bytes[17]
                    + "." + bytes[18] + "." + bytes[19];

                if (datalen < datastart + 16)
                {
                    orderID = 0;
                    tranID = 0;
                    return;
                }
                orderID = (OrderID)BitConverter.ToUInt64(bytes, datastart);
                tranID = BitConverter.ToUInt64(bytes, datastart + 8);

            }
        }

        private Dictionary<ulong, packet> packets = new Dictionary<ulong, packet>();
        private void device_OnPacketArrival(object sender, PacketCapture e)
        {



            var time = e.Header.Timeval.Date;
            var len = e.Data.Length;
            var rawPacket = e.GetPacket();

            byte[] bytes = rawPacket.Data;


            packet packet = new packet(bytes);

            //var sendpacket = PacketDotNet.Packet.ParsePacket(e.Device.LinkType, e.Data.ToArray());
            //recvdevice.SendPacket(sendpacket);

            //if (!ipbanlist.Contains(packet.destIP))
            //{
            //    var sendpacket = PacketDotNet.Packet.ParsePacket(e.Device.LinkType, e.Data.ToArray());
            //    device.SendPacket(sendpacket);
            //}

            string r = (bytes.Length - datastart).ToString("X4") + "\t";
            int index = datastart;
            for (int i = index; i < bytes.Length; i++)
            {
                r += bytes[i].ToString("X2");
                if (i % 2 == 1)
                {
                    r += "\t";
                }
            }

            List<string> datas = r.Split('\t').ToList();
            List<string> filters = datafliter.Split('\t').ToList();



            string rstr = "";

            for (int i = 0; i < datas.Count; i++)
            {
                if(filters.Count > i)
                {
                    if(!string.IsNullOrEmpty(filters[i]))
                    {
                        if(filters[i][0] == '!')
                        {
                            if (datas[i] == filters[i].Substring(1))
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (datas[i] != filters[i])
                            {
                                return;
                            }
                        }

                        
                    }
                }
            }


            //string orderid = ReadUInt64(bytes, ref index);
            //string ping = ReadUInt16(bytes, ref index);
            //string fix = ReadUInt16(bytes, ref index);

            //string ping1 = ReadUInt16(bytes, ref index);
            //string ping2 = ReadUInt16(bytes, ref index);


            //string datas = "";
            //for (int i = index; i < bytes.Length; i++)
            //{
            //    datas += bytes[i].ToString("X2") + " ";
            //}


            rstr = packet.sourceIP + "\t" + packet.destIP + "\t" + r + "\n";

            if(PacketReceive != null)
            {
                PacketReceive.Invoke(rstr, new EventArgs());
            }
        }

        private string ReadUInt64(byte[] bytes, ref int index)
        {
            if (bytes.Length < index + 8)
            {
                return "";
            }

            string r = BitConverter.ToUInt16(bytes, index).ToString("X8");

            index += 8;

            return r;
        }


        private string ReadUInt16(byte[] bytes, ref int index)
        {
            if(bytes.Length < index + 2)
            {
                return "";
            }

            string r = BitConverter.ToUInt16(bytes, index).ToString("X4");

            index += 2;

            return r;
        }












        private byte ReadByte(byte[] bytes, int pos)
        {
            int i = pos + datastart;

            if (bytes.Length <= i)
            {
                return 0;
            }


            return bytes[i];
        }
        private bool CheckBytes(byte[] bytes, int pos, byte[] value)
        {
            bool rbool = true;
            for (int i = 0; i < value.Count(); i++)
            {
                if (ReadByte(bytes, pos + i) != value[i])
                {
                    return false;
                }
            }


            return rbool;
        }
        private string ReadString(byte[] bytes, int pos)
        {
            List<byte> blist = new List<byte>();

            int i = 0;
            while (true)
            {
                byte b = ReadByte(bytes, pos + i++);

                if (b == 0)
                {
                    return System.Text.Encoding.UTF8.GetString(blist.ToArray());
                }

                blist.Add(b);
            }
        }





        public void Close()
        {
            recvdevice.StopCapture();
            recvdevice.Close();

            //senddevice.StopCapture();
            //senddevice.Close();
        }
    }
}
