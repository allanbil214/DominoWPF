namespace DominoWPF
{
    public interface ICard
    {
        int GetLeftValueCard();
        int GetRightValueCard();
        int GetOtherValueCard(int value);
        void SetLeftValueCard(int value);
        void SetRightValueCard(int value);
    }
}