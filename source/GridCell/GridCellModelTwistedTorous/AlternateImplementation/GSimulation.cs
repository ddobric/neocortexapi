using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoCortexApi;

namespace GridCell.js
{
    public class GSimulation
    {
        private List<double> moveXX;
        private List<double> moveYY;
        private GCell grid;

        public GSimulation(GCell grid, List<double> moveXX, List<double> moveYY)
        {
            this.moveXX = moveXX;
            this.moveYY = moveYY;
            this.grid = grid;
        }

        public GSimulation(GCell grid)
        {
            moveXX = new();
            moveYY = new();
            this.grid = grid;
        }

        public void RunContinuous()
        {
            var agent = new RealTimeNavigation(grid.config.arenaSize);

            HeatMap hm = new HeatMap();

            var index = 0;
            while(true)
            {
                agent.Move();
                var deltaMove = agent.speedvect; 

                var currentActiveCells = grid.Compute(deltaMove);

                //var indexes = currentActiveCells.Select(cell => cell.Index).ToArray();


                //Console.WriteLine(StringifySdr(grid.networkActivity));

                

                if (index == 10000)
                {
                    agent.MoveTo(25, 25);
                    grid.Compute(agent.speedvect).Select(cell => cell.Index).ToArray();

                    Console.WriteLine(StringifySdr(grid.networkActivity));

                    hm.Generate("heatmap_" + index, grid.GetHeatMapValues());

                    var j = 0;
                    while (j < 100)
                    {
                        agent.Move();
                        grid.Compute(agent.speedvect);
                        j++;
                    }


                    //agent.MoveTo(40, 40);
                    //grid.Compute(agent.speedvect).Select(cell => cell.Index).ToArray();

                    //Console.WriteLine(StringifySdr(grid.networkActivity));

                    //hm.Generate("heatmap_diff" + index + 1, grid.GetHeatMapValues());


                    //agent.MoveTo(25, 25);
                    //grid.Compute(agent.speedvect).Select(cell => cell.Index).ToArray();

                    //Console.WriteLine(StringifySdr(grid.networkActivity));

                    //hm.Generate("heatmap_" + index + 1, grid.GetHeatMapValues());


                    //agent.MoveTo(2, 5);
                    //grid.Compute(agent.speedvect).Select(cell => cell.Index).ToArray();

                    //Console.WriteLine(StringifySdr(grid.networkActivity));

                    //hm.Generate("heatmap_diff" + index + 2, grid.GetHeatMapValues());


                    //agent.MoveTo(25, 25);
                    //grid.Compute(agent.speedvect).Select(cell => cell.Index).ToArray();

                    //Console.WriteLine(StringifySdr(grid.networkActivity));

                    //hm.Generate("heatmap_" + index + 2, grid.GetHeatMapValues());



                    //agent.MoveTo(0, 0);
                    //grid.Compute(agent.speedvect).Select(cell => cell.Index).ToArray();

                    //Console.WriteLine(StringifySdr(grid.networkActivity));

                    //hm.Generate("heatmap_diff" + index + 3, grid.GetHeatMapValues());


                    agent.MoveTo(25, 25);
                    grid.Compute(agent.speedvect).Select(cell => cell.Index).ToArray();

                    Console.WriteLine(StringifySdr(grid.networkActivity));

                    hm.Generate("heatmap_" + index + 3, grid.GetHeatMapValues());


                    break;
                }
                index++;
            }
        }

        public void Run()
        {
            for (int i = 1; i < moveXX.Count; i++)
            {
                var deltaMove = new double[]
                {
                    (moveXX[i] - moveXX[i - 1]), (moveYY[i] - moveYY[i - 1])
                };

                var currentActiveCells = grid.Compute(deltaMove);

                var indexes = currentActiveCells.Select(cell => cell.Index).ToArray();

                Console.WriteLine(Helpers.StringifyVector(indexes));

                //Console.WriteLine(StringifySdr(grid.networkActivity));
                //Console.WriteLine("-----------------------------------");
            }
        }

        private void moveTo(double[] deltaMove)
        {
            var currentActiveCells = grid.Compute(deltaMove);

            var indexes = currentActiveCells.Select(cell => cell.Index).ToArray();

            Console.WriteLine(Helpers.StringifyVector(indexes));
        }

        public static string StringifySdr(List<double[]> sdrs)
        {
            var outputs = new StringBuilder();
            
            foreach (var sdr in sdrs)
            {
                for (int i = 0; i < sdr.Length; i++)
                {
                    outputs.Append(sdr[i]);
                    outputs.Append(", ");
                }
                outputs.AppendLine("");
            }

            return outputs.ToString();
        }
    }
}

