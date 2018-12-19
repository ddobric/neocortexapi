import { Component, OnInit, AfterViewInit } from '@angular/core';
//import * as Plotly from 'plotly.js';
import * as Plotlyjs from 'plotly.js/dist/plotly';
import { neoCortexUtils } from '../neocortexutils';
import { color } from 'd3';
import { environment as env } from "../environments/environment";
import { NotificationsService } from 'angular2-notifications';
import { NeoCortexModel, Area, Synapse, Minicolumn, Cell, NeocortexSettings, InputModel, CellId, Location } from '../neocortexmodel';


@Component({
  selector: 'app-ainet',
  templateUrl: './ainet.component.html',
  styleUrls: ['./ainet.component.css']
})
export class AinetComponent implements OnInit, AfterViewInit {

  weightGivenByUser: string;
  error: string;

  constructor(private _service: NotificationsService) {

  }
  /*  types = ['alert', 'error', 'info', 'warn', 'success'];
   animationTypes = ['fromRight', 'fromLeft', 'scale', 'rotate']; */

  ngOnInit() {
  }
  ngAfterViewInit() {

    this.createChart();
  }

  showAllNeurons() {

  }

  displayError() {
    this.options;
    this._service.error(
      "Error",
      this.error,
      {
        timeOut: 3000,
        showProgressBar: true,
        pauseOnHover: false,
        clickToClose: true,
        maxLength: 30
      }
    )
  }

  public options = {
    position: ["top", "right"],
    timeOut: 3000,
  };

  updateChartTest1() {
   //let updateModel : NeoCortexModel = new NeoCortexModel(null)
  }


  createChart() {
    let data = this.fillChart();
    let xCoordinates = data[0];
    let yCoordinates = data[1];
    let zCoordinates = data[2];
    let xSynap = data[4];
    let ySynap = data[5];
    let zSynap = data[6];

    console.log(xCoordinates, "X");
    console.log(yCoordinates, "Y");
    console.log(zCoordinates, "Z");

    //let colourArray = this.getHeatColor();
    //let cellColours = colourArray[0];
    let getHeatMap = this.generateHeatMap()
    let assignColours = getHeatMap[0];
    //let weights = colourArray[1];

    let overlap = data[7];
    const neurons = {
      x: xCoordinates,
      y: yCoordinates,
      z: zCoordinates,
      text: overlap,
      name: 'Neuron',
      mode: 'markers',

      //connectgaps: true,
      /*  visible: true,
       legendgroup: true, */
      /* line: {
        width: 4,
        colorscale: 'Viridis',
        color: '#7CFC00'
      }, */
      marker: {
        opacity: env.opacityOfNeuron,
        size: env.sizeOfNeuron,
        // color: '#00BFFF',
        color: assignColours,
        symbol: 'circle',
        line: {
          //color: '#7B68EE',
          // width:10
        },

      },
      type: 'scatter3d',
      //scene: "scene1",

    };
    const synapses = {
      //the first point in the array will be joined with a line with the next one in the array ans so on...
      type: 'scatter3d',
      mode: 'lines',
      name: 'Synapse',
      x: xSynap,
      y: ySynap,
      z: zSynap,
      text: overlap,
      opacity: env.opacityOfSynapse,
      line: {
        width: env.lineWidthOfSynapse,
        color: assignColours,

        //color: '#7CFC00'
        //colorscale: 'Viridis'
      }
    };

    const neuralChartLayout = {

      //showlegend: false, Thgis option is to show the name of legend/DataSeries 
      /*    scene: {
           aspectmode: "manual",
           aspectratio: {
             x: env.xRatio, y: env.yRatio, z: env.zRatio,
           }
         }, */

      legend: {
        x: 0.5,
        y: 1
      },
      //width: 1800,
      // height: 800,
      margin: {
        l: 0,
        r: 0,
        b: 0,
        t: 0,
        pad: 4

      },

      scene: {
        //"auto" | "cube" | "data" | "manual" 
        aspectmode: 'manual',
        aspectratio: {
             x: 7,
             y: 1,
             z: 0.5
         }, 
        camera: {
          center: {
            x: 0,
            y: 0,
            z: 0
          },
          eye: {
            x: 1.25,
            y: 1.25,
            z: 1.25
          },
          up: {
            x: 0,
            y: 0,
            z: 1

          }
        },
        /*     xaxis: {
                type: 'linear',
                zeroline: false
            },
            yaxis: {
                type: 'linear',
                zeroline: false
            },
            zaxis: {
                type: 'linear',
                zeroline: false
            }*/
      },
    };

    const neuralChartConfig = {
      //displayModeBar: false,
      title: '3DChart',
      displaylogo: false,
      showLink: false,
      // showlegend: false

    };
    /*    const update = {
         opacity: 0.8,
         marker:{
           color: 'red',
           size: 25
         },
         x: [[41.5]],
         y: [[0.5]],
         z: [[3.5]]
       }; */

    const PointsT = {
      x: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2],
      y: [0, 1, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2],
      z: [0, 0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2],
      name: 'PointsT',
      mode: 'markers',
      marker: {
        opacity: env.opacityOfNeuron,
        size: env.sizeOfNeuron,
        color: "yellow",
        symbol: 'circle',
      },
      type: 'scatter3d',
    };
    const linesT = {
      x: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2],
      y: [0, 1, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2],
      z: [0, 0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2],

      //x: [0, 0, null, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2],
      //y: [0, 1, null, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2],
      //z: [0, 0, null, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2],
      name: 'linesT',
      mode: 'lines',
      marker: {
        color: "red",
        width: 10
      },
      type: 'scatter3d',
    };


    let graphDOM = this.makeChartResponsive();


    Plotlyjs.newPlot(graphDOM, [neurons, synapses], neuralChartLayout, neuralChartConfig);
    //Plotlyjs.newPlot(graphDOM, [PointsT, linesT], neuralChartLayout);
    // Plotlyjs.newPlot(graphDOM, [test1, test2]);
    //Plotlyjs.restyle(gd,  update, [0]);


    this.showAllNeurons = function () {
      Plotlyjs.newPlot(graphDOM, [neurons, synapses], neuralChartLayout, neuralChartConfig);
    }

    // this function gives the selected neurons by weight 
    this.showNeuronsSmallerByWeight = function () {
      let selectedWeights = [];
      let selectedColours = [];
      let sWeights = [];
      let sColours = [];


      let neuronWeight = parseFloat(this.weightGivenByUser);


      if (neuronWeight > 1) {
        this.error = "Weight could not be greater than 1";
        throw this.displayError();
      }

      let heatColourArray = this.generateHeatMap();
      let data = this.fillChart();
      let weights = data[7];
      let cellColours = heatColourArray[0];
      let indexOfNeuron = weights.indexOf(neuronWeight);

      // This segment is to handle the case if we have n same numbers of Elements in our list then slice will just pick the very first
      // To avoid it we count the occrunce of that element   
      if (indexOfNeuron == 0) {
        let i = 0;
        for (i; i < weights.length; i++) {
          if (weights[i] > 0) {
            break;
          }

        }
        indexOfNeuron = i-1;
      }

      console.log(indexOfNeuron, neuronWeight);

      if (indexOfNeuron == -1) {
        this.error = "Given weight is not present";
        throw this.displayError();
      }

      /*   filteredXCoordinates = xCoordinates.slice(0, indexOfNeuron);
        filteredYCoordinates = yCoordinates.slice(0, indexOfNeuron);
        filteredZCoordinates = zCoordinates.slice(0, indexOfNeuron); */
      //selectedWeights = weights.slice(0, indexOfNeuron);
      sWeights = weights.slice(0, indexOfNeuron + 1);

      sWeights.forEach(sWeight => {
        selectedWeights.push(sWeight);
      });
      for (let j = 0; j < (xCoordinates.length - 1 - indexOfNeuron); j++) {
        selectedWeights.push("NaN");

      }
      console.log(selectedWeights, "SW");

      sColours = cellColours.slice(0, indexOfNeuron + 1);

      sColours.forEach(sColour => {
        selectedColours.push(sColour);
      });
      for (let k = 0; k < (xCoordinates.length - 1 - indexOfNeuron); k++) {
        //selectedColours.push("grey");
        selectedColours.push("hsl(0, 0%, 72%)");
      }
      //selectedColours = cellColours.slice(0, indexOfNeuron);
      console.log(selectedColours, "SC");


      const updateNeurons = {
        x: xCoordinates,
        y: yCoordinates,
        z: zCoordinates,
        text: selectedWeights,
        name: 'Neuron',
        mode: 'markers',
        marker: {
          opacity: env.opacityOfNeuron,
          size: env.sizeOfNeuron,
          color: selectedColours,
          symbol: 'circle',
        },
        type: 'scatter3d',
      };
      const updateSynapses = {
        //the first point in the array will be joined with a line with the next one in the array ans so on...
        type: 'scatter3d',
        mode: 'lines',
        name: 'Synapse',
        x: xCoordinates,
        y: yCoordinates,
        z: zCoordinates,
        text: selectedWeights,
        opacity: env.opacityOfSynapse,
        line: {
          width: env.lineWidthOfSynapse,
          color: selectedColours,
        }
      };
      Plotlyjs.newPlot(graphDOM, [updateNeurons, updateSynapses], neuralChartLayout, neuralChartConfig);
    }

    // this function gives the selected neurons by weight 
    this.showNeuronsGreaterByWeight = function () {
      let selectedWeights = [];
      let selectedColours = [];
      let sWeights = [];
      let sColours = [];


      let neuronWeight = parseFloat(this.weightGivenByUser);

      if (neuronWeight > 1) {
        this.error = "Weight could not be greater than 1";
        throw this.displayError();
      }

      //let heatColourArray = this.getHeatColor();
      let heatColourArray = this.generateHeatMap();
      let data = this.fillChart();
      //let weights = heatColourArray[1];
      let weights = data[7];
      let cellColours = heatColourArray[0];
      let indexOfNeuron = weights.indexOf(neuronWeight);
      console.log(indexOfNeuron, neuronWeight);

      if (indexOfNeuron == -1) {
        this.error = "Given weight is not present";
        throw this.displayError();
      }

      /*    selectedXCoordinates = xCoordinates.slice(indexOfNeuron);
         selectedYCoordinates = yCoordinates.slice(indexOfNeuron);
         selectedZCoordinates = zCoordinates.slice(indexOfNeuron); */

      //selectedWeights = weights.slice(indexOfNeuron);
      sWeights = weights.slice(indexOfNeuron, xCoordinates.length);

      for (let j = 0; j < indexOfNeuron; j++) {
        selectedWeights.push("NaN");

      }
      sWeights.forEach(sWeight => {
        selectedWeights.push(sWeight);
      });
      console.log(selectedWeights, "SW");

      // selectedColours = cellColours.slice(indexOfNeuron);
      sColours = cellColours.slice(indexOfNeuron, xCoordinates.length);
      for (let k = 0; k < indexOfNeuron; k++) {
        //selectedColours.push("grey");
        selectedColours.push("hsl(0, 0%, 72%)");
      }
      sColours.forEach(sColour => {
        selectedColours.push(sColour);
      });
      console.log(selectedColours, "SC");



      const updateNeurons = {
        x: xCoordinates,
        y: yCoordinates,
        z: zCoordinates,
        text: selectedWeights,
        name: 'Neuron',
        mode: 'markers',
        marker: {
          opacity: env.opacityOfNeuron,
          size: env.sizeOfNeuron,
          color: selectedColours,
          symbol: 'circle',
        },
        type: 'scatter3d',
      };
      const updateSynapses = {
        //the first point in the array will be joined with a line with the next one in the array ans so on...
        type: 'scatter3d',
        mode: 'lines',
        name: 'Synapse',
        x: xCoordinates,
        y: yCoordinates,
        z: zCoordinates,
        text: selectedWeights,
        opacity: env.opacityOfSynapse,
        line: {
          width: env.lineWidthOfSynapse,
          color: selectedColours,
        }
      };
      Plotlyjs.newPlot(graphDOM, [updateNeurons, updateSynapses], neuralChartLayout, neuralChartConfig);

    }

    this.updateChartTest1 = function(){
     let data =  this.getHeatMap();
     let heatMap = data[0]; 
     let overlapVal = data[1]; 
      const updateNeurons = {
        x: xCoordinates,
        y: yCoordinates,
        z: zCoordinates,
       text: overlapVal,
        name: 'Neuron',
        mode: 'markers',
        marker: {
          opacity: env.opacityOfNeuron,
          size: env.sizeOfNeuron,
          color: heatMap,
          symbol: 'circle',
        },
        type: 'scatter3d',
      };
      const updateSynapses = {
        //the first point in the array will be joined with a line with the next one in the array ans so on...
        type: 'scatter3d',
        mode: 'lines',
        name: 'Synapse',
        x: xCoordinates,
        y: yCoordinates,
        z: zCoordinates,
        text: overlapVal,
        opacity: env.opacityOfSynapse,
        line: {
          width: env.lineWidthOfSynapse,
          color: heatMap,
        }
      };
      Plotlyjs.newPlot(graphDOM, [updateNeurons, updateSynapses], neuralChartLayout, neuralChartConfig);

    }


    window.onresize = function () {
      Plotlyjs.Plots.resize(graphDOM);
    };
  }
  makeChartResponsive() {
    let d3 = Plotlyjs.d3;
    let WIDTH_IN_PERCENT_OF_PARENT = 90;
    let HEIGHT_IN_PERCENT_OF_PARENT = 90;
    let gd3 = d3.select('body').append('div').style({
      width: WIDTH_IN_PERCENT_OF_PARENT + '%',
      'margin-left': (100 - WIDTH_IN_PERCENT_OF_PARENT) / 2 + '%',

      height: HEIGHT_IN_PERCENT_OF_PARENT + 'vh',
      'margin-top': (100 - HEIGHT_IN_PERCENT_OF_PARENT) / 2 + 'vh'
    });
    let graphDOM = gd3.node();
    return graphDOM;

  }
  fillChart() {
    let model = neoCortexUtils.createModel(3, [100, 5], 6); // createModel (numberOfAreas, [xAxis, zAxis], yAxis)
    // this.opacityValues = new Array(areaSection).fill(0.5, 0, 1200).fill(1.8, 1200, 2400);
    //this.colour = new Array(areaSection).fill('#00BFFF', 0, 800).fill('#48afd1', 800, 1600).fill('#236d86', 1600, 2499);
    let xCoord: Array<any> = [];
    let yCoord: Array<any> = [];
    let zCoord: Array<any> = [];
    let overlap: Array<any> = [];

    let numOfAreas = model.areas;
    let ai;
    for (ai = 0; ai < model.areas.length; ai++) {
      for (let i = 0; i < model.areas[ai].minicolumns.length; i++) {
        overlap = model.areas[ai].overlap;
        xCoord.push(model.areas[ai].minicolumns[i][i].posX);
        yCoord.push(model.areas[ai].minicolumns[i][i].posY);
        zCoord.push(model.areas[ai].minicolumns[i][i].posZ);

      }
    }
    console.log(overlap, "overlap Array");
    //Choose uniformly n Random indexes in the range of the x, y, or z Array. (0 -> length.XCoord)
    //Pick data from that indexes and add the data(randomly) into the copy of w x, x, z arrays
    //Assign that arrays to synapses 


    let xSynapse = xCoord.slice(); // creating copy of list
    let ySynapse = yCoord.slice(); // creating copy of list
    let zSynapse = zCoord.slice(); // creating copy of list
    let randomIndexArray = []; // purely random variables 
    let randomInsertArray = []; // choosed(randomIndexArray) random variables will be inserted after the purely radom indexes 

    let rangeOfXVariables = xCoord.length;

    let totalRandomIndexs = 3;
    for (let j = 0; j < totalRandomIndexs; j++) {
      let randomIndex = Math.floor(Math.random() * Math.floor(rangeOfXVariables));
      randomIndexArray.push(randomIndex);  // generating/filling purely random variables 
    }

    let totalRandomInsertIndex = 3;
    for (let k = 0; k < totalRandomInsertIndex; k++) {
      let randomInsertIndex = Math.floor(Math.random() * Math.floor(rangeOfXVariables));
      randomInsertArray.push(randomInsertIndex);// generating/filling purely random indexes 
    }
    /* 
        for (let l = 0; l < randomIndexArray.length; l++) {
          let xPointAtXi = xCoord[randomIndexArray[l]];
          let yPointAtXi = yCoord[randomIndexArray[l]];
          let zPointAtXi = zCoord[randomIndexArray[l]];
    
          for (let m = 0; m < randomInsertArray.length; m++) {
    
            xSynapse.splice(randomInsertArray[m], 0, xPointAtXi); //(index, 0, element)
            ySynapse.splice(randomInsertArray[m], 0, yPointAtXi);
            zSynapse.splice(randomInsertArray[m], 0, zPointAtXi);
          }
    
        } */
    for (let l = 0; l < randomInsertArray.length; l++) {
      // reading specific vector from randomIndexArray at l index
      let xPointAtXi = xCoord[randomIndexArray[l]];
      let yPointAtXi = yCoord[randomIndexArray[l]];
      let zPointAtXi = zCoord[randomIndexArray[l]];
      // inserting specific vector into randomInsertArray at l index
      xSynapse.splice(randomInsertArray[l], 0, xPointAtXi); //(index, 0, element)
      ySynapse.splice(randomInsertArray[l], 0, yPointAtXi);
      zSynapse.splice(randomInsertArray[l], 0, zPointAtXi);


    }
    console.log(randomIndexArray, "rein Zufällig X");
    console.log(randomInsertArray, 'Zufällig einfügen');
    console.log(xSynapse, "xSynapse");
    console.log(ySynapse, "ySynapse");
    console.log(zSynapse, "zSynapse");





    return [xCoord, yCoord, zCoord, numOfAreas, xSynapse, ySynapse, zSynapse, overlap];

  }
  generateHeatMap() {
    let getData = this.fillChart();
    let overlapValues = getData[7];
    let totalAreas = getData[3];

    let heatMap: Array<any> = [];
    let colourCoding: Array<any> = [];

    for (let oV = 0; oV < overlapValues.length; oV++) {
      let H = (1.0 - overlapValues[oV]) * 240;
      colourCoding.push("hsl(" + H + ", 100%, 50%)");
    }
    console.log(colourCoding, "colorCoding");

    for (let i = 0; i < totalAreas.length; i++) {
      for (let j = 0; j < colourCoding.length; j++) {
        heatMap.push(colourCoding[j]);
      }
    }
    console.log(heatMap, "heat map for all areas");
    return [heatMap];
  }

  getHeatMap() {

    let colourScheme = [];
    let colourCodingArea = [];
    let heatMap = [];
    let overLaps = [];
    let overlapValAreas = [];
    let overlapValues = [];

    let data = this.fillChart();
    let xCoordLength = data[0].length;
    let totalAreas = data[3].length;
// implement it again 
    
    for (let overlap = 0; overlap < 1; overlap = overlap + (1 / (env.numberOfColours))) {
      let H = (1.0 - overlap) * 240;
      colourScheme = Array((xCoordLength / totalAreas) / (env.numberOfColours)).fill("hsl(" + H + ", 100%, 50%)");
      let fixedOL = overlap.toFixed(3);
      overLaps = Array((xCoordLength / totalAreas) / (env.numberOfColours)).fill(parseFloat(fixedOL));

      for (let hsl = 0; hsl < (colourScheme.length); hsl++) {
        colourCodingArea.push(colourScheme[hsl]);// assigning colour for one area/segment

      }

      for (let i = 0; i < (overLaps.length); i++) {
        overlapValAreas.push(overLaps[i]); // inserting overlap values for one area/segment
      }
    }

    for (let j = 0; j < totalAreas; j++) {
      for (let k = 0; k < colourCodingArea.length; k++) {
        heatMap.push(colourCodingArea[k]);
      }

      for (let l = 0; l < totalAreas; l++) {
        for (let m = 0; m < overlapValAreas.length; m++) {
          overlapValues.push(overlapValAreas[m]);
        }
      }
    } 

    return [heatMap, overlapValues];
  }

  showNeuronsGreaterByWeight() {
  }

  showNeuronsSmallerByWeight() {

  }

}
