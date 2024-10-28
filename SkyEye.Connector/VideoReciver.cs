using Gst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector
{
    public class VideoReciver : IVideoReceiver
    {
        public VideoReciver(string addressIp, int port) 
        {
            RunVideo();
        }

        private void RunVideo()
        {
            



        }
    }
}
