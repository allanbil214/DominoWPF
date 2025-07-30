using System;

namespace DominoWPF
{
    public class GameController
    {
        private List<IPlayer> _players;
        private IDiscardTile _discardTile;
        private ICard card;
        private Dictionary<IPlayer, List<ICard>> _hand;
        private int _currentPlayerIndex;
        private List<ICard> _deck;

        public Action<ICard> OnStart;
        public Action OnScore;

        Random rand = new Random();

        public GameController(List<IPlayer> players)
        {
            _players = players;
            _discardTile = new DiscardTile();
            _hand = new Dictionary<IPlayer, List<ICard>>();
            _currentPlayerIndex = 0;
            _deck = new List<ICard>();
        }

        public void StartGame()
        {

        }

        // new func not in class diagram

        public IPlayer GetCurrentPlayer()
        {
            return _players[_currentPlayerIndex];
        }

        public List<ICard> GetPlayerHand(IPlayer player)
        {
            if(_hand.ContainsKey(player))
            {
                return _hand[player];
            }
            else
            {
                return new List<ICard> { };
            }
        }

        public IDiscardTile GetDiscardTile()
        {
            return _discardTile;
        }

        public void InitDeck()
        {
            for (int i = 0; i <= 6; i++)
            {
                for (int j = i; j <= 6; j++)
                {
                    _deck.Add(new Card(i, j));
                }
            }
        }

        public void ShuffleDeck()
        {
            for (int i = _deck.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                ICard temp = _deck[i];
                _deck[i] = _deck[j];
                _deck[j] = temp;
            }
        }

        public void InitHand()
        {
            int cardLimit = 7;
            foreach (var players in _players) 
            {
                for (int i = 0; i <= cardLimit; i++) 
                { 
                  if(_deck.Count > 0)
                    {
                        _hand[players].Add(_deck[0]);
                        _deck.RemoveAt(0);
                    }
                }

            }
        }

        // end of new func not in class diagram

        public void AddCard(ICard card)
        {
            this.card = card;
        }

        public bool IsDoubleValue()
        {
            if(card != null)
            {
                return card.GetLeftValueCard() == card.GetRightValueCard();
            }
            return false;
        }

        public bool IsEmpty()
        {
            return _discardTile.GetPlayedCards().Count == 0;
        }

        public IPlayer DetermineStartingPlayer()
        {
            List<int> totalValue = new List<int>();
            Dictionary<IPlayer, int> maxValue = new Dictionary<IPlayer, int>();
            foreach (var player in _players)
            {
                foreach(Card card in _deck)
                {
                    totalValue.Add(card.GetLeftValueCard() + card.GetRightValueCard()); 
                }
                maxValue.Add(player, totalValue.Max());
            }
            maxValue = (Dictionary<IPlayer, int>)maxValue.OrderBy(x => x.Value);

            return maxValue.Keys.First();
        }

        public void NextTurn()
        {
            _currentPlayerIndex = _currentPlayerIndex + 1;
        }

        public bool HasPlayableCard(IDiscardTile discardTile)
        {
            var currentPlayer = _players[_currentPlayerIndex];
            var currentHand = _hand[currentPlayer];

            if(discardTile.GetPlayedCards().Count == 0)
            {
                return (currentHand.Count > 0);
            }

            int leftValue = discardTile.GetLeftValueDiscardTile();
            int rightValue = discardTile.GetRightValueDiscardTile();

            foreach (var card in currentHand) 
            {
                if(card.GetLeftValueCard() == leftValue || card.GetLeftValueCard() == rightValue ||
                    card.GetRightValueCard() == rightValue || card.GetRightValueCard() == leftValue)
                {
                    return true;
                }
            }

            return false;
        }

        public ICard FindPlayableCard(IDiscardTile discardTile)
        {
            var currentPlayer = _players[_currentPlayerIndex];
            var currentHand = _hand[currentPlayer];

            if (discardTile.GetPlayedCards().Count == 0)
            {
                return (currentHand.FirstOrDefault());
            }

            int leftValue = discardTile.GetLeftValueDiscardTile();
            int rightValue = discardTile.GetRightValueDiscardTile();

            foreach (var card in currentHand)
            {
                if (card.GetLeftValueCard() == leftValue || card.GetLeftValueCard() == rightValue ||
                    card.GetRightValueCard() == rightValue || card.GetRightValueCard() == leftValue)
                {
                    return card;
                }
            }

            return null;
        }

        public bool PlayCard(IPlayer player, ICard card, string potionCard)
        {
            return true;
        }

        public void RotateValue()
        {

        }

        public bool PlaceCard(ICard card, string potionCard)
        {
            return true;
        }

        public bool CheckWinCondition()
        {
            return false;
        }

        public void AddScore(int points)
        {

        }

        public int CalculateScore()
        {
            return 0;
        }

        public void EndGame()
        {

        }
    }
}