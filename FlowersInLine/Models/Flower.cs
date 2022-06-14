using FlowersInLine.Enums;
using FlowersInLine.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FlowersInLine.Models
{
    struct Flower
    {

        public string mainProperty { get; private set; }
        public int coordinateX { get; private set; }
        public int coordinateY { get; private set; }
        public Brush color { get; private set; }

        public Flower(string str,int x , int y)
        {
            mainProperty = str;
            coordinateX = x;
            coordinateY = y;
            color = null;

            for (int i = 0; i < Data.flowersItems.Length; i++)
            {
                if(mainProperty == Data.flowersItems[i])
                {
                    color = Data.colors[i];
                }
            }
        }

        public Flower(string str, int x, int y , Brush brush)
        {
            mainProperty = str;
            coordinateX = x;
            coordinateY = y;
            color = brush;
        }


        public bool ChecEquality(Flower flower)
        {
            if(this.mainProperty == flower.mainProperty)
            {
                if (this.mainProperty != Data.emtyItem)
                    return true;
            }
            foreach (string st in Data.specialItems)
            {
                if (this.mainProperty == st)
                    return true;
            }
            return false;
        }

        public bool Coincide(Flower flower)
        {
            if (this.mainProperty == flower.mainProperty)
            {
                if (this.mainProperty != Data.emtyItem)
                    return true;
            }
            return false;
        }

        public void ToBombBonus()
        {
            this.mainProperty = Data.bombBonusItem;
        }

        public void ToLineBonus()
        {
            this.mainProperty = Data.lineBonusItem;
        }

        public void Delete()
        {
                mainProperty = Data.emtyItem;
        }
    }
}
