namespace DominoWPF
{
    public class Card : ICard
    {
        private int _leftValueCard;
        private int _rightValueCard;

        public Card(int leftValueCard, int rightValueCard)
        {
            leftValueCard = _leftValueCard;
            rightValueCard = _rightValueCard;
        }

        public int GetLeftValueCard()
        {
            return _leftValueCard;
        }
        public int GetRightValueCard()
        {
            return _rightValueCard;
        }
        public int GetOtherValueCard(int value)
        {
            if (value == _leftValueCard) return _rightValueCard;
            else return _leftValueCard;
        }
        public void SetLeftValueCard(int value)
        {
            _leftValueCard = value;
        }
        public void SetRightValueCard(int value)
        {
            _rightValueCard = value;
        }
    }
}