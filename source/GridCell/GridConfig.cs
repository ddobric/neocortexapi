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
        public Tuple<int, int> spatialNavigationSize;

        public GridConfig()
        {

        }

        /// <summary>
        /// Set default value for parameters of <see cref="GridConfig"/>
        /// </summary>
        public void SetGridConfigDefaultParameters(Tuple<int, int> spatialNavigationSize)
        {
            mm = 5;
            nn = 5;
            tao = 0.9;
            ii = 0.3;
            sigma = 0.24;
            sigma2 = Math.Pow(sigma, 2);
            tt = 0.05;
            gridGain = new double[] { 0.04, 0.05, 0.06, 0.07, 0.08 };
            gridLayers = gridGain.Length;
            this.spatialNavigationSize = spatialNavigationSize;
        }

        public void Serialize(object obj, string name, StreamWriter sw)
        {
            throw new NotImplementedException();
        }

    }
}

