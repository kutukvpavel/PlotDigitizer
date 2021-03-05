using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DigitizerEngine
{
    public class ImageDigitizer
    {
        public Color BackgroundEdge { get; set; } = Color.FromArgb(255, 255, 255);
        public int RequiredNeighbours { get; set; } = 2;
        public int MaxBackgroundDistance { get; set; } = 50;
        public int PointMarkerSize { get; set; } = 8;
        public Bitmap SourceBitmap { get; private set; }
        public int?[] Output { get; private set; }
        public DirectBitmap OutputBitmap { get; private set; }

        /*
         *  -1  0   1
         * 
         *  - > - > - >
         * ^          |  1
         * |          |  
         * ^          |  0
         * |          |
         * ^          | -1
         * |          |
         * < - < - < -
         *
         */
        private static readonly int[][] _Neighbours = new int[][] 
        { 
            new int[] { -1, 1 }, 
            new int[] { 0, 1 },
            new int[] { 1, 1 },
            new int[] { 1, 0 },
            new int[] { 1, -1 },
            new int[] { 0, -1 },
            new int[] { -1, -1 },
            new int[] { -1, 0 },
        };
        private int MaxBackgroundDistanceSquared;

        private DirectBitmap OpenImage(string path)
        {
            var image = Image.FromFile(path);
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            var res = new DirectBitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(res.Bitmap))
            {
                g.DrawImage(image, 0, 0, image.Width, image.Height);
            }
            SourceBitmap = new Bitmap(res.Bitmap);
            SourceBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return res;
        }
        private bool BackgroundColorDistanceCheck(Color c)
        {
            int diffR = BackgroundEdge.R - c.R;
            int diffG = BackgroundEdge.G - c.G;
            int diffB = BackgroundEdge.B - c.B;
            return (diffR * diffR + diffB * diffB + diffG * diffG) > MaxBackgroundDistanceSquared;
        }
        private int GetNeighbourCount(DirectBitmap bitmap, int column, int row)
        {
            return _Neighbours.Count(x =>
                BackgroundColorDistanceCheck(bitmap.GetPixel(column + x[0], row + x[1])));
        }
        private int? GetColumnPoint(DirectBitmap bitmap, int column)
        {
            List<int> buffer = new List<int>();
            int weightSum = 0;
            int limit = bitmap.Height - 1;
            for (int j = 1; j < limit; j++)
            {
                var current = bitmap.GetPixel(column, j);
                var neighbours = GetNeighbourCount(bitmap, column, j);
                /* Include a point into the buffer only if it differs from the background significantly
                 * and has at least N neighbours that satisfy the same condition
                 */
                if (BackgroundColorDistanceCheck(current) && neighbours >= RequiredNeighbours)
                {
                    buffer.Add(j * neighbours); //Multiply by averaging weight right away to save resources
                    weightSum += neighbours;
                }
            }
            if (buffer.Count == 0) return null;
            //Average the Y values with weights depending on the number of neighbours
            return (int)Math.Round(buffer.Sum() / (double)weightSum);
        }

        public void Digitize(string path)
        {
            //Open image
            var bitmap = OpenImage(path);
            Output = new int?[bitmap.Width - 2];

            //Construct dataset
            MaxBackgroundDistanceSquared = MaxBackgroundDistance * MaxBackgroundDistance;
            var options = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
            Parallel.For(1, bitmap.Width - 1, options, (i) =>
            {
                Output[i - 1] = GetColumnPoint(bitmap, i); //Scan clumns in parallel
            });

            //Subtract constant baseline and trim empty elements
            Output = Output.SkipWhile(x => x == null).ToArray();
            var parallel = Output.AsParallel();
            var baseline = parallel.Min();
            if (baseline > 0)
            {
                parallel = parallel.Select(x => (x - baseline));
            }
            Output = parallel.ToArray();

            //Create an image of constructed dataset
            OutputBitmap = new DirectBitmap(bitmap.Width, bitmap.Height);
            using (Graphics g = Graphics.FromImage(OutputBitmap.Bitmap))
            using (Brush bw = Brushes.White)
            using (Brush bb = Brushes.Black)
            {
                g.FillRectangle(bw, 0, 0, bitmap.Width, bitmap.Height);
                var points = Output.AsParallel().Select((x, i) =>
                {
                    if (x == null)
                    {
                        return new Rectangle(0, 0, 0, 0);
                    }
                    else 
                    {
                        return new Rectangle(i, (int)x, PointMarkerSize, PointMarkerSize); 
                    }
                }).Where(x => x.Width != 0);
                g.FillRectangles(bb, points.ToArray());
            }
            OutputBitmap.Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }
    }
}
