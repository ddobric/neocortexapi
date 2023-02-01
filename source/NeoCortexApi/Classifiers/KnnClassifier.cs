using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace NeoCortexApi.Classifiers
{
    public class KNeighborsClassifier<TIN, TOUT> : IClassifier<TIN, TOUT>
    {
        private int _nNeighbors;
        private Double[][] _model;

        KNeighborsClassifier(int nNeighbors = 5)
        {
            Debug.Assert(nNeighbors % 2 != 0);
            _nNeighbors = nNeighbors;
        }


        double Distance(double[] comparable, double[] dataItem)
        {
            double acc = 0;

            for (int i = 0; i < comparable.Length; i++)
                acc += Math.Pow(comparable[i] - dataItem[i], 2);

            return Math.Sqrt(acc);
        }

        void GetPredictedInputValue(double[] dataItem)
        {
            foreach (var element in _model)
            {
            }
        }

        void Learn(object x, double[] tags)
        {
            var X = x as Double[][];
            Debug.Assert(X != null);
            int dataStructureLength = X[0].Length;
            
            if (X.Length != tags.Length)
                throw new System.Exception("length of x and y are not same");

            List<Double> coordinates = new List<double>();
            for (int i = 0; i < X.Length; i++)
            {
                for (int j = 0; j < dataStructureLength; j++)
                    coordinates.Add(X[i][j]);
                
                _model[i] = new Double[] { coordinates[0], tags[i] };
            }
        }
    }
}