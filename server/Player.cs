using System.Net;


namespace server
{
    internal class Player
    {

        private int id = 0;
        private int guid = 0;
        public int hp = 100;
        public string nickname = "New_Player";
        public IPEndPoint ip;

        public float posX = 0.0f;
        public float posY = 0.0f;
        public float posZ = 0.0f;
        public float rot = 0.0f;
        public int ping = 0;
        public int worldID = 1;


        public void initPlayer(int plyId, int plyGuid)
        {

            id = plyId;
            guid = plyGuid;

        }

        public int getPlayerID()
        {

            return id;

        }

    }

}
