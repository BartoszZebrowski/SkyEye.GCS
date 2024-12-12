﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyEye.Connector
{
    public interface IVideoReceiver
    {
        Task RunVideo();
        Action NewFrameRecived; 

    }
}
