namespace DominoWPF
{
    public class DiscardTile : IDiscardTile
    {
        List<ICard> _playedDominos;
        int _leftValueDiscardTile;
        int _rightValueDiscardTile;

        public DiscardTile()
        {
            _leftValueDiscardTile = 0;
            _rightValueDiscardTile = 0;
        }

        public int GetLeftValueDiscardTile()
        {
            return _leftValueDiscardTile;
        }
        public int GetRightValueDiscardTile()
        {
            return _rightValueDiscardTile;
        }
        public List<ICard> GetPlayedCards()
        {
            return _playedDominos;
        }
        public void SetLeftValueDiscardPile(int value)
        {
            _leftValueDiscardTile = value;
        }
        public void SetRightValueDiscardPile(int value)
        {
            _rightValueDiscardTile = value;
        }
        public void SetPlayedCards(List<ICard> cards)
        {
            _playedDominos = cards;
        }
    }
}
