using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FlowersInLine.config;
using FlowersInLine.Mappers;
using FlowersInLine.Models;
using FlowersInLine.ViewModels;

namespace FlowersInLine.Pages
{
    /// <summary>
    /// Логика взаимодействия для TheGame.xaml
    /// </summary>
    public partial class TheGame : Page
    {
        const int _rows = 8;
        const int _columns = 8;

        private int _timerSeconds;

        const int _width = 560;
        const int _height = 560;

        private double _elemaentWidth;
        private double _elemaentHeight;

        private Rectangle _cadre;

        private Random _rnd;
        private Rectangle[,] _backImage;
        private Rectangle[,] _elementStorage;
        private FlowersGame _fg;

        public TheGame()
        {
            InitializeComponent();

            //взаимодействие с MainWindow средствами статического класса Transmision
            Transmision.RenevalScoreInvoke(0);
            Transmision.SeePartInfoInvoke();
            Transmision.MusicPlayInvoke(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Music\\AchievementAct1.mp3");

            _timerSeconds = 60;

            _rnd = new Random();
            _backImage = new Rectangle[_rows, _columns];

            //формирование рамки для выделенного элемента
            _cadre = new Rectangle()
            {
                Fill = new ImageBrush(new BitmapImage(new Uri(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Images\\Cadre\\Cadre.png", UriKind.RelativeOrAbsolute))),
            };

            //создание экземпляра класса FlowersGame + обработка всех его событий
            _fg = new FlowersGame(_rows,_columns);

            _fg.ChangingPlaces += _SwapFlowers;
            _fg.RenevalGame += _RenevalGame;
            _fg.Fallen += _FallenItems;
            _fg.BombDetonation += BoomBoom;
            _fg.LineHorizontalDetonation +=_LineBonusHorActive;
            _fg.LineVerticalDetonation += _LineBonusVerActive;

            //таймер
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler((sender, e) =>
            {
                if (_timerSeconds != 0)
                {
                    Transmision.RenevalTimerInvoke(_timerSeconds);
                    Transmision.RenevalScoreInvoke(_fg.score);
                    _timerSeconds -= 1;

                }
                else
                {
                    Transmision.RenevalTimerInvoke(0);
                    (sender as DispatcherTimer).Stop();
                    NavigationService.Navigate(new Uri("Pages\\TheEnd.xaml", UriKind.Relative));
                }
            });
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();

            _CreateBackImage();
            _DrawGame();
        }

        //Заполняет массив фоновых изаброжений
        private void _CreateBackImage()
        {
            string fullPath = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Images\\BackGround\\TheGame\\";

            string[] images = Directory.GetFiles(fullPath);

            for (int x = 0; x < _backImage.GetLength(0); x++)
            {
                for (int y = 0; y < _backImage.GetLength(1); y++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Fill = new ImageBrush(new BitmapImage(new Uri( images[_rnd.Next(0,images.Length-1)], UriKind.RelativeOrAbsolute)));
                    _backImage[x, y] = rect;
                }
            }
        }

        //Перерисовка всего игрового пространства
        private void _DrawGame()
        {
            game_area.Children.Clear();
            _RenevalSize();
            _DrawBackground();
            _DrawCadre();
            _DrawFlowers();
            _DrawSelectedFlow();
        }

        //Перерисовка фона
        private void _DrawBackground()
        {
            for (int x = 0; x < _columns; x++)
            {
                for (int y = 0; y < _rows; y++)
                {
                    Canvas.SetLeft(_backImage[x, y], x * _elemaentWidth);
                    Canvas.SetTop(_backImage[x, y], y * _elemaentHeight);
                    Canvas.SetZIndex(_backImage[x, y], -1);
                    _backImage[x, y].Width = _elemaentWidth;
                    _backImage[x, y].Height = _elemaentHeight;
                    game_area.Children.Add(_backImage[x, y]);
                }
            }
        }

        //перерисовка "рамки курсора"
        private void _DrawCadre()
        {
            _cadre.Width = _elemaentWidth;
            _cadre.Height = _elemaentHeight;
            Canvas.SetZIndex(_cadre, 1);
            game_area.Children.Add(_cadre);
        }

        //перерисовка всех "Цветов"
        private void _DrawFlowers()
        {
            Rectangle[,] rectangles = Model2ViewModel.GetFlowersMatrix(_fg.GetFlowerMatrix());
            _elementStorage = new Rectangle[rectangles.GetLength(0), rectangles.GetLength(1)];

            for (int x = 0; x < rectangles.GetLength(0); x++)
            {
                for (int y = 0; y < rectangles.GetLength(1); y++)
                {
                    Canvas.SetLeft(rectangles[x, y], x * _elemaentWidth);
                    Canvas.SetTop(rectangles[x, y], y * _elemaentHeight);
                    Canvas.SetZIndex(rectangles[x, y], 2);
                    rectangles[x, y].Width = _elemaentWidth;
                    rectangles[x, y].Height = _elemaentHeight;
                    rectangles[x, y].MouseMove += _MouseMove;
                    rectangles[x, y].MouseLeftButtonDown += _MouseClic;

                    _elementStorage[x, y] = rectangles[x, y];
                     game_area.Children.Add(rectangles[x, y]);
                }
            }
        }

        //Перерисовка для рамки выденленного элемента
        private void _DrawSelectedFlow()
        {
            if(_fg.selectedX != null && _fg.selectedY != null)
            {
                Rectangle selFlow = new Rectangle
                {
                    Fill = new ImageBrush(new BitmapImage(new Uri(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Images\\Cadre\\Cadre.png", UriKind.RelativeOrAbsolute))),
                    Width = _elemaentWidth,
                    Height = _elemaentHeight,
                };
                Canvas.SetLeft(selFlow, (int)_fg.selectedX * _elemaentWidth);
                Canvas.SetTop(selFlow, (int)_fg.selectedY * _elemaentHeight);
                Canvas.SetZIndex(selFlow, 1);
                game_area.Children.Add(selFlow);
            }
        } 
  
        //метод, вызываемый при наведении на "цветок"
        private void _MouseMove(object sender, RoutedEventArgs e)
        {
            string[] Coordinates = (sender as Rectangle).Tag.ToString().Split(new char[] { ' ' });

            int x = Convert.ToInt32(Coordinates[0]);
            int y = Convert.ToInt32(Coordinates[1]);

            Canvas.SetLeft(_cadre, x * _elemaentWidth);
            Canvas.SetTop(_cadre, y * _elemaentHeight);
            //_DrawGame();
        }

        //метод, при нажатии на ЛКМ по "цветку"
        private void _MouseClic(object sender, RoutedEventArgs e)
        {
            string[] Coordinates = (sender as Rectangle).Tag.ToString().Split(new char[] { ' ' });

            int x = Convert.ToInt32(Coordinates[0]);
            int y = Convert.ToInt32(Coordinates[1]);

            _fg.ClickToFlower(x, y);
            _DrawGame();
        }

        //Анимация на событие перемены местами 2 элементов
        private void _SwapFlowers(int x1, int y1, int x2, int y2, TimeSpan tp)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                int el1 = game_area.Children.IndexOf(_elementStorage[x1, y1]);
                int el2 = game_area.Children.IndexOf(_elementStorage[x2, y2]);

                DoubleAnimation invaderAnime1 = new DoubleAnimation();
                invaderAnime1.From = x1 * _elemaentWidth;
                invaderAnime1.To = x2 * _elemaentWidth;
                invaderAnime1.Duration = tp;
                game_area.Children[el1].BeginAnimation(Canvas.LeftProperty, invaderAnime1);

                DoubleAnimation invaderAnime2 = new DoubleAnimation();
                invaderAnime2.From = x2 * _elemaentWidth;
                invaderAnime2.To = x1 * _elemaentWidth;
                invaderAnime2.Duration = tp;
                game_area.Children[el2].BeginAnimation(Canvas.LeftProperty, invaderAnime2);

                DoubleAnimation Anime3 = new DoubleAnimation();
                Anime3.From = y1 * _elemaentHeight;
                Anime3.To = y2 * _elemaentHeight;
                Anime3.Duration = tp;
                game_area.Children[el1].BeginAnimation(Canvas.TopProperty, Anime3);

                DoubleAnimation Anime4 = new DoubleAnimation();
                Anime4.From = y2 * _elemaentHeight;
                Anime4.To = y1 * _elemaentHeight;
                Anime4.Duration = tp;
                game_area.Children[el2].BeginAnimation(Canvas.TopProperty, Anime4);

            });
        }

        //Анимация на событие обновления данных об состоянии игры
        private void _RenevalGame()
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                _DrawGame();
            });
        }

        //Обновление ширины и высоты
        private void _RenevalSize()
        {
            game_area.Width = _width;
            game_area.Height = _height;

            _elemaentWidth = _width / _columns;
            _elemaentHeight = _height / _rows;
        }

        //Анимация на событие "падения" колонн
        private void _FallenItems(List<FallInfo> fallList)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                for (int i = 0; i < fallList.Count; i++)
                {
                        int column = fallList[i].Column;

                        game_area.Children.Remove(_elementStorage[column, fallList[i].yy]);
                        for (int y = 0; y < fallList[i].yy; y++)
                        {
                            DoubleAnimation Anime = new DoubleAnimation();
                            Anime.From = (y) * _elemaentHeight;
                            Anime.To = (y + 1) * _elemaentHeight;
                            Anime.Duration = new Duration(fallList[i].time);
                            _elementStorage[column, y].BeginAnimation(Canvas.TopProperty, Anime);
                        }                   
                }
            });
        }

        //Анимация на событие взрыва бомбы
        private void BoomBoom(int x, int y, Brush brush,TimeSpan ts)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                Ellipse el = new Ellipse()
                {
                    Fill = brush,               
                };

                Canvas.SetZIndex(el, 5);
                Canvas.SetLeft(el, x * _elemaentWidth);
                Canvas.SetTop(el, y * _elemaentHeight);
                game_area.Children.Add(el);


                DoubleAnimation anime1 = new DoubleAnimation();
                anime1.From = 0;
                anime1.To = 3 * _elemaentWidth;
                anime1.Duration = ts;
                game_area.Children[game_area.Children.IndexOf(el)].BeginAnimation(Canvas.WidthProperty, anime1);
                game_area.Children[game_area.Children.IndexOf(el)].BeginAnimation(Canvas.HeightProperty, anime1);

                DoubleAnimation anime2 = new DoubleAnimation();
                anime2.From = (x + 0.5) * _elemaentWidth;
                anime2.To = (x-1) * _elemaentWidth;
                anime2.Duration = ts;
                game_area.Children[game_area.Children.IndexOf(el)].BeginAnimation(Canvas.LeftProperty, anime2);

                DoubleAnimation anime3 = new DoubleAnimation();
                anime3.From = (y + 0.5) * _elemaentWidth;
                anime3.To = (y - 1) * _elemaentWidth;
                anime3.Duration = ts;
                game_area.Children[game_area.Children.IndexOf(el)].BeginAnimation(Canvas.TopProperty, anime3);

            });
        }

        //Анимация на событие активации бонус-линии горизонтально
        private void _LineBonusHorActive(int x,int y, Brush brush, TimeSpan ts)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                String Way = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)+ "\\Images\\Inweder\\Invader.png";

                DoubleAnimation invaderAnime1 = new DoubleAnimation();
                invaderAnime1.From = x * _elemaentWidth;
                invaderAnime1.To = _columns * _elemaentWidth;
                invaderAnime1.Duration = new Duration(ts);

                DoubleAnimation invaderAnime2 = new DoubleAnimation();
                invaderAnime2.From = x * _elemaentWidth;
                invaderAnime2.To = 0;
                invaderAnime2.Duration = new Duration(ts);

                DoubleAnimation anime3 = new DoubleAnimation();
                anime3.From = 0;
                anime3.To = (_columns - x) * _elemaentWidth;
                anime3.Duration = new Duration(ts);

                DoubleAnimation anime4 = new DoubleAnimation();
                anime4.From =  0;
                anime4.To = x * _elemaentWidth;
                anime4.Duration = new Duration(ts);

                DoubleAnimation anime5 = new DoubleAnimation();
                anime5.From = x*_elemaentWidth;
                anime5.To = 0;
                anime5.Duration = new Duration(ts);

                Rectangle rec1 = new Rectangle()
                {
                    Stretch = Stretch.Fill,
                    Fill = new ImageBrush(new BitmapImage(new Uri(Way , UriKind.RelativeOrAbsolute))),
                    Width = _elemaentWidth,
                    Height = _elemaentHeight,
                    Tag = $"{x} {y}" 
                };
                Rectangle rec2 = new Rectangle()
                {
                    Stretch = Stretch.Fill,
                    Fill = new ImageBrush(new BitmapImage(new Uri(Way , UriKind.RelativeOrAbsolute))),
                    Width = _elemaentWidth,
                    Height = _elemaentHeight,
                    Tag = $"{x} {y}"
                };

                Rectangle rec3 = new Rectangle()
                {
                    Stretch = Stretch.Fill,
                    Fill = brush,
                    Width = _elemaentWidth,
                    Height = _elemaentHeight,
                    Tag = $"{x} {y}"
                };
                Rectangle rec4 = new Rectangle()
                {
                    Stretch = Stretch.Fill,
                    Fill = brush,
                    Width = _elemaentWidth,
                    Height = _elemaentHeight,
                    Tag = $"{x} {y}"
                };

                Canvas.SetLeft(rec3, x * _elemaentWidth);
                Canvas.SetTop(rec3, y * _elemaentHeight);
                Canvas.SetZIndex(rec3, 4);

                Canvas.SetLeft(rec4, x * _elemaentWidth);
                Canvas.SetTop(rec4, y * _elemaentHeight);
                Canvas.SetZIndex(rec4, 4);

                Canvas.SetTop(rec1, y * _elemaentWidth);
                Canvas.SetLeft(rec1, x * _elemaentWidth);
                Canvas.SetZIndex(rec1, 5);

                Canvas.SetTop(rec2, y * _elemaentHeight);
                Canvas.SetLeft(rec2, x * _elemaentWidth);
                Canvas.SetZIndex(rec2, 5);

                game_area.Children.Add(rec1);
                game_area.Children.Add(rec2);
                game_area.Children.Add(rec3);
                game_area.Children.Add(rec4);

                rec2.BeginAnimation(Canvas.LeftProperty, invaderAnime2);
                rec1.BeginAnimation(Canvas.LeftProperty, invaderAnime1);
                rec3.BeginAnimation(Canvas.WidthProperty, anime3);
                rec4.BeginAnimation(Canvas.WidthProperty, anime4);
                rec4.BeginAnimation(Canvas.LeftProperty, anime5);
            });
        }

        //Анимация на событие активации бонус-линии вертикально
        private void _LineBonusVerActive(int x, int y, Brush brush, TimeSpan ts)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                String Way = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Images\\Inweder\\Invader.png";

                DoubleAnimation invaderinvaderAnime1 = new DoubleAnimation();
                invaderinvaderAnime1.From = y * _elemaentHeight;
                invaderinvaderAnime1.To = _height;
                invaderinvaderAnime1.Duration = new Duration(ts);

                DoubleAnimation invaderAnime2 = new DoubleAnimation();
                invaderAnime2.From = y * _elemaentHeight;
                invaderAnime2.To = 0;
                invaderAnime2.Duration = new Duration(ts);

                DoubleAnimation anime3 = new DoubleAnimation();
                anime3.From = 0;
                anime3.To = (_rows-y)*_elemaentHeight;
                anime3.Duration = new Duration(ts);

                DoubleAnimation anime4 = new DoubleAnimation();
                anime4.From = 0;
                anime4.To = y*_elemaentHeight;
                anime4.Duration = new Duration(ts);

                DoubleAnimation anime5 = new DoubleAnimation();
                anime5.From = y*_elemaentHeight;
                anime5.To = 0;
                anime5.Duration = new Duration(ts);

                Rectangle rec1 = new Rectangle()
                {
                    Stretch = Stretch.Fill,
                    Fill = new ImageBrush(new BitmapImage(new Uri(Way, UriKind.Absolute))),
                    Width = _elemaentWidth,
                    Height = _elemaentHeight,
                    Tag = $"{x} {y}"
                };
                Rectangle rec2 = new Rectangle()
                {
                    Stretch = Stretch.Fill,
                    Fill = new ImageBrush(new BitmapImage(new Uri(Way , UriKind.Absolute))),
                    Width = _elemaentWidth,
                    Height = _elemaentHeight,
                    Tag = $"{x} {y}"
                };

                Rectangle rec3 = new Rectangle()
                {
                    Stretch = Stretch.Fill,
                    Fill = brush,
                    Width = _elemaentWidth,
                    Height = _elemaentHeight,
                    Tag = $"{x} {y}"
                };
                Rectangle rec4 = new Rectangle()
                {
                    Stretch = Stretch.Fill,
                    Fill = brush,
                    Width = _elemaentWidth,
                    Height = _elemaentHeight,
                    Tag = $"{x} {y}"
                };

                Canvas.SetLeft(rec3, x * _elemaentWidth);
                Canvas.SetTop(rec3, y * _elemaentHeight);
                Canvas.SetZIndex(rec3, 4);

                Canvas.SetLeft(rec4, x * _elemaentWidth);
                Canvas.SetTop(rec4, y * _elemaentHeight);
                Canvas.SetZIndex(rec4, 4);

                Canvas.SetLeft(rec1, x * _elemaentWidth);
                Canvas.SetTop(rec1, y * _elemaentHeight);
                Canvas.SetZIndex(rec1, 5);

                Canvas.SetLeft(rec2, x * _elemaentWidth);
                Canvas.SetTop(rec2, y * _elemaentHeight);
                Canvas.SetZIndex(rec2, 5);

                game_area.Children.Add(rec1);
                game_area.Children.Add(rec2);
                game_area.Children.Add(rec3);
                game_area.Children.Add(rec4);

                rec2.BeginAnimation(Canvas.TopProperty, invaderAnime2);
                rec1.BeginAnimation(Canvas.TopProperty, invaderinvaderAnime1);
                rec3.BeginAnimation(Canvas.HeightProperty, anime3);
                rec4.BeginAnimation(Canvas.HeightProperty, anime4);
                rec4.BeginAnimation(Canvas.TopProperty, anime5);

            });

        }
    }
}
