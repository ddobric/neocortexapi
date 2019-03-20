import { Component, OnInit, AfterViewInit } from '@angular/core';
//import * as Plotly from 'plotly.js';
import * as Plotlyjs from 'plotly.js/dist/plotly';
import { neoCortexUtils } from '../neocortexutils';
import { environment as env } from "../environments/environment";
import { NotificationsService } from 'angular2-notifications';
import { NeoCortexModel, Area, Synapse, Minicolumn, Cell, NeocortexSettings, InputModel, CellId } from '../neocortexmodel';


@Component({
  selector: 'app-ainet',
  templateUrl: './ainet.component.html',
  styleUrls: ['./ainet.component.css']
})
export class AinetComponent implements OnInit, AfterViewInit {

  public model: NeoCortexModel;
  xNeurons: Array<number> = [];
  yNeurons: Array<number> = [];
  zNeurons: Array<number> = [];
  xSynapse: Array<number> = [];
  ySynapse: Array<number> = [];
  zSynapse: Array<number> = [];
  overlap: Array<number> = [];
  permanence: Array<number> = [];
  neuronsColours: Array<string> = [];
  synapseColours: Array<string> = [];

  xCoordinatesAllAreas: Array<any> = [];
  yCoordinatesAllAreas: Array<any> = [];
  zCoordinatesAllAreas: Array<any> = [];
  xCoordinatesForOneArea = [];
  yCoordinatesForOneArea = [];
  zCoordinatesForOneArea = [];

  weightGivenByUser: string;
  error: string;

  areaID: any = 0;
  miniColumnXDimension: any = 0;
  miniColumnZDimension: any = 0;
  newOverlapValue: any = 0;
  mapData: any;
  overlapIntervalStart: number = null;
  overlapIntervalEnd: number = null;
  permanenceIntervalStart: number = null;
  permanenceIntervalEnd: number = null;

  xNeuron: number = null;
  layerNeuron: number = null;
  zNeuron: number = null;
  cellAreaIndex: number = null;

  xInputModel = [];
  zInputModel = [];
  yInputModel = [];
  inputModelOverlap = [];




  constructor(private _service: NotificationsService) {

  }

  ngOnInit() {
  }

  ngAfterViewInit() {
    //this.model = neoCortexUtils.createModel([0, 0, 0, 0, 1, 1, 1, 2, 2, 3], [10, 1], 6);
    this.model = neoCortexUtils.createModel([0, 0, 0, 1, 2, 1], [10, 1], 6); // createModel (numberOfAreas, [xAxis, zAxis], yAxis)
    this.fillChart(this.model);
    // this.model = neoCortexUtils.createModel([0, 0, 0, 0, 1, 1, 1, 2, 2, 3], [10, 1], 6);
    this.generateColoursFromOverlap();
    this.generateColoursFromPermanences();
    this.plotChart();


  }

  /**
   * createChart just plot the chart by using plotly library 
   */
  plotChart() {
    const neurons = {
      x: this.xNeurons,
      y: this.yNeurons,
      z: this.zNeurons,
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
        color: this.neuronsColours,
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
      text: this.permanence,
      opacity: env.opacityOfSynapse,
      line: {
        width: env.lineWidthOfSynapse,
        color: this.synapseColours,
        //color: '#7CFC00'
        //colorscale: 'Viridis'
      }
    };
    const inputModel = {
      x: this.xInputModel,
      y: this.yInputModel,
      z: this.zInputModel,
      text: this.inputModelOverlap,
      name: 'InputModel',
      mode: 'markers',
      marker: {
        opacity: env.opacityOfNeuron,
        size: env.sizeOfNeuron,
         color: this.inputModelOverlap,
        symbol: 'circle',
        
      },
      type: 'scatter3d',
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
      /*  width: 1500,
       height: 500, */
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
            x: 2,
            y: 2,
            z: 0.1
            /*  x:2.5, y:0.1, z:0.1 */
          },
          up: {
            x: 0,
            y: 0,
            z: 1
          }
        },
      },
    };

    const neuralChartConfig = {
      //displayModeBar: false,
      title: '3DChart',
      displaylogo: false,
      showLink: false,
      responsive: true
      // showlegend: false
    };

    // to make the chart responsive 
    let d3 = Plotlyjs.d3;
    let WIDTH_IN_PERCENT_OF_PARENT = 80;
    let HEIGHT_IN_PERCENT_OF_PARENT = 80;
    let gd3 = d3.select('body').append('div').style({
      width: WIDTH_IN_PERCENT_OF_PARENT + '%',
      'margin-left': (100 - WIDTH_IN_PERCENT_OF_PARENT) / 2 + '%',
      height: HEIGHT_IN_PERCENT_OF_PARENT + 'vh',
      'margin-top': (100 - HEIGHT_IN_PERCENT_OF_PARENT) / 2 + 'vh'
    });
    let graphDiv = gd3.node();

    //let graphDOM = document.getElementById('graph');
    Plotlyjs.react(graphDiv, [neurons, synapses, inputModel], neuralChartLayout, neuralChartConfig);
    //Plotlyjs.newPlot(graphDOM, [PointsT, linesT], neuralChartLayout);

    window.onresize = function () {
      Plotlyjs.Plots.resize(graphDiv);
    };
  }


  /**
   * Fillchart method gets the data from the model, that will be later used to plot the chart by createchart method
   * @param model 
   */
  fillChart(model: NeoCortexModel) {
    let areaIndx: any;
    let lastLevel = 0;
    let levelCnt = 0;
    let xOffset = 0;
    let x: number;
    let y: number;
    let z: number;
    let xFactor = 15;
    let yFactor = 5;



    for (areaIndx = 0; areaIndx < model.areas.length; areaIndx++) {

      var areaXWidth = env.cellXRatio * model.areas[areaIndx].minicolumns.length + env.areaXOffset;
      var areaZWidth = env.cellZRatio * model.areas[areaIndx].minicolumns[0].length + env.areaZOffset;
      var areaYWidth = env.cellYRatio * model.areas[areaIndx].minicolumns[0][0].cells.length + env.areaYOffset;

      if (model.areas[areaIndx].level != lastLevel) {
        levelCnt++;
        lastLevel = model.areas[areaIndx].level;
        xOffset = areaXWidth + levelCnt * areaXWidth / 2;
      }
      else
        xOffset += areaXWidth;
      this.xCoordinatesForOneArea = [];
      this.yCoordinatesForOneArea = [];
      this.zCoordinatesForOneArea = [];

      for (let i = 0; i < model.areas[areaIndx].minicolumns.length; i++) {
        for (let j = 0; j < model.areas[areaIndx].minicolumns[i].length; j++) {
          for (let cellIndx = 0; cellIndx < model.areas[areaIndx].minicolumns[i][j].cells.length; cellIndx++) {

            if (this.overlapIntervalStart != null && this.overlapIntervalEnd != null) {
              if (this.overlapIntervalStart <= model.areas[areaIndx].minicolumns[i][j].overlap && this.overlapIntervalEnd >= model.areas[areaIndx].minicolumns[i][j].overlap) {
                this.overlap.push(model.areas[areaIndx].minicolumns[i][j].overlap);
                this.xNeurons.push(i * env.cellXRatio + xOffset);
                this.yNeurons.push(areaYWidth * model.areas[areaIndx].level + cellIndx * env.cellYRatio);
                this.zNeurons.push(areaZWidth * j);

                let xCoorvalue = (i * env.cellXRatio + xOffset);
                let yCoorvalue = (areaYWidth * model.areas[areaIndx].level + cellIndx * env.cellYRatio);
                let zCoorvalue = (areaZWidth * j);

                this.xCoordinatesForOneArea[i] = xCoorvalue;
                this.yCoordinatesForOneArea[cellIndx] = yCoorvalue;
                this.zCoordinatesForOneArea[j] = zCoorvalue;

                this.xCoordinatesAllAreas[areaIndx] = this.xCoordinatesForOneArea;
                this.yCoordinatesAllAreas[areaIndx] = this.yCoordinatesForOneArea;
                this.zCoordinatesAllAreas[areaIndx] = this.zCoordinatesForOneArea;
              }

            }
            else {

              this.overlap.push(model.areas[areaIndx].minicolumns[i][j].overlap);

              this.xNeurons.push(i * env.cellXRatio + xOffset);
              this.yNeurons.push(areaYWidth * model.areas[areaIndx].level + cellIndx * env.cellYRatio);
              this.zNeurons.push(areaZWidth * j);

              let xCoorvalue = (i * env.cellXRatio + xOffset);
              let yCoorvalue = (areaYWidth * model.areas[areaIndx].level + cellIndx * env.cellYRatio);
              let zCoorvalue = (areaZWidth * j);

              this.xCoordinatesForOneArea[i] = xCoorvalue;
              this.yCoordinatesForOneArea[cellIndx] = yCoorvalue;
              this.zCoordinatesForOneArea[j] = zCoorvalue;

              this.xCoordinatesAllAreas[areaIndx] = this.xCoordinatesForOneArea;
              this.yCoordinatesAllAreas[areaIndx] = this.yCoordinatesForOneArea;
              this.zCoordinatesAllAreas[areaIndx] = this.zCoordinatesForOneArea;

            }
          }
        }
      }

    }
    this.inputModelData(this.model);
    this.synapsesData();

  }

  /**
   * synapsesData gets the synapses coordinates to draw the synapses 
   */
  synapsesData() {

    for (let i = 0; i < this.model.synapses.length; i++) {
      for (let out = 0; out < this.model.synapses[i].preSynaptic.outgoingSynapses.length; out++) {
        for (let inc = 0; inc < this.model.synapses[i].postSynaptic.incomingSynapses.length; inc++) {

          if (this.permanenceIntervalStart != null && this.permanenceIntervalEnd != null) {
            if (this.permanenceIntervalStart <= this.model.synapses[i].permanence && this.permanenceIntervalEnd >= this.model.synapses[i].permanence) {
              this.permanence.push(this.model.synapses[i].permanence);
              this.permanence.push(this.model.synapses[i].permanence);
              this.permanence.push(null);

              let xPre = this.xCoordinatesAllAreas[this.model.synapses[i].preSynaptic.areaIndex][this.model.synapses[i].preSynaptic.outgoingSynapses[out].preSynaptic.X];
              this.xSynapse.push(xPre);
              let xPost = this.xCoordinatesAllAreas[this.model.synapses[i].postSynaptic.areaIndex][this.model.synapses[i].postSynaptic.incomingSynapses[inc].postSynaptic.X];
              this.xSynapse.push(xPost);
              this.xSynapse.push(null);

              let yPre = this.yCoordinatesAllAreas[this.model.synapses[i].preSynaptic.areaIndex][this.model.synapses[i].preSynaptic.outgoingSynapses[out].preSynaptic.Layer];
              this.ySynapse.push(yPre);
              let yPost = this.yCoordinatesAllAreas[this.model.synapses[i].postSynaptic.areaIndex][this.model.synapses[i].postSynaptic.incomingSynapses[inc].postSynaptic.Layer];
              this.ySynapse.push(yPost);
              this.ySynapse.push(null);

              let zPre = this.zCoordinatesAllAreas[this.model.synapses[i].preSynaptic.areaIndex][this.model.synapses[i].preSynaptic.outgoingSynapses[out].preSynaptic.Z];
              this.zSynapse.push(zPre);
              let zPost = this.zCoordinatesAllAreas[this.model.synapses[i].postSynaptic.areaIndex][this.model.synapses[i].postSynaptic.incomingSynapses[inc].postSynaptic.Z];
              this.zSynapse.push(zPost);
              this.zSynapse.push(null);
            }


          }
          else {
            this.permanence.push(this.model.synapses[i].permanence);
            this.permanence.push(this.model.synapses[i].permanence);
            this.permanence.push(null);

            let xPre = this.xCoordinatesAllAreas[this.model.synapses[i].preSynaptic.areaIndex][this.model.synapses[i].preSynaptic.outgoingSynapses[out].preSynaptic.X];
            this.xSynapse.push(xPre);
            let xPost = this.xCoordinatesAllAreas[this.model.synapses[i].postSynaptic.areaIndex][this.model.synapses[i].postSynaptic.incomingSynapses[inc].postSynaptic.X];
            this.xSynapse.push(xPost);
            this.xSynapse.push(null);

            let yPre = this.yCoordinatesAllAreas[this.model.synapses[i].preSynaptic.areaIndex][this.model.synapses[i].preSynaptic.outgoingSynapses[out].preSynaptic.Layer];
            this.ySynapse.push(yPre);
            let yPost = this.yCoordinatesAllAreas[this.model.synapses[i].postSynaptic.areaIndex][this.model.synapses[i].postSynaptic.incomingSynapses[inc].postSynaptic.Layer];
            this.ySynapse.push(yPost);
            this.ySynapse.push(null);

            let zPre = this.zCoordinatesAllAreas[this.model.synapses[i].preSynaptic.areaIndex][this.model.synapses[i].preSynaptic.outgoingSynapses[out].preSynaptic.Z];
            this.zSynapse.push(zPre);
            let zPost = this.zCoordinatesAllAreas[this.model.synapses[i].postSynaptic.areaIndex][this.model.synapses[i].postSynaptic.incomingSynapses[inc].postSynaptic.Z];
            this.zSynapse.push(zPost);
            this.zSynapse.push(null);

          }


        }
      }
    }
    /*  console.log(this.xSynapse);
     console.log(this.ySynapse);
     console.log(this.zSynapse); */


  }
  inputModelData(model: NeoCortexModel) {
    for (let inputmodel = 0; inputmodel < model.input.cells.length; inputmodel++) {
      for (let inputModelDim1 = 0; inputModelDim1 < model.settings.minicolumnDims[1]; inputModelDim1++) {
        this.xInputModel.push(model.input.cells[inputmodel][inputModelDim1].X);
        this.yInputModel.push(0);
        this.zInputModel.push(model.input.cells[inputmodel][inputModelDim1].Z);
        this.inputModelOverlap.push(0);
      }
    }

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

  /**
   * 
   * @param areaID 
   * @param cell 
   */
  showOutgoingIncomingSynapses(areaID: any, cellData: any) {
    //console.log(areaID);
    //console.log(cell);
    let splitCoordinates = cellData.split(",");
    let cellX = parseInt(splitCoordinates[0]);
    let cellLayer = parseInt(splitCoordinates[1]);
    let cellZ = parseInt(splitCoordinates[2]);
    let areaIndex = parseInt(areaID);

    this.xNeuron = cellX;
    this.layerNeuron = cellLayer;
    this.zNeuron = cellZ;
    this.cellAreaIndex = areaIndex;
    console.log(areaID);


    this.xNeurons = [];
    this.yNeurons = [];
    this.zNeurons = [];
    this.xSynapse = [];
    this.ySynapse = [];
    this.zSynapse = [];

    this.neuronsColours = [];
    this.overlap = [];
    this.permanence = [];
    this.synapseColours = [];

    this.xNeurons.push(this.xCoordinatesAllAreas[areaIndex][cellX]);
    this.yNeurons.push(this.yCoordinatesAllAreas[areaIndex][cellLayer]);
    this.zNeurons.push(this.zCoordinatesAllAreas[areaIndex][cellZ]);
    this.overlap.push(this.model.areas[areaIndex].minicolumns[cellX][cellZ].overlap);

    let numberOfOutgoingSynapses = this.model.areas[areaIndex].minicolumns[cellX][cellZ].cells[cellLayer].outgoingSynapses.length;
    let numberOfIncomingSynapses = this.model.areas[areaIndex].minicolumns[cellX][cellZ].cells[cellLayer].incomingSynapses.length;

    for (let out = 0; out < numberOfOutgoingSynapses; out++) {
      let postCell = this.model.areas[areaIndex].minicolumns[cellX][cellZ].cells[cellLayer].outgoingSynapses[out].postSynaptic;

      this.xNeurons.push(this.xCoordinatesAllAreas[postCell.areaIndex][postCell.X]);
      this.yNeurons.push(this.yCoordinatesAllAreas[postCell.areaIndex][postCell.Layer]);
      this.zNeurons.push(this.zCoordinatesAllAreas[postCell.areaIndex][postCell.Z]);
      this.overlap.push(this.model.areas[postCell.areaIndex].minicolumns[postCell.X][postCell.Z].overlap);

      let cell = this.model.areas[areaIndex].minicolumns[cellX][cellZ].cells[cellLayer].outgoingSynapses[out];

      this.xSynapse.push(this.xCoordinatesAllAreas[cell.preSynaptic.areaIndex][cell.preSynaptic.X]);
      this.xSynapse.push(this.xCoordinatesAllAreas[cell.postSynaptic.areaIndex][cell.postSynaptic.X]);
      this.xSynapse.push(null);


      this.ySynapse.push(this.yCoordinatesAllAreas[cell.preSynaptic.areaIndex][cell.preSynaptic.Layer]);
      this.ySynapse.push(this.yCoordinatesAllAreas[cell.postSynaptic.areaIndex][cell.postSynaptic.Layer]);
      this.ySynapse.push(null);

      this.zSynapse.push(this.zCoordinatesAllAreas[cell.preSynaptic.areaIndex][cell.preSynaptic.Z]);
      this.zSynapse.push(this.zCoordinatesAllAreas[cell.postSynaptic.areaIndex][cell.postSynaptic.Z]);
      this.zSynapse.push(null);

      this.permanence.push(this.model.areas[areaIndex].minicolumns[cellX][cellZ].cells[cellLayer].outgoingSynapses[out].permanence);
      this.permanence.push(this.model.areas[areaIndex].minicolumns[cellX][cellZ].cells[cellLayer].outgoingSynapses[out].permanence);
      this.permanence.push(null);

    }

    for (let inc = 0; inc < numberOfIncomingSynapses; inc++) {
      let preCell = this.model.areas[areaIndex].minicolumns[cellX][cellZ].cells[cellLayer].incomingSynapses[inc].preSynaptic;

      this.xNeurons.push(this.xCoordinatesAllAreas[preCell.areaIndex][preCell.X]);
      this.yNeurons.push(this.yCoordinatesAllAreas[preCell.areaIndex][preCell.Layer]);
      this.zNeurons.push(this.zCoordinatesAllAreas[preCell.areaIndex][preCell.Z]);
      this.overlap.push(this.model.areas[preCell.areaIndex].minicolumns[preCell.X][preCell.Z].overlap);

      let cell = this.model.areas[areaIndex].minicolumns[cellX][cellZ].cells[cellLayer].incomingSynapses[inc];

      this.xSynapse.push(this.xCoordinatesAllAreas[cell.preSynaptic.areaIndex][cell.preSynaptic.X]);
      this.xSynapse.push(this.xCoordinatesAllAreas[cell.postSynaptic.areaIndex][cell.postSynaptic.X]);
      this.xSynapse.push(null);


      this.ySynapse.push(this.yCoordinatesAllAreas[cell.preSynaptic.areaIndex][cell.preSynaptic.Layer]);
      this.ySynapse.push(this.yCoordinatesAllAreas[cell.postSynaptic.areaIndex][cell.postSynaptic.Layer]);
      this.ySynapse.push(null);

      this.zSynapse.push(this.zCoordinatesAllAreas[cell.preSynaptic.areaIndex][cell.preSynaptic.Z]);
      this.zSynapse.push(this.zCoordinatesAllAreas[cell.postSynaptic.areaIndex][cell.postSynaptic.Z]);
      this.zSynapse.push(null);

      this.permanence.push(this.model.areas[areaIndex].minicolumns[cellX][cellZ].cells[cellLayer].incomingSynapses[inc].permanence);
      this.permanence.push(this.model.areas[areaIndex].minicolumns[cellX][cellZ].cells[cellLayer].incomingSynapses[inc].permanence);
      this.permanence.push(null);
    }
    /* 
       console.log(this.xSynapse);
       console.log(this.ySynapse);
       console.log(this.zSynapse);
       console.log(this.permanence); */

    this.generateColoursFromOverlap();
    this.generateColoursFromPermanences();
    this.plotChart();

  }

  /**
   * filterPermanence filters the synapses by permanence according to a definite enclosed interval 
   * @param permanenceInterval 
   */
  filterPermanence(permanenceInterval: any) {
    let splitpermanenceInterval = permanenceInterval.split(" ");
    let permanenceIntervalStart = parseFloat(splitpermanenceInterval[0]);
    let permanenceIntervalEnd = parseFloat(splitpermanenceInterval[1]);
    console.log(permanenceIntervalStart);
    console.log(permanenceIntervalEnd);
    this.permanenceIntervalStart = permanenceIntervalStart;
    this.permanenceIntervalEnd = permanenceIntervalEnd;


    this.xNeurons = [];
    this.yNeurons = [];
    this.zNeurons = [];
    this.xCoordinatesAllAreas = [];
    this.yCoordinatesAllAreas = [];
    this.zCoordinatesAllAreas = [];
    this.xCoordinatesForOneArea = [];
    this.yCoordinatesForOneArea = [];
    this.zCoordinatesForOneArea = [];
    this.xSynapse = [];
    this.ySynapse = [];
    this.zSynapse = [];

    this.neuronsColours = [];
    this.overlap = [];
    this.permanence = [];
    this.synapseColours = [];

    this.fillChart(this.model);
    this.generateColoursFromOverlap();
    this.generateColoursFromPermanences();
    this.plotChart();
  }

  /**
   * filterOverlap filters the neurons by certain overlap values, that lies in the specified enclosed interval
   * @param overlapInterval 
   */
  filterOverlap(overlapInterval: any) {
    let splitOverlapInterval = overlapInterval.split(" ");
    let overlapIntervalStart = parseFloat(splitOverlapInterval[0]);
    let overlapIntervalEnd = parseFloat(splitOverlapInterval[1]);
    console.log(overlapIntervalStart);
    console.log(overlapIntervalEnd);
    this.overlapIntervalStart = overlapIntervalStart;
    this.overlapIntervalEnd = overlapIntervalEnd;

    this.xNeurons = [];
    this.yNeurons = [];
    this.zNeurons = [];
    this.xCoordinatesAllAreas = [];
    this.yCoordinatesAllAreas = [];
    this.zCoordinatesAllAreas = [];
    this.xCoordinatesForOneArea = [];
    this.yCoordinatesForOneArea = [];
    this.zCoordinatesForOneArea = [];
    this.xSynapse = [];
    this.ySynapse = [];
    this.zSynapse = [];

    this.neuronsColours = [];
    this.overlap = [];
    this.permanence = [];
    this.synapseColours = [];

    this.fillChart(this.model);
    this.generateColoursFromOverlap();
    this.generateColoursFromPermanences();
    this.plotChart();

  }

  updateSynapses(preCellAreaId: any, postCellAreaID: any, preCell: Cell, postCell: Cell, permanenc: any) {
    let permaValue = parseFloat(permanenc);
    let preCellArea = parseInt(preCellAreaId);
    let postCellArea = parseInt(postCellAreaID);
    let prX = parseInt(preCell[0]);
    let prY = parseInt(preCell[2]);
    let prZ = parseInt(preCell[4]);
    let poX = parseInt(postCell[0]);
    let poY = parseInt(postCell[2]);
    let poZ = parseInt(postCell[4]);

    this.updateSynaps([
      {
        preCellArea: preCellArea,
        postCellArea: postCellArea,
        preCell:
        {
          cellX: prX,
          cellY: prY,
          cellZ: prZ,
        },
        postCell: {
          cellX: poX,
          cellY: poY,
          cellZ: poZ,
        },
        permanence: permaValue
      }
    ]);

  }


  updateSynaps(perms: any) {
    this.xNeurons = [];
    this.yNeurons = [];
    this.zNeurons = [];
    this.overlap = [];
    this.neuronsColours = [];

    this.xSynapse = [];
    this.ySynapse = [];
    this.zSynapse = [];
    this.permanence = [];
    this.synapseColours = [];

    this.lookupSynapse(perms);
  }

  /**
   * Lookup method search certain synapse and calls the further methods to update or create a new synapse
   * @param permancences 
   */
  private lookupSynapse(permancences: any[]) {
    let preCell: any;
    let postCell: any;
    let perm: any;
    for (let i = 0; i < permancences.length; i++) {
      perm = permancences[i];
      let preMinCol = this.model.areas[perm.preCellArea].minicolumns[perm.preCell.cellX][perm.preCell.cellZ];
      let postMinCol = this.model.areas[perm.postCellArea].minicolumns[perm.postCell.cellX][perm.postCell.cellZ];
      preCell = preMinCol.cells[perm.preCell.cellY];
      postCell = postMinCol.cells[perm.postCell.cellY];

    }

    let synapseFound = false;
    loop:
    for (let out = 0; out < preCell.outgoingSynapses.length; out++) {
      for (let inc = 0; inc < postCell.incomingSynapses.length; inc++) {

        if ((preCell.outgoingSynapses[out].postSynaptic.X === postCell.X &&
          preCell.outgoingSynapses[out].postSynaptic.Layer === postCell.Layer &&
          preCell.outgoingSynapses[out].postSynaptic.Z === postCell.Z) &&

          (postCell.incomingSynapses[inc].preSynaptic.X === preCell.X &&
            postCell.incomingSynapses[inc].preSynaptic.Layer === preCell.Layer &&
            postCell.incomingSynapses[inc].preSynaptic.Z === preCell.Z)) {

          console.log("Synapse Exists");
          this.updatePermanenceOfSynaps(perm.permanence, preCell, postCell);
          synapseFound = true;
          break loop;
        }


      }
    }
    if (synapseFound === false) {
      console.log("Synapse does not exists");
      this.createSynapse(perm.permanence, preCell, postCell);
    }


  }
  /**
   * This method updates the permanence value of an existing synapse
   * @param permanence 
   * @param preCell 
   * @param postCell 
   */
  updatePermanenceOfSynaps(newPermanence: number, preCell: Cell, postCell: Cell) {
    console.log("Permannence will be updated");

    /*   preCell.outgoingSynapses[0].permanence = newPermanence;
      postCell.incomingSynapses[0].permanence = newPermanence; */
    //maybe we need it later
    //this.model.areas[preCell.areaIndex].minicolumns[preCell.X][preCell.Z].cells[preCell.Layer].outgoingSynapses[?].permanence = newPermanence;
    for (let findSynapse = 0; findSynapse < this.model.synapses.length; findSynapse++) {

      if (this.model.synapses[findSynapse].preSynaptic.areaIndex === preCell.areaIndex &&
        this.model.synapses[findSynapse].preSynaptic.X === preCell.X &&
        this.model.synapses[findSynapse].preSynaptic.Layer === preCell.Layer &&
        this.model.synapses[findSynapse].preSynaptic.Z === preCell.Z &&

        this.model.synapses[findSynapse].postSynaptic.areaIndex === postCell.areaIndex &&
        this.model.synapses[findSynapse].postSynaptic.X === postCell.X &&
        this.model.synapses[findSynapse].postSynaptic.Layer === postCell.Layer &&
        this.model.synapses[findSynapse].postSynaptic.Z === postCell.Z) {

        /* this.model.synapses[findSynapse].preSynaptic.outgoingSynapses[0].permanence = newPermanence;
        this.model.synapses[findSynapse].postSynaptic.incomingSynapses[0].permanence = newPermanence */;
        this.model.synapses[findSynapse].permanence = newPermanence;
      }


    }
    this.fillChart(this.model);
    this.generateColoursFromOverlap();
    this.generateColoursFromPermanences();
    this.plotChart();
  }


  /**
   * This method creates a synapse
   * @param permanence 
   * @param preCell 
   * @param postCell 
   */
  createSynapse(permanence: number, preCell: Cell, postCell: Cell) {

    preCell = new Cell(preCell.areaIndex, preCell.X, preCell.Layer, preCell.Z, [], []);
    postCell = new Cell(postCell.areaIndex, postCell.X, postCell.Layer, postCell.Z, [], []);
    let incomingSynapse = new Synapse(permanence, preCell, postCell);
    let outgoingSynapse = new Synapse(permanence, preCell, postCell);

    /*  preCell = new Cell(preCell.areaIndex, preCell.X, preCell.Layer, preCell.Z, [], [outgoingSynapse]);
     postCell = new Cell(postCell.areaIndex, postCell.X, postCell.Layer, postCell.Z, [incomingSynapse], []); */

    preCell.outgoingSynapses.push(outgoingSynapse);
    postCell.incomingSynapses.push(incomingSynapse);

    this.model.areas[preCell.areaIndex].minicolumns[preCell.X][preCell.Z].cells[preCell.Layer].outgoingSynapses.push(outgoingSynapse);
    this.model.areas[postCell.areaIndex].minicolumns[postCell.X][postCell.Z].cells[postCell.Layer].incomingSynapses.push(incomingSynapse);

    console.log("Synapse will be created");
    let synapse: Synapse;
    synapse = new Synapse(permanence, preCell, postCell);
    this.model.synapses.push(synapse);
    this.fillChart(this.model);
    this.generateColoursFromOverlap();
    this.generateColoursFromPermanences();
    this.plotChart();

  }


  //update column overlaP
  clickFunc() {
    this.updateOverlapCell(0, 0, 0, [0.5, 0.7, 1, 0.75, 0.4, 1]);
  }


  updateOverlapCell(selectAreaIndex: any, miniColumnXDimension: any, miniColumnZDimension: any, overlapArray: any[]) {

    let overlaps = [];

    overlaps.push({ selectAreaIndex: selectAreaIndex, miniColumnXDimension: miniColumnXDimension, miniColumnZDimension: miniColumnZDimension, overlapArray: overlapArray });
    this.xNeurons = [];
    this.yNeurons = [];
    this.zNeurons = [];
    this.overlap = [];
    this.xSynapse = [];
    this.ySynapse = [];
    this.zSynapse = [];
    this.synapseColours = [];
    this.permanence = [];
    this.neuronsColours = [];

    this.updateOverlapColumn(overlaps);
  }
  updateOverlapColumn(overlaps: any[]) {
    for (var i = 0; i < overlaps.length; i++) {
      for (var j = 0; j < overlaps[i].overlapArray.length; j++) {
        this.model.areas[overlaps[i].selectAreaIndex].minicolumns[overlaps[i].miniColumnXDimension][overlaps[i].miniColumnZDimension].overlap = parseFloat(overlaps[i].overlapArray[j]);
      }
    }
    this.fillChart(this.model);
    this.generateColoursFromOverlap();
    this.generateColoursFromPermanences();
    this.plotChart();
  }

  // This method is for updating overlap column from userinterface
  updateOverlapV(selectAreaIndex: any, miniColumnXDimension: any, miniColumnZDimension: any, newOverlapValue: any) {
    this.areaID = selectAreaIndex;
    this.miniColumnXDimension = miniColumnXDimension;
    this.miniColumnZDimension = miniColumnZDimension;
    this.newOverlapValue = newOverlapValue;
    this.xNeurons = [];
    this.yNeurons = [];
    this.zNeurons = [];
    this.overlap = [];
    this.xSynapse = [];
    this.ySynapse = [];
    this.zSynapse = [];
    this.synapseColours = [];
    this.permanence = [];
    this.neuronsColours = [];

    this.model.areas[this.areaID].minicolumns[this.miniColumnXDimension][this.miniColumnZDimension].overlap = parseFloat(this.newOverlapValue);

    this.fillChart(this.model);
    this.generateColoursFromOverlap();
    this.generateColoursFromPermanences();
    this.plotChart();
  }

  generateColoursFromOverlap() {

    for (const overlapVal of this.overlap) {
      let H = (1.0 - overlapVal) * 240;
      this.neuronsColours.push("hsl(" + H + ", 100%, 50%)");
    }

  }

  generateColoursFromPermanences() {

    for (const permanenceVal of this.permanence) {
      let H = (1.0 - permanenceVal) * 240;
      this.synapseColours.push("hsl(" + H + ", 100%, 50%)");
    }
    /*  for (let permanenceValue = 0; permanenceValue < this.xSynapse.length; permanenceValue++) {
       let H = (1.0 - this.permanence[permanenceValue]) * 240;
       this.synapseColours.push("hsl(" + H + ", 100%, 50%)");
     } */
    //console.log(this.synapseColours);


  }


}
