using gridcells;

var spatial = new SpatialNavigation();
spatial.RandomNavigation(3);
spatial.Plot();


var grid = new Grid();
var simulation = new Simulation(grid, spatial.txx, spatial.tyy);
simulation.run();

Console.WriteLine("End");

