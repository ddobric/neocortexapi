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
  neuralChartLayout: any;
  neuralChartConfig: any;

  selectAreaIndex: any = 0;
  miniColumnXDimension: any = 0;
  miniColumnZDimension: any = 0;
  newOverlapValue: any = 0;
  mapData: any;

  constructor(private _service: NotificationsService) {

  }

  ngOnInit() {
  }

  ngAfterViewInit() {
    //this.model = neoCortexUtils.createModel([0, 0, 0, 0, 1, 1, 1, 2, 2, 3], [10, 1], 6);
    this.model = neoCortexUtils.createModel([0, 0, 0, 1, 2, 1], [10, 1], 6); // createModel (numberOfAreas, [xAxis, zAxis], yAxis)
    this.fillChart(this.model);
    // this.model = neoCortexUtils.createModel([0, 0, 0, 0, 1, 1, 1, 2, 2, 3], [10, 1], 6);
    this.generateColoursFromOverlap(this.model);
    this.generateColoursFromPermanences(this.model);
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
    let x: number;
    let y: number;
    let z: number;
    let xFactor = 15;
    let yFactor = 5;
    let areaLength

    for (areaIndx = 0; areaIndx < model.areas.length; areaIndx++) {

      var areaXWidth = env.cellXRatio * model.areas[areaIndx].minicolumns.length + env.areaXOffset;
      var areaZWidth = env.cellZRatio * model.areas[areaIndx].minicolumns[0].length + env.areaZOffset;
      var areaYWidth = env.cellYRatio * model.areas[areaIndx].minicolumns[0][0].cells.length + env.areaYOffset;
      areaLength = model.areas[areaIndx].minicolumns.length * model.areas[areaIndx].minicolumns[0].length * model.areas[areaIndx].minicolumns[0][0].cells.length;
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

    this.synapsesData();

  }

  synapsesData() {

    for (let i = 0; i < this.model.synapses.length; i++) {
      for (let out = 0; out < this.model.synapses[i].preSynaptic.outgoingSynapses.length; out++) {
        for (let ing = 0; ing < this.model.synapses[i].postSynaptic.incomingSynapses.length; ing++) {
          this.permanence.push(this.model.synapses[i].permanence);
          this.permanence.push(this.model.synapses[i].permanence);
          this.permanence.push(this.model.synapses[i].permanence);

          let xPre = this.xCoordinatesAllAreas[this.model.synapses[i].preSynaptic.areaIndex][this.model.synapses[i].preSynaptic.outgoingSynapses[out].preSynaptic.X];
          this.xSynapse.push(xPre);
          let xPost = this.xCoordinatesAllAreas[this.model.synapses[i].postSynaptic.areaIndex][this.model.synapses[i].postSynaptic.incomingSynapses[ing].postSynaptic.X];
          this.xSynapse.push(xPost);
          this.xSynapse.push(null);

          let yPre = this.yCoordinatesAllAreas[this.model.synapses[i].preSynaptic.areaIndex][this.model.synapses[i].preSynaptic.outgoingSynapses[out].preSynaptic.Layer];
          this.ySynapse.push(yPre);
          let yPost = this.yCoordinatesAllAreas[this.model.synapses[i].postSynaptic.areaIndex][this.model.synapses[i].postSynaptic.incomingSynapses[ing].postSynaptic.Layer];
          this.ySynapse.push(yPost);
          this.ySynapse.push(null);

          let zPre = this.zCoordinatesAllAreas[this.model.synapses[i].preSynaptic.areaIndex][this.model.synapses[i].preSynaptic.outgoingSynapses[out].preSynaptic.Z];
          this.zSynapse.push(zPre);
          let zPost = this.zCoordinatesAllAreas[this.model.synapses[i].postSynaptic.areaIndex][this.model.synapses[i].postSynaptic.incomingSynapses[ing].postSynaptic.Z];
          this.zSynapse.push(zPost);
          this.zSynapse.push(null);

        }
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


  //update synapse permanace
  clickFunc2() {
    this.updatePermanenceOfSynaps(
      /*  [
         {
           area: 0,
           preCell:
           {
             cellX: 0,
             cellY: 3,
             cellZ: 0,
           },
           postCell: {
             cellX: 9,
             cellY: 0,
             cellZ: 0,
           },
           permanence: 1
         }
       ] */
      [
        {
          preCellArea: 0,
          postCellArea: 0,
          preCell:
          {
            cellX: 1,
            cellY: 0,
            cellZ: 0,
          },
          postCell: {
            cellX: 1,
            cellY: 1,
            cellZ: 0,
          },
          permanence: 1
        }
      ]
    );

  }

  updatePermanenceOfSynaps(perms: any) {
    console.log(this.permanence, "former");
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


    this.updateSynapsePermanance(perms);
  }
  private updateSynapsePermanance(permancences: any[]) {
    let preCell: Cell;
    let postCell: Cell;
    let perm: any;
    for (let i = 0; i < permancences.length; i++) {
      perm = permancences[i];
      let preMinCol = this.model.areas[perm.preCellArea].minicolumns[perm.preCell.cellX][perm.preCell.cellZ];
      let postMinCol = this.model.areas[perm.postCellArea].minicolumns[perm.postCell.cellX][perm.postCell.cellZ];
      preCell = preMinCol.cells[perm.preCell.cellY];
      postCell = postMinCol.cells[perm.postCell.cellY];

    }
    if (preCell.outgoingSynapses[0] == null && postCell.incomingSynapses[0] == null) {
      console.log("Synapse does not exists, it will be created");
      this.createSynapse(perm.permanence, preCell, postCell);
    }

    else if (preCell.outgoingSynapses[0] != null && postCell.incomingSynapses[0] != null) {
      console.log("Synapse Exists, Permannence will be updated");
      preCell.outgoingSynapses[0].permanence = perm.permanence;
      postCell.incomingSynapses[0].permanence = perm.permanence;
      this.fillChart(this.model);
      this.generateColoursFromOverlap(this.model);
      this.generateColoursFromPermanences(this.model);
      console.log(this.permanence, "after");
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


  }

  createSynapse(permanence: number, preCell: Cell, postCell: Cell) {

    preCell = new Cell(preCell.areaIndex, preCell.X, preCell.Layer, preCell.Z, [null], [null]);
    postCell = new Cell(postCell.areaIndex, postCell.X, postCell.Layer, postCell.Z, [null], [null]);
    let incomingSynapse = new Synapse(permanence, preCell, postCell);
    let outgoingSynapse = new Synapse(permanence, preCell, postCell); 
    
    preCell = new Cell(preCell.areaIndex, preCell.X, preCell.Layer, preCell.Z, [null], [outgoingSynapse]);
    postCell = new Cell(postCell.areaIndex, postCell.X, postCell.Layer, postCell.Z, [incomingSynapse], [null]);

    console.log("Synapse does not exists");
    let synapse: Synapse;
    synapse = new Synapse(permanence, preCell, postCell);
    this.model.synapses.push(synapse);
    this.fillChart(this.model);
    this.generateColoursFromOverlap(this.model);
    /*  this.permanence = [];
     this.synapseColours= []; */
    //this.synapsesData();
    this.generateColoursFromPermanences(this.model);
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


  /*  private updateSynapsePermanance1(permancences: any[]) {
 
     let synapse: Synapse;
     let preCell0: Cell;
     let postCell0: Cell;
     let preCell: Cell;
     let postCell: Cell;
     let synapse1: Synapse;
     for (let k = 0; k < permancences.length; k++) {
       var perm = permancences[k];
 
       let preMinCol = this.model.areas[perm.area].minicolumns[perm.preCell.cellX][perm.preCell.cellZ];
       let postMinCol = this.model.areas[perm.area].minicolumns[perm.postCell.cellX][perm.postCell.cellZ];
 
       preCell = preMinCol.cells[perm.preCell.cellY];
       postCell = postMinCol.cells[perm.postCell.cellY];
 
       preCell0 = new Cell(0, perm.preCell.cellX, perm.preCell.cellY, perm.preCell.cellZ, null, null);
       postCell0 = new Cell(0, perm.postCell.cellX, perm.postCell.cellY, perm.postCell.cellZ, null, null);
 
       synapse = new Synapse(perm.permanence, preCell0, postCell0);
       synapse1 = new Synapse(perm.permanence, preCell, postCell);
 
     }
     this.getSynapse(preCell, postCell);
     this.getSynapse(preCell0, postCell0);
 
     this.generateColoursFromPermanences(this.model);
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
   } */

  /*   private getSynapse(preCell: Cell, postCell: Cell) {
  
      if (preCell.outgoingSynapses == null && postCell.outgoingSynapses == null) {
        console.log("Synapse does not exisits. New Synapse will be Created");
      }
      if (this.model.areas[0].minicolumns[preCell.X][preCell.Z].cells[preCell.Layer] != null) {
  
      }
  
  
    } */

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
    this.generateColoursFromOverlap(this.model);
    this.generateColoursFromPermanences(this.model);
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

  // This method is for updating overlap column from userinterface
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
    this.generateColoursFromPermanences(this.model);
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

  generateColoursFromOverlap(model: NeoCortexModel) {

    for (const overlapVal of this.overlap) {
      let H = (1.0 - overlapVal) * 240;
      this.neuronsColours.push("hsl(" + H + ", 100%, 50%)");
    }

  }

  generateColoursFromPermanences(model: NeoCortexModel) {

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
