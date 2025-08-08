using System.Windows;
using NUnit.Framework; 
using System.Collections.Generic;  
using DominoWPF;  

namespace DominoWPF.Tests;

public class Tests
{
    private GameController _gameController;
    private List<IPlayer> _players;

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
    public void GetCurrentPlayerIndex_GettingCurrentPlayerIndex_ReturnCurrentPlayerIndex()
    {
        int expected = 0;

        int actual = _gameController.GetCurrentPlayerIndex();

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void GetCurrentPlayer_GettingCurrentPlayer_ReturnCurrentPlayer()
    {
        var expected = (Player)_players[0];

        var actual = _gameController.GetCurrentPlayer();

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void GetPlayerHand_GettingPlayerHand_ReturnListCard()
    {
        var actual = _gameController.GetPlayerHand(_players[0]);

        Assert.That(actual, Is.InstanceOf<List<ICard>>());
    }

    [Test]
    public void GetDiscardTile_GettingDiscardTile_ReturnDiscardTile()
    {
        var actual = _gameController.GetDiscardTile();

        Assert.That(actual, Is.InstanceOf<IDiscardTile>());
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

    [Test]
    public void DetermineStartingPlayer_DeterminingStartingPlayer_ReturnPlayerIndex()
    {
        var actual = _gameController.DetermineStartingPlayer();

        Assert.That(actual, Is.InstanceOf<Player>());
    }

    [Test]
    public void NextTurn_ChangeTurnToNextPlayer_ReturnChangedPlayedIndex()
    {
        _gameController.NextTurn();
        int expectedd = 1;

        int actual = _gameController.GetCurrentPlayerIndex();

        Assert.That(actual, Is.EqualTo(expectedd));
    }

    [Test]
    public void HasPlayableCard_CardAreAvailable_ReturnBool()
    {
        bool expectedd = true;

        bool actual = _gameController.HasPlayableCard(_gameController.GetDiscardTile());

        Assert.That(actual, Is.EqualTo(expectedd));
    }

    [Test]
    public void FindPlayableCard_FindPlayableCard_ReturnBool()
    {
        var playerHand = _gameController.GetPlayerHand(_players[0]);

        bool actual = _gameController.FindPlayableCard(_gameController.GetDiscardTile(), playerHand[0]);

        Assert.That(actual, Is.True);
    }

    [Test]
    public void PlayCard_PlayTheCard_ReturnBool() 
    {
        var playerHand = _gameController.GetPlayerHand(_players[0]);

        bool actual = _gameController.PlayCard(_players[0], playerHand[0], "right");

        Assert.That(actual, Is.True);
    }

    [Test]
    public void PlaceCard_PutCardToDiscardTile_ReturnBool()
    {
        var playerHand = _gameController.GetPlayerHand(_players[0]);

        bool actual = _gameController.PlaceCard(playerHand[0], "right");

        Assert.That(actual, Is.True);
    }

    [Test]
    public void RemoveCard_RemoveCardFromHand_ReturnBool()
    {
        var playerHand = _gameController.GetPlayerHand(_players[0]);

        bool actual = _gameController.RemoveCard(playerHand[0]);

        Assert.That(actual, Is.True);
    }

    [TearDown]
    public void TearDown()
    {
        _gameController = null;
        _players = null;
    }
}
