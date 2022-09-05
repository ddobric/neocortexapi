using GridCell;

var spatial = new SpatialNavigation();
spatial.RandomNavigation(10);
spatial.Plot();


var grid = new Grid();
var simulation = new Simulation(grid, spatial.txx, spatial.tyy);
simulation.run();

var spike = new Spikes(simulation.GridCellsLog);
spike.run(5);

Console.WriteLine("End");

