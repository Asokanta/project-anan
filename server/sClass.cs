using System.Net;


namespace server
{
    internal class Server
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


}
