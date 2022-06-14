using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FlowersInLine.ViewModels
{
    public class StartMenuViewModel
    {

        public event Action leaf;

        //получение изображения для фона в формате Brush
        public Brush backImage
        {
            get
            {
                return new ImageBrush(new BitmapImage(new Uri("Images\\BackGround\\StartMenu\\IronMan.png", UriKind.RelativeOrAbsolute)));
            }
        }

        //команда выполняемая при нажатии на кнопку
        public ICommand ButtonClick
        {
            get
            {
                return new DelegateCommand( (obj) =>
                {
                    leaf.Invoke();
                });
            }
        }




    }
}
