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



    public class SpatialPoolerMock3 : SpatialPooler
    {
        private static long serialVersionUID = 1L;

        private double m_avgConnectedSpanForColumnND;

        private double m_avgColumnsPerInput;

        public SpatialPoolerMock3(double avgConnectedSpanForColumnND, double avgColumnsPerInput)
        {
            this.m_avgConnectedSpanForColumnND = avgConnectedSpanForColumnND;
            this.m_avgColumnsPerInput = avgColumnsPerInput;

        }

        public override double avgConnectedSpanForColumnND(Connections c, int columnIndex)
        {
            return 3;
        }

        public override double avgColumnsPerInput(Connections c)
        {
            return 4;
        }
    };


    public class SpatialPoolerMock4 : SpatialPooler
    {
        private static long serialVersionUID = 1L;


        public SpatialPoolerMock4()
        {
           

        }

        public override void raisePermanenceToThreshold(Connections c, double[] perm, int[] maskPotential)
        {
            //Mock out
        }
    };
}
