using System;
using System.Collections.Generic;
using System.Text;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;

namespace GridCell.js
{
    public class GCell
    {
        public GConfig config;
        public List<double[]> networkActivity = new();
        public List<double[]> networkActivityCopy = new();

        public double mean = 0;
	    public double min = 1000;
	    public double max = 0;

        public double[] modulation = new double[2] { 0, 0 };

        public GCell(GConfig config)
        {
            this.config = config;
            CreateNetwork();
        }

        private void CreateNetwork()
        {
            for (var i = 0; i < config.colNum; i += 1)
            {
                var subarray = new double[config.rowNum];
                for (var j = 0; j < config.rowNum; j += 1)
                {
                    var act = new Random().NextDouble() / (Math.Sqrt(config.rowNum * config.colNum)) * 10;
                    subarray[j] =  act;
                }
                networkActivity.Add(subarray);
                networkActivityCopy.Add(subarray);
            }
        }

        public List<Cell> Compute(double[] deltaMove, bool learn = true)
        {
            GetMean();
            for (var i = 0; i < config.colNum; i += 1)
            {
                for (var j = 0; j < config.rowNum; j += 1)
                {
                    var cellAcitvity = networkActivityCopy[i][j];
                    for (var ii = 0; ii < config.colNum; ii += 1)
                    {
                        for (var jj = 0; jj < config.rowNum; jj += 1)
                        {
                            cellAcitvity += networkActivityCopy[ii][jj] * UpdateWeights(i, j, ii, jj, deltaMove);
                        }
                    }
                    networkActivity[i][j] = cellAcitvity + config.tao * (cellAcitvity / mean - cellAcitvity);
                    if (networkActivity[i][j] < 0.0) { networkActivity[i][j] = 0.0; }
                }
            }

            CopyMatrix();
            return CalculateActiveCells();
        }

        private List<Cell> CalculateActiveCells() {
            List<Cell> activeCells = new();
            var builder = new StringBuilder();
            for (var x = 0; x < config.colNum; x++){
                for (var y = 0; y < config.rowNum; y++)
                {
                    var val = (networkActivity[x][y] - min) / (max - min);
                    builder.Append(val);
                    builder.Append(",");
                    if (val > 0)
                    {
                        activeCells.Add(new Cell(x, y, config.rowNum, CellActivity.ActiveCell));
                    }
                }
            }
            //Console.WriteLine(builder.ToString());
            //Console.WriteLine("-----------------------------------------------------------");
            return activeCells;
        }

        public double[,] GetHeatMapValues()
        {
            var arr = new double[config.rowNum, config.colNum];
            for (int i = 0; i < config.rowNum; i++)
            {
                for (int j = 0; j < config.colNum; j++)
                {
                    arr[i, j] = (networkActivity[i][j] - min) / (max - min);
                }
            }

            return arr;
        }


        //private void findCorrelation()
        //{
        //    if (values1.Length != values2.Length)
        //        throw new ArgumentException("values must be the same length");

        //    var avg1 = values1.Average();
        //    var avg2 = values2.Average();

        //    var sum1 = values1.Zip(values2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();

        //    var sumSqr1 = values1.Sum(x => Math.Pow((x - avg1), 2.0));
        //    var sumSqr2 = values2.Sum(y => Math.Pow((y - avg2), 2.0));

        //    var result = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);

        //    return result;
        //}

        private void CopyMatrix()
        {
            for (var x = 0; x < config.rowNum; x++)
            {
                for (var y = 0; y < config.colNum; y++)
                {
                    networkActivityCopy[x][y] = networkActivity[x][y];
                    if (networkActivityCopy[x][y] < min) { min = networkActivityCopy[x][y]; }
                    if (networkActivityCopy[x][y] > max) { max = networkActivityCopy[x][y]; }
                }
            }
        }

        private double UpdateWeights(int x, int y, int xx, int yy, double[] deltaMove)
        {
            ModulationFunc(deltaMove);

            var i = new double[] {
                ((x - 0.5) / config.colNum),
                (((Math.Sqrt(3) / 2) * (y - 0.5)) / config.rowNum)
            };    // i=(ix,iy)

            var j = new double[] {
                ((xx - 0.5) / config.colNum),
                (((Math.Sqrt(3) / 2) * (yy - 0.5)) / config.rowNum)
            }; // j=(jx,jy)

            var su2 = new double[] {
                i[0] - j[0] + modulation[0],
                i[1] - j[1] + modulation[1]
            };

            var distTri = new List<double>();
            var disMin = 1000.0;

            for (var q = 0; q < config.sdist.Count; q++)
            {
                var norm = new double[]{
                    su2[0] + config.sdist[q][0],
                    su2[1] + config.sdist[q][1]
                };
                distTri.Add(Math.Sqrt(Math.Pow(norm[0], 2) + Math.Pow(norm[1], 2)));

                if (distTri[q] < disMin) { disMin = distTri[q]; }
            }
            var wij = config.ii * Math.Exp(-1 * Math.Pow(disMin, 2) / (2.0 * config.sigma2)) - config.tt;

            return wij;
        }

        private void GetMean()
        {
            min = 1000;
            max = 0;

            double mean = 0;
            for (var i = 0; i < config.colNum; i += 1)
            {
                for (var j = 0; j < config.rowNum; j += 1)
                {
                    mean += networkActivity[i][j];
    
                }
            }

            this.mean = mean / (config.colNum * config.rowNum);
        }

        private void ModulationFunc(double[] deltaMove)
        {
            modulation[0] = config.gain * (deltaMove[0] * Math.Cos(config.bias) - deltaMove[1] * Math.Sin(config.bias));
            modulation[1] = config.gain * (deltaMove[0] * Math.Sin(config.bias) + deltaMove[1] * Math.Cos(config.bias));
        }
    }
}

