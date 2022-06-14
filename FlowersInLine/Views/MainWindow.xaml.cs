using FlowersInLine.config;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlowersInLine
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MediaPlayer Music = new MediaPlayer() ;

        public MainWindow()
        {
            InitializeComponent();
            BaseArea.NavigationService.Navigate(new Uri("Pages\\StartMenu.xaml", UriKind.Relative));
            Sl_volume.Value = 50;

            Music.Volume = 0.5;

            //обработчики событий из статического класса "Transmision"
            Transmision.RenewalTimer += (second) =>
            {
                lb_timer.Content = second.ToString();
            };

            Transmision.RenewalScore += (score) =>
            {
                lb_score.Content = score.ToString();
            };

            Transmision.HideSomeInfo += () =>
            {
                Sp_match_info.Visibility = Visibility.Hidden;
            };

            Transmision.SeeSomeInfo += () =>
            {
                Sp_match_info.Visibility = Visibility.Visible;
            };

            Transmision.MusicPlay += (string path) =>
            {
                Music.Stop();
                Music.Open( new Uri(path,UriKind.RelativeOrAbsolute));
                Music.Play();
            };

        }

        //метод вызываемый при нажатии на "Х"
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //изменение громкости при взаимодействии со "Splider"
        private void Sl_volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Music.Volume = (sender as Slider).Value / (sender as Slider).Maximum;
        }
    }
}
