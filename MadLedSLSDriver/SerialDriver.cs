using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadLedFrameworkSDK
{
    public class MadLedSerialDriver : IDisposable
    {
        private System.IO.Ports.SerialPort myPort;
        private Action<string> getNextLine;
        public string SerialPort { get; set; }
        private string sendBuffer = "";
        public MadLedSerialDriver(string comPort)
        {
            SerialPort = comPort;

            myPort = new SerialPort(SerialPort, 19200);
            myPort.Open();
            myPort.DataReceived += MyPort_DataReceived;
            Debug.WriteLine(Ping().TotalMilliseconds + "ms");
        }

        private string dataReceived = "";
        private void MyPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indataall = sp.ReadExisting().Replace("\r", "");

            dataReceived = dataReceived + indataall;

            string tmp = dataReceived;
            while (tmp.Contains("\n"))
            {
                var splitee = tmp.Split('\n');
                if (splitee.Length > 1)
                {
                    string firstLine = splitee[0];
                    tmp = tmp.Substring(firstLine.Length + 1);
                    if (!firstLine.StartsWith(">"))
                    {
                        Debug.WriteLine("}" + firstLine);
                        getNextLine?.Invoke(firstLine);
                        getNextLine = null;
                    }
                    else
                    {
                        Debug.WriteLine(firstLine);
                    }
                }
            }

            dataReceived = tmp;
        }

        DateTime lastSend = DateTime.MinValue;
       
        public void ByteCommand(byte[] cmd)
        {
            myPort.Write(cmd,0,cmd.Length);
        }

        public void SetLED(int bank, int led, int r, int g, int b)
        {
            ByteCommand(new byte[]{(byte)bank, (byte)led, (byte)r, (byte)g, (byte)b ,19,19});

        }

        public TimeSpan Ping()
        {
            DateTime start = DateTime.Now;

            bool gotResponse = false;

            getNextLine = s =>
            {
                if (s.Contains("pong"))
                {
                    gotResponse = true;
                    getNextLine = null;
                }
            };
            int tries = 0;
            while (tries < 10&&!gotResponse)
            {
                ByteCommand(new byte[] {13, 10, 10, 10, 19, 19, 19});

                while (!gotResponse)
                {
                    if ((DateTime.Now - start).TotalSeconds > 10)
                    {
                        break;
                    }
                }

                tries++;
            }

            return (DateTime.Now - start);
        }


        public void Dispose()
        {
            myPort?.Close();
            myPort?.Dispose();
        }
    }

}
