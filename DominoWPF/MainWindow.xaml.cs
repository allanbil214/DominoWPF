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

            LoadButton(Player1CardStackPanel, true);
            LoadButton(Player3CardStackPanel, true);

            LoadButton(Player2CardStackPanel, false);
            LoadButton(Player4CardStackPanel, false);
            
            ChangeWindowSize();

            LoadStartup();
        }

        public void LoadStartup()
        {
            this.Effect = new BlurEffect();

            StartupWindow startup = new();
            startup.ShowDialog();

            List<string> playerNames = [startup.player1TextBox.Text,
            startup.player2TextBox.Text, startup.player3TextBox.Text, startup.player4TextBox.Text];

            Startup_GetNames(playerNames);
            InitPlayer();
            ChangePlayerTurn();

            this.Effect = null;
        }

        public void InitPlayer()
        {
            player1NameLabel.Content = players[0].GetName();
            player2NameLabel.Content = players[1].GetName();
            player3NameLabel.Content = players[2].GetName();
            player4NameLabel.Content = players[3].GetName();

            game = new GameController(players);

            //MessageBox.Show($"{players[0].GetName()} {players[1].GetName()} {players[2].GetName()} {players[3].GetName()}");
        }

        public void ChangePlayerTurn()
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
                    break;
            }
            playerTurn += 1;
            if (playerTurn > 3) playerTurn = 0;
            MessageBox.Show(playerTurn.ToString());
        }

        public void Startup_GetNames(List<string> names)
        {
            Player player1 = new Player(names[0]);
            Player player2 = new Player(names[1]);
            Player player3 = new Player(names[2]);
            Player player4 = new Player(names[3]);

            players.Add(player1);
            players.Add(player2);
            players.Add(player3);
            players.Add(player4);
        }

        public void ChangeWindowSize()
        {
            this.Width = 816;
            this.Height = 465;
        }

        public void LoadButton(StackPanel stackPanel, bool isHorizontal)
        {
            for(int i = 1; i < 8; i++)
            {
                Button newButton = new();
                newButton.Content = $"{rand.Next(0, i + 1)} | {rand.Next(0, 6)}";
                newButton.Click += (sender, EventArgs) => { GetButtonValue(sender, EventArgs, newButton); };
                newButton.Width = 75;
                newButton.Height = 25;
                newButton.FontSize = 16;
                newButton.HorizontalAlignment = HorizontalAlignment.Center;
                newButton.VerticalAlignment = VerticalAlignment.Center;
                newButton.Margin = new Thickness(10, 0, 0, 0);
                stackPanel.Children.Add(newButton);
                if (!isHorizontal)
                {
                    newButton.Margin = new Thickness(0, 10, 0, 0);
                }
            }
        }

        public void RotateButton(Button button)
        {
            RotateTransform rt = new RotateTransform(90);
            button.RenderTransform = rt;
        }

        public void GetButtonValue(object sender, RoutedEventArgs e, Button button)
        {
            string value = button.Content.ToString();
            int leftValue = 0;
            int rightValue = 0;

            if (int.TryParse(value[0].ToString(), out leftValue))
            {
                selectedCardLeftValue = leftValue;
            }
            
            if (int.TryParse(value[4].ToString(), out rightValue))
            {
                selectedCardRightValue = rightValue;
            }

            lastButton = currentButton;
            currentButton = button;

            currentButton.IsEnabled = false;

            if (lastButton != null) lastButton.IsEnabled = true;

            //MessageBox.Show($"{selectedCardLeftValue} | {selectedCardRightValue}");
        }

        private void PlaceLeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentButton != null)
            {
                ContentInsert(selectedCardLeftValue, selectedCardRightValue, true);
                RemoveButton();

                ChangePlayerTurn();
            }
            else MessageBox.Show("Please select a card first!");
        }

        private void PlaceRightButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentButton != null)
            {
                ContentInsert(selectedCardLeftValue, selectedCardRightValue, false);
                RemoveButton();

                ChangePlayerTurn();
            }
            else MessageBox.Show("Please select a card first!");
        }

        public void ContentInsert(int value1, int value2, bool isLeft)
        {
            Button newButton = new();
            newButton.Content = $"{value1} : {value2}";
            newButton.Width = 39.6233333333333;
            newButton.Height = 19.96;
            newButton.FontSize = 12;
            newButton.IsEnabled = false;
            StackPanelManager(newButton, isLeft);
        }

        public void StackPanelManager(Button button, bool isLeft)
        {
            int layerBottomCount = LayerBottomStackPanel.Children.Count;
            int layerRightCount = LayerRightStackPanel.Children.Count;
            int layerTopCount = LayerTopStackPanel.Children.Count;
            int layerLeftCount = LayerLeftStackPanel.Children.Count;

            if (layerBottomCount == 8 && layerRightCount != 8 && layerTopCount != 8)
            {
                LayerBottomStackPanel.Margin = new Thickness(0, 150, 0, 0);
                
                if (isLeft) LayerRightStackPanel.Children.Insert(0, button);
                else LayerRightStackPanel.Children.Add(button);
            }
            else if (layerBottomCount == 8 && layerRightCount == 8 && layerTopCount != 8)
            {
                if (isLeft) LayerTopStackPanel.Children.Insert(0, button);
                else LayerTopStackPanel.Children.Add(button);
            }
            else if (layerBottomCount == 8 && layerRightCount == 8 && layerTopCount == 8)
            {
                if (isLeft) LayerLeftStackPanel.Children.Insert(0, button);
                else LayerLeftStackPanel.Children.Add(button);
            }
            else
            {
                if (isLeft) LayerBottomStackPanel.Children.Insert(0, button);
                else LayerBottomStackPanel.Children.Add(button);
            }
        }

        public void RemoveButton()
        {
            StackPanel parent = (StackPanel)currentButton.Parent;

            int index = parent.Children.IndexOf(currentButton);
            MessageBox.Show(index.ToString());

            parent.Children.RemoveAt(index);

            currentButton = null;
            lastButton = null;
        }

        public void GetPlayable()
        {

        }
    }
}