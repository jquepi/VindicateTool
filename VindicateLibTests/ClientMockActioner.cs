/*
    Vindicate - An LLMNR/NBNS/mDNS Spoofing Detection Toolkit
    Copyright (C) 2017 Danny Moules

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VindicateLib.Interfaces;

namespace VindicateLibTests
{
    public class ClientMockActioner : IClientActioner
    {
        public Byte[] LastSendDatagram;
        public String LastSendHostname;
        public Int32 LastSendPort;

        public Byte[] ReceiveBuffer = null;
        public IPEndPoint ReceiveEndPoint = null;

        public void Send(Socket client, Byte[] datagram, String hostname, Int32 port)
        {
            LastSendDatagram = datagram;
            LastSendHostname = hostname;
            LastSendPort = port;
        }

        public Byte[] Receive(Socket client, out IPEndPoint remoteEndPoint)
        {
            remoteEndPoint = ReceiveEndPoint;
            return ReceiveBuffer;
        }
    }
}
