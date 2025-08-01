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
        Random rand = new Random();
        ICard selectedCard = null;
        int selectedCardLeftValue = 0;
        int selectedCardRightValue = 0;
        Button currentButton = null;
        Button lastButton = null;
        List<IPlayer> players = [];
        GameController game = null;
        int playerTurn = 0;

        public MainWindow()
        {
            InitializeComponent();

            ChangeWindowSize();

            LoadStartup();
        }

        public void ChangeWindowSize()
        {
            this.Width += 16;
            this.Height += 15;
        }


        public void LoadStartup()
        {
            this.Effect = new BlurEffect();

            StartupWindow startup = new();
            startup.ShowDialog();

            List<string> playerNames = [startup.player1TextBox.Text,
            startup.player2TextBox.Text, startup.player3TextBox.Text, startup.player4TextBox.Text];

            Startup_GetNames(playerNames);
            InitGame();
            LoadPlayerCards();
            ChangePlayerTurn();

            this.Effect = null;
        }

        public void InitGame()
        {
            List<Label> playerNameLabels = new List<Label>
            {
                player1NameLabel,
                player2NameLabel,
                player3NameLabel,
                player4NameLabel
            };

            for (int i = 0; i < players.Count && i < playerNameLabels.Count; i++)
            {
                playerNameLabels[i].Content = players[i].GetName();
            }

            game = new GameController(players);
            game.InitDeck();
            game.ShuffleDeck();
            game.InitHand();

            //MessageBox.Show($"{players[0].GetName()} {players[1].GetName()} {players[2].GetName()} {players[3].GetName()}");
        }
        public void Startup_GetNames(List<string> names)
        {
            int maxPlayers = Math.Min(names.Count, 4);

            for (int i = 0; i < maxPlayers; i++)
            {
                players.Add(new Player(names[i]));
            }
        }

        public void LoadPlayerCards()
        {
            Player1CardStackPanel.Children.Clear();
            Player2CardStackPanel.Children.Clear();
            Player3CardStackPanel.Children.Clear();
            Player4CardStackPanel.Children.Clear();

            LoadButton(Player1CardStackPanel, players[0], true);
            LoadButton(Player3CardStackPanel, players[2], true);
            LoadButton(Player2CardStackPanel, players[1], false);
            LoadButton(Player4CardStackPanel, players[3], false);
        }

        public string GetBrailleFace(int number)
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

        public void LoadButton(StackPanel stackPanel, IPlayer player, bool isHorizontal)
        {
            var playerHand = game.GetPlayerHand(player);

            foreach (var card in playerHand)
            {
                Button newButton = new();
                newButton.Content = $"{(card.GetLeftValueCard())} │ {(card.GetRightValueCard())}";
                newButton.Click += (sender, EventArgs) => { GetButtonValue(sender, EventArgs, newButton, card); };
                newButton.Width = 75;
                newButton.Height = 25;
                newButton.FontSize = 16;
                newButton.HorizontalAlignment = HorizontalAlignment.Center;
                newButton.VerticalAlignment = VerticalAlignment.Center;
                newButton.Margin = new Thickness(10, 0, 0, 0);

                newButton.IsEnabled = IsPlayableCard(card);

                stackPanel.Children.Add(newButton);
                if (!isHorizontal)
                {
                    newButton.Margin = new Thickness(0, 10, 0, 0);
                }

                //MessageBox.Show($"{GetBrailleFace(card.GetLeftValueCard())} {card.GetLeftValueCard()} │ " +
                //    $"{GetBrailleFace(card.GetRightValueCard())} {card.GetRightValueCard()}");
            }
        }

        public bool IsPlayableCard(ICard card)
        {
            var discardTile = game.GetDiscardTile();

            if (discardTile.GetPlayedCards().Count == 0) return true;

            int leftValue = discardTile.GetLeftValueDiscardTile();
            int rightValue = discardTile.GetRightValueDiscardTile();

            return (card.GetLeftValueCard() == leftValue || card.GetLeftValueCard() == rightValue ||
                card.GetRightValueCard() == rightValue || card.GetRightValueCard() == leftValue);
        }

        public void ChangePlayerTurn() // todo refactor this monstrocity
        {
            game.NextTurn();
            switch (playerTurn)
            {
                case 0:
                    Player1HandGrid.IsEnabled = true;
                    Player2HandGrid.IsEnabled = false;
                    Player3HandGrid.IsEnabled = false;
                    Player4HandGrid.IsEnabled = false;

                    PlaceLeftButton.Margin = new Thickness(10, 260, 0, 0);
                    PlaceRightButton.Margin = new Thickness(540, 260, 10, 10);


                    PlaceLeftButton.RenderTransformOrigin = new Point(0.5, 0.5);
                    PlaceLeftButton.RenderTransform = new RotateTransform(0);
                    PlaceRightButton.RenderTransformOrigin = new Point(0.5, 0.5);
                    PlaceRightButton.RenderTransform = new RotateTransform(0);

                    UpdateCardAvailability(Player1CardStackPanel);

                    break;

                case 1:
                    Player1HandGrid.IsEnabled = false;
                    Player2HandGrid.IsEnabled = true;
                    Player3HandGrid.IsEnabled = false;
                    Player4HandGrid.IsEnabled = false;

                    PlaceLeftButton.Margin = new Thickness(549, 236, 0, 0);
                    PlaceRightButton.Margin = new Thickness(550, 43, 0, 226);

                    PlaceLeftButton.RenderTransformOrigin = new Point(0.5, 0.5);
                    PlaceLeftButton.RenderTransform = new RotateTransform(-90);
                    PlaceRightButton.RenderTransformOrigin = new Point(0.5, 0.5);
                    PlaceRightButton.RenderTransform = new RotateTransform(-90);

                    UpdateCardAvailability(Player2CardStackPanel);

                    break;

                case 2:
                    Player1HandGrid.IsEnabled = false;
                    Player2HandGrid.IsEnabled = false;
                    Player3HandGrid.IsEnabled = true;
                    Player4HandGrid.IsEnabled = false;

                    PlaceLeftButton.Margin = new Thickness(540, 10, 0, 0);
                    PlaceRightButton.Margin = new Thickness(10, 10, 540, 260);

                    PlaceLeftButton.RenderTransformOrigin = new Point(0.5, 0.5);
                    PlaceLeftButton.RenderTransform = new RotateTransform(0);
                    PlaceRightButton.RenderTransformOrigin = new Point(0.5, 0.5);
                    PlaceRightButton.RenderTransform = new RotateTransform(0);

                    UpdateCardAvailability(Player3CardStackPanel);

                    break;

                case 3:
                    Player1HandGrid.IsEnabled = false;
                    Player2HandGrid.IsEnabled = false;
                    Player3HandGrid.IsEnabled = false;
                    Player4HandGrid.IsEnabled = true;

                    PlaceLeftButton.Margin = new Thickness(-2, 36, 0, 0);
                    PlaceRightButton.Margin = new Thickness(-3, 233, 553, 36);

                    PlaceLeftButton.RenderTransformOrigin = new Point(0.5, 0.5);
                    PlaceLeftButton.RenderTransform = new RotateTransform(90);
                    PlaceRightButton.RenderTransformOrigin = new Point(0.5, 0.5);
                    PlaceRightButton.RenderTransform = new RotateTransform(90);

                    UpdateCardAvailability(Player4CardStackPanel);

                    break;
            }

            var discardTile = game.GetDiscardTile();
            if (!game.HasPlayableCard(discardTile))
            {
                MessageBox.Show($"{game.GetCurrentPlayer().GetName()} has no playable cards! Skipping turn.");
                NextTurn();
            }
            //MessageBox.Show(playerTurn.ToString());
        }

        public void UpdateCardAvailability(StackPanel stackPanel)
        {
            var currentPlayer = game.GetCurrentPlayer();
            var playerHand = game.GetPlayerHand(currentPlayer);

            for(int i = 0; i < stackPanel.Children.Count && i < playerHand.Count; i++)
            {
                if(stackPanel.Children[i] is Button button)
                {
                    var card = playerHand[i];
                    button.IsEnabled = IsPlayableCard(card);
                }
            }
        }

        public void NextTurn()
        {
            selectedCard = null;
            currentButton = null;
            if(lastButton != null)
            {
                lastButton.IsEnabled = true;
                lastButton = null;
            }

            game.NextTurn();
            playerTurn = game.GetCurrentPlayerIndex();

            if (game.CheckWinCondition())
            {
                MessageBox.Show($"{game.GetCurrentPlayer().GetName()} wins!");
                game.EndGame();
                return;
            }
            ChangePlayerTurn();
        }

        public void RotateButton(Button button)
        {
            RotateTransform rt = new RotateTransform(90);
            button.RenderTransform = rt;
        }

        public void GetButtonValue(object sender, RoutedEventArgs e, Button button, ICard card)
        {
            selectedCard = card;
            lastButton = currentButton;
            currentButton = button;

            currentButton.IsEnabled = false;

            if (lastButton != null) lastButton.IsEnabled = true;
        }

        private void PlaceLeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCard != null && currentButton != null)
            {
                var currentPlayer = game.GetCurrentPlayer();

                if(game.PlayCard(currentPlayer, selectedCard, "left"))
                {
                    ContentInsert(selectedCard.GetLeftValueCard(), selectedCard.GetRightValueCard(), true);
                    game.RemoveCard(selectedCard);
                    RemoveButton();
                    NextTurn();
                }
                else
                {
                    MessageBox.Show("Cannot place card on the left side!");
                    currentButton.IsEnabled = true;
                    currentButton = null;
                }
            }
            else MessageBox.Show("Please select a card first!");
        }

        private void PlaceRightButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedCard != null && currentButton != null)
            {
                var currentPlayer = game.GetCurrentPlayer();

                MessageBox.Show($"{currentPlayer.GetName()} {selectedCard.GetLeftValueCard} {selectedCard.GetRightValueCard}");

                if (game.PlayCard(currentPlayer, selectedCard, "right"))
                {
                    ContentInsert(selectedCard.GetLeftValueCard(), selectedCard.GetRightValueCard(), false);
                    game.RemoveCard(selectedCard);
                    RemoveButton();
                    NextTurn();
                }
                else
                {
                    MessageBox.Show("Cannot place card on the right side!");
                    currentButton.IsEnabled = true;
                    currentButton = null;
                }
            }
            else
            {
                MessageBox.Show("Please select a card first!");
            }
        }

        public void ContentInsert(int value1, int value2, bool isLeft)
        {
            Button newButton = new();
            newButton.Content = $"{(value1)} : {(value2)}";
            newButton.Width = 39.6233333333333;
            newButton.Height = 19.96;
            newButton.FontSize = 12;
            newButton.IsEnabled = false;
            StackPanelManager(newButton, isLeft);
        }

        public void StackPanelManager(Button button, bool isLeft)
        {
            const int maxPerStack = 8;

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
                    if (isLeft)
                        stack.Children.Insert(0, button);
                    else
                        stack.Children.Add(button);

                    if (stack == LayerBottomStackPanel && stack.Children.Count == maxPerStack)
                    {
                        stack.Margin = new Thickness(0, 150, 0, 0);
                    }

                    break;
                }
            }
        }

        public void RemoveButton()
        {
            if (currentButton != null)
            {
                StackPanel parent = (StackPanel)currentButton.Parent;
                parent.Children.Remove(currentButton);
                currentButton = null;
                lastButton = null;
            }
        }
    }
}