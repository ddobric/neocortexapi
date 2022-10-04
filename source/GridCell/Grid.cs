using System;
using System.Collections.Generic;
using System.Numerics;
using NeoCortexApi.Entities;
using NeoCortexEntities.NeuroVisualizer;
using NumSharp;

namespace GridCell
{
    public class Grid //IHtmModule<TIN, TOUT>
    {
        // Horizontal size of the grid
        public readonly int mm;

        // Vertical size of the grid
        public readonly int nn;

        private readonly double tao;
        private readonly double ii;
        private readonly double sigma2;
        private readonly double tt;
        private readonly double[] gridGain;

        // Number of grid stacked on top of each other
        public readonly int gridLayers;

        private NDArray gridActivity;

        public NDArray GridActivity
        {
            get { return gridActivity; }
        }
        private Tuple<NDArray, NDArray> distTri;

        public NDArray logGridCells;

        private double activeCellThreshold;

        private int index = 0;

        public Grid(GridConfig config)
        {
            mm = config.mm;
            nn = config.nn;
            tao = config.tao;
            ii = config.ii;
            sigma2 = config.sigma2;
            tt = config.tt;
            gridGain = config.gridGain;
            gridLayers = config.gridLayers;
            activeCellThreshold = config.activeCellThreshold;
            logGridCells = np.ndarray(((config.arenaSize / 2) - 1, mm * nn * gridLayers));

            gridActivity = np.random.uniform(0, 1, (mm, nn, gridLayers));
            distTri = BuildTopology(mm, nn);
        }



        public List<Cell> Compute(Tuple<double, double> deltaMove, bool learn = true)
        {
            for (int jj = 0; jj < gridLayers; jj++)
            {
                var rrr = new Complex(gridGain[jj], 0);
                NDArray matWeights = UpdateWeight(deltaMove, rrr);

                // self.grid_activity[:,:,jj] == In 3d array, get all fro outer 2 array but get only the item in the jj index
                // so, 3d becomes 2d array
                // and ravel flatten 2d to 1d of size (400)
                // this.gridActivity.GetNDArrays
                // var activityVect = np.ravel(this.gridActivity[:,:, jj
                var activityVect = np.ravel(gridActivity[Slice.All, Slice.All, jj]);

                //// update  the activityVect by the matWeights
                activityVect = Bfunc(activityVect, matWeights);

                var activityTemp = activityVect.reshape(mm, nn);
                activityTemp += tao * (activityTemp / np.mean(activityTemp) - activityTemp);


                activityTemp[activityTemp.GetDouble() < 0] = 0;

                gridActivity[Slice.All, Slice.All, jj] = (activityTemp - np.min(activityTemp)) / (np.max(activityTemp) - np.min(activityTemp)) * 30;
            }

            CalculateActiveCells();
            return activeCells;
        }

        public void Reset()
        {
            activeCells.Clear();
            index = 0;
        }

        private List<Cell> activeCells = new();

        public List<Cell> GridCells
        {
            get
            {
                return activeCells;
            }
        }

        /// <summary>
        /// CalculateActiveCells
        /// </summary>
        /// <param name="level"></param>
        private void CalculateActiveCells()
        {
            activeCells.Clear();

            var currentGridState = GridActivity.flatten();
            logGridCells[index] = currentGridState;

            var i = 0;
            var max = (currentGridState.max() * activeCellThreshold).GetDouble();

            //for (int cellNum = 0; cellNum < currentGridState.Shape[0]; cellNum++)
            foreach (double val in currentGridState)
            {
                //var celula = logGridCells[Slice.All, cellNum];
                //var max = (celula.max() * .9).GetDouble();

                //var i = 0;
                //var activeGridCells = new List<Cell>();
                //foreach (double val in celula)
                //{
                //    if (val >= max)
                //    {
                //        //activeGridCells.Add(new Cell(0, i, 0, CellActivity.ActiveCell));
                //        activeCells.Add(new Cell(0, i, 0, CellActivity.ActiveCell));
                //    }
                //    i++;
                //}

               
                if (val >= max)
                {
                    //activeGridCells.Add(new Cell(0, i, 0, CellActivity.ActiveCell));
                    activeCells.Add(new Cell(0, i, 0, CellActivity.ActiveCell));
                }
                i++;
               

                //activeCells.Add(activeGridCells);
            }
            index++;
        }

        private NDArray UpdateWeight(Tuple<double, double> speed, Complex rrr)
        {
            var topologyAbs = distTri.Item1;
            var topologyImg = distTri.Item2;

            NDArray matWeights = np.ndarray(topologyAbs.shape);

            for (int i = 0; i < matWeights.shape[0]; i++)
            {
                for (int j = 0; j < matWeights.shape[1]; j++)
                {
                    var mult = Complex.Multiply(rrr, new Complex(speed.Item1, speed.Item2));
                    var abs = new Complex(Math.Pow(topologyAbs[i, j] - mult.Real, 2), Math.Pow(topologyImg[i, j] - mult.Imaginary, 2)).Magnitude;

                    matWeights[i, j] = ii * np.exp(-abs / sigma2) - tt;
                }
            }
            return matWeights;
        }


        /// Perform matrix multiplaction of the activity with weight
        private NDArray Bfunc(NDArray activity, NDArray matWeights)
        {  //Eq 1
            activity += np.multiply(activity, matWeights)[0];
            return activity;
        }


        private Tuple<NDArray, NDArray> BuildTopology(int mm, int nn)
        {
            var mmm = (np.arange(mm) + (0.5 / mm)) / mm;
            var nnn = ((np.arange(nn) + (0.5 / nn)) / nn) * np.sqrt(3) / 2;

            // The purpose of meshgrid is to create a rectangular grid out of an array of x values and an array of y values.
            // xx && yy is 20x20 array
            var (xx, yy) = np.meshgrid(mmm, nnn);

            Tuple<double, double>[] sdist = {
                Tuple.Create(0.0, 0.0),
                Tuple.Create(-0.5, Math.Sqrt(3) / 2),
                Tuple.Create(-0.5, -Math.Sqrt(3) / 2),
                Tuple.Create(0.5, Math.Sqrt(3) / 2),
                Tuple.Create(0.5, -Math.Sqrt(3) / 2),
                Tuple.Create(-1.0, 0.0),
                Tuple.Create(1.0, 0.0),
            };

            var (xxAbs, yyAbs) = np.meshgrid(np.ravel(xx), np.ravel(xx));
            var (xxImg, yyImg) = np.meshgrid(np.ravel(yy), np.ravel(yy));

            var distMatAbs = xxAbs - yyAbs; // np.ndarray((xx.size, xx.size));
            var distMatImg = xxImg - yyImg; //np.ndarray((yy.size, yy.size));

            //for (int i = 0; i < distMatAbs.size; i++)
            //{
            //    for (int j = 0; distMatAbs.size < nn; j++)
            //    {
            //        distMatAbs[i, j] = xxAbs - yyAbs;
            //        distMatImg[i, j] = xxImg - yyImg;
            //    }
            //}

            for (int i = 0; i < sdist.Length; i++)
            {
                var aaa1 = distMatAbs;

                var rrrAbs = distMatAbs - sdist[i].Item1;
                var rrrImg = distMatImg - sdist[i].Item2;

                //var aaa2Abs = np.ndarray((xx.size, xx.size));
                //var aaa2Img = np.ndarray((yy.size, yy.size));

                //for (int k = 0; k < aaa2Abs.size; k++)
                //{
                //    for (int l = 0; aaa2Abs.size < nn; l++)
                //    {
                //        aaa2Abs[k, l] = distMatAbs[k, l] + sdist[i].Item1;
                //        aaa2Img[k, l] = distMatAbs[k, l] + sdist[i].Item2;
                //    }
                //}

                var aaa2 = rrrAbs;

                for (int k = 0; k < aaa2.size; k++)
                {
                    for (int l = 0; aaa2.size < nn; l++)
                    {
                        if (aaa2[k, l] < aaa1[k, l])
                        {
                            distMatAbs[k, l] = rrrAbs[k, l];
                            distMatImg[k, l] = rrrImg[k, l];
                        }
                    }
                }
            }


            return Tuple.Create(distMatAbs.transpose(), distMatImg.transpose());

        }
    }


}

