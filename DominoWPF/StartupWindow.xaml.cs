using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DominoWPF
{
    /// <summary>
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        public bool closeMainWindow = true;
        public StartupWindow()
        {
            InitializeComponent();
            InitPlayerNumber();
        }

        public void InitPlayerNumber()
        {
            for (int i = 2; i <= 4; i++)
            {
                playerNumberComboBox.Items.Add(i.ToString());
            }
        }

        private void playerNumberComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedPlayers = playerNumberComboBox.SelectedIndex + 2;

            SetPlayerControls(player3TextBox, player3Label, selectedPlayers >= 3);
            SetPlayerControls(player4TextBox, player4Label, selectedPlayers >= 4);
        }

        private void SetPlayerControls(Control textBox, Control label, bool isEnabled)
        {
            textBox.IsEnabled = isEnabled;
            label.IsEnabled = isEnabled;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int selectedPlayers = playerNumberComboBox.SelectedIndex + 2;

            if (string.IsNullOrWhiteSpace(player1TextBox.Text) ||
                string.IsNullOrWhiteSpace(player2TextBox.Text) ||
                (selectedPlayers >= 3 && string.IsNullOrWhiteSpace(player3TextBox.Text)) ||
                (selectedPlayers >= 4 && string.IsNullOrWhiteSpace(player4TextBox.Text)))
            {
                MessageBox.Show("Please enter all player names before continuing.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(maxScoreTextBox.Text))
            {
                MessageBox.Show("Please enter maximum score before continuing.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void player4TextBox_Copy_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0) && !(e.Text == "." && !((TextBox)sender).Text.Contains(".")))
            {
                e.Handled = true;
            }
        }

        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            player1TextBox.Text = "John";
            player2TextBox.Text = "Jane";
            player3TextBox.Text = "Rusty";
            player4TextBox.Text = "Maris";

            playerNumberComboBox.SelectedIndex = 2;
            maxScoreTextBox.Text = "10";
        }
    }
}
