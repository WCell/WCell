namespace WCell.Terrain.MPQ.ADT.Components
{
    /// <summary>
    /// MCVT Chunk - Height information for the MCNK
    /// </summary>
    public class MapVertices
    {
        /// <summary>
        /// 145 Floats for the 9 x 9 and 8 x 8 grid of height data.
        /// </summary>
        public float[] Heights = new float[145];

        /// <summary>
        /// 145 Floats for the 9 x 9 and 8 x 8 grid of height data.
        /// </summary>
        public float[] GetLowResMapArray()
        {
            var heights = new float[81];

            for (var r = 0; r < 17; r++)
            {
                if (r % 2 != 0) continue;
                for (var c = 0; c < 9; c++)
                {
                    var count = ((r / 2) * 9) + ((r / 2) * 8) + c;
                    heights[c + ((r / 2) * 8)] = heights[count];
                }
            }
            return heights;
        }

        /// <summary>
        /// 145 Floats for the 9 x 9 and 8 x 8 grid of height data.
        /// </summary>
        public float[,] GetLowResMapMatrix()
        {
            // *  1    2    3    4    5    6    7    8    9       Row 0
            // *    10   11   12   13   14   15   16   17         Row 1
            // *  18   19   20   21   22   23   24   25   26      Row 2
            // *    27   28   29   30   31   32   33   34         Row 3
            // *  35   36   37   38   39   40   41   42   43      Row 4
            // *    44   45   46   47   48   49   50   51         Row 5
            // *  52   53   54   55   56   57   58   59   60      Row 6
            // *    61   62   63   64   65   66   67   68         Row 7
            // *  69   70   71   72   73   74   75   76   77      Row 8
            // *    78   79   80   81   82   83   84   85         Row 9
            // *  86   87   88   89   90   91   92   93   94      Row 10
            // *    95   96   97   98   99   100  101  102        Row 11
            // * 103  104  105  106  107  108  109  110  111      Row 12
            // *   112  113  114  115  116  117  118  119         Row 13
            // * 120  121  122  123  124  125  126  127  128      Row 14
            // *   129  130  131  132  133  134  135  136         Row 15
            // * 137  138  139  140  141  142  143  144  145      Row 16
            // We only want even rows
            var heights = new float[9, 9];
            for (var x = 0; x < 17; x++)
            {
                if (x % 2 != 0) continue;
                for (var y = 0; y < 9; y++)
                {
                    var count = ((x / 2) * 9) + ((x / 2) * 8) + y;
                    heights[y, x/2] = Heights[count];
                }
            }
            return heights;
        }
    }
}
