using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlowersInLine
{
    interface IDelegateCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
}
