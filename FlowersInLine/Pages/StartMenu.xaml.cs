using System;
using System.Collections.Generic;
using System.IO;
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
using FlowersInLine.config;
using FlowersInLine.ViewModels;

namespace FlowersInLine.Views
{
    /// <summary>
    /// Логика взаимодействия для StartMenu.xaml
    /// </summary>
    public partial class StartMenu : Page
    {
        

        public StartMenu()
        {
            InitializeComponent();
            StartMenuViewModel smvm = new StartMenuViewModel();
            smvm.leaf += () =>
            {
                this.NavigationService.Navigate(new Uri("Pages\\TheGame.xaml", UriKind.Relative));
            };
            DataContext = smvm;

            //взаимодействие с MainWindow средствами статического класса Transmision
            Transmision.HidePartInfoInvoke();
            Transmision.MusicPlayInvoke(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)+ "\\Music\\MainMenu.mp3");

        }

    }
}
