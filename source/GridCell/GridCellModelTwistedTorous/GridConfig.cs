using System;
using System.IO;
using NeoCortexApi.Entities;

namespace GridCell
{
    public class GridConfig : ISerializable
    {
        public int mm;
        public int nn;
        public double tao;
        public double ii;
        public double sigma;
        public double sigma2;
        public double tt;
        public double[] gridGain;
        public int gridLayers;
        public int arenaSize;
        public double activeCellThreshold;

        public GridConfig(int arenaSize)
        {
            mm = 10;
            nn = 10;
            tao = 0.9;
            ii = 0.3;
            sigma = 0.24;
            sigma2 = Math.Pow(sigma, 2);
            tt = 0.05;
            gridGain = new double[] { 0.04, 0.05, 0.06, 0.07, 0.08 };
            gridLayers = gridGain.Length;
            activeCellThreshold = 0.9;
            this.arenaSize = arenaSize;
        }

        public GridConfig(int mm, int nn, double tao, double ii, double sigma, double tt, double activeCellThreshold, int arenaSize)
        {
            this.mm = mm;
            this.nn = nn;
            this.tao = tao;
            this.ii = ii;
            this.sigma = sigma;
            sigma2 = Math.Pow(sigma, 2);
            this.tt = tt;
            gridGain = new double[] { 0.04, 0.05, 0.06, 0.07, 0.08 };
            gridLayers = gridGain.Length;
            this.activeCellThreshold = activeCellThreshold;
            this.arenaSize = arenaSize;
        }

        public void Serialize(object obj, string name, StreamWriter sw)
        {
            throw new NotImplementedException();
        }

    }
}

