namespace DominoWPF
{
    public interface IDiscardTile
    {
        int GetLeftValueDiscardTile();
        int GetRightValueDiscardTile();
        List<ICard> GetPlayedCards();
        void SetLeftValueDiscardPile(int value);
        void SetRightValueDiscardPile(int value);
        void SetPlayedCards(List<ICard> cards);
    }
}

