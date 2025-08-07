using DominoWPF.Classes;
using Microsoft.VisualBasic.Devices;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.IO;
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
using System.Windows.Threading;

// nuget install NAudio

namespace DominoWPF
{
    public partial class MainWindow : Window
    {
        #region Fields and Constants

        private Random rand = new Random();
        private ICard selectedCard = null;
        private Button currentButton = null;
        private Button lastButton = null;
        private List<IPlayer> players = [];
        private GameController game = null;
        private int maxScore = 150;

        private readonly Thickness baseBottomMargin = new Thickness(0, 0, 0, 180);
        private readonly Thickness baseTopMargin = new Thickness(0, 29, 185, 0);
        private readonly Thickness baseRightMargin = new Thickness(0, 319, -298, 0);
        private readonly Thickness baseLeftMargin = new Thickness(-285, 10, 0, 0);
        
        static AudioManager audio = new AudioManager();
        AudioSettingsWindow _audioSettingsWindow = null;
        VoiceManager voiceManager = new VoiceManager(audio);

        #endregion

        #region Initialization & Setup

        public MainWindow()
        {
            InitializeComponent();
            InitStackPanel();
            ChangeWindowSize();
            audio.PlayMusic("Sounds/bg_music.wav");

            LoadStartup();
            RandomKiryu();
        }

        private void RandomKiryu()
        {
            string folderPath = "Images/Wallpaper";
            string[] imageFiles = Directory.GetFiles(folderPath, "*.jpg", SearchOption.TopDirectoryOnly);

            if (imageFiles.Length == 0) return;

            Random rand = new Random();
            int index = rand.Next(imageFiles.Length);
            string imagePath = imageFiles[index];

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(System.IO.Path.GetFullPath(imagePath), UriKind.Relative);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            kiryu.Source = bitmap;
        }

        private void ChangeWindowSize()
        {
            this.Width += 16;
            this.Height += 15;
        }

        private void LoadStartup()
        {
            this.Effect = new BlurEffect();

            StartupWindow startup = new(audio);
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

            this.WindowState = WindowState.Maximized;
            this.Effect = null;
        }

        private void InitStackPanel()
        {
            LayerLeftWrapper.Margin = baseLeftMargin;
            LayerRightWrapper.Margin = baseRightMargin;

            LayerBottomStackPanel.Margin = baseBottomMargin;
            LayerTopStackPanel.Margin = baseTopMargin;

            MiddleEllipse.Visibility = Visibility.Hidden;
        }

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

            List<Label> playerNameLabels2 = new List<Label>
            {
                player1NameLabel2, player2NameLabel2, player3NameLabel2, player4NameLabel2
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
                playerNameLabels2[i].Content = players[i].GetName();
                playerScoreLabels[i].Content = $"Score: {players[i].GetScore()}";

                if (i < playerStackPanels.Count)
                {
                    playerStackPanels[i].Visibility = Visibility.Visible;
                    playerNameLabels2[i].Visibility = Visibility.Visible;
                }
            }

            Player1HandGrid.Visibility = players.Count >= 1 ? Visibility.Visible : Visibility.Collapsed;
            Player2HandGrid.Visibility = players.Count >= 2 ? Visibility.Visible : Visibility.Collapsed;
            Player3HandGrid.Visibility = players.Count >= 3 ? Visibility.Visible : Visibility.Collapsed;
            Player4HandGrid.Visibility = players.Count >= 4 ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Game Flow Management

        private void StartNewRound()
        {
            ClearGameBoard();

            game.ResetRound();
            game.InitDeck();
            game.ShuffleDeck();
            game.InitHand();

            game.StartGame();

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
                voiceManager.PlayRandom(game.GetCurrentPlayerIndex(), "skipped");
                MessageBox.Show($"{game.GetCurrentPlayer().GetName()} has no playable cards! Skipping turn.", "Skipping Player", MessageBoxButton.OK, MessageBoxImage.Information);
                game.NextTurn();
                skipCount++;

                if (skipCount >= players.Count)
                {
                    MessageBox.Show("No players can play! Game is blocked.", "Game Blocked", MessageBoxButton.OK, MessageBoxImage.Information);
                    game.HandleBlockedGame();

                    string blockedWinner = game.GetCurrentPlayer().GetName();
                    voiceManager.PlayRandom(game.GetCurrentPlayerIndex(), "taunt");
                    MessageBox.Show($"{blockedWinner} wins the blocked game!", $"{blockedWinner} Win This Round!", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    var gameWinner = players.FirstOrDefault(p => p.GetScore() >= maxScore);
                    if (gameWinner != null)
                    {
                        voiceManager.PlayRandom(game.GetCurrentPlayerIndex(), "taunt");
                        MessageBox.Show($"{gameWinner.GetName()} wins the entire game with {gameWinner.GetScore()} points!", $"{gameWinner.GetName()} Win The Game!", MessageBoxButton.OK, MessageBoxImage.Exclamation);

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
            voiceManager.PlayRandom(currentIndex, "myturn");

            Player1HandGrid.IsEnabled = false;
            Player2HandGrid.IsEnabled = false;
            Player3HandGrid.IsEnabled = false;
            Player4HandGrid.IsEnabled = false;

            List<Label> playerNameLabels2 = new List<Label>
                {
                    player1NameLabel2, player2NameLabel2, player3NameLabel2, player4NameLabel2
                };

            foreach (var label in playerNameLabels2)
            {
                label.Opacity = 0.5;
            }

            switch (currentIndex)
            {
                case 0:
                    Player1HandGrid.IsEnabled = true;
                    player1NameLabel2.Opacity = 1;
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
                    player2NameLabel2.Opacity = 1;
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
                    player3NameLabel2.Opacity = 1;
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
                    player4NameLabel2.Opacity = 1;
                    UpdateCardAvailability(Player4CardStackPanel);
                    break;
            }
        }

        #endregion

        #region Card & UI Management

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
            var discardTile = game.GetDiscardTile();

            foreach (var card in playerHand)
            {
                Button newButton = CreateDominoButton(card, game.FindPlayableCard(discardTile, card));
                newButton.Click += (sender, e) => GetButtonValue(sender, e, newButton, card);
                newButton.MouseEnter += HandButton_MouseEnter;
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
            Style dimStyle = (Style)FindResource("ExtraDimDisabledButton");
            Button newButton = new Button();
            newButton.Content = $"{GetBrailleFace(card.GetLeftValueCard())} │ {GetBrailleFace(card.GetRightValueCard())}";
            newButton.Width = 75;
            newButton.Height = 25;
            newButton.FontSize = 16;
            newButton.FontFamily = (FontFamily)FindResource("radjani_semibold");
            newButton.HorizontalAlignment = HorizontalAlignment.Center;
            newButton.VerticalAlignment = VerticalAlignment.Center;
            newButton.IsEnabled = isEnabled;
            newButton.Tag = game.IsDoubleValue();
            newButton.Style = dimStyle;

            LinearGradientBrush neonWhiteBrush = new LinearGradientBrush();
            neonWhiteBrush.StartPoint = new Point(0, 0);
            neonWhiteBrush.EndPoint = new Point(1, 1);
            neonWhiteBrush.GradientStops.Add(new GradientStop(Color.FromRgb(255, 255, 255), 0.0)); 
            neonWhiteBrush.GradientStops.Add(new GradientStop(Color.FromRgb(240, 248, 255), 0.3));
            neonWhiteBrush.GradientStops.Add(new GradientStop(Color.FromRgb(248, 248, 255), 1.0));
            newButton.Background = neonWhiteBrush;

            newButton.BorderBrush = (Brush)FindResource("BorderGlow");
            newButton.BorderThickness = new Thickness(1.5);

            DropShadowEffect glowEffect = new DropShadowEffect();
            glowEffect.Color = Color.FromRgb(74, 158, 255); 
            glowEffect.BlurRadius = 8;
            glowEffect.ShadowDepth = 0;
            glowEffect.Opacity = 0.6;
            newButton.Effect = glowEffect;

            game.AddCard(card);
            if (game.IsDoubleValue())
            {
                LinearGradientBrush neonRedBrush = (LinearGradientBrush)FindResource("NeonRedGradient");
                newButton.Foreground = neonRedBrush;

                DropShadowEffect redGlow = new DropShadowEffect();
                redGlow.Color = Color.FromRgb(255, 0, 64);
                redGlow.BlurRadius = 10;
                redGlow.ShadowDepth = 0;
                redGlow.Opacity = 0.8;
                newButton.Effect = redGlow;
            }
            else
            {
                newButton.Foreground = (Brush)FindResource("NeonBlueGradient");
            }

            return newButton;
        }

        private void UpdateCardAvailability(StackPanel stackPanel)
        {
            var currentPlayer = game.GetCurrentPlayer();
            var playerHand = game.GetPlayerHand(currentPlayer);
            var discardTile = game.GetDiscardTile();

            for (int i = 0; i < Math.Min(stackPanel.Children.Count, playerHand.Count); i++)
            {
                if (stackPanel.Children[i] is Button button)
                {
                    button.IsEnabled = game.FindPlayableCard(discardTile, playerHand[i]);
                }
            }
        }

        private void PlaceCard(string position)
        {
            if (selectedCard == null || currentButton == null)
            {
                MessageBox.Show("Please select a card first!", "Select Card", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                audio.PlaySfx("Sounds/domino_put.wav");
                StartDelay(() => ContentInsert(
                    cardToPlay.GetLeftValueCard(),
                    cardToPlay.GetRightValueCard(),
                    position == "left"
                ), 0.2);
                
                game.RemoveCard(cardToPlay);

                RemoveButton();
                NextTurn();
            }
            else
            {
                MessageBox.Show($"Cannot place card on the {position} side!", "Select Different Side", MessageBoxButton.OK, MessageBoxImage.Warning);
                currentButton.IsEnabled = true;
                currentButton = null;
            }
        }

        #endregion

        #region Visual Display & Layout Management

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
            newButton.Background = Brushes.White;
			newButton.Foreground = (Brush)FindResource("NeonPurpleGradient");
            game.AddCard(lastCard);

            newButton.Tag = game.IsDoubleValue();

            if (game.IsDoubleValue())
            {
				newButton.Foreground = (Brush)FindResource("NeonRedGradient");
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

            AdjustAllMargins();
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

        private void AdjustAllMargins()
        {
            if (LayerBottomStackPanel.Children.Count >= 8 && LayerBottomStackPanel.Margin.Bottom != 29)
            {
                LayerBottomStackPanel.Margin = new Thickness(0, 0, 0, 29);
                MiddleEllipse.Visibility = Visibility.Visible;
            }

            var rightMargin = new Thickness(baseRightMargin.Left, baseRightMargin.Top, baseRightMargin.Right, baseRightMargin.Bottom);
            var leftMargin = new Thickness(baseLeftMargin.Left, baseLeftMargin.Top, baseLeftMargin.Right, baseLeftMargin.Bottom);
            var topMargin = new Thickness(baseTopMargin.Left, baseTopMargin.Top, baseTopMargin.Right, baseTopMargin.Bottom);

            int totalInRight = LayerRightStackPanel.Children.Count;
            int totalInTop = LayerTopStackPanel.Children.Count;
            int verticalInBottom = CountVerticalCardsInStack(LayerBottomStackPanel);
            int verticalInRight = CountVerticalCardsInStack(LayerRightStackPanel);
            int verticalInTop = CountVerticalCardsInStack(LayerTopStackPanel);
            int bottomCount = LayerBottomStackPanel.Children.Count;
            int rightCount = LayerRightStackPanel.Children.Count;
            int topCount = LayerTopStackPanel.Children.Count;
            int topValue = 30;

            if (bottomCount >= 8 && rightCount < 8)
            {
                rightMargin.Top -= topValue;
            }

            if (totalInRight > 0)
            {
                int[] cardAdjustments = { 12, 52, 92, 132, 172, 212, 252, 292 };
                rightMargin.Top -= cardAdjustments[totalInRight - 1];
                if (totalInRight == 8)
                {
                    rightMargin.Top -= 27;
                }
            }

            if (verticalInBottom > 0 && bottomCount >= 8)
            {
                rightMargin.Top -= (topValue - 12);
                rightMargin.Right = baseRightMargin.Right + 12 * (verticalInBottom + 1);
            }

            if (verticalInRight > 0)
            {
                rightMargin.Top += (topValue - 14) * verticalInRight;
            }

            if (rightCount > 0 && bottomCount == 8)
            {
                if (LayerBottomStackPanel.Children[7] is Button bottomButton && LayerRightStackPanel.Children[0] is Button rightButton)
                {
                    if (!(bool)bottomButton.Tag && (bool)rightButton.Tag)
                    {
                        rightMargin.Top += 24;
                        rightMargin.Right -= 25;
                    }
                    else if ((bool)bottomButton.Tag && !(bool)rightButton.Tag)
                    {
                        rightMargin.Top -= 4;
                        rightMargin.Right -= 3;
                    }
                    else if (!(bool)bottomButton.Tag && !(bool)rightButton.Tag)
                    {
                        rightMargin.Top += 20;
                        rightMargin.Right -= 4;
                    }
                }
            }

            if (verticalInBottom == 0)
            {
                rightMargin.Top -= 17;
                rightMargin.Right += 1;
            }

            if (bottomCount > 0 && LayerBottomStackPanel.Children[bottomCount - 1] is Button bottomLastButton)
            {
                if ((bool)bottomLastButton.Tag)
                {
                    topMargin.Right -= 6;
                    topMargin.Top -= 8;
                }
            }

            if (rightCount > 0 && LayerRightStackPanel.Children[0] is Button rightFirstButton)
            {
                if ((bool)rightFirstButton.Tag)
                {
                    topMargin.Top += 1;
                    topMargin.Right -= 10;
                }
            }

            if (rightCount > 0 && LayerRightStackPanel.Children[rightCount - 1] is Button rightLastButton)
            {
                if ((bool)rightLastButton.Tag)
                {
                    topMargin.Top += 6;
                    topMargin.Right += 5;
                }
            }

            if (topCount > 0 && LayerTopStackPanel.Children[0] is Button topFirstButton)
            {
                if ((bool)topFirstButton.Tag)
                {
                    topMargin.Right += 2;
                    topMargin.Top -= 8;
                }
            }

            if (topCount > 0 && rightCount > 0)
            {
                var bottomLast = LayerBottomStackPanel.Children[7] as Button;
                var rightLast = LayerRightStackPanel.Children[7] as Button;
                var topFirst = LayerTopStackPanel.Children[0] as Button;
                var rightFirst = LayerRightStackPanel.Children[0] as Button;

                if (bottomLast != null && rightLast != null)
                {
                    if ((bool)bottomLast.Tag && (bool)rightLast.Tag)
                    {
                        topMargin.Right += 4;
                        topMargin.Top -= 6;
                    }
                    else if ((bool)bottomLast.Tag && !(bool)rightLast.Tag)
                    {
                        topMargin.Top -= 2;
                    }

                    if ((bool)topFirst.Tag && (bool)bottomLast.Tag)
                    {
                        topMargin.Right += 4;
                        topMargin.Top -= 3;
                    }

                    if ((bool)topFirst.Tag && (bool)rightFirst.Tag)
                    {
                        topMargin.Right += 4;
                        topMargin.Top -= 6;
                    }
                }
            }

            if (verticalInBottom > 0 && bottomCount >= 8)
            {
                topMargin.Right += 8 * verticalInBottom;
            }

            if (rightCount >= 8 && verticalInRight > 0)
            {
                topMargin.Top += 10 * verticalInRight;
                topMargin.Top += 4;
            }

            if (verticalInRight == 0 && bottomCount > 0 && rightCount > 0)
            {
                var bottomLast = LayerBottomStackPanel.Children[bottomCount - 1] as Button;
                if (bottomLast != null && (bool)bottomLast.Tag)
                {
                    topMargin.Top -= 4;
                    topMargin.Right += 2;
                }
            }

            if (verticalInRight > 0)
            {
                topMargin.Top += 1 * verticalInRight;
                topMargin.Top += 2;
            }


            if (verticalInRight > 0 && rightCount >= 8)
            {
                leftMargin.Top += (topValue - 28) * verticalInRight;
                leftMargin.Left = baseLeftMargin.Left + 14 * verticalInTop;
            }

            LayerRightWrapper.Margin = rightMargin;
            LayerLeftWrapper.Margin = leftMargin;
            LayerTopStackPanel.Margin = topMargin;
        }

        private int CountVerticalCardsInStack(StackPanel stack)
        {
            int count = 0;
            foreach (Button child in stack.Children.OfType<Button>())
            {
                if (child.Tag is bool isVertical && isVertical)
                {
                    count++;
                }
            }
            return count;
        }

        #endregion

        #region Utilities

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
        private void StartDelay(Action methodToCall, double seconds)
        {
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(seconds)
            };

            timer.Tick += (s, e) =>
            {
                timer.Stop();
                methodToCall?.Invoke();
            };

            timer.Start();
        }

        #endregion

        #region Game State & Cleanup

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

                voiceManager.PlayRandom(game.GetCurrentPlayerIndex(), "taunt");
                MessageBox.Show($"{winnerName} wins the round with {points} points!", $"{winnerName} Win The Round!", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                var gameWinner = players.FirstOrDefault(p => p.GetScore() >= maxScore);
                if (gameWinner != null)
                {
                    voiceManager.PlayRandom(game.GetCurrentPlayerIndex(), "taunt");
                    MessageBox.Show($"{gameWinner.GetName()} wins the entire game with {gameWinner.GetScore()} points!", $"{gameWinner.GetName()} Won The Game", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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

        #endregion

        #region Event Handlers

        private void AudioSettings_Click(object sender, MouseButtonEventArgs e)
        {
            audio.PlaySfx("Sounds/select.wav");

            if (_audioSettingsWindow == null || !_audioSettingsWindow.IsLoaded)
            {
                _audioSettingsWindow = new AudioSettingsWindow(audio, voiceManager);
                _audioSettingsWindow.SetPosition(this);

                _audioSettingsWindow.Closed += AudioSettingsWindow_Closed;

                _audioSettingsWindow.Show();
            }
            else
            {
                _audioSettingsWindow.Activate();
                _audioSettingsWindow.Focus();
            }
        }

        private void AudioSettingsWindow_Closed(object sender, EventArgs e)
        {
            if (_audioSettingsWindow != null)
            {
                _audioSettingsWindow = null;
            }
        }

        private void AudioSettingsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            audio.PlaySfx("Sounds/hover.wav");
        }

        private void GetButtonValue(object sender, RoutedEventArgs e, Button button, ICard card)
        {
            audio.PlaySfx("Sounds/domino_click.wav");

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
        private void HandButton_MouseEnter(object sender, MouseEventArgs e)
        {
            audio.PlaySfx("Sounds/domino_hover.wav");
        }
        #endregion
    }
}