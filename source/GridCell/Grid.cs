using System;
using System.Numerics;
using NumSharp;

namespace gridcells
{
    public class Grid
    {
        public readonly int mm;
        public readonly int nn;
        private readonly double tao;
        private readonly double ii;
        private readonly double sigma;
        private readonly double sigma2;
        private readonly double tt;
        private readonly double[] gridGain;
        public readonly int gridLayers;

        private NDArray gridActivity;

        public NDArray GridActivity
        {
            get { return gridActivity; }
        }
        private Tuple<NDArray, NDArray> distTri;
        private Complex speedVector;


        public Grid()
        {
            mm = 20;
            nn = 20;
            tao = 0.9;
            ii = 0.3;
            sigma = 0.24;
            sigma2 = Math.Pow(sigma, 2);
            tt = 0.05;
            gridGain = new double[] { 0.04, 0.05, 0.06, 0.07, 0.08 };
            gridLayers = gridGain.Length;

            gridActivity = np.random.uniform(0, 1, (mm, nn, gridLayers));

            distTri = buildTopology(mm, nn);
        }

        

        public void update(Complex speedVector) {
            this.speedVector = speedVector;

            for (int jj = 0; jj < gridLayers; jj++)
            {
                var rrr = new Complex(gridGain[jj], 0);
                NDArray matWeights = updateWeight(this.distTri, rrr);

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
        }

        public NDArray updateWeight(Tuple<NDArray, NDArray> topology, Complex rrr) {

            var topologyAbs = topology.Item1;
            var topologyImg = topology.Item2;

            NDArray matWeights = np.ndarray(topologyAbs.shape);

            for (int i = 0; i < matWeights.shape[0]; i++)
            {
                for (int j = 0; j < matWeights.shape[1]; j++)
                {
                    var mult = Complex.Multiply(rrr, this.speedVector);
                    var abs = new Complex (Math.Pow(topologyAbs[i, j] - mult.Real, 2), Math.Pow(topologyImg[i, j] - mult.Imaginary, 2)).Magnitude;

                    matWeights[i, j] = this.ii + np.exp(abs / this.sigma2) - this.tt;
                }
            }
            return matWeights;
        }


        /// Perform matrix multiplaction of the activity with weight
        NDArray Bfunc(NDArray activity, NDArray matWeights)
        {  //Eq 1
            activity += np.multiply(activity, matWeights)[0];
            return activity;
        }


        private Tuple<NDArray, NDArray> buildTopology(int mm, int nn)
        {
            var mmm = (np.arange(mm) + (0.5 / mm)) / mm;
            var nnn = ((np.arange(nn) + (0.5 / nn)) / nn) * np.sqrt(3) / 2;

            // The purpose of meshgrid is to create a rectangular grid out of an array of x values and an array of y values.
            // xx && yy is 20x20 array
            var (xx, yy) = np.meshgrid(mmm, nnn);

            //Complex[] sdist = {
            //    new Complex(0, 0),
            //    new Complex(-0.5, Math.Sqrt(3) / 2),
            //    new Complex(-0.5, -Math.Sqrt(3) / 2),
            //    new Complex(0.5, Math.Sqrt(3) / 2),
            //    new Complex(0.5, -Math.Sqrt(3) / 2),
            //    new Complex(-1, 0),
            //    new Complex(1, 0),
            //};

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

            var distMatAbs = np.ndarray((xx.size, xx.size));
            var distMatImg = np.ndarray((yy.size, yy.size));

            for (int i = 0; i < distMatAbs.size; i++)
            {
                for (int j = 0; distMatAbs.size < nn; j++)
                {
                    distMatAbs[i, j] = xxAbs - yyAbs;
                    distMatImg[i, j] = xxImg - yyImg;
                }
            }

            for (int i = 0; i < sdist.Length; i++)
            {
                var aaa1Abs = distMatAbs;

                var aaa2Abs = np.ndarray((xx.size, xx.size));
                var aaa2Img = np.ndarray((yy.size, yy.size));

                for (int k = 0; k < aaa2Abs.size; k++)
                {
                    for (int l = 0; aaa2Abs.size < nn; l++)
                    {
                        aaa2Abs[k, l] = distMatAbs[k, l] + sdist[i].Item1;
                        aaa2Img[k, l] = distMatAbs[k, l] + sdist[i].Item2;
                    }
                }


                for (int k = 0; k < aaa2Abs.size; k++)
                {
                    for (int l = 0; aaa2Abs.size < nn; l++)
                    {
                        if (aaa2Abs[k, l] < aaa1Abs[k, l]) {
                            distMatAbs[k, l] = aaa2Abs[k, l];
                            distMatImg[k, l] = aaa2Img[k, l];
                        }
                    }
                }
            }


            return Tuple.Create(distMatAbs.transpose(), distMatImg.transpose());

        }
    }


}

