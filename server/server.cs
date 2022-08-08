using System.Net;
using System.Net.Sockets;
using System.Text;


namespace server
{

    public class UDPSocket
    {
        public Socket _socket;
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback recv = null;

        private int maxPlayers;
        private int playerCount;

        private IPEndPoint servIP;

        private Server servObj;

        private enum returnMessages{ FATAL_ERROR = -1, RETURN, SUCCESS }
        private string sep = "☺";

        // players array

        private Player[] PlyArray;
        public UDPSocket(int servMax, string serverName)
        {

            maxPlayers = servMax; 

            PlyArray = new Player[servMax]; // initilaise players array

            for (int x = 0; x < servMax; x++) 
            {

                PlyArray[x] = new Player();

            }

            servObj = new Server(servIP, servMax, serverName);

        }


        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public void Server(string address, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
            servIP = new IPEndPoint(IPAddress.Parse(address), port);
            Receive();
        }


        public void Send(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndSend(ar);
                Console.WriteLine("SEND: {0}, {1}", bytes, text);
            }, state);
        }

        private void functionCallback(string data, IPEndPoint ieClient, UdpClient servClient)
        {
            // function callback
            string[] args = data.Split("☺");
            string[] dataToSend = new string[4];
            IPEndPoint[] playerToSend = new IPEndPoint[servObj.maxPlayers]; // 4 is max sends
            playerToSend[0] = ieClient;

            for (int x = 0; x < dataToSend.Count(); x++)
            {
                dataToSend[x] = "";

            }

            // 0 -> function id. 1 -> player id or sth else

            int funcID = int.Parse(args[0]);

            switch (funcID)
            {

                case 0: // connect
                    bool foundEmptyId = false;
                    int newPlyID = 0;
                    
                    for (int x = 1; x < PlyArray.Count(); x++)
                    {

                        //Console.WriteLine("{0}", PlyArray[x].getPlayerID());

                        if(!(PlyArray[x].getPlayerID() >= 1))
                        {

                            foundEmptyId = true;
                            newPlyID = x;
                            break;
                        }

                    }
                    
                    if(foundEmptyId == true)
                    {
                        PlyArray[newPlyID] = new Player();
                        PlyArray[newPlyID].initPlayer(newPlyID, 31);
                        // TODO# CHECK PLAYER'S NICKNAME
                        PlyArray[newPlyID].nickname = args[1];
                        PlyArray[newPlyID].ip = ieClient;
                        //
                        dataToSend[0] = funcID + sep + (int) returnMessages.SUCCESS + sep + newPlyID + sep + args[1] + sep + maxPlayers + sep + servObj.name; // 1 is success


                        playerCount++;



                        // tell online clients someone is connected.
                        for (int x = 1; x < PlyArray.Count(); x++)
                       {                                         


                            if (PlyArray[x].getPlayerID() >= 1 && x != newPlyID)
                           {



                                string sendit = 2 + sep +
                                    (int) returnMessages.SUCCESS + sep +
                                    PlyArray[newPlyID].getPlayerID() + sep +
                                    PlyArray[newPlyID].nickname + sep +
                                    PlyArray[newPlyID].posX + sep +
                                    PlyArray[newPlyID].posY + sep +
                                    PlyArray[newPlyID].posZ + sep +
                                    PlyArray[newPlyID].rot + sep +
                                    PlyArray[newPlyID].ping;
                                byte[] senddata = Encoding.UTF8.GetBytes(sendit);
                                servClient.Send(senddata, senddata.Length, PlyArray[x].ip);

                            }        
                                                                 
                       }
                      

                    }
                    else
                    {
                        // tell we can't connect him
                        dataToSend[0] = funcID + sep + (int) returnMessages.FATAL_ERROR; // -1 is not succesful at all! 
                    }

                    break;
                case 1: // send player ids

                    string send = "";

                    for (int x = 1; x < PlyArray.Count(); x++)
                    {

                        //Console.WriteLine("{0}", PlyArray[x].getPlayerID());

                        if ((PlyArray[x].getPlayerID() >= 1))
                        {

                            if(x == 1)
                            {

                                send += x;

                            }
                            else
                            {

                                send += "," + x;

                            }


                        }

                    }

                    dataToSend[0] = funcID + sep + (int) returnMessages.SUCCESS + sep + send; // 1 is success
                    break;
                case 2: // send player data


                    int plyID = int.Parse(args[1]);

                    dataToSend[0] = 2 + sep +
                        (int)returnMessages.SUCCESS + sep +
                        PlyArray[plyID].getPlayerID() + sep +
                        PlyArray[plyID].nickname + sep +
                        PlyArray[plyID].posX + sep +
                        PlyArray[plyID].posY + sep +
                        PlyArray[plyID].posZ + sep +
                        PlyArray[plyID].rot + sep +
                        PlyArray[plyID].ping;

                    break;
                default:
                    break;
            }

           if (dataToSend[0].Length >= 1) // good?
           {
               byte[] senddata = Encoding.UTF8.GetBytes(dataToSend[0]);
               servClient.Send(senddata, senddata.Length, playerToSend[0]);
           }
            



        }

        private void Receive()
        {

            UdpClient udpSClient = new UdpClient(27000); // client to send data to other clients

            _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                try
                {
                    State so = (State)ar.AsyncState;
                    int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                    _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                    String data = Encoding.UTF8.GetString(so.buffer, 0, bytes);
                    Console.WriteLine("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, data);

                    IPEndPoint ipeClient = (IPEndPoint)epFrom;
                    functionCallback(data.ToString(), ipeClient, udpSClient);


                }
                catch { }

            }, state);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            UDPSocket s = new UDPSocket(64, "Anan's Server"); // 64 is max players server will accept in.

            s.Server("127.0.0.1", 27000);




            Console.ReadKey();
            s._socket.Close(); //Fixed closing bug (System.ObjectDisposedException)
                               //Bugfix allows to relaunch server
            Console.WriteLine("Closed Server \n Press any key to exit");
            Console.ReadKey();
        }
    }

}