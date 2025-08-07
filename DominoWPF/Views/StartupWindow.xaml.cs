using DominoWPF.Classes;
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
    public partial class StartupWindow : Window
    {
        public bool closeMainWindow = true;
        AudioManager _audio = null;
        public StartupWindow(AudioManager audio)
        {
            _audio = audio;
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

        private void SetPlayerControls(TextBox textBox, Label label, bool isEnabled)
        {
            _audio.PlaySfx("Sounds/select.wav");
            textBox.IsEnabled = isEnabled;
            textBox.Clear();
            label.IsEnabled = isEnabled;
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
            playerNumberComboBox.SelectedIndex = 2;

            player1TextBox.Text = "Coach";
            player2TextBox.Text = "Ellis";
            player3TextBox.Text = "Nick";
            player4TextBox.Text = "Louis";

            maxScoreTextBox.Text = "100";
        }

        private void playerNumberComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            _audio.PlaySfx("Sounds/hover.wav");
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            _audio.PlaySfx("Sounds/hover.wav");
        }

        private void player1TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            _audio.PlaySfx("Sounds/hover.wav");
        }

        private void player2TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            _audio.PlaySfx("Sounds/hover.wav");
        }

        private void player3TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            _audio.PlaySfx("Sounds/hover.wav");
        }

        private void player4TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            _audio.PlaySfx("Sounds/hover.wav");
        }

        private void maxScoreTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            _audio.PlaySfx("Sounds/hover.wav");
        }

        private void player1TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _audio.PlaySfx("Sounds/select.wav");
        }

        private void player2TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _audio.PlaySfx("Sounds/select.wav");
        }

        private void player3TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _audio.PlaySfx("Sounds/select.wav");
        }

        private void player4TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _audio.PlaySfx("Sounds/select.wav");
        }

        private void maxScoreTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _audio.PlaySfx("Sounds/select.wav");
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

            _audio.PlaySfx("Sounds/select.wav");

            this.DialogResult = true;
            this.Close();
        }
    }
}
