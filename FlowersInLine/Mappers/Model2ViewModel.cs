using FlowersInLine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FlowersInLine.Mappers
{
    static class Model2ViewModel
    {
           
        private static string fullPath = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location) + "";

        //получение матрицы из изображений с координатами переведенными из Model
        public static Rectangle[,] GetFlowersMatrix(Flower[,] flowers)
        {
            Rectangle[,] rectangles = new Rectangle[flowers.GetLength(0), flowers.GetLength(1)];

            for(int x =0; x < rectangles.GetLength(0); x++)
            {
                for (int y = 0; y < rectangles.GetLength(1); y++)
                {
                    rectangles[x, y] = Flower2Rectangle(flowers[x, y]);
                }
            }

            return rectangles;
        }

        //перевод из Flower в Rectangle
        private static Rectangle Flower2Rectangle(Flower flower)
        {
            Rectangle rect = new Rectangle()
            {
                Fill = new ImageBrush(new BitmapImage(new Uri(flower.mainProperty, UriKind.RelativeOrAbsolute))),
                Tag = $"{flower.coordinateX} {flower.coordinateY}"
            };

            return rect;
        }

    }
}
