namespace DominoWPF
{
    public class Player : IPlayer
    {
        private string _name;
        private int _score;

        public Player(string name)
        {
            name = _name;
        }

        public int GetScore()
        {
            return _score;
        }

        public string GetName()
        {
            return _name;
        }

        public void SetScore(int score)
        {
            _score = score;
        }

        public void SetName(string value)
        {
            _name = value;
        }
    }
}