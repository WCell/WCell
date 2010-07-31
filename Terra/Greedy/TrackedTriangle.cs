using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terra.Geometry;
using Terra.Memory;

namespace Terra.Greedy
{
    internal class TrackedTriangle : Triangle
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
            var greedySub = sub as GreedySubdivision;
            if (greedySub == null)
            {
                Console.WriteLine("Attempted reference to a null GreedySubdivision.");
                return;
            }
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
