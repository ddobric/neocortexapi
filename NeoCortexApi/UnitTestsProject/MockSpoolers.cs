using NeoCortexApi;
using NeoCortexApi.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTestsProject
{
    public class SpatialPoolerMock : SpatialPooler
    {
        private int[] m_InhibitColumns;

        private static long serialVersionUID = 1L;

        public SpatialPoolerMock(int[] inhibitColumns)
        {
            m_InhibitColumns = inhibitColumns;
        }
        public override int[] inhibitColumns(Connections c, double[] overlaps)
        {
            return m_InhibitColumns;// new int[] { 0, 1, 2, 3, 4 };
        }
    };


    public class SpatialPoolerMock2 : SpatialPooler
    {
        private static long serialVersionUID = 1L;

        private Action<double> m_CallBack;

        public SpatialPoolerMock2( Action<double> callBack)
        {
            m_CallBack = callBack;
         
        }
        

        public override int[] inhibitColumnsGlobal(Connections c, double[] overlap, double density)
        {
            m_CallBack?.Invoke(density);
            //setGlobalCalled(true);
            //_density = density;
            return new int[] { 1 };
        }
    };

}
