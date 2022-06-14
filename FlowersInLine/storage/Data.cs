using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using Color = System.Drawing.Color;

namespace FlowersInLine.storage
{
    static class Data
    {
        //все икоки из папки Flowers
        public static string[] flowersItems { get; private set; }
        //все иконки из папки Special
        public static string[] specialItems { get; private set; }
        //массив цветов соответствующий иконкам из папки Flowers
        public static System.Windows.Media.Brush[] colors { get; private set; }
        //иконка пустого элемента
        public static string emtyItem { get; private set; }
        //иконка бонуса бомбы
        public static string bombBonusItem { get; private set; }
        //иконка бонуса линии
        public static string lineBonusItem { get; private set; }


        //Заполнение данных
        static Data()
        {
            flowersItems = Directory.GetFiles(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Images\\Flowers\\");
            specialItems = Directory.GetFiles(Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Images\\Special\\");
            emtyItem = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Images\\NullElement\\null.png";
            bombBonusItem = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Images\\Special\\BombBonus.png";
            lineBonusItem = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Images\\Special\\LineBonus.png";
            _FillColors();
        }

        //заполнение массива colors 
        private static void _FillColors()
        {
            colors = new System.Windows.Media.Brush[flowersItems.Length];
            for(int i = 0; i < flowersItems.Length; i++) 
            {
                Bitmap bitmap = new Bitmap(flowersItems[i]);

                int R = 0;
                int G = 0;
                int B = 0;

                int pixelSum = 0;

                for(int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y <  bitmap.Height; y++)
                    {
                        System.Drawing.Color color = bitmap.GetPixel(x, y);
                        if(color.A == 255)
                        {
                            R += color.R;
                            G += color.G;
                            B += color.B;
                            pixelSum++;
                        }
                    }
                }

                R = R / pixelSum;
                G = G / pixelSum;
                B = B / pixelSum;

                colors[i] = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)R,(byte)G,(byte)B));
            }
        }
    }
}

