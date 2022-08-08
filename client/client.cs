using System;
using System.Net;
using System.Net.Sockets;
using System.Text;




class Server
{

    public IPEndPoint ip;
    public int maxPlayers = 0;
    public string name = "Server0";

    public Server(IPEndPoint servIP, int maxP, string servName)
    {
        ip = servIP;
        maxPlayers = maxP;
        name = servName;

    }
 

}


namespace client
{
    public class UDPSocket
    {
        public Socket _socket;
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback recv = null;

        private Server serv = new Server(null, 0, null);
        private Player[] PlyArray;
        private int myPlayerID = 0;

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }



        public void Client(string address, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Connect(IPAddress.Parse(address), port);
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

        private void readCallback(string callback)
        {
            // function callback
            string[] args = callback.Split("☺");


            // 0 -> function id. 1 -> player id or sth else

            int funcID = int.Parse(args[0]);

            switch (funcID)
            {

                case 0: // connect


                    if (int.Parse(args[1]) == 1) {
                        int maxPlayers = int.Parse(args[4]);
                        string serverName = args[5];
                        Console.WriteLine("Successfuly connected to {2} with nickname: {0} and id: {1}", args[3], args[2], serverName);

                        myPlayerID = int.Parse(args[2]);

                        // we have got the server                        
                        serv = new Server((IPEndPoint) epFrom, maxPlayers, serverName);

                        // get players now
                        // first initilaise players array to use it anywhere 
                        // do it in connect to do it once
                        PlyArray = new Player[serv.maxPlayers]; // initilaise players array

                        for (int x = 0; x < serv.maxPlayers; x++) // is it necessarry
                        {                                         //
                                                                  //
                            PlyArray[x] = new Player();           //
                                                                  //
                        }
                        // ask server for players list
                        this.Send("1☺1");

                    }

                    break;
                case 1: // get player list (ids)
                    

                    if (int.Parse(args[1]) == 1) // success
                    {
                        string playersStr = args[2]; 
                        string[] ids = playersStr.Split(",");
                        
                        // initialise them
                        foreach(string id in ids)
                        {
                            int plyID = int.Parse(id);
                            PlyArray[plyID] = new Player();
                            PlyArray[plyID].initPlayer(plyID);
                            Console.WriteLine("id {0} initilaised", plyID);


                            this.Send("2☺" + plyID);

                        }

                    }
                    break;
                case 2: // get player specific data
                    // first check if player is initialised
                    // 2, 1 ,id, username, posx, posy, posz, rotation, ping
                   
                    
                    if(int.Parse(args[1]) == 1)
                    {

                       int playersID = int.Parse(args[2]);
                       if(object.ReferenceEquals(null, PlyArray[playersID])) {
                           PlyArray[playersID] = new Player();
                           PlyArray[playersID].initPlayer(playersID);
                       }
                    
                       string username = args[3];
                       float posX = float.Parse(args[4]);
                       float posY = float.Parse(args[5]);
                       float posZ = float.Parse(args[6]);
                       float rot = float.Parse(args[7]);
                       int ping = int.Parse(args[8]);
                    
                       PlyArray[playersID].nickname = username; 
                       PlyArray[playersID].posX = posX;
                       PlyArray[playersID].posY = posY;
                       PlyArray[playersID].posZ = posZ;
                       PlyArray[playersID].rot = rot;
                       PlyArray[playersID].ping = ping;


                        Console.WriteLine("Player {0}[{1}] is ready.", username, playersID);
                    }
                   
                    


                    break;
                default:
                    break;
            }



        }

        private void Receive()
        {
            _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                try
                {
                    State so = (State)ar.AsyncState;
                    int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                    _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                    Console.WriteLine("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.UTF8.GetString(so.buffer, 0, bytes));


                    readCallback(Encoding.UTF8.GetString(so.buffer, 0, bytes));

                }
                catch { }

            }, state);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            UDPSocket c = new UDPSocket();
            c.Client("127.0.0.1", 27000);// http://134.255.232.216:7240/
            Random random = new Random();
            int num = random.Next(10, 5000);
            c.Send("0☺mynick" + num); // 


            Console.ReadKey();

        }
    }

}