import { Component, OnInit, AfterViewInit } from '@angular/core';
//import * as Plotly from 'plotly.js';
import * as Plotlyjs from 'plotly.js/dist/plotly';
import { neoCortexUtils } from '../neocortexutils';
import { color, area } from 'd3';
import { environment as env } from "../environments/environment";
import { NotificationsService } from 'angular2-notifications';
import { NeoCortexModel, Area, Synapse, Minicolumn, Cell, NeocortexSettings, InputModel, CellId } from '../neocortexmodel';
import { last } from '@angular/router/src/utils/collection';
import { modelGroupProvider } from '@angular/forms/src/directives/ng_model_group';


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

  weightGivenByUser: string;
  error: string;
  neuralChartLayout: any;
  neuralChartConfig: any;

  selectAreaIndex: any = 0;
  miniColumnXDimension: any = 0;
  miniColumnZDimension: any = 0;
  newOverlapValue: any = 0;

  constructor(private _service: NotificationsService) {

  }

  ngOnInit() {
  }

  ngAfterViewInit() {

    this.model = neoCortexUtils.createModel([0, 0, 0, 0, 1, 1, 1, 2, 2, 3], [10, 1], 6); // createModel (numberOfAreas, [xAxis, zAxis], yAxis)
    this.fillChart(this.model);
    // this.model = neoCortexUtils.createModel([0, 0, 0, 0, 1, 1, 1, 2, 2, 3], [10, 1], 6);
    this.generateColoursFromOverlap(this.model);
    this.generateColoursForSynPermanences(this.model);
    this.createChart();


  }

  createChart() {

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
      height: 500,
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
      },
    };

    this.neuralChartConfig = {
      //displayModeBar: false,
      title: '3DChart',
      displaylogo: false,
      showLink: false,
      responsive: true
      // showlegend: false
    };

    let graphDOM = document.getElementById('graph');
    Plotlyjs.newPlot(graphDOM, [neurons, synapses], this.neuralChartLayout, this.neuralChartConfig);
    //Plotlyjs.newPlot(graphDOM, [PointsT, linesT], neuralChartLayout);
    // Plotlyjs.newPlot(graphDOM, [test1, test2]);
    //Plotlyjs.restyle(gd,  update, [0]);
  }



  fillChart(model: NeoCortexModel) {

    let areaIndx: any;
    let lastLevel = 0;
    let levelCnt = 0;
    let xOffset = 0;

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


      for (let i = 0; i < model.areas[areaIndx].minicolumns.length; i++) {
        for (let j = 0; j < model.areas[areaIndx].minicolumns[i].length; j++) {
          for (let cellIndx = 0; cellIndx < model.areas[areaIndx].minicolumns[i][j].cells.length; cellIndx++) {
            this.overlap.push(model.areas[areaIndx].minicolumns[i][j].overlap);

            // this.permanence.push(model.areas[areaIndx].minicolumns[i][j].cells[cellIndx].preSynapses[cellIndx].permanence);
            this.xNeurons.push(i * env.cellXRatio + xOffset);
            this.yNeurons.push(areaYWidth * model.areas[areaIndx].level + cellIndx * env.cellYRatio);
            this.zNeurons.push(areaZWidth * j);

          }
        }
      }
    }
  
    for (let dummyValue = 0; dummyValue < model.synapses.length; dummyValue++) {
      this.permanence.push(null);
    }
    for (let readPerma = 0; readPerma < model.synapses.length; readPerma++) {
      let index = model.synapses[readPerma].preSynaptic.Y + model.synapses[readPerma].preSynaptic.Z;
/* 
      this.permanence.splice(index, 0, model.synapses[readPerma].permanence);
      this.permanence.splice(index+1, 0, model.synapses[readPerma].permanence); */
      this.permanence[index] = model.synapses[readPerma].permanence;
      this.permanence[index+1] = model.synapses[readPerma].permanence;
      
    
      this.xSynapse.push(model.synapses[readPerma].preSynaptic.X);
      this.xSynapse.push(model.synapses[readPerma].postSynaptic.X);
      this.xSynapse.push(null);

      this.ySynapse.push(model.synapses[readPerma].preSynaptic.Y);
      this.ySynapse.push(model.synapses[readPerma].postSynaptic.Y);
      this.ySynapse.push(null);

      this.zSynapse.push(model.synapses[readPerma].preSynaptic.Z);
      this.zSynapse.push(model.synapses[readPerma].postSynaptic.Z);
      this.zSynapse.push(null);
    }
    console.log(this.permanence);
    /*  console.log(this.overlap, "overlap Array");
     console.log(this.permanence, "permanence"); */


    /*  console.log(this.xSynapse, "X Synaps");
     console.log(this.ySynapse, "y Synaps");
     console.log(this.zSynapse, "z Synaps");
 
 
 
     
         console.log(this.xNeurons, "X Neurons");
         console.log(this.yNeurons, "Y Neurons");
         console.log(this.zNeurons, "Z Neurons");  */
    //console.log(this.permanence, "permanence");
  }

  generateSynapses() {


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
  clickFunc() {
    // this.updateOverlapCell(0, 0, 0, [0.5, 0.7, 1, 0.75, 0.4, 1]);
    this.updatePermanenceOfSynaps(
      [
        {
          area: 0,
          preCell:
          {
            cellX: 13,
            cellY: 0,
            cellZ: 0,
          },
          postCell: {
            cellX: 3,
            cellY: 7,
            cellZ: 0,
          },
          permanence: 1
        }
      ]
    );
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

    this.update(overlaps, null, null);
  }

  updatePermanenceOfSynaps(perms: any) {
     console.log(this.permanence, "former");
    /*  this.xNeurons = [];
     this.yNeurons = [];
     this.zNeurons = [];
     this.overlap = [];
     this.neuronsColours = []; */

    /*  this.xSynapse = [];
     this.ySynapse = [];
     this.zSynapse = []; */
   // this.permanence = [];
    this.synapseColours = [];


    this.update(null, perms, null);
  }

  private getSynapse(preCell: Cell, postCell: Cell) {
    let synapseIndex: number = 0;

    var cellMinColl = this.model.areas[preCell.id.area].minicolumns[preCell.X][preCell.Y];
   // cellMinColl.cells[preCell.Z].outgoingSynapces


    for (let synapIndex = 0; synapIndex < this.model.synapses.length; synapIndex++) {

  

        // console.log("True", synapIndex);
        synapseIndex = synapseIndex + synapIndex;


      }
    
  }


  private update(overlaps: any[], permancences: any[], activeCells: any[]) {
    /* for (var i = 0; i < overlaps.length; i++) {
      for (var j = 0; j < overlaps[i].overlapArray.length; j++) {
        this.model.areas[overlaps[i].selectAreaIndex].minicolumns[overlaps[i].miniColumnXDimension][overlaps[i].miniColumnZDimension].overlap = parseFloat(overlaps[i].overlapArray[j]);
      }
    } */

    let synapse: Synapse;
    let preCell0: Cell;
    let postCell0: Cell;
    for (let k = 0; k < permancences.length; k++) {
      var perm = permancences[k];

      /*  let preMinCol = this.model.areas[perm.area].minicolumns[perm.preCell.cellX][perm.preCell.cellZ];
       let postMinCol = this.model.areas[perm.area].minicolumns[perm.postCell.cellX][perm.postCell.cellZ];
 
       let preCell = preMinCol.cells[perm.preCell.cellY];
       let postCell = postMinCol.cells[perm.postCell.cellY];
  */
      preCell0 = new Cell(null, null, null, null, perm.preCell.cellX, perm.preCell.cellY, perm.preCell.cellZ);
      postCell0 = new Cell(null, null, null, null, perm.postCell.cellX, perm.postCell.cellY, perm.postCell.cellZ);
      /* 
            console.log(preCell0, "pre");
            console.log(postCell0, "post"); */
      synapse = new Synapse(null, perm.permanence, preCell0, postCell0);
    }

    let synapseIndex: number = 0;
    for (let synapIndex = 0; synapIndex < this.model.synapses.length; synapIndex++) {

      if (this.model.synapses[synapIndex].preSynaptic.X == preCell0.X &&
        this.model.synapses[synapIndex].postSynaptic.X == postCell0.X &&
        this.model.synapses[synapIndex].preSynaptic.Y == preCell0.Y &&
        this.model.synapses[synapIndex].postSynaptic.Y == postCell0.Y &&
        this.model.synapses[synapIndex].preSynaptic.Z == preCell0.Z &&
        this.model.synapses[synapIndex].postSynaptic.Z == postCell0.Z
      ) {

        // console.log("True", synapIndex);
        synapseIndex = synapseIndex + synapIndex;


      }

    }

    this.model.synapses[synapseIndex] = synapse;
    console.log(this.permanence, "ist");

    for (let readPerma = 0; readPerma < this.model.synapses.length; readPerma++) {
      let index = this.model.synapses[readPerma].preSynaptic.Y + this.model.synapses[readPerma].preSynaptic.Z;

      /* this.permanence.splice(index, 0, this.model.synapses[readPerma].permanence);
      this.permanence.splice(index+1, 0, this.model.synapses[readPerma].permanence); */

      this.permanence[index] = this.model.synapses[readPerma].permanence;
      this.permanence[index+1] = this.model.synapses[readPerma].permanence;
      //this.permanence.push(null);

    }
    //this.fillChart(this.model);

    //this.model.synapses.splice(3, 1, synapse);
    // find synapse in model.synapses (push/update)

    //console.log(this.permanence, "Be");


    //this.generateColoursFromOverlap(this.model);
    this.generateColoursForSynPermanences(this.model);
    console.log(this.permanence, "last");
    const updateNeurons = {
      x: this.xNeurons,
      y: this.yNeurons,
      z: this.zNeurons,
      // text: this.overlap,
      name: 'Neuron',
      mode: 'markers',
      marker: {
        opacity: env.opacityOfNeuron,
        size: env.sizeOfNeuron,
        color: this.neuronsColours,
        symbol: 'circle',
      },
      type: 'scatter3d',
    };

    const updateSynapses = {
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
      }
    };

    let graphDOM = document.getElementById('graph');

    Plotlyjs.newPlot(graphDOM, [updateNeurons, updateSynapses], this.neuralChartLayout, this.neuralChartConfig);
  }

  updateOverlapV(selectAreaIndex: any, miniColumnXDimension: any, miniColumnZDimension: any, newOverlapValue: any) {
    this.selectAreaIndex = selectAreaIndex;
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

    this.model.areas[this.selectAreaIndex].minicolumns[this.miniColumnXDimension][this.miniColumnZDimension].overlap = parseFloat(this.newOverlapValue);

    this.fillChart(this.model);
    this.generateColoursFromOverlap(this.model);
    this.generateColoursForSynPermanences(this.model);
    const updateNeurons = {
      x: this.xNeurons,
      y: this.yNeurons,
      z: this.zNeurons,
      // text: this.overlap,
      name: 'Neuron',
      mode: 'markers',
      marker: {
        opacity: env.opacityOfNeuron,
        size: env.sizeOfNeuron,
        color: this.neuronsColours,
        symbol: 'circle',
      },
      type: 'scatter3d',
    };

    const updateSynapses = {
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
      }
    };

    let graphDOM = document.getElementById('graph');

    Plotlyjs.newPlot(graphDOM, [updateNeurons, updateSynapses], this.neuralChartLayout, this.neuralChartConfig);
    // Plotlyjs.restyle(graphDOM, updateNeurons, this.neuralChartLayout, this.neuralChartConfig);
  }

  updatePermanenceV() {

  }

  generateColoursFromOverlap(model: NeoCortexModel) {

    for (const overlapVal of this.overlap) {
      let H = (1.0 - overlapVal) * 240;
      this.neuronsColours.push("hsl(" + H + ", 100%, 50%)");
    }

  }

  generateColoursForSynPermanences(model: NeoCortexModel) {

    for (const permanenceVal of this.permanence) {
      let H = (1.0 - permanenceVal) * 240;
      this.synapseColours.push("hsl(" + H + ", 100%, 50%)");
    }
    /*  for (let permanenceValue = 0; permanenceValue < this.xSynapse.length; permanenceValue++) {
       let H = (1.0 - this.permanence[permanenceValue]) * 240;
       this.synapseColours.push("hsl(" + H + ", 100%, 50%)");
     } */

  }
}
