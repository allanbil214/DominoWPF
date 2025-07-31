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

        private void WindowStartup_Closed(object sender, EventArgs e)
        {
            if (closeMainWindow)
            {
                App.Current.MainWindow.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            closeMainWindow = false;
            this.Close();
        }
    }
}
