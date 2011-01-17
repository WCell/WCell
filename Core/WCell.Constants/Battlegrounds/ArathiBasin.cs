using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Battlegrounds
{
    public enum ABSounds
    {
        NodeContested = 8192,
        NodeCapturedAlliance = 8173,
        NodeCapturedHorde = 8213,
        NodeAssaultedAlliance = 8212,
        NodeAssaultedHorde = 8174,
        NearVictory = 8456
    }

    public enum BaseStates
    {
        Neutral = 0,
        CapturedAlliance = 1,
        CapturedHorde = 2,
        ContestedAlliance = 3,
        ContestedHorde = 4,
        End = 5,
    }
}
