using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VindicateLib;
using VindicateLib.Enums;

namespace VindicateLibTests
{
    //This class is kind of a mess, but until we have a legit SMB service we can use it as a stop-gap
    [TestClass]
    public class SMBTesterTests
    {
        private const String RemoteServerAddress = "127.0.0.1";
        private const String LocalServerAddress = "127.0.0.1";
        private const Int32 NBOverTCPPort = 139;
        private const Int32 SMBDirectHostPort = 445;

        [TestMethod]
        public void PerformSMBTest_Port139_Exists()
        {
            TcpListener tcpListener = SMBServiceFakeHelper.CreateSMBService(NBOverTCPPort);
            try
            {
                SMBServiceFakeHelper.PerformSingleSMBServiceListen(tcpListener);
                SpoofDetectionResult result = SMBTester.PerformSMBTest(IPAddress.Parse(RemoteServerAddress), LocalServerAddress);
                Assert.IsTrue(result.Detected);
                Assert.AreEqual(ConfidenceLevel.Medium, result.Confidence);
                Assert.AreEqual(Protocol.SMB, result.Protocol);
                Assert.IsNull(result.ErrorMessage);
                Assert.AreEqual("Open", result.Response);
                Assert.AreEqual(RemoteServerAddress, result.Endpoint.Address.ToString());
                Assert.AreEqual(NBOverTCPPort, result.Endpoint.Port);
            }
            catch (SocketException ex)
            {
                //If there is already a service, we can't run this test. Just pass the test.
                if (ex.Message == "Only one usage of each socket address (protocol/network address/port) is normally permitted")
                    return;
                throw;
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        [TestMethod]
        public void PerformSMBTest_Port445_Exists()
        {
            TcpListener tcpListener = SMBServiceFakeHelper.CreateSMBService(SMBDirectHostPort);
            try
            {
                SMBServiceFakeHelper.PerformSingleSMBServiceListen(tcpListener);
                SpoofDetectionResult result = SMBTester.PerformSMBTest(IPAddress.Parse(RemoteServerAddress), LocalServerAddress);
                Assert.IsTrue(result.Detected);
                Assert.AreEqual(ConfidenceLevel.Medium, result.Confidence);
                Assert.AreEqual(Protocol.SMB, result.Protocol);
                Assert.IsNull(result.ErrorMessage);
                Assert.AreEqual("Open", result.Response);
                Assert.AreEqual(RemoteServerAddress, result.Endpoint.Address.ToString());
                Assert.AreEqual(SMBDirectHostPort, result.Endpoint.Port);
            }
            catch (SocketException ex)
            {
                //If there is already a service, we can't run this test. Just pass the test.
                if (ex.Message == "Only one usage of each socket address (protocol/network address/port) is normally permitted")
                    return;
                throw;
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        [TestMethod]
        public void PerformSMBTest_NoService()
        {
            //This test is pointless if we're already running an SMB server, so establish that first
            //There's obvious race conditions here, but you shouldn't really be messing with an SMB service while in middle of running SMB tests...
            TcpListener tcpListener = SMBServiceFakeHelper.CreateSMBService(NBOverTCPPort);
            try
            {
                SMBServiceFakeHelper.PerformSingleSMBServiceListen(tcpListener);
            }
            catch (SocketException ex)
            {
                if (ex.Message == "Only one usage of each socket address (protocol/network address/port) is normally permitted")
                    return;
                throw;
            }
            finally
            {
                tcpListener.Stop();
            }
            tcpListener = SMBServiceFakeHelper.CreateSMBService(SMBDirectHostPort);
            try
            {
                SMBServiceFakeHelper.PerformSingleSMBServiceListen(tcpListener);
            }
            catch (SocketException ex)
            {
                if (ex.Message == "Only one usage of each socket address (protocol/network address/port) is normally permitted")
                    return;
                throw;
            }
            finally
            {
                tcpListener.Stop();
            }


            //Now attempt to connect to service that doesn't exist
            SpoofDetectionResult result = SMBTester.PerformSMBTest(IPAddress.Parse(RemoteServerAddress), LocalServerAddress);
            Assert.IsFalse(result.Detected);
            Assert.AreEqual(ConfidenceLevel.FalsePositive, result.Confidence);
            Assert.AreEqual(Protocol.SMB, result.Protocol);
            Assert.AreEqual(String.Format("No connection could be made because the target machine actively refused it {0}:{1}", RemoteServerAddress, SMBDirectHostPort), result.ErrorMessage);
            Assert.IsNull(result.Response);
            Assert.AreEqual(RemoteServerAddress, result.Endpoint.Address.ToString());
            Assert.AreEqual(SMBDirectHostPort, result.Endpoint.Port);
        }
    }
}
