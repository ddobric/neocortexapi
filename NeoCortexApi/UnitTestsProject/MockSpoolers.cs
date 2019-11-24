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

        private Func<double, int[]> m_CallBackGlobal;
        private Func<double, int[]> m_CallBackLocal;

        public SpatialPoolerMock2( Func<double, int[]> callBackGlobal, Func<double, int[]> callBackLocal)
        {
            m_CallBackGlobal = callBackGlobal;
            m_CallBackLocal = callBackLocal;
        }
        

        public override int[] inhibitColumnsGlobal(Connections c, double[] overlap, double density)
        {
            m_CallBackGlobal?.Invoke(density);
            //setGlobalCalled(true);
            //_density = density;
            return new int[] { 1 };
        }

        public override int[] InhibitColumnsLocal(Connections c, double[] overlap, double density)
        {
            m_CallBackLocal?.Invoke(density);
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

        public override double GetAvgSpanOfConnectedSynapses(Connections c, int columnIndex)
        {
            return this.m_avgConnectedSpanForColumnND;
        }

        public override double calcAvgColumnsPerInput(Connections c)
        {
            return this.m_avgColumnsPerInput;
        }
    };


    public class SpatialPoolerMock4 : SpatialPooler
    {
        private static long serialVersionUID = 1L;


        public SpatialPoolerMock4()
        {
           

        }

        public override void RaisePermanenceToThreshold(HtmConfig htmConfig, double[] perm, int[] maskPotential)
        {
            //Mock out
        }
    };
}
