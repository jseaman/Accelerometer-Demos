using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace IphoneComm
{
    public class CoordListener
    {
        public CoordListener(int Port)
        {
            try
            {
                this.Port = Port;
                
                ReceiverThread = new Thread(new ThreadStart(Execute));
                ReceiverThread.Start();
            }
            catch (Exception E)
            {
                if (!Terminated)
                    throw new IphoneCommException(E.Message);
            }
        }

        public void Stop ()
        {
            Terminated = true;
            Server.Stop();

            if (ReceiverSock!=null)
                ReceiverSock.Close();

            ReceiverSock = null;
        }

        private void Execute()
        {
            try
            {
                Server = new TcpListener(IPAddress.Any, Port);

                Server.Start();
                ReceiverSock = Server.AcceptSocket();

                while (!Terminated)
                {
                    byte[] type = new byte[1];
                    RecvAllBytes(ReceiverSock, 1, ref type);

                    byte []b;

                    switch ((char)type[0])
                    {
                        case 'A' :
                            b = new byte[24];

                            RecvAllBytes(ReceiverSock, 24, ref b);

                            double x = BitConverter.ToDouble(b, 0);
                            double y = BitConverter.ToDouble(b, 8);
                            double z = BitConverter.ToDouble(b, 16);

                            lock (_AccelerometerVector)
                            {
                                _AccelerometerVector = new Vector(x, y, z);
                            }
                            break;

                        case 'C':
                            b = new byte[16];

                            RecvAllBytes(ReceiverSock, 16, ref b);

                            double Angle = BitConverter.ToDouble(b, 0);
                            double Accuracy = BitConverter.ToDouble(b, 8);

                            lock (_CompassData)
                            {
                                _CompassData = new Orientation(Angle, Accuracy);
                            }
                            break;
                    }
                }
            }
            catch (Exception E)
            {
                if (!Terminated)
                    throw new IphoneCommException(E.Message);
            }
            finally
            {
                Stop();
            }
        }

        private void RecvAllBytes(Socket Sock, int size, ref Byte[] buff)
        {
            int received = 0;

            do
            {
                received += Sock.Receive(buff, received, size - received, SocketFlags.None);
            } while (received < size);
        }

        public Vector AccelerometerVector
        {
            get 
            { 
                lock (_AccelerometerVector)
                {
                    return _AccelerometerVector; 
                }
            }
        }

        public Orientation CompassData
        {
            get 
            {
                lock (_CompassData)
                {
                    return _CompassData;
                }
            }
        }

        public Boolean Connected
        {
            get { return ReceiverSock != null; }
        }

        private Boolean Terminated = false;
        private int Port = 0;


        private TcpListener Server = null;
        private Socket ReceiverSock = null;
        private Thread ReceiverThread = null;

        private Vector _AccelerometerVector = new Vector();
        private Orientation _CompassData = new Orientation();
    }
}
