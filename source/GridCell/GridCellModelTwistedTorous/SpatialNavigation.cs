using NumSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GridCell
{
    public class SpatialNavigation
    {
        public readonly int arenaSize;

        public List<double> txx;
        public List<double> tyy;

        public bool add = true;

        private int directionX = 2;
        private int directionY = 2;

        public SpatialNavigation(int arenaSize)
        {
            this.arenaSize = arenaSize;
            txx = new() { arenaSize / 2 };
            tyy = new() { arenaSize / 2 };
        }

        public void RandomNavigation(int length)
        {
            //var theta = 90.0;

            for (int i = 0; i < length; i++)
            {
                //if (txx.Last() < 2)
                //{
                //    theta = np.random.randint(-85, 85);
                //}

                //if (txx.Last() > arenaSize - 2)
                //{
                //    theta = np.random.randint(95, 260);
                //}

                //if (tyy.Last() < 2)
                //{
                //    theta = np.random.randint(10, 170);
                //}

                //if (tyy.Last() > arenaSize - 2)
                //{
                //    theta = np.random.randint(190, 350);
                //}

                //var (cosine, sine) = Conv(theta);

                //txx.Add(txx.Last() + cosine + np.random.uniform(-0.5, 0.5).GetDouble());
                //tyy.Add(tyy.Last() + sine + np.random.uniform(-0.5, 0.5).GetDouble());

                //if (add) {
                //    txx.Add(txx.Last() + 30);
                //    tyy.Add(tyy.Last() + 30);
                //} else {
                //    txx.Add(txx.Last() - 30);
                //    tyy.Add(tyy.Last() - 30);
                //}

                //add = !add;

                if (txx.Last() < 2)
                {
                    directionX = 2;
                }

                if (txx.Last() > arenaSize - 2)
                {
                    directionX = -2;
                }

                if (tyy.Last() < 2)
                {
                    directionY = 2;
                }

                if (tyy.Last() > arenaSize - 2)
                {
                    directionY = -2;
                }

                txx.Add(txx.Last() + directionX + new Random().NextDouble() * 2 - 2);
                tyy.Add(tyy.Last() + directionY + new Random().NextDouble() * 2 - 2);
            }

            //Console.WriteLine(string.Join("\t", txx));
        }

        private Tuple<double, double> Conv(double degree)
        {
            var radians = Math.PI * degree / 180.0;
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);

            return Tuple.Create(cos, sin);
        }

        public void Plot()
        {
            //var plt = new ScottPlot.Plot(800, 600);
            //plt.AddScatter(txx.ToArray(), tyy.ToArray());
            //plt.SaveFig("quickstart.png");
        }
    }
}

