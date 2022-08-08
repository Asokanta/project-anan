namespace client
{
    internal class Player
    {
        // these are accessable ones from client
        private int id = 0;
        public int hp = 100;
        public string nickname = "New_Player";

        public float posX = 0.0f;
        public float posY = 0.0f;
        public float posZ = 0.0f;
        public float rot = 0.0f;
        public int ping = 0;


        public void initPlayer(int plyId)
        {

            id = plyId;

        }

        public int getPlayerID()
        {

            return this.id;

        }

    }
}
