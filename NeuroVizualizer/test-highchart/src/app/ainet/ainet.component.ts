import { Component, OnInit, AfterViewInit } from '@angular/core';
//import * as Plotly from 'plotly.js';
import * as Plotlyjs from 'plotly.js/dist/plotly';
import { neoCortexUtils } from '../neocortexutils';
import { color, area } from 'd3';
import { environment as env } from "../environments/environment";
import { NotificationsService } from 'angular2-notifications';
import { NeoCortexModel, Area, Synapse, Minicolumn, Cell, NeocortexSettings, InputModel, CellId, Location } from '../neocortexmodel';


@Component({
  selector: 'app-ainet',
  templateUrl: './ainet.component.html',
  styleUrls: ['./ainet.component.css']
})
export class AinetComponent implements OnInit, AfterViewInit {

  public model: NeoCortexModel;

  xCoordinates: Array<any> = [];
  yCoordinates: Array<any> = [];
  zCoordinates: Array<any> = [];
  overlap: Array<any> = [];
  weightGivenByUser: string;
  error: string;
  xSynapse: any;
  ySynapse: any;
  zSynapse: any;
  heatMap = [];
  overlapValues = [];
  neuralChartLayout: any;
  neuralChartConfig: any;
  numOfAreas: any;
  colours = [];

  constructor(private _service: NotificationsService) {

  }

  setOverlap(model: NeoCortexModel, areaIndx:number, miniColIndx:number){
    
    //this.overlap[]
    // for (var ai = 0; ai < model.areas.length; ai++) {
    //   for (let i = 0; i < model.areas[ai].minicolumns.length; i++) {
    //     this.overlap.push(this.model.areas[ai].minicolumns[i][i].overlap)   ;    

    //   }
    // }
  }

  ngOnInit() {
  }

  ngAfterViewInit() {

    this.fillChart();
    this.generateColoursFromOverlap();
    this.createChart();
  }

  createChart() {
    const neurons = {
      x: this.xCoordinates,
      y: this.yCoordinates,
      z: this.zCoordinates,
      text: this.overlap,
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
         color: this.colours,
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
      x: this.xSynapse,
      y: this.ySynapse,
      z: this.zSynapse,
      text: this.overlap,
      opacity: env.opacityOfSynapse,
      line: {
        width: env.lineWidthOfSynapse,
        color: this.colours,

        //color: '#7CFC00'
        //colorscale: 'Viridis'
      }
    };

    this.neuralChartLayout = {

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
      width: 1500,
      height: 600,
      margin: {
        l: 0,
        r: 0,
        b: 0,
        t: 0,
        pad: 4

      },

      scene: {
        //"auto" | "cube" | "data" | "manual" 
        aspectmode: 'data',
        aspectratio: {
          x: 1,
          y: 1,
          z: 1
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

    this.neuralChartConfig = {
      //displayModeBar: false,
      title: '3DChart',
      displaylogo: false,
      showLink: false,
      // showlegend: false

    };
    //let graphDOM = this.makeChartResponsive();
    let graphDOM = document.getElementById('graph');

    Plotlyjs.newPlot(graphDOM, [neurons, synapses], this.neuralChartLayout, this.neuralChartConfig);
    //Plotlyjs.newPlot(graphDOM, [PointsT, linesT], neuralChartLayout);
    // Plotlyjs.newPlot(graphDOM, [test1, test2]);
    //Plotlyjs.restyle(gd,  update, [0]);
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
    //this.model = neoCortexUtils.createModel([0,0,1], [100, 5], 6); // createModel (numberOfAreas, [xAxis, zAxis], yAxis)
    this.model = neoCortexUtils.createModel([0,0,0,1,1,2], [10, 2], 6); // createModel (numberOfAreas, [xAxis, zAxis], yAxis)
    // this.opacityValues = new Array(areaSection).fill(0.5, 0, 1200).fill(1.8, 1200, 2400);
    //this.colour = new Array(areaSection).fill('#00BFFF', 0, 800).fill('#48afd1', 800, 1600).fill('#236d86', 1600, 2499);

    

    this.numOfAreas = this.model.areas;
    let areaIndx;
    for (areaIndx = 0; areaIndx < this.model.areas.length; areaIndx++) {

      var areaXWidth = env.cellXRatio * this.model.areas[areaIndx].minicolumns[0].length + env.areaXOffset;
      var areaZWidth  = env.cellZRatio * this.model.areas[areaIndx].minicolumns[1].length + env.areaZOffset;
      var areaYWidth = env.cellYRatio * this.model.areas[areaIndx].minicolumns[0][0].cells.length + env.areaYOffset;
      

      for (let i = 0; i < this.model.areas[areaIndx].minicolumns.length; i++) {
        for (let j = 0; j < this.model.areas[areaIndx].minicolumns[i].length; j++) {
          for (let cellIndx = 0; cellIndx < this.model.areas[areaIndx].minicolumns[i][j].cells.length; cellIndx++) {
        
            this.overlap.push(this.model.areas[areaIndx].minicolumns[i][j].overlap);
            this.xCoordinates.push(areaXWidth * areaIndx + i * env.cellXRatio);
            this.zCoordinates.push(areaZWidth * j);
            this.yCoordinates.push(areaYWidth * this.model.areas[areaIndx].level + cellIndx * env.cellYRatio);
          }
        }
      }
    }
    console.log(this.overlap, "overlap Array");

    this.xSynapse = this.xCoordinates.slice(); // creating copy of list
    this.ySynapse = this.yCoordinates.slice(); // creating copy of list
    this.zSynapse = this.zCoordinates.slice(); // creating copy of list
    let randomIndexArray = []; // purely random variables 
    let randomInsertArray = []; // choosed(randomIndexArray) random variables will be inserted after the purely radom indexes 

    let rangeOfXVariables = this.xCoordinates.length;

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
   
    for (let l = 0; l < randomInsertArray.length; l++) {
      // reading specific vector from randomIndexArray at l index
      let xPointAtXi = this.xCoordinates[randomIndexArray[l]];
      let yPointAtXi = this.yCoordinates[randomIndexArray[l]];
      let zPointAtXi = this.zCoordinates[randomIndexArray[l]];
      // inserting specific vector into randomInsertArray at l index
      this.xSynapse.splice(randomInsertArray[l], 0, xPointAtXi); //(index, 0, element)
      this.ySynapse.splice(randomInsertArray[l], 0, yPointAtXi);
      this.zSynapse.splice(randomInsertArray[l], 0, zPointAtXi);


    }
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
    let overLaps = [];
    let overlapValAreas = [];

    for (let overlap = 0; overlap < 1; overlap = overlap + (1 / (env.numberOfColours))) {
      let H = (1.0 - overlap) * 240;
      colourScheme = Array((this.xCoordinates.length / this.numOfAreas) / (env.numberOfColours)).fill("hsl(" + H + ", 100%, 50%)");
      let fixedOL = overlap.toFixed(3);
      overLaps = Array((this.xCoordinates.length / this.numOfAreas) / (env.numberOfColours)).fill(parseFloat(fixedOL));

      for (let hsl = 0; hsl < (colourScheme.length); hsl++) {
        colourCodingArea.push(colourScheme[hsl]);// assigning colour for one area/segment

      }

      for (let i = 0; i < (overLaps.length); i++) {
        overlapValAreas.push(overLaps[i]); // inserting overlap values for one area/segment
      }
    }

    for (let j = 0; j < this.numOfAreas; j++) {
      for (let k = 0; k < colourCodingArea.length; k++) {
        this.heatMap.push(colourCodingArea[k]);
      }

      for (let l = 0; l < this.numOfAreas; l++) {
        for (let m = 0; m < overlapValAreas.length; m++) {
          this.overlapValues.push(overlapValAreas[m]);
        }
      }
    }
  }

  showAllNeurons() {
    this.createChart();

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


  showNeuronsGreaterByWeight() {
    // this function gives the selected neurons by weight 
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
    //  console.log(indexOfNeuron, neuronWeight);

    if (indexOfNeuron == -1) {
      this.error = "Given weight is not present";
      throw this.displayError();
    }

    //selectedWeights = weights.slice(indexOfNeuron);
    sWeights = weights.slice(indexOfNeuron, this.xCoordinates.length);

    for (let j = 0; j < indexOfNeuron; j++) {
      selectedWeights.push("NaN");

    }
    sWeights.forEach(sWeight => {
      selectedWeights.push(sWeight);
    });
    // console.log(selectedWeights, "SW");

    // selectedColours = cellColours.slice(indexOfNeuron);
    sColours = cellColours.slice(indexOfNeuron, this.xCoordinates.length);
    for (let k = 0; k < indexOfNeuron; k++) {
      //selectedColours.push("grey");
      selectedColours.push("hsl(0, 0%, 72%)");
    }
    sColours.forEach(sColour => {
      selectedColours.push(sColour);
    });
    // console.log(selectedColours, "SC");



    const updateNeurons = {
      x: this.xCoordinates,
      y: this.yCoordinates,
      z: this.zCoordinates,
      text: selectedWeights,
      name: 'Neuron',
      mode: 'markers',
      marker: {
        opacity: env.opacityOfNeuron,
        size: env.sizeOfNeuron,
        //color: selectedColours,
        symbol: 'circle',
      },
      type: 'scatter3d',
    };
    const updateSynapses = {
      //the first point in the array will be joined with a line with the next one in the array ans so on...
      type: 'scatter3d',
      mode: 'lines',
      name: 'Synapse',
      x: this.xCoordinates,
      y: this.yCoordinates,
      z: this.zCoordinates,
      text: selectedWeights,
      opacity: env.opacityOfSynapse,
      line: {
        width: env.lineWidthOfSynapse,
        //color: selectedColours,
      }
    };
    let graphDOM = document.getElementById("graph");
    Plotlyjs.newPlot(graphDOM, [updateNeurons, updateSynapses], this.neuralChartLayout, this.neuralChartConfig);

  }

  showNeuronsSmallerByWeight() {
    // this function gives the selected neurons by weight 
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
      indexOfNeuron = i - 1;
    }

    // console.log(indexOfNeuron, neuronWeight);

    if (indexOfNeuron == -1) {
      this.error = "Given weight is not present";
      throw this.displayError();
    }
    sWeights = weights.slice(0, indexOfNeuron + 1);

    sWeights.forEach(sWeight => {
      selectedWeights.push(sWeight);
    });
    for (let j = 0; j < (this.xCoordinates.length - 1 - indexOfNeuron); j++) {
      selectedWeights.push("NaN");

    }
    // console.log(selectedWeights, "SW");

    sColours = cellColours.slice(0, indexOfNeuron + 1);

    sColours.forEach(sColour => {
      selectedColours.push(sColour);
    });
    for (let k = 0; k < (this.xCoordinates.length - 1 - indexOfNeuron); k++) {
      //selectedColours.push("grey");
      selectedColours.push("hsl(0, 0%, 72%)");
    }
    //selectedColours = cellColours.slice(0, indexOfNeuron);
    // console.log(selectedColours, "SC");


    const updateNeurons = {
      x: this.xCoordinates,
      y: this.yCoordinates,
      z: this.zCoordinates,
      text: selectedWeights,
      name: 'Neuron',
      mode: 'markers',
      marker: {
        opacity: env.opacityOfNeuron,
        size: env.sizeOfNeuron,
        //color: selectedColours,
        symbol: 'circle',
      },
      type: 'scatter3d',
    };
    const updateSynapses = {
      //the first point in the array will be joined with a line with the next one in the array ans so on...
      type: 'scatter3d',
      mode: 'lines',
      name: 'Synapse',
      x: this.xCoordinates,
      y: this.yCoordinates,
      z: this.zCoordinates,
      text: selectedWeights,
      opacity: env.opacityOfSynapse,
      line: {
        width: env.lineWidthOfSynapse,
        //color: selectedColours,
      }
    };
    let graphDOM = this.makeChartResponsive();
    window.onresize = function () {
      Plotlyjs.Plots.resize(graphDOM);
    };
    Plotlyjs.newPlot(graphDOM, [updateNeurons, updateSynapses], this.neuralChartLayout, this.neuralChartConfig);
  }

  updateChart() {
    /* this.model.areas[0].minicolumns[0][1].overlap += 0.1;
    this.model.areas[0].minicolumns[0][3].overlap += 0.2; */
    console.log(this.overlap,"before");
    this.model.areas[0].minicolumns[0][1].overlap += 0.1;
    this.model.areas[0].minicolumns[0][3].overlap += 0.2;

    //this.setOverlap(this.model, );
    // model -> overlaps -> this.colours

    this.generateColoursFromOverlap();
    console.log(this.overlap, "after");

    console.log("update")
    const updateNeurons = {
      x: this.xCoordinates,
      y: this.yCoordinates,
      z: this.zCoordinates,
      text: this.overlap,
      name: 'Neuron',
      mode: 'markers',
      marker: {
        opacity: env.opacityOfNeuron,
        size: env.sizeOfNeuron,
        color: this.colours,
        symbol: 'circle',
      },
      type: 'scatter3d',
    };
    const updateSynapses = {
      //the first point in the array will be joined with a line with the next one in the array ans so on...
      type: 'scatter3d',
      mode: 'lines',
      name: 'Synapse',
      x: this.xCoordinates,
      y: this.yCoordinates,
      z: this.zCoordinates,
      text: this.overlap,
      opacity: env.opacityOfSynapse,
      line: {
        width: env.lineWidthOfSynapse,
        color: this.colours,
      }
    };
    let graphDOM = document.getElementById('graph');

    //let graphDOM = this.makeChartResponsive();

    Plotlyjs.newPlot(graphDOM, [updateNeurons, updateSynapses], this.neuralChartLayout, this.neuralChartConfig);
  }

  generateColoursFromOverlap() {

    for (let overlap = 0; overlap < this.overlap.length; overlap++) {
      let H = (1.0 - this.overlap[overlap]) * 240;
      this.colours.push("hsl(" + H + ", 100%, 50%)")
    }

  }

}
