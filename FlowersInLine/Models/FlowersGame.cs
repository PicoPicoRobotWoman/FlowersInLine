using FlowersInLine.Enums;
using FlowersInLine.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FlowersInLine.Models
{
    class FlowersGame
    {
        //время за которое должна выполятся любая анимация
        private TimeSpan _standartTime;
        //событие перемены мест элементов из flowerMatrix
        public event Action<int, int, int, int, TimeSpan> ChangingPlaces;
        //событие обновления данных об игре
        public event Action RenevalGame;
        //событие опускание вниз столбцов
        public event Action<List<FallInfo>> Fallen;
        //событие взрыва бонуса бомбы
        public event Action<int, int, Brush, TimeSpan> BombDetonation;
        //событие Активации бонуса линии горизантально
        public event Action<int, int, Brush, TimeSpan> LineHorizontalDetonation;
        //событие Активации бонуса линии вертикально
        public event Action<int, int, Brush, TimeSpan> LineVerticalDetonation;

        private Flower[,] flowerMatrix;
        private Flower[,] virtualFlowersMatrix;

        public int? selectedX { get; private set; }
        public int? selectedY { get; private set; }

        public int score { get; private set; }
        private Moved _moved;

        private Random _rnd;

        public FlowersGame(int rows, int collumns)
        {
            _rnd = new Random();
            _standartTime = new TimeSpan(0,0,0,0,250);
            virtualFlowersMatrix = new Flower[collumns, rows];

            score = 0;

            flowerMatrix = new Flower[collumns, rows];
            selectedX = null;
            selectedY = null;
            _moved = Moved.Yes;

            do
            {    
                for (int x = 0; x < flowerMatrix.GetLength(0); x++)
                {
                    for (int y = 0; y < flowerMatrix.GetLength(0); y++)
                    {
                        flowerMatrix[x, y] = new Flower(Data.flowersItems[_rnd.Next(0, Data.flowersItems.Length )], x, y);
                    }
                }
            }
            while (_AreTherelines());
        }

        //получение копии flowerMatrix
        public Flower[,]  GetFlowerMatrix()
        {
            return (Flower[,])flowerMatrix.Clone();
        }

        //метод вызываемый при нажатии на "цветок"
        public void ClickToFlower(int x, int y)
        {
            if (_moved == Moved.Yes)
            {
                if (selectedX == null && selectedY == null)
                {
                    selectedX = x;
                    selectedY = y;
                }
                else
                {
                    _GameProgress(x, y, (int)selectedX, (int)selectedY);
                    selectedX = null;
                    selectedY = null;
                }
            }
        }

        //проверка на наличие выстроеных по 3 одинаковых элементов из flowerMatrix
        private bool _AreTherelines()
        {
            for (int x = 1; x < flowerMatrix.GetLength(0) - 1; x++)
            {
                for (int y = 0; y < flowerMatrix.GetLength(1); y++)
                {
                    if (flowerMatrix[x, y].mainProperty == flowerMatrix[x - 1, y].mainProperty && flowerMatrix[x, y].mainProperty == flowerMatrix[x + 1, y].mainProperty)
                    {
                        return true;
                    }
                }
            }

            for (int x = 0; x < flowerMatrix.GetLength(0); x++)
            {
                for (int y = 1; y < flowerMatrix.GetLength(1) - 1; y++)
                {
                    if (flowerMatrix[x, y].mainProperty ==  flowerMatrix[x, y - 1].mainProperty && flowerMatrix[x, y].mainProperty ==  flowerMatrix[x, y + 1].mainProperty)
                    {
                        return true;
                    }
                }
            }

            List<Flower> flList = new List<Flower>();

            for (int x = 0; x < flowerMatrix.GetLength(0); x++)
            {
                for (int y = 0; y < flowerMatrix.GetLength(1); y++)
                {
                    if (flowerMatrix[x, y].mainProperty == Data.lineBonusItem || flowerMatrix[x, y].mainProperty == Data.bombBonusItem)
                    {
                        flList.Add(flowerMatrix[x, y]);
                    }
                }
            }

            foreach (Flower fl in flList)
            {
                int x = fl.coordinateX;
                int y = fl.coordinateY;
                if (x - 1 >= 0 && x + 1 < flowerMatrix.GetLength(0))
                {
                    if (flowerMatrix[x - 1, y].mainProperty == flowerMatrix[x + 1, y].mainProperty)
                    {
                        return true;
                    }
                }

                if (y - 1 >= 0 && y + 1 < flowerMatrix.GetLength(1))
                {
                    if (flowerMatrix[x, y - 1].mainProperty == flowerMatrix[x, y + 1].mainProperty)
                    {
                        return true;
                    }
                }

                if (x + 2 < flowerMatrix.GetLength(0))
                {
                    if (flowerMatrix[x + 1, y].mainProperty == flowerMatrix[x + 2, y].mainProperty)
                    {
                        return true;
                    }
                }

                if (y + 2 < flowerMatrix.GetLength(1))
                {
                    if (flowerMatrix[x, y + 1].mainProperty == flowerMatrix[x, y + 2].mainProperty)
                    {
                        return true;
                    }
                }

                if (x - 2 >= 0)
                {
                    if (flowerMatrix[x - 1, y].mainProperty == flowerMatrix[x - 2, y].mainProperty)
                    {
                        return true;
                    }
                }

                if (y - 2 >= 0)
                {
                    if (flowerMatrix[x, y - 1].mainProperty == flowerMatrix[x, y - 2].mainProperty)
                    {
                        return true;
                    }
                }
            }

            return false;

        }

        //метод меняющий местами элементы в flowerMatrix
        private void _Swap(int x1,int y1, int x2, int y2)
        {
            ChangingPlaces.Invoke(x1, y1, x2, y2, _standartTime);
            Thread.Sleep(_standartTime);

            Flower flow = flowerMatrix[x1, y1];
            flowerMatrix[x1, y1] = new Flower( flowerMatrix[x2, y2].mainProperty,x1,y1, flowerMatrix[x2, y2].color);
            flowerMatrix[x2, y2] = new Flower( flow.mainProperty,x2,y2,flow.color);

            RenevalGame.Invoke();

        }

        //асинронное логика игры
        private async void _GameProgress(int x1, int y1,int x2,int y2)
        {
            await Task.Run(() =>
            {
                _moved = Moved.No;
                int dx = Math.Abs(x2 - x1);
                int dy = Math.Abs(y2 - y1);
                if (dy + dx == 1)
                {

                    _Swap(x1, y1, x2, y2);
                    if (_AreTherelines())
                    {
                        do
                        {
                            virtualFlowersMatrix = GetFlowerMatrix();
                            _BonusActive();
                            _MatrixAnalis();
                            _EmtyFlowersAnalisis();
                            _FallenDown();
                        }
                        while (_AreTherelines());
                        //while (false);
                    }
                    else
                    {
                        _Swap(x1, y1, x2, y2);
                    }
                }
                _moved = Moved.Yes;
            });
        }

        //метод Активирующий бонусы
        private void _BonusActive()
        {
            List<Flower> flList = new List<Flower>();

            for(int x = 0; x < flowerMatrix.GetLength(0); x++)
            {
                for (int y = 0; y < flowerMatrix.GetLength(1); y++)
                {
                    if ( flowerMatrix[x, y].mainProperty == Data.lineBonusItem || flowerMatrix[x, y].mainProperty == Data.bombBonusItem)
                    {
                        flList.Add(flowerMatrix[x, y]);
                    }
                }
            }

            foreach (Flower fl in flList)
            {
                int x = fl.coordinateX;
                int y = fl.coordinateY;
                if (x - 1 >= 0 && x + 1 < flowerMatrix.GetLength(0))
                {
                    if (flowerMatrix[x - 1, y].mainProperty == flowerMatrix[x + 1, y].mainProperty)
                    {
                        if (fl.mainProperty == Data.bombBonusItem)
                        {
                            _BombBonusActivate(x, y);
                        }
                        if (fl.mainProperty == Data.lineBonusItem)
                        {
                            _LineBonusActivate(x, y, LineBonusProperty.Horizontal);                           
                        }
                        continue;
                    }
                }
                
                if (y - 1 >= 0 && y + 1 < flowerMatrix.GetLength(1))
                {
                    if (flowerMatrix[x, y - 1].mainProperty == flowerMatrix[x, y + 1].mainProperty)
                    {
                        if (fl.mainProperty == Data.bombBonusItem)
                        {
                            _BombBonusActivate(x, y);
                        }
                        if (fl.mainProperty == Data.lineBonusItem)
                        {
                            _LineBonusActivate(x, y, LineBonusProperty.Vertical);
                        }
                        continue;
                    }
                }

                if (x+2 <flowerMatrix.GetLength(0))
                {
                    if (flowerMatrix[x + 1, y].mainProperty == flowerMatrix[x + 2, y].mainProperty)
                    {
                        if (fl.mainProperty == Data.bombBonusItem)
                        {
                            _BombBonusActivate(x, y);
                        }
                        if (fl.mainProperty == Data.lineBonusItem)
                        {
                            _LineBonusActivate(x, y, LineBonusProperty.Horizontal);
                        }
                        continue;
                    }
                }

                if (y + 2 < flowerMatrix.GetLength(1))
                {
                    if (flowerMatrix[x , y+1].mainProperty == flowerMatrix[x , y+2].mainProperty)
                    {
                        if (fl.mainProperty == Data.bombBonusItem)
                        {
                            _BombBonusActivate(x, y);
                        }
                        if (fl.mainProperty == Data.lineBonusItem)
                        {
                            _LineBonusActivate(x, y, LineBonusProperty.Vertical);
                        }
                        continue;
                    }
                }

                if (x - 2 >= 0)
                {
                    if (flowerMatrix[x - 1, y].mainProperty == flowerMatrix[x - 2, y].mainProperty)
                    {
                        if (fl.mainProperty == Data.bombBonusItem)
                        {
                            _BombBonusActivate(x, y);
                        }
                        if (fl.mainProperty == Data.lineBonusItem)
                        {
                            _LineBonusActivate(x, y, LineBonusProperty.Horizontal);
                        }
                        continue;
                    }
                }

                if (y - 2 >=0)
                {
                    if (flowerMatrix[x, y - 1].mainProperty == flowerMatrix[x, y - 2].mainProperty)
                    {
                        if (fl.mainProperty == Data.bombBonusItem)
                        {
                            _BombBonusActivate(x, y);
                        }
                        if (fl.mainProperty == Data.lineBonusItem)
                        {
                            _LineBonusActivate(x, y, LineBonusProperty.Vertical);
                        }
                        continue;
                    }
                }

            }
        }

        //метод вычисляющий собранные структуры по 3 и более "цветов" 
        private void _MatrixAnalis()
        {

            for (int x = 1; x < flowerMatrix.GetLength(0) - 1; x++)
            {
                for (int y = 0; y < flowerMatrix.GetLength(1); y++)
                {
                    if (flowerMatrix[x, y].mainProperty == flowerMatrix[x - 1, y].mainProperty && flowerMatrix[x, y].mainProperty == flowerMatrix[x + 1, y].mainProperty)
                    {
                        score += 30;
                        virtualFlowersMatrix[x - 1, y].Delete();
                        virtualFlowersMatrix[x, y].Delete();
                        virtualFlowersMatrix[x + 1, y].Delete();
                    }
                }
            }

            for (int x = 0; x < flowerMatrix.GetLength(0); x++)
            {
                for (int y = 1; y < flowerMatrix.GetLength(1) - 1; y++)
                {
                    if (flowerMatrix[x, y].mainProperty == flowerMatrix[x, y - 1].mainProperty && flowerMatrix[x, y].mainProperty == flowerMatrix[x, y + 1].mainProperty)
                    {
                        score += 30;
                        virtualFlowersMatrix[x, y - 1].Delete();
                        virtualFlowersMatrix[x, y].Delete();
                        virtualFlowersMatrix[x, y + 1].Delete();
                    }
                }
            }

        }

        //метод анализирующий virtualFlowersMatrix
        private void _EmtyFlowersAnalisis()
        {
            List<Flower> emptyFlowers = new List<Flower>();

            for(int x = 0; x < virtualFlowersMatrix.GetLength(0); x++)
            {
                for (int y = 0; y < virtualFlowersMatrix.GetLength(1); y++)
                {
                    if(virtualFlowersMatrix[x,y].mainProperty == Data.emtyItem)
                    {
                        emptyFlowers.Add(virtualFlowersMatrix[x, y]);
                    }
                }
            }

            for (int i = 0; i < emptyFlowers.Count; i++)
            {
                if (_GetCloseFlowers(flowerMatrix[emptyFlowers[i].coordinateX, emptyFlowers[i].coordinateY]) >= 4)
                {
                    flowerMatrix[emptyFlowers[i].coordinateX, emptyFlowers[i].coordinateY].ToBombBonus();
                    emptyFlowers.Remove(emptyFlowers[i]);
                    i--;
                }
            }

            for (int i = 0; i < emptyFlowers.Count; i++)
            {
                if (_GetCloseFlowers(flowerMatrix[emptyFlowers[i].coordinateX, emptyFlowers[i].coordinateY]) == 3)
                {
                    flowerMatrix[emptyFlowers[i].coordinateX, emptyFlowers[i].coordinateY].ToLineBonus();
                    emptyFlowers.Remove(emptyFlowers[i]);
                    i--;
                }
            }

            foreach (Flower fl in emptyFlowers)
            {
                    flowerMatrix[fl.coordinateX, fl.coordinateY].Delete();               
            }

            RenevalGame.Invoke();

        }
        
        //возращает количество близких схожих элементов
        private int _GetCloseFlowers(Flower flower)
        {
            int count = 0;
            if(flower.coordinateY - 1 >= 0 && virtualFlowersMatrix[flower.coordinateX,flower.coordinateY-1].mainProperty == Data.emtyItem)
            {
                if (flower.Coincide(flowerMatrix[flower.coordinateX, flower.coordinateY - 1] ))
                {
                    count++;
                    if(flower.coordinateY - 2 >= 0 && virtualFlowersMatrix[flower.coordinateX, flower.coordinateY - 2].mainProperty == Data.emtyItem)
                    {
                        if (flower.Coincide(flowerMatrix[flower.coordinateX, flower.coordinateY - 2]))
                        {
                            count++;
                        }
                    }
                }
            }
            if(flower.coordinateY + 1 < flowerMatrix.GetLength(1) && virtualFlowersMatrix[flower.coordinateX, flower.coordinateY +1].mainProperty == Data.emtyItem)
            {
                if (flower.Coincide(flowerMatrix[flower.coordinateX, flower.coordinateY + 1]))
                {
                    count++;
                    if (flower.coordinateY + 2 < flowerMatrix.GetLength(1) && virtualFlowersMatrix[flower.coordinateX, flower.coordinateY + 2].mainProperty == Data.emtyItem)
                    {
                        if (flower.Coincide(flowerMatrix[flower.coordinateX, flower.coordinateY + 2]))
                        {
                            count++;
                        }

                    }
                }
            }
            if (flower.coordinateX + 1 < flowerMatrix.GetLength(0) && virtualFlowersMatrix[flower.coordinateX+1, flower.coordinateY ].mainProperty == Data.emtyItem)
            {
                if (flower.Coincide(flowerMatrix[flower.coordinateX + 1, flower.coordinateY]))
                {
                    count++;
                    if (flower.coordinateX + 2 < flowerMatrix.GetLength(0) &&  virtualFlowersMatrix[flower.coordinateX +2, flower.coordinateY ].mainProperty == Data.emtyItem)
                    {
                        if (flower.Coincide(flowerMatrix[flower.coordinateX + 2, flower.coordinateY]))
                        {
                            count++;
                        }

                    }
                }
            }
            if (flower.coordinateX - 1>=0 && virtualFlowersMatrix[flower.coordinateX-1, flower.coordinateY ].mainProperty == Data.emtyItem)
            {
                if (flower.Coincide(flowerMatrix[flower.coordinateX - 1, flower.coordinateY]))
                {
                    count++;
                    if(flower.coordinateX - 2 >= 0 && virtualFlowersMatrix[flower.coordinateX-2, flower.coordinateY ].mainProperty == Data.emtyItem)
                    {
                        if (flower.Coincide(flowerMatrix[flower.coordinateX - 2, flower.coordinateY]))
                        {
                            count++;
                        }

                    }
                }
            }     
            return count;
        }

        //проверяет существование пустого элемента в flowerMatrix
        private bool _IsExistNullElement()
        {
            bool answer = false;

            for (int x=0;x<flowerMatrix.GetLength(0);x++)
            {
                for(int y = 0; y < flowerMatrix.GetLength(1); y++)
                {
                    if(flowerMatrix[x,y].mainProperty == Data.emtyItem)
                    {
                        answer = true;
                    }
                }
            }
            return answer;
        }

        //метод опускающий элемент вниз по пустым элементам в flowMatrix
        private void _FallenDown()
        {
            while (true)
            {
                if (!_IsExistNullElement())
                    break;
                Thread.Sleep(_standartTime);

                List<FallInfo> fallInfoList = new List<FallInfo>();

                for (int x = 0; x < flowerMatrix.GetLength(0); x++)
                {
                    if (_ReturnNullElementInCollum(x) != -1)
                    {
                        int y = _ReturnNullElementInCollum(x);
                        for (int i = y; i > 0; i--)
                        {
                            flowerMatrix[x, i] = new Flower( flowerMatrix[x, i - 1].mainProperty,x,i, flowerMatrix[x, i - 1].color);

                        }
                        flowerMatrix[x, 0] = new Flower(Data.flowersItems[_rnd.Next(0, Data.flowersItems.Length)], x,0);
                        fallInfoList.Add(new FallInfo(x,y,_standartTime));
                    }
                }

                Fallen.Invoke(fallInfoList);
                Thread.Sleep(_standartTime);
                RenevalGame.Invoke();
            }

        }

        //метод возращающий позицию первого пустого элемента из столбце в flowerMatrix
        private int _ReturnNullElementInCollum(int coll)
        {
            for (int y = 0; y < flowerMatrix.GetLength(1); y++)
            {
                if (flowerMatrix[coll, y].mainProperty == Data.emtyItem)
                    return y;
            }

            return -1;
        }

        //метод взрывающий бомбу
        private void _BombBonusActivate(int x, int y)
        {

            flowerMatrix[x, y].Delete();

            score += 50;
            BombDetonation.Invoke(x,y,flowerMatrix[x,y].color,_standartTime);
            Thread.Sleep(_standartTime);

            if (x >=0 && y+1 < flowerMatrix.GetLongLength(1))
            {
                if (flowerMatrix[x,y + 1].mainProperty == Data.bombBonusItem)
                {
                    _BombBonusActivate(x, y+1);
                }
                else if (flowerMatrix[x, y + 1].mainProperty == Data.lineBonusItem)
                {
                    _LineBonusActivate(x, y + 1, LineBonusProperty.Horizontal);
                }
                else
                {
                    flowerMatrix[x, y + 1].Delete();
                }
            }
            if (x >= 0 && y - 1 >=0)
            {
                if (flowerMatrix[x, y - 1].mainProperty == Data.bombBonusItem)
                {
                    _BombBonusActivate(x, y - 1);
                }
                else if (flowerMatrix[x, y - 1].mainProperty == Data.lineBonusItem)
                {
                    _LineBonusActivate(x, y - 1, LineBonusProperty.Horizontal);
                }
                else
                {
                    flowerMatrix[x, y - 1].Delete();
                }
            }
            if (x -1 >= 0 && y  >= 0)
            {
                if (flowerMatrix[x-1, y ].mainProperty == Data.bombBonusItem)
                {
                    _BombBonusActivate(x-1, y );
                }
                else if (flowerMatrix[x -1, y ].mainProperty == Data.lineBonusItem)
                {
                    _LineBonusActivate(x -1, y , LineBonusProperty.Horizontal);
                }
                else
                {
                    flowerMatrix[x -1 , y ].Delete();
                }
            }
            if (x + 1 < flowerMatrix.GetLength(0) && y >= 0)
            {
                if (flowerMatrix[x+1, y ].mainProperty == Data.bombBonusItem)
                {
                    _BombBonusActivate(x + 1, y);
                }
                else if (flowerMatrix[x + 1, y].mainProperty == Data.lineBonusItem)
                {
                    _LineBonusActivate(x + 1, y, LineBonusProperty.Horizontal);
                }
                else
                {
                    flowerMatrix[x + 1, y].Delete();
                }
            }
            if (x - 1 >= 0 && y -1 >= 0)
            {
                if (flowerMatrix[x - 1, y - 1].mainProperty == Data.bombBonusItem)
                {
                    _BombBonusActivate(x - 1, y - 1);
                }
                else if (flowerMatrix[x - 1, y - 1].mainProperty == Data.lineBonusItem)
                {
                    _LineBonusActivate(x - 1, y - 1, LineBonusProperty.Horizontal);
                }
                else
                {
                    flowerMatrix[x - 1, y - 1].Delete();
                }
            }
            if (x + 1 < flowerMatrix.GetLength(0) && y + 1 <flowerMatrix.GetLength(1))
            {
                if (flowerMatrix[x + 1, y + 1].mainProperty == Data.bombBonusItem)
                {
                    _BombBonusActivate(x + 1, y + 1);
                }
                else if (flowerMatrix[x + 1, y + 1].mainProperty == Data.lineBonusItem)
                {
                    _LineBonusActivate(x + 1, y + 1, LineBonusProperty.Horizontal);
                }
                else
                {
                    flowerMatrix[x + 1, y + 1].Delete();
                }
            }
            if (x - 1 >= 0 && y + 1 < flowerMatrix.GetLength(1))
            {
                if (flowerMatrix[x - 1, y + 1].mainProperty == Data.bombBonusItem)
                {
                    _BombBonusActivate(x - 1, y + 1);
                }
                else if (flowerMatrix[x - 1, y + 1].mainProperty == Data.lineBonusItem)
                {
                    _LineBonusActivate(x - 1, y + 1, LineBonusProperty.Horizontal);
                }
                else
                {
                    flowerMatrix[x - 1, y + 1].Delete();
                }
            }
            if (x + 1 < flowerMatrix.GetLength(0) && y - 1 >=0)
            {
                if (flowerMatrix[x + 1, y - 1].mainProperty == Data.bombBonusItem)
                {
                    _BombBonusActivate(x + 1, y - 1);
                }
                else if (flowerMatrix[x + 1, y - 1].mainProperty == Data.lineBonusItem)
                {
                    _LineBonusActivate(x + 1, y - 1, LineBonusProperty.Horizontal);
                }
                else
                {
                    flowerMatrix[x + 1, y - 1].Delete();
                }
            }

        }

        //метод активации бонуса линии
        private void _LineBonusActivate(int x, int y, LineBonusProperty lbp)
        {
            flowerMatrix[x, y].Delete();
            if(lbp == LineBonusProperty.Horizontal)
            {
                score += 50;
                LineHorizontalDetonation.Invoke(x,y,flowerMatrix[x,y].color,_standartTime);
                Thread.Sleep(_standartTime);
                for(int i = 0; i < flowerMatrix.GetLength(0); i++)
                {
                    if (flowerMatrix[i,y].mainProperty == Data.bombBonusItem)
                    {
                        _BombBonusActivate(i, y);
                    }
                    if(flowerMatrix[i, y].mainProperty == Data.lineBonusItem)
                    {
                        _LineBonusActivate(i, y, LineBonusProperty.Vertical);
                    }
                    flowerMatrix[i, y].Delete();
                }
            }

            if(lbp == LineBonusProperty.Vertical)
            {
                score += 50;
                LineVerticalDetonation.Invoke(x, y, flowerMatrix[x, y].color, _standartTime);
                Thread.Sleep(_standartTime);
                for (int i = 0; i < flowerMatrix.GetLength(0); i++)
                {
                    if (flowerMatrix[x, i].mainProperty == Data.bombBonusItem)
                    {
                        _BombBonusActivate(x, i);
                    }
                    if (flowerMatrix[x, i].mainProperty == Data.lineBonusItem)
                    {
                        _LineBonusActivate(x, i, LineBonusProperty.Horizontal);
                    }
                    flowerMatrix[x, i].Delete();
                }
            }

          
        }
    }
}
