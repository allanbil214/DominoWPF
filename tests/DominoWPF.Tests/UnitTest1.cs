using System.Windows;
using NUnit.Framework; 
using System.Collections.Generic;  
using DominoWPF;  

namespace DominoWPF.Tests;

public class Tests
{
    private GameController _gameController;
    private List<IPlayer> _players = [];

    [SetUp]
    public void Setup()
    {
        _players = new List<IPlayer>();

        for (int i = 1;  i <= 4; i++)
        {
            _players.Add(new Player($"Player {i}"));
        }
        _gameController = new GameController(_players);

        _gameController.InitDeck();
        _gameController.ShuffleDeck();
        _gameController.InitHand();
    }

    [Test]
    public void AddScore_AddScore_ScoreAdded()
    {
        int score = 10;
        int expected = 10;

        _gameController.AddScore(score);

        Assert.That(_players[0].GetScore(), Is.EqualTo(expected));
    }

    [Test]
    public void IsDoubleValue_IsDoubleValue_ReturnTrue()
    {
        Card card = new Card(2, 2);
        _gameController.AddCard(card);

        bool actual = _gameController.IsDoubleValue();

        Assert.That(actual, Is.True);
    }

    [Test]
    public void IsEmpty_DiscardPileIsEmpty_ReturnTrue()
    {
        bool actual = _gameController.IsEmpty();

        Assert.That(actual, Is.True);
    }

    [Test]
    public void CalculateScore_CalculateAllPlayersScore_ReturnCorrectTotalScore()
    {
        _gameController.InitDeck();
        _gameController.ShuffleDeck();
        _gameController.InitHand();

        int expectedScore = 0;
        for (int i = 1; i < _players.Count; i++)
        {
            var playerHand = _gameController.GetPlayerHand(_players[i]);
            foreach (var card in playerHand)
            {
                expectedScore += card.GetLeftValueCard() + card.GetRightValueCard();
            }
        }

        int actualScore = _gameController.CalculateScore();

        Assert.That(actualScore, Is.EqualTo(expectedScore));
    }

    [TearDown]
    public void TearDown()
    {
        _gameController = null;
        _players = null;
    }
}
