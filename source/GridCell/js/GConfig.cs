using System;
using System.Collections.Generic;

namespace GridCell.js
{
    public class GConfig
    {
        public int rowNum;
        public int colNum;
        public double gain; // one of the gridGain
        public double tao;
        public double ii;
        public double sigma;
        public double sigma2;
        public double tt;
        public List<double[]> sdist;
        public double bias;
        public int arenaSize;

        public GConfig(int arenaSize)
        {
            this.arenaSize = arenaSize;
            rowNum = 10;
            colNum = 10;
            tao = 0.8 / 2;
            ii = 0.3 / 2;
            sigma = 0.24 / 2;
            sigma2 = Math.Pow(sigma, 2);
            tt = 0.05 / 2;
            sdist = new List<double[]>(){ 
                new double[] {0, 0},
                new double[] {-0.5, Math.Sqrt(3)/2 },
                new double[] {-0.5,-Math.Sqrt(3)/2 },
                new double[] {0.5,Math.Sqrt(3)/2 },
                new double[] {0.5,-Math.Sqrt(3)/2 },
                new double[] {-1,0 },
                new double[] {1,0 }
            };
            bias = Math.PI / 3;
            gain = 0.05;

            //activeCellThreshold = 0.9;
            //this.arenaSize = arenaSize;
        }
    }
}

