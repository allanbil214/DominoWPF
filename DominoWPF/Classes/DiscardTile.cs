namespace DominoWPF
{
    public class DiscardTile : IDiscardTile
    {
        List<ICard> _playedDominos = new List<ICard>();
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
        public void SetLeftValueDiscardTile(int value)
        {
            _leftValueDiscardTile = value;
        }
        public void SetRightValueDiscardTile(int value)
        {
            _rightValueDiscardTile = value;
        }
        public void SetPlayedCards(List<ICard> cards)
        {
            _playedDominos = cards;
        }
        public void Reset()
        {
            _playedDominos.Clear();
            _leftValueDiscardTile = 0;
            _rightValueDiscardTile = 0;
        }
    }
}
