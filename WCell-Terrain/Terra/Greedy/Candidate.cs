using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Terra.Greedy
{
    internal class Candidate
    {
        public int X;
        public int Y;
        public float Importance;

        public Candidate()
        {
            Importance = float.MinValue;
        }
            
        public void Consider(int subX, int subY, float importance)
        {
            if (importance <= Importance) return;
            
            X = subX;
            Y = subY;
            Importance = importance;
        }
    }
}
