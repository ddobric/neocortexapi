using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace NeoCortexApi.Classifiers
{
    public class KNeighborsClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>
    {
        private int _nNeighbors;
        private List<Double[]> _model;

        KNeighborsClassifier(int nNeighbors = 5)
        {
            Debug.Assert(nNeighbors % 2 != 0);
            _nNeighbors = nNeighbors;
        }


        double Distance(double[] comparable, double[] dataItem)
        {
            return Math.Sqrt(Math.Pow(comparable[0] - dataItem[1], 2) - Math.Pow(comparable[1] - dataItem[1], 2));
        }

        int CompareTo()

        void GetPredictedInputValue(double[] dataItem)
        {
            foreach (var element in _model)
            {
            }
        }

        void Learn(double[,] x, double[] tags)
        {
            if (x.Length != tags.Length)
                throw new System.Exception("length of x and y are not same");

            for (int i = 0; i < x.Length; i++)
                _model.Add(new Double[] { x[i, 0], x[i, 1], tags[i] });
        }
    }
}