namespace DominoWPF
{
    public interface IDiscardTile
    {
        int GetLeftValueDiscardTile();
        int GetRightValueDiscardTile();
        List<ICard> GetPlayedCards();
        void SetLeftValueDiscardTile(int value);
        void SetRightValueDiscardTile(int value);
        void SetPlayedCards(List<ICard> cards);
        void Reset();
    }
}

