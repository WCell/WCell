using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terra.Geometry;
using Terra.Memory;

namespace Terra.Greedy
{
    public class TrackedTriangle<T> : Triangle
    {
        // Candidate position
        private int subX;
        private int subY;

        public TrackedTriangle (Edge edge) : this (edge, Heap.NOT_IN_HEAP)
        {
        }

        public TrackedTriangle(Edge edge, int token) : base (edge, token)
        {
            
        }

        public override void Update(Subdivision sub)
        {
            var greedySub = (GreedySubdivision<T>) sub;
            greedySub.ScanTriangle(this);
        }

        public void SetCandidate(int x, int y, float f)
        {
            subX = x;
            subY = y;
        }

        public void GetCandidate(out int x, out int y)
        {
            x = subX;
            y = subY;
        }
    }
}
