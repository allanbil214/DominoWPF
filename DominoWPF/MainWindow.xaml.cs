using System.Collections.Specialized;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DominoWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private Random rand = new Random();
        private ICard selectedCard = null;
        private Button currentButton = null;
        private Button lastButton = null;
        private List<IPlayer> players = [];
        private GameController game = null;
        private int maxScore = 150;

        private readonly Thickness baseBottomMargin = new Thickness(0, 0, 0, 180);
        private readonly Thickness baseTopMargin = new Thickness(0, 29, 0, 0);
        private readonly Thickness baseRightMargin = new Thickness(0, 319, -297, 0);
        private readonly Thickness baseLeftMargin = new Thickness(-297, -319, 0, 0);

        // Constructor and Initialization
        public MainWindow()
        {
            InitializeComponent();

            InitStackPanel();
            ChangeWindowSize();
            LoadStartup();

            var m = LayerRightWrapper.Margin;
            debuglabel.Content = $"{m.Left.ToString()}, {m.Top.ToString()}, {m.Right.ToString()}, {m.Bottom.ToString()}";
        }

        private void ChangeWindowSize()
        {
            this.Width += 16;
            this.Height += 15;
        }

        private void LoadStartup()
        {
            this.Effect = new BlurEffect();

            StartupWindow startup = new();
            bool? result = startup.ShowDialog();

            if (result != true)
            {
                this.Close();
                return;
            }

            List<string> playerNames = new List<string>
            {
                startup.player1TextBox.Text,
                startup.player2TextBox.Text,
                startup.player3TextBox.Text,
                startup.player4TextBox.Text
            };

            InitMaxScore(int.Parse(startup.maxScoreTextBox.Text));
            InitPlayers(playerNames);
            InitGame();
            LoadPlayerCards();
            ChangePlayerTurn();

            this.Effect = null;
        }

        private void InitStackPanel()
        {
            LayerLeftWrapper.Margin = baseLeftMargin;
            LayerRightWrapper.Margin = baseRightMargin;

            LayerBottomStackPanel.Margin = baseBottomMargin;
            LayerTopStackPanel.Margin = baseTopMargin;
        }


        // Game Initialization and Setup
        private void InitMaxScore(int value)
        {
            maxScore = value;
        }

        private void InitPlayers(List<string> names)
        {
            players.Clear();

            foreach (string name in names.Take(4))
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    players.Add(new Player(name.Trim()));
                }
            }
        }

        private void InitGame()
        {
            InitPlayerLabels();

            if (game == null)
            {
                game = new GameController(players);
                game.OnScore += UpdateScoreDisplay;
            }

            StartNewRound();
            UpdateScoreDisplay();
        }

        private void InitPlayerLabels()
        {
            List<Label> playerNameLabels = new List<Label>
            {
                player1NameLabel, player2NameLabel, player3NameLabel, player4NameLabel
            };

            List<Label> playerScoreLabels = new List<Label>
            {
                player1ScoreLabel, player2ScoreLabel, player3ScoreLabel, player4ScoreLabel
            };

            List<StackPanel> playerStackPanels = new List<StackPanel>();

            for (int i = 0; i < playerNameLabels.Count; i++)
            {
                if (playerNameLabels[i].Parent is StackPanel stackPanel)
                {
                    playerStackPanels.Add(stackPanel);
                }
            }

            foreach (var stackPanel in playerStackPanels)
            {
                stackPanel.Visibility = Visibility.Collapsed;
            }

            for (int i = 0; i < Math.Min(players.Count, playerNameLabels.Count); i++)
            {
                playerNameLabels[i].Content = players[i].GetName();
                playerScoreLabels[i].Content = $"Score: {players[i].GetScore()}";

                if (i < playerStackPanels.Count)
                {
                    playerStackPanels[i].Visibility = Visibility.Visible;
                }
            }

            Player1HandGrid.Visibility = players.Count >= 1 ? Visibility.Visible : Visibility.Collapsed;
            Player2HandGrid.Visibility = players.Count >= 2 ? Visibility.Visible : Visibility.Collapsed;
            Player3HandGrid.Visibility = players.Count >= 3 ? Visibility.Visible : Visibility.Collapsed;
            Player4HandGrid.Visibility = players.Count >= 4 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Game Flow Management
        private void StartNewRound()
        {
            ClearGameBoard();

            game.ResetRound();
            game.InitDeck();
            game.ShuffleDeck();
            game.InitHand();

            var startingPlayer = game.DetermineStartingPlayer();
            game.SetCurrentPlayer(startingPlayer);

            LoadPlayerCards();
            ChangePlayerTurn();
        }

        private void NextTurn()
        {
            ResetSelectedCard();

            if (CheckAndHandleWinCondition()) return;

            game.NextTurn();
            LoadPlayerCards();
            ChangePlayerTurn();
        }

        private void ChangePlayerTurn()
        {
            var discardTile = game.GetDiscardTile();
            int skipCount = 0;

            while (!game.HasPlayableCard(discardTile) && skipCount < players.Count)
            {
                MessageBox.Show($"{game.GetCurrentPlayer().GetName()} has no playable cards! Skipping turn.");
                game.NextTurn();
                skipCount++;

                if (skipCount >= players.Count)
                {
                    MessageBox.Show("No players can play! Game is blocked.");
                    game.HandleBlockedGame();

                    string blockedWinner = game.GetCurrentPlayer().GetName();
                    MessageBox.Show($"{blockedWinner} wins the blocked game!");

                    var gameWinner = players.FirstOrDefault(p => p.GetScore() >= maxScore);
                    if (gameWinner != null)
                    {
                        MessageBox.Show($"{gameWinner.GetName()} wins the entire game with {gameWinner.GetScore()} points!");

                        ResetGame();
                        return;
                    }

                    StartNewRound();
                    return;
                }
            }

            int currentIndex = game.GetCurrentPlayerIndex();

            while (currentIndex >= players.Count)
            {
                game.NextTurn();
                currentIndex = game.GetCurrentPlayerIndex();
            }

            SetPlayerTurnUI(currentIndex);
        }

        private void SetPlayerTurnUI(int currentIndex)
        {
            Player1HandGrid.IsEnabled = false;
            Player2HandGrid.IsEnabled = false;
            Player3HandGrid.IsEnabled = false;
            Player4HandGrid.IsEnabled = false;

            switch (currentIndex)
            {
                case 0:
                    Player1HandGrid.IsEnabled = true;
                    UpdateCardAvailability(Player1CardStackPanel);
                    break;

                case 1:
                    if (players.Count < 2)
                    {
                        game.NextTurn();
                        ChangePlayerTurn();
                        return;
                    }
                    Player2HandGrid.IsEnabled = true;
                    UpdateCardAvailability(Player2CardStackPanel);
                    break;

                case 2:
                    if (players.Count < 3)
                    {
                        game.NextTurn();
                        ChangePlayerTurn();
                        return;
                    }
                    Player3HandGrid.IsEnabled = true;
                    UpdateCardAvailability(Player3CardStackPanel);
                    break;

                case 3:
                    if (players.Count < 4)
                    {
                        game.NextTurn();
                        ChangePlayerTurn();
                        return;
                    }
                    Player4HandGrid.IsEnabled = true;
                    UpdateCardAvailability(Player4CardStackPanel);
                    break;
            }
        }

        // Card and Game State Management
        private void LoadPlayerCards()
        {
            ClearAllPlayerCards();

            if (players.Count > 0) LoadButton(Player1CardStackPanel, players[0], true);
            if (players.Count > 1) LoadButton(Player2CardStackPanel, players[1], false);
            if (players.Count > 2) LoadButton(Player3CardStackPanel, players[2], true);
            if (players.Count > 3) LoadButton(Player4CardStackPanel, players[3], false);
        }

        private void LoadButton(StackPanel stackPanel, IPlayer player, bool isHorizontal)
        {
            var playerHand = game.GetPlayerHand(player);

            foreach (var card in playerHand)
            {
                Button newButton = CreateDominoButton(card, IsPlayableCard(card));
                newButton.Click += (sender, EventArgs) => { GetButtonValue(sender, EventArgs, newButton, card); };

                stackPanel.Children.Add(newButton);
                if (!isHorizontal)
                {
                    newButton.Margin = new Thickness(0, 5, 0, 0);
                }
                else
                {
                    newButton.Margin = new Thickness(5, 0, 0, 0);
                }
            }
        }

        private Button CreateDominoButton(ICard card, bool isEnabled)
        {
            return new Button
            {
                Content = $"{GetBrailleFace(card.GetLeftValueCard())} │ {GetBrailleFace(card.GetRightValueCard())}",
                Width = 75,
                Height = 25,
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsEnabled = isEnabled,
                Tag = card,
                Style = (Style)FindResource(typeof(Button))
            };
        }

        private bool IsPlayableCard(ICard card)
        {
            var discardTile = game.GetDiscardTile();

            if (discardTile.GetPlayedCards().Count == 0) return true;

            int leftValue = discardTile.GetLeftValueDiscardTile();
            int rightValue = discardTile.GetRightValueDiscardTile();

            return (card.GetLeftValueCard() == leftValue || card.GetLeftValueCard() == rightValue ||
                card.GetRightValueCard() == rightValue || card.GetRightValueCard() == leftValue);
        }

        private void UpdateCardAvailability(StackPanel stackPanel)
        {
            var currentPlayer = game.GetCurrentPlayer();
            var playerHand = game.GetPlayerHand(currentPlayer);

            for (int i = 0; i < Math.Min(stackPanel.Children.Count, playerHand.Count); i++)
            {
                if (stackPanel.Children[i] is Button button)
                {
                    button.IsEnabled = IsPlayableCard(playerHand[i]);
                }
            }
        }

        private void PlaceCard(string position)
        {
            if (selectedCard == null || currentButton == null)
            {
                MessageBox.Show("Please select a card first!");
                return;
            }

            var currentPlayer = game.GetCurrentPlayer();

            ICard cardToPlay = selectedCard;
            if (currentButton.Tag is ICard tagCard)
            {
                cardToPlay = tagCard;
            }

            if (game.PlayCard(currentPlayer, cardToPlay, position))
            {
                ContentInsert(cardToPlay.GetLeftValueCard(), cardToPlay.GetRightValueCard(), position == "left");
                game.RemoveCard(cardToPlay);
                RemoveButton();
                NextTurn();
            }
            else
            {
                MessageBox.Show($"Cannot place card on the {position} side!");
                currentButton.IsEnabled = true;
                currentButton = null;
            }
        }

        // UI Management and Display
        private void UpdateScoreDisplay()
        {
            List<Label> scoreLabels = new List<Label>
            {
                player1ScoreLabel,
                player2ScoreLabel,
                player3ScoreLabel,
                player4ScoreLabel
            };

            for (int i = 0; i < players.Count && i < scoreLabels.Count; i++)
            {
                scoreLabels[i].Content = $"Score: {players[i].GetScore()}";
                scoreLabels[i].Visibility = Visibility.Visible;
            }

            for (int i = players.Count; i < scoreLabels.Count; i++)
            {
                scoreLabels[i].Visibility = Visibility.Hidden;
            }
        }

        private void ContentInsert(int value1, int value2, bool isLeft)
        {
            Button newButton = new();

            var playedCards = game.GetDiscardTile().GetPlayedCards();
            var lastCard = isLeft ? playedCards.First() : playedCards.Last();

            newButton.Content = $"{GetBrailleFace(lastCard.GetLeftValueCard())} : {GetBrailleFace(lastCard.GetRightValueCard())}";
            newButton.Width = 40;
            newButton.Height = 20;
            newButton.FontSize = 12;
            newButton.IsEnabled = false;
            newButton.Style = (Style)FindResource(typeof(Button));

            bool isVertical = lastCard.GetLeftValueCard() == lastCard.GetRightValueCard();
            newButton.Tag = isVertical;

            if (isVertical)
            {
                TransformGroup transformGroup = new();
                transformGroup.Children.Add(new ScaleTransform());
                transformGroup.Children.Add(new SkewTransform());
                transformGroup.Children.Add(new RotateTransform(90));
                transformGroup.Children.Add(new TranslateTransform());

                newButton.RenderTransform = transformGroup;
                newButton.RenderTransformOrigin = new Point(0.5, 0.5);
                newButton.Margin = new Thickness(-8, 0, -8, 0);
            }

            StackPanelManager(newButton, isLeft);
        }

        private void StackPanelManager(Button button, bool isLeft)
        {
            const int maxPerStack = 8;
    
            if (isLeft)
            {
                LayerBottomStackPanel.Children.Insert(0, button);
        
                if (LayerBottomStackPanel.Children.Count > maxPerStack)
                {
                    CascadeOverflow(LayerBottomStackPanel, LayerRightStackPanel, LayerTopStackPanel, LayerLeftStackPanel);
                }
            }
            else
            {
                var stackOrder = new List<StackPanel> {
                    LayerBottomStackPanel,
                    LayerRightStackPanel,
                    LayerTopStackPanel,
                    LayerLeftStackPanel
                };
        
                foreach (var stack in stackOrder)
                {
                    if (stack.Children.Count < maxPerStack)
                    {
                        stack.Children.Add(button);
                        break;
                    }
                }
            }
            AdjustStackMargins(maxPerStack);
        }

        private void CascadeOverflow(params StackPanel[] stackOrder)
        {
            const int maxPerStack = 8;
    
            for (int i = 0; i < stackOrder.Length - 1; i++)
            {
                var current = stackOrder[i];
                var next = stackOrder[i + 1];
        
                if (current.Children.Count > maxPerStack)
                {
                    var childToMove = current.Children[current.Children.Count - 1];
                    current.Children.RemoveAt(current.Children.Count - 1);
                    next.Children.Insert(0, childToMove);
                }
            }
        }

        private void AdjustStackMargins(int maxPerStack)
        {
            //LayerRightWrapper.Margin = new Thickness(m.Left, m.Top, m.Right + (10 * 1), m.Bottom);

            if (LayerBottomStackPanel.Children.Count == maxPerStack && LayerBottomStackPanel.Margin.Bottom != 29)
            {
                LayerBottomStackPanel.Margin = new Thickness(0, 0, 0, 29);
            }
            else if (LayerBottomStackPanel.Children.Count == maxPerStack && LayerRightWrapper.Children.Count <= 8)
            {
                var m = LayerRightWrapper.Margin;
                LayerRightWrapper.Margin = new Thickness(m.Left, m.Top - 40, m.Right, m.Bottom);
            }
            else if (LayerBottomStackPanel.Children.Count == maxPerStack && LayerRightWrapper.Children.Count == 8 && LayerLeftWrapper.Children.Count <= 8)
            {
                var m = LayerLeftWrapper.Margin;
                LayerLeftWrapper.Margin = new Thickness(m.Left, m.Top + 40, m.Right, m.Bottom);
            }
        }



        // Utility Methods
        private string GetBrailleFace(int number)
        {
            string[] brailleDice = {
                "\u2800", // empty
                "\u2802", // 1: ⠂
                "\u2805", // 2: ⠅
                "\u2807", // 3: ⠇
                "\u282D", // 4: ⠭
                "\u283B", // 5: ⠻
                "\u283F"  // 6: ⠿
            };

            return brailleDice[number];
        }

        // Game Reset and Cleanup
        public void ResetGame()
        {
            players.Clear();
            game = null;
            selectedCard = null;
            currentButton = null;
            lastButton = null;

            ClearAllPlayerCards();
            ClearGameBoard();

            LoadStartup();
        }

        private void ClearAllPlayerCards()
        {
            Player1CardStackPanel.Children.Clear();
            Player2CardStackPanel.Children.Clear();
            Player3CardStackPanel.Children.Clear();
            Player4CardStackPanel.Children.Clear();
        }

        private void ClearGameBoard()
        {
            LayerBottomStackPanel.Children.Clear();
            LayerRightStackPanel.Children.Clear();
            LayerTopStackPanel.Children.Clear();
            LayerLeftStackPanel.Children.Clear();
            LayerBottomStackPanel.Margin = new Thickness(0, 0, 0, 200);
        }

        private void ResetSelectedCard()
        {
            selectedCard = null;
            currentButton = null;
            if (lastButton != null)
            {
                lastButton.IsEnabled = true;
                lastButton = null;
            }
        }

        private bool CheckAndHandleWinCondition()
        {
            if (game.CheckWinCondition())
            {
                string winnerName = game.GetCurrentPlayer().GetName();
                int points = game.CalculateScore();
                game.EndGame();

                MessageBox.Show($"{winnerName} wins the round with {points} points!");

                var gameWinner = players.FirstOrDefault(p => p.GetScore() >= maxScore);
                if (gameWinner != null)
                {
                    MessageBox.Show($"{gameWinner.GetName()} wins the entire game with {gameWinner.GetScore()} points!");
                    ResetGame();
                    return true;
                }

                StartNewRound();
                return true;
            }
            return false;
        }

        private void RemoveButton()
        {
            if (currentButton != null)
            {
                StackPanel parent = (StackPanel)currentButton.Parent;
                parent.Children.Remove(currentButton);
                currentButton = null;
                lastButton = null;
            }
        }

        // Event Handlers
        private void GetButtonValue(object sender, RoutedEventArgs e, Button button, ICard card)
        {
            selectedCard = (button.Tag is ICard tagCard) ? tagCard : card;
            lastButton = currentButton;
            currentButton = button;

            currentButton.IsEnabled = false;

            if (lastButton != null) lastButton.IsEnabled = true;
        }

        private void PlaceLeftButton_Click(object sender, RoutedEventArgs e)
        {
            PlaceCard("left");
        }

        private void PlaceRightButton_Click(object sender, RoutedEventArgs e)
        {
            PlaceCard("right");
        }

        private void debugButton_Click(object sender, RoutedEventArgs e)
        {
            var m = LayerRightWrapper.Margin;
            LayerRightWrapper.Margin = new Thickness(m.Left, m.Top, m.Right - 10, m.Bottom);
            debuglabel.Content = $"{m.Left.ToString()}, {m.Top.ToString()}, {m.Right.ToString()}, {m.Bottom.ToString()}";

        }

        private void debugButton2_Click(object sender, RoutedEventArgs e)
        {
            var m = LayerRightWrapper.Margin;
            LayerRightWrapper.Margin = new Thickness(m.Left, m.Top - 10, m.Right, m.Bottom);
            debuglabel.Content = $"{m.Left.ToString()}, {m.Top.ToString()}, {m.Right.ToString()}, {m.Bottom.ToString()}";

        }

        private void updateLabelDebug_Click(object sender, RoutedEventArgs e)
        {
            var m = LayerRightWrapper.Margin;
            debuglabel.Content = $"{m.Left.ToString()}, {m.Top.ToString()}, {m.Right.ToString()}, {m.Bottom.ToString()}";
        }
    }
}