using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowersInLine.config
{
    static class Transmision
    {
        //событие на обновление времени на табло
        static public event Action<int> RenewalTimer;

        //событие на обновление очков игры на табло
        static public event Action<int> RenewalScore;

        //событие на скрытие части информации на табло
        static public event Action HideSomeInfo;

        //событие на показ части информации на табло
        static public event Action SeeSomeInfo;

        //событие на проигрывание музыки по абсолютному или относительному пути
        static public event Action<string> MusicPlay;

        static public void RenevalTimerInvoke(int second)
        {
            RenewalTimer.Invoke(second);
        }

        static public void RenevalScoreInvoke(int score)
        {
            RenewalScore.Invoke(score);
        }

        static public void HidePartInfoInvoke()
        {
            HideSomeInfo.Invoke();
        }

        static public void SeePartInfoInvoke()
        {
            SeeSomeInfo.Invoke();
        }

        static public void MusicPlayInvoke(string path)
        {
            MusicPlay.Invoke(path);
        }

    }
}
