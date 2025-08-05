using System;
using System.Numerics;
using System.Windows;

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
            var startingPlayer = DetermineStartingPlayer();
            _currentPlayerIndex = _players.IndexOf(startingPlayer);
            OnStart?.Invoke(null);
        }

        // new func not in class diagram,

        public int GetCurrentPlayerIndex()
        {
            return _currentPlayerIndex;
        }

        public IPlayer GetCurrentPlayer()
        {
            return _players[_currentPlayerIndex];
        }

        public List<ICard> GetPlayerHand(IPlayer player)
        {
            if (_hand.TryGetValue(player, out var cards))
            {
                return cards;
            }
            else
            {
                return new List<ICard>();
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
            foreach (var player in _players)
            {
                if (!_hand.ContainsKey(player))
                    _hand[player] = new List<ICard>();

                for (int i = 0; i < cardLimit; i++)
                {
                    if (_deck.Count > 0)
                    {
                        _hand[player].Add(_deck[0]);
                        _deck.RemoveAt(0);
                    }
                }
            }
        }

        public void ResetRound()
        {
            _deck.Clear();
            _hand.Clear();
            _discardTile = new DiscardTile();
            _currentPlayerIndex = 0;
        }

        public void HandleBlockedGame()
        {
            Dictionary<IPlayer, int> handValues = new Dictionary<IPlayer, int>();

            foreach (var player in _players)
            {
                int handValue = 0;
                if (_hand.ContainsKey(player))
                {
                    foreach (var card in _hand[player])
                    {
                        handValue += card.GetLeftValueCard() + card.GetRightValueCard();
                    }
                }
                handValues.Add(player, handValue);
            }

            var winner = handValues.OrderBy(x => x.Value).First().Key;
            _currentPlayerIndex = _players.IndexOf(winner);

            int totalOpponentPoints = 0;
            foreach (var kvp in handValues)
            {
                if (kvp.Key != winner)
                {
                    totalOpponentPoints += kvp.Value;
                }
            }

            winner.SetScore(winner.GetScore() + totalOpponentPoints);
            OnScore?.Invoke();
        }

        // end of new func not in class diagram,

        public void AddCard(ICard card)
        {
            this.card = card;
        }

        public bool IsDoubleValue()
        {
            if (card != null)
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
            Dictionary<IPlayer, int> maxValue = new Dictionary<IPlayer, int>();

            foreach (var player in _players)
            {
                int maxCardValue = 0;
                if (_hand.ContainsKey(player))
                {
                    foreach (ICard card in _hand[player])
                    {
                        int cardTotal = card.GetLeftValueCard() + card.GetRightValueCard();
                        if (cardTotal > maxCardValue)
                            maxCardValue = cardTotal;
                    }
                }
                maxValue.Add(player, maxCardValue);
            }

            return maxValue.OrderByDescending(x => x.Value).First().Key;
        }

        public void NextTurn()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        }

        public bool HasPlayableCard(IDiscardTile discardTile)
        {
            var currentPlayer = _players[_currentPlayerIndex];
            var currentHand = GetPlayerHand(currentPlayer);

            if (discardTile.GetPlayedCards().Count == 0)
            {
                return (currentHand.Count > 0);
            }

            int leftValue = discardTile.GetLeftValueDiscardTile();
            int rightValue = discardTile.GetRightValueDiscardTile();

            return currentHand.Any(card =>
                card.GetLeftValueCard() == leftValue || card.GetLeftValueCard() == rightValue ||
                card.GetRightValueCard() == rightValue || card.GetRightValueCard() == leftValue);
        }

        public bool FindPlayableCard(IDiscardTile discardTile, ICard cardToCheck)   // originally ICard FindPlayableCard
        {
            if (IsEmpty()) return true;
            int leftValue = discardTile.GetLeftValueDiscardTile();
            int rightValue = discardTile.GetRightValueDiscardTile();
            return (cardToCheck.GetLeftValueCard() == leftValue || cardToCheck.GetLeftValueCard() == rightValue ||
                cardToCheck.GetRightValueCard() == rightValue || cardToCheck.GetRightValueCard() == leftValue);
        }
        public bool PlayCard(IPlayer player, ICard card, string positionCard)
        {
            var playerHand = GetPlayerHand(player);
            if (!playerHand.Contains(card)) return false;
            return PlaceCard(card, positionCard);
        }

        public void RotateValue()
        {
            if (card != null)
            {
                int temp = card.GetLeftValueCard();
                card.SetLeftValueCard(card.GetRightValueCard());
                card.SetRightValueCard(temp);
            }
        }

        public bool PlaceCard(ICard card, string positionCard)
        {
            var playedCard = _discardTile.GetPlayedCards();

            if (playedCard.Count == 0)
            {
                playedCard.Add(card);
                _discardTile.SetLeftValueDiscardTile(card.GetLeftValueCard());
                _discardTile.SetRightValueDiscardTile(card.GetRightValueCard());
                return true;
            }

            int leftValue = _discardTile.GetLeftValueDiscardTile();
            int rightValue = _discardTile.GetRightValueDiscardTile();

            if (positionCard.ToLower() == "left")
            {
                if (card.GetRightValueCard() == leftValue)
                {
                    playedCard.Insert(0, card);
                    _discardTile.SetLeftValueDiscardTile(card.GetLeftValueCard());
                    return true;
                }
                else if (card.GetLeftValueCard() == leftValue)
                {
                    AddCard(card);
                    RotateValue();
                    playedCard.Insert(0, card);
                    _discardTile.SetLeftValueDiscardTile(card.GetLeftValueCard());
                    return true;
                }
            }
            else if (positionCard.ToLower() == "right")
            {
                if (card.GetLeftValueCard() == rightValue)
                {
                    playedCard.Add(card);
                    _discardTile.SetRightValueDiscardTile(card.GetRightValueCard());
                    return true;
                }
                else if (card.GetRightValueCard() == rightValue)
                {
                    AddCard(card);
                    RotateValue();
                    playedCard.Add(card);
                    _discardTile.SetRightValueDiscardTile(card.GetRightValueCard());
                    return true;
                }
            }

            return false;
        }

        public bool RemoveCard(ICard card)
        {
            var currentPlayer = _players[_currentPlayerIndex];
            var playerHand = GetPlayerHand(currentPlayer);
            return playerHand.Remove(card);
        }

        public bool CheckWinCondition()
        {
            var currentPlayer = _players[_currentPlayerIndex];
            var playerHand = GetPlayerHand(currentPlayer);
            return playerHand.Count == 0;
        }

        public void AddScore(int points)
        {
            var currentPlayer = _players[_currentPlayerIndex];
            currentPlayer.SetScore(currentPlayer.GetScore() + points);
            OnScore?.Invoke();
        }

        public int CalculateScore()
        {
            int totalScore = 0;
            foreach (var player in _players)
            {
                if (player != _players[_currentPlayerIndex])
                {
                    var playerHand = GetPlayerHand(player);
                    foreach (var card in playerHand)
                    {
                        totalScore += card.GetRightValueCard() + card.GetLeftValueCard();
                    }
                }
            }
            return totalScore;
        }

        public void EndGame()
        {
            var winner = _players[_currentPlayerIndex];
            int points = CalculateScore();
            AddScore(points);
        }
    }
}