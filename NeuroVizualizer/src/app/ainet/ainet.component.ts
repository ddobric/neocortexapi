import { Component, OnInit, AfterViewInit } from '@angular/core';
//import * as Plotly from 'plotly.js';
import * as Plotlyjs from 'plotly.js/dist/plotly';
import { environment as env } from "../../environments/environment.prod";
import { NotificationsService } from 'angular2-notifications';
import { NeoCortexUtilsService } from '../services/neocortexutils.service';
import { NeoCortexModel, Cell, Synapse } from '../Entities/NeoCortexModel';
import { NotifierService } from "angular-notifier";




@Component({
  selector: 'app-ainet',
  templateUrl: './ainet.component.html',
  styleUrls: ['./ainet.component.scss']
})
export class AinetComponent implements OnInit, AfterViewInit {
  private readonly notifier: NotifierService;
  Model: any;
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
  xCoordinatesForOneArea: Array<any> = [];
  yCoordinatesForOneArea: Array<any> = [];
  zCoordinatesForOneArea: Array<any> = [];

  overlapIntervalStart: number = null;
  overlapIntervalEnd: number = null;
  permanenceIntervalStart: number = null;
  permanenceIntervalEnd: number = null;

  options: any;

  xInputModel: Array<any> = [];
  zInputModel: Array<any> = [];
  yInputModel: Array<any> = [];
  inputModelOverlap: Array<any> = [];

  neuronsHoverInformation: Array<any> = [];
  synapsesHoverInformation: Array<any> = [];

  overlapInterval: any;
  permanenceInterval: any;
  showSynapses: any;
  cellAreaId: any;

  neuralchart: object = {};
  pattern: string | RegExp;


  constructor(private _service: NotificationsService, private neoUtilsService: NeoCortexUtilsService, notifierService: NotifierService) {
    this.notifier = notifierService;



  }




  ngOnInit() {

  }
  /*  let reg1 = new RegExp(/^\d+[.,]?\d{0,2}$/g);
     let reg = new RegExp(/^\d[.,]?\d{0,4}$/g); */
  /* restrictNumbers(e) {

    let input = String.fromCharCode(e.charCode);
    let reg2 = new RegExp(/^[0][.,][0-9]{0,4}$/);
    if (!reg2.test(input)) {
      e.preventDefault();
    }

  } */

  ngAfterViewInit() {


    if (!this.Model) {
      this.plotDummyChart();
    }
    this.neoUtilsService.data.subscribe(a => {
      this.Model = a.dataModel;
      this.clearData();
      this.fillChart(this.Model);
      this.generateColoursFromOverlap();
      this.generateColoursFromPermanences();
      this.plotChart();
      this.pushNotification(a.notification);
    });




    //this.model = new NeoCortexGenerator().createModel([0, 0, 0, 1, 2, 1], [10, 1], 6);
    //this.model = neoCortexUtils.createModel([0, 0, 0, 1, 2, 1], [10, 1], 6); // createModel (numberOfAreas, [xAxis, zAxis], yAxis)
    /*  this.fillChart(this.model);
     this.generateColoursFromOverlap();
     this.generateColoursFromPermanences();
     this.plotChart(); */

  }
  pushNotification(notify) {
    console.log(notify);
    this.notifier.notify(notify.type, notify.msg);
  }


  plotDummyChart() {
    const neurons = {
      x: [0],
      y: [0],
      z: [0],
      hovertext: [],
      hoverinfo: 'text',
      type: 'scatter3d',
      opacity: [1],
      size: [0],
      marker: {
        color: 'rgba(100, 100, 100, 0.5)',
        symbol: 'circle'
      }
    };

    const neuralChartLayout = {
      legend: {
        x: 0.5,
        y: 1
      },
      width: 1500,
      height: 500,
      margin: {
        l: 0,
        r: 0,
        b: 0,
        t: 0,
        pad: 4
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

    let graphDOM = document.getElementById('graph');
    Plotlyjs.react(graphDOM, [neurons], neuralChartLayout, neuralChartConfig);
    //Plotlyjs.newPlot(graphDOM, [PointsT, linesT], neuralChartLayout);
    window.onresize = function () {
      Plotlyjs.relayout(graphDOM, {
        width: 0.9 * window.innerWidth,
        height: 0.9 * window.innerHeight
      });
    }
  }

  /**
   * plotChart just plot the chart by using plotly library 
   */
  plotChart() {
    const neurons = {
      x: this.xNeurons,
      y: this.yNeurons,
      z: this.zNeurons,
      hovertext: this.neuronsHoverInformation,
      hoverinfo: 'text',
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
      //text: this.permanence,
      hovertext: this.synapsesHoverInformation,
      hoverinfo: 'text',
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
      uirevision: true,
    };

    const neuralChartConfig = {
      //displayModeBar: false,
      title: '3DChart',
      displaylogo: false,
      showLink: false,
      responsive: true
      // showlegend: false
    };



    let graphDOM = document.getElementById('graph');
    Plotlyjs.react(graphDOM, [neurons, synapses, inputModel], neuralChartLayout, neuralChartConfig);
    //Plotlyjs.newPlot(graphDOM, [PointsT, linesT], neuralChartLayout);

    window.onresize = function () {
      Plotlyjs.relayout(graphDOM, {
        width: 0.9 * window.innerWidth,
        height: 0.9 * window.innerHeight
      });
    }
  }



  /**
   * Fillchart method gets the data from the model, that will be later used to plot the chart by plotchart method
   * @param model 
   */
  fillChart(model: NeoCortexModel) {
    let lastLevel = 0;
    let levelCnt = 0;
    let xOffset = 0;

    for (let areaIndx = 0; areaIndx < model.Areas.length; areaIndx++) {

      var areaXWidth = env.cellXRatio * model.Areas[areaIndx].Minicolumns.length + env.areaXOffset;
      var areaZWidth = env.cellZRatio * model.Areas[areaIndx].Minicolumns[0].length + env.areaZOffset;
      var areaYWidth = env.cellYRatio * model.Areas[areaIndx].Minicolumns[0][0].Cells.length + env.areaYOffset;

      if (model.Areas[areaIndx].level != lastLevel) {
        levelCnt++;
        lastLevel = model.Areas[areaIndx].level;
        xOffset = areaXWidth + levelCnt * areaXWidth / 2;
      }
      else
        xOffset += areaXWidth;
      this.xCoordinatesForOneArea = [];
      this.yCoordinatesForOneArea = [];
      this.zCoordinatesForOneArea = [];

      for (let i = 0; i < model.Areas[areaIndx].Minicolumns.length; i++) {
        for (let j = 0; j < model.Areas[areaIndx].Minicolumns[i].length; j++) {
          for (let cellIndx = 0; cellIndx < model.Areas[areaIndx].Minicolumns[i][j].Cells.length; cellIndx++) {

            let xCoorvalue = (i * env.cellXRatio + xOffset);
            let yCoorvalue = (areaYWidth * model.Areas[areaIndx].level + cellIndx * env.cellYRatio);
            let zCoorvalue = (areaZWidth * j);

            if (this.overlapIntervalStart != null && this.overlapIntervalEnd != null) {
              if (this.overlapIntervalStart <= model.Areas[areaIndx].Minicolumns[i][j].overlap && this.overlapIntervalEnd >= model.Areas[areaIndx].Minicolumns[i][j].overlap) {

                this.drawNeuronsCoordinates(model, areaIndx, i, j, cellIndx, areaYWidth, xOffset, areaZWidth);
                this.bijection(areaIndx, i, cellIndx, j, xCoorvalue, yCoorvalue, zCoorvalue);

              }

            }
            else {

              this.drawNeuronsCoordinates(model, areaIndx, i, j, cellIndx, areaYWidth, xOffset, areaZWidth);
              this.bijection(areaIndx, i, cellIndx, j, xCoorvalue, yCoorvalue, zCoorvalue);



            }
          }
        }
      }

    }
    this.inputModelData(this.Model);
    this.synapsesData();

  }

  drawNeuronsCoordinates(model: any, areaIndx: number, i: number, j: number, cellIndx: number, areaYWidth: number, xOffset: number, areaZWidth: number) {
    this.overlap.push(model.Areas[areaIndx].Minicolumns[i][j].overlap);
    this.neuronsHoverInformation.push('N' + '<br>' + model.Areas[areaIndx].Minicolumns[i][j].overlap.toString() + '<br>' + areaIndx.toString());
    this.xNeurons.push(i * env.cellXRatio + xOffset);
    this.yNeurons.push(areaYWidth * model.Areas[areaIndx].level + cellIndx * env.cellYRatio);
    this.zNeurons.push(areaZWidth * j);

  }

  bijection(areaIndx: number, i: number, cellIndx: number, j: number, xCoorvalue: number, yCoorvalue: number, zCoorvalue: number) {

    this.xCoordinatesForOneArea[i] = xCoorvalue;
    this.yCoordinatesForOneArea[cellIndx] = yCoorvalue;
    this.zCoordinatesForOneArea[j] = zCoorvalue;

    this.xCoordinatesAllAreas[areaIndx] = this.xCoordinatesForOneArea;
    this.yCoordinatesAllAreas[areaIndx] = this.yCoordinatesForOneArea;
    this.zCoordinatesAllAreas[areaIndx] = this.zCoordinatesForOneArea;

  }

  synapsesData() {

    for (let i = 0; i < this.Model.Synapses.length; i++) {
      if (this.permanenceIntervalStart != null && this.permanenceIntervalEnd != null) {
        if (this.permanenceIntervalStart <= this.Model.Synapses[i].permanence && this.permanenceIntervalEnd >= this.Model.Synapses[i].permanence) {
          this.drawSynapsesCoordinates(i);

        }
      }
      else {
        if (typeof this.xCoordinatesAllAreas[this.Model.Synapses[i].preSynaptic.areaIndex] !== "undefined" && typeof this.xCoordinatesAllAreas[this.Model.Synapses[i].postSynaptic.areaIndex] !== "undefined") {

          this.drawSynapsesCoordinates(i);

        }
      }

    }

  }

  drawSynapsesCoordinates(i: number) {
    let xPre = this.xCoordinatesAllAreas[this.Model.Synapses[i].preSynaptic.areaIndex][this.Model.Synapses[i].preSynaptic.X];
    this.xSynapse.push(xPre);
    let xPost = this.xCoordinatesAllAreas[this.Model.Synapses[i].postSynaptic.areaIndex][this.Model.Synapses[i].postSynaptic.X];
    this.xSynapse.push(xPost);
    this.xSynapse.push(null);

    let yPre = this.yCoordinatesAllAreas[this.Model.Synapses[i].preSynaptic.areaIndex][this.Model.Synapses[i].preSynaptic.Layer];
    this.ySynapse.push(yPre);
    let yPost = this.yCoordinatesAllAreas[this.Model.Synapses[i].postSynaptic.areaIndex][this.Model.Synapses[i].postSynaptic.Layer];
    this.ySynapse.push(yPost);
    this.ySynapse.push(null);

    let zPre = this.zCoordinatesAllAreas[this.Model.Synapses[i].preSynaptic.areaIndex][this.Model.Synapses[i].preSynaptic.Z];
    this.zSynapse.push(zPre);
    let zPost = this.zCoordinatesAllAreas[this.Model.Synapses[i].postSynaptic.areaIndex][this.Model.Synapses[i].postSynaptic.Z];
    this.zSynapse.push(zPost);
    this.zSynapse.push(null);

    this.permanence.push(this.Model.synapses[i].permanence);
    this.permanence.push(this.Model.synapses[i].permanence);
    this.permanence.push(null);

    this.synapsesHoverInformation.push('S' + '<br>' + this.Model.synapses[i].permanence.toString() + '<br>' + this.Model.synapses[i].preSynaptic.areaIndex.toString());
    this.synapsesHoverInformation.push('S' + '<br>' + this.Model.synapses[i].permanence.toString() + '<br>' + this.Model.synapses[i].postSynaptic.areaIndex.toString());
    this.synapsesHoverInformation.push(null);
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

  clearData() {
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
    this.synapsesHoverInformation = [];
    this.neuronsHoverInformation = [];
  }

  clearCoordinates() {
    this.xCoordinatesAllAreas = [];
    this.yCoordinatesAllAreas = [];
    this.zCoordinatesAllAreas = [];
  }


  /**
   * 
   * @param areaID 
   * @param cell 
   */
  showOutgoingIncomingSynapses() {

    try {

      let splitCoordinates = this.showSynapses.split(",");
      let cellX = parseInt(splitCoordinates[0]);
      let cellLayer = parseInt(splitCoordinates[1]);
      let cellZ = parseInt(splitCoordinates[2]);
      let areaIndex = parseInt(this.cellAreaId);

      this.clearData();

      this.xNeurons.push(this.xCoordinatesAllAreas[areaIndex][cellX]);
      this.yNeurons.push(this.yCoordinatesAllAreas[areaIndex][cellLayer]);
      this.zNeurons.push(this.zCoordinatesAllAreas[areaIndex][cellZ]);
      this.overlap.push(this.Model.Areas[areaIndex].minicolumns[cellX][cellZ].overlap);
      this.neuronsHoverInformation.push('N' + '<br>' + this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].overlap.toString() + '<br>' + areaIndex.toString());

      let numberOfOutgoingSynapses = this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].outgoingSynapses.length;
      let numberOfIncomingSynapses = this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].incomingSynapses.length;

      for (let out = 0; out < numberOfOutgoingSynapses; out++) {

        let postCells = this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].outgoingSynapses[out].postSynaptic;
        let preCells = this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].outgoingSynapses[out].preSynaptic;

        this.xNeurons.push(this.xCoordinatesAllAreas[postCells.areaIndex][postCells.X]);
        this.yNeurons.push(this.yCoordinatesAllAreas[postCells.areaIndex][postCells.Layer]);
        this.zNeurons.push(this.zCoordinatesAllAreas[postCells.areaIndex][postCells.Z]);
        this.overlap.push(this.Model.Areas[postCells.areaIndex].Minicolumns[postCells.X][postCells.Z].overlap);
        this.neuronsHoverInformation.push('N' + '<br>' + this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].overlap.toString() + '<br>' + postCells.areaIndex.toString());


        this.xSynapse.push(this.xCoordinatesAllAreas[preCells.areaIndex][preCells.X]);
        this.xSynapse.push(this.xCoordinatesAllAreas[postCells.areaIndex][postCells.X]);
        this.xSynapse.push(null);


        this.ySynapse.push(this.yCoordinatesAllAreas[preCells.areaIndex][preCells.Layer]);
        this.ySynapse.push(this.yCoordinatesAllAreas[postCells.areaIndex][postCells.Layer]);
        this.ySynapse.push(null);

        this.zSynapse.push(this.zCoordinatesAllAreas[preCells.areaIndex][preCells.Z]);
        this.zSynapse.push(this.zCoordinatesAllAreas[postCells.areaIndex][postCells.Z]);
        this.zSynapse.push(null);

        this.permanence.push(this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].outgoingSynapses[out].permanence);
        this.permanence.push(this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].outgoingSynapses[out].permanence);
        this.permanence.push(null);

        this.synapsesHoverInformation.push('S' + '<br>' + this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].outgoingSynapses[out].permanence.toString() + '<br>' +
          preCells.areaIndex.toString());
        this.synapsesHoverInformation.push('S' + '<br>' + this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].outgoingSynapses[out].permanence.toString() + '<br>' +
          postCells.areaIndex.toString());
        this.synapsesHoverInformation.push(null);

      }

      for (let inc = 0; inc < numberOfIncomingSynapses; inc++) {

        let preCells = this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].incomingSynapses[inc].preSynaptic;
        let postCells = this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].incomingSynapses[inc].postSynaptic;

        this.xNeurons.push(this.xCoordinatesAllAreas[preCells.areaIndex][preCells.X]);
        this.yNeurons.push(this.yCoordinatesAllAreas[preCells.areaIndex][preCells.Layer]);
        this.zNeurons.push(this.zCoordinatesAllAreas[preCells.areaIndex][preCells.Z]);
        this.overlap.push(this.Model.areas[preCells.areaIndex].Minicolumns[preCells.X][preCells.Z].overlap);

        this.xSynapse.push(this.xCoordinatesAllAreas[preCells.areaIndex][preCells.X]);
        this.xSynapse.push(this.xCoordinatesAllAreas[postCells.areaIndex][postCells.X]);
        this.xSynapse.push(null);


        this.ySynapse.push(this.yCoordinatesAllAreas[preCells.areaIndex][preCells.Layer]);
        this.ySynapse.push(this.yCoordinatesAllAreas[postCells.areaIndex][postCells.Layer]);
        this.ySynapse.push(null);

        this.zSynapse.push(this.zCoordinatesAllAreas[preCells.areaIndex][preCells.Z]);
        this.zSynapse.push(this.zCoordinatesAllAreas[postCells.areaIndex][postCells.Z]);
        this.zSynapse.push(null);

        this.permanence.push(this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].incomingSynapses[inc].permanence);
        this.permanence.push(this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].incomingSynapses[inc].permanence);
        this.permanence.push(null);

        this.synapsesHoverInformation.push('S' + '<br>' + this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].incomingSynapses[inc].permanence.toString() + '<br>' +
          preCells.areaIndex.toString());
        this.synapsesHoverInformation.push('S' + '<br>' + this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].Cells[cellLayer].incomingSynapses[inc].permanence.toString() + '<br>' +
          postCells.areaIndex.toString());
        this.synapsesHoverInformation.push(null);

        this.neuronsHoverInformation.push('N' + '<br>' + this.Model.Areas[areaIndex].Minicolumns[cellX][cellZ].overlap.toString() + '<br>' + preCells.areaIndex.toString());
      }
      /* 
         console.log(this.xSynapse);
         console.log(this.ySynapse);
         console.log(this.zSynapse);
         console.log(this.permanence); */
      console.log(this.neuronsHoverInformation);
      console.log(this.synapsesHoverInformation);
      this.generateColoursFromOverlap();
      this.generateColoursFromPermanences();
      this.plotChart();
      this.notifier.notify("success", "Done");

    } catch (error) {
      this.notifier.notify("error", error);

    }


  }

  /**
   * filterPermanence filters the synapses by permanence according to a definite enclosed interval 
   * @param permanenceInterval 
   */
  filterPermanence() {
    let splitpermanenceInterval = this.permanenceInterval.split(" ");
    let permanenceIntervalStart = parseFloat(splitpermanenceInterval[0]);
    let permanenceIntervalEnd = parseFloat(splitpermanenceInterval[1]);
    console.log(permanenceIntervalStart);
    console.log(permanenceIntervalEnd);
    this.permanenceIntervalStart = permanenceIntervalStart;
    this.permanenceIntervalEnd = permanenceIntervalEnd;

    this.clearData();
    this.clearCoordinates();
    this.fillChart(this.Model);
    this.generateColoursFromOverlap();
    this.generateColoursFromPermanences();
    this.plotChart();
    this.permanenceIntervalStart = null;
    this.permanenceIntervalEnd = null;
  }

  /**
   * filterOverlap filters the neurons by certain overlap values, that lies in the specified enclosed interval
   * @param overlapInterval 
   */
  filterOverlap() {
    try {
      let splitOverlapInterval = this.overlapInterval.split(" ");
      let overlapIntervalStart = parseFloat(splitOverlapInterval[0]);
      let overlapIntervalEnd = parseFloat(splitOverlapInterval[1]);
      console.log(overlapIntervalStart);
      console.log(overlapIntervalEnd);
      this.overlapIntervalStart = overlapIntervalStart;
      this.overlapIntervalEnd = overlapIntervalEnd;

      this.clearData();
      this.clearCoordinates();
      this.fillChart(this.Model);
      this.generateColoursFromOverlap();
      this.generateColoursFromPermanences();
      this.plotChart();
      //this.updatePlot();
      this.overlapIntervalStart = null;
      this.overlapIntervalEnd = null;
      this.notifier.notify("success", "Overlap updated");
    } catch (error) {
      this.notifier.notify("error", error);

    }


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

    this.clearData();
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
      let preMinCol = this.Model.Areas[perm.preCellArea].Minicolumns[perm.preCell.cellX][perm.preCell.cellZ];
      let postMinCol = this.Model.Areas[perm.postCellArea].Minicolumns[perm.postCell.cellX][perm.postCell.cellZ];
      preCell = preMinCol.Cells[perm.preCell.cellY];
      postCell = postMinCol.Cells[perm.postCell.cellY];

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

          // this.create("Synapse Exists", "Permanence will be updated", 'info');
          this.updatePermanenceOfSynaps(perm.permanence, preCell, postCell);
          synapseFound = true;
          break loop;
        }


      }
    }
    if (synapseFound === false) {
      //this.create("Synapse doesn't Exists", "It will be created", 'info');
      this.generateNewSynapse(perm.permanence, preCell, postCell);
    }


  }
  /**
   * This method updates the permanence value of an existing synapse
   * @param permanence 
   * @param preCell 
   * @param postCell 
   */
  updatePermanenceOfSynaps(newPermanence: number, preCell: Cell, postCell: Cell) {
    //console.log("Permannence will be updated");

    /*   preCell.outgoingSynapses[0].permanence = newPermanence;
      postCell.incomingSynapses[0].permanence = newPermanence; */
    //maybe we need it later
    //this.model.areas[preCell.areaIndex].minicolumns[preCell.X][preCell.Z].cells[preCell.Layer].outgoingSynapses[?].permanence = newPermanence;
    for (let findSynapse = 0; findSynapse < this.Model.Synapses.length; findSynapse++) {

      if (this.Model.synapses[findSynapse].preSynaptic.areaIndex === preCell.areaIndex &&
        this.Model.synapses[findSynapse].preSynaptic.X === preCell.X &&
        this.Model.synapses[findSynapse].preSynaptic.Layer === preCell.Layer &&
        this.Model.synapses[findSynapse].preSynaptic.Z === preCell.Z &&

        this.Model.synapses[findSynapse].postSynaptic.areaIndex === postCell.areaIndex &&
        this.Model.synapses[findSynapse].postSynaptic.X === postCell.X &&
        this.Model.synapses[findSynapse].postSynaptic.Layer === postCell.Layer &&
        this.Model.synapses[findSynapse].postSynaptic.Z === postCell.Z) {

        /* this.model.synapses[findSynapse].preSynaptic.outgoingSynapses[0].permanence = newPermanence;
        this.model.synapses[findSynapse].postSynaptic.incomingSynapses[0].permanence = newPermanence */;
        this.Model.synapses[findSynapse].permanence = newPermanence;
      }


    }
    //this.create("Permanence Updated", "Finished", 'success');
    this.fillChart(this.Model);
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
  generateNewSynapse(synapsePermanence: number, preCell: Cell, postCell: Cell) {

    let newSynapse: Synapse = {
      permanence: synapsePermanence,
      preSynaptic: preCell,
      postSynaptic: postCell
    };


    preCell.outgoingSynapses.push(newSynapse);
    postCell.incomingSynapses.push(newSynapse);

    this.Model.Areas[preCell.areaIndex].Minicolumns[preCell.X][preCell.Z].Cells[preCell.Layer].outgoingSynapses.push(newSynapse);
    this.Model.Areas[postCell.areaIndex].Minicolumns[postCell.X][postCell.Z].Cells[postCell.Layer].incomingSynapses.push(newSynapse);

    //console.log("Synapse will be created");

    //this.model.synapses.push(newSynapse);
    this.Model.Synapses.push(newSynapse);
    //this.create("Synapse Created", "Finished", 'success');
    this.fillChart(this.Model);
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

    this.clearData();
    this.updateOverlapColumn(overlaps);
  }
  updateOverlapColumn(overlaps: any[]) {
    for (var i = 0; i < overlaps.length; i++) {
      for (var j = 0; j < overlaps[i].overlapArray.length; j++) {
        this.Model.Areas[overlaps[i].selectAreaIndex].Minicolumns[overlaps[i].miniColumnXDimension][overlaps[i].miniColumnZDimension].overlap = parseFloat(overlaps[i].overlapArray[j]);
      }
    }
    this.fillChart(this.Model);
    this.generateColoursFromOverlap();
    this.generateColoursFromPermanences();
    this.plotChart();
  }

  // This method is for updating overlap column from userinterface
  updateOverlapV(areaID: any, miniColumnXDimension: any, miniColumnZDimension: any, newOverlapValue: any) {
    let areaIDOfCell = parseInt(areaID);
    let minColXDim = parseInt(miniColumnXDimension);
    let minColZDim = parseInt(miniColumnZDimension);
    let updateOverlapValue = parseFloat(newOverlapValue);
    this.clearData();
    this.Model.Areas[areaIDOfCell].Minicolumns[minColXDim][minColZDim].overlap = updateOverlapValue;
    this.fillChart(this.Model);
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

  showEntireModel() {
    this.clearData();
    this.fillChart(this.Model);
    this.generateColoursFromOverlap();
    this.generateColoursFromPermanences();
    this.plotChart();
  }

}
