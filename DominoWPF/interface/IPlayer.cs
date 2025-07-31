namespace DominoWPF
{
    public interface IPlayer
    {
        string GetName();
        int GetScore();
        void SetScore(int score);
        void SetName(string name);
    }
}

