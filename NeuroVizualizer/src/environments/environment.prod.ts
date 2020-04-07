export const environment = {
  production: true,

  URL: "ws://localhost:5555/ws/NeuroVisualizer",

  //change numberOfColours to specify the total numbers of colours for neurons
  //The colour scale lasts from 0 to 1, 0 == blue and 1 == red 
  numberOfColours: 500,

  cellXRatio: 1,
  cellYRatio: 1,
  cellZRatio: 1,
  areaXOffset: 2,
  areaYOffset: 5,
  areaZOffset: 5,


  xRatio: 7,
  yRatio: 1,
  zRatio: 0.5,
  sizeOfNeuron: 15,
  opacityOfNeuron: 1,
  opacityOfSynapse: 1,
  lineWidthOfSynapse: 4,
  cellHeightInMiniColumn: 5,
  miniColumnWidth: 5
};