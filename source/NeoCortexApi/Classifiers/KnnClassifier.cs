using System;
using System.Collections.Generic;


namespace NeoCortexApi.Classifiers
{
    public class KNeighborsClassifier<TIN, TOUT>
    {
        private int _nNeighbors;
        private List<Double[]> _model;
        
        
        KNeighborsClassifier(int nNeighbors = 5)
        {
            _nNeighbors = nNeighbors;
        }

        void KReduceFunc(double[,] dist, int start)
        {
        }

        void fit(double[,] x, double[] y)
        {
            if (x.Length != y.Length)
                throw new System.Exception("length of x and y are not same");

            for (int i = 0; i < x.Length; i++)
                _model.Add(new Double[] { x[i, 0], x[i, 1], y[i] });
        }
    }
}
