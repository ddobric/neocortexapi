using System;

namespace GridCell.js
{
    public class HeatMap
    {
        public HeatMap()
        {

        }

        public void Generate(String fileName, double[,] data)
        {
            var plt = new ScottPlot.Plot(600, 400);
            plt.AddHeatmap(data, ScottPlot.Drawing.Colormap.Blues);
          

            plt.SaveFig(fileName + ".png");
        }
    }
}

