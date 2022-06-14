using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowersInLine.Models
{
    class FallInfo
    {
        public int Column { get; private set; }
        public int yy { get; private set; }
        public TimeSpan time { get; private set; }

        public FallInfo(int col, int Y, TimeSpan ts)
        {
            Column = col;
            yy = Y;
            time = ts;
        }
    }
}
