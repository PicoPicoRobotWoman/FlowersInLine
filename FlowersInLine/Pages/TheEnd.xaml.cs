using FlowersInLine.config;
using FlowersInLine.ViewModels;
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

namespace FlowersInLine.Pages
{
    /// <summary>
    /// Логика взаимодействия для TheEnd.xaml
    /// </summary>
    public partial class TheEnd : Page
    {
        public TheEnd()
        {
            InitializeComponent();
            TheEndViewModel tevm = new TheEndViewModel();
            DataContext = tevm;

            tevm.leaf += () =>
            {
                NavigationService.Navigate(new Uri("Pages\\StartMenu.xaml", UriKind.Relative));
            };

            //взаимодействие с MainWindow средствами статического класса Transmision
            Transmision.HidePartInfoInvoke();
            Transmision.MusicPlayInvoke(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Music\\EndGame.mp3");

        }
    }
}
