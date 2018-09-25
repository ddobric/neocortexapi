import { Component, OnInit, AfterViewInit } from '@angular/core';
//import * as Plotly from 'plotly.js';
import * as Plotlyjs from 'plotly.js/dist/plotly';
import { neoCortexUtils } from '../neocortexutils';
import { color } from 'd3';

@Component({
  selector: 'app-ainet',
  templateUrl: './ainet.component.html',
  styleUrls: ['./ainet.component.css']
})
export class AinetComponent implements OnInit, AfterViewInit {

  colour = [];
  constructor() {

  }
  ngOnInit() {
  }
  ngAfterViewInit() {

    // model = createModel();
    // crateChart(model);

    // this.fillChart();
    // this.gradient('#E92517', '#2AEE44', 100);
    //this.dellInvisiblepoints();
    //this.createChartinternal(getCord(), getColors());
    this.createChart();
    //this.showNeuronsByWeight(null);
  }
  //createChart(cords:any[][], color:any[]) {
  // createChart() {
  createChart() {
    let getCoordinates = this.fillChart();
    let xCoordinates = getCoordinates[0];
    let yCoordinates = getCoordinates[1];
    let zCoordinates = getCoordinates[2];
    console.log(xCoordinates,"X");

    let colourArray = this.getHeatColor();

    //let graph = document.getElementById('graph');
    // to make the chart responsive 
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

    const neurons = {
      x: xCoordinates,
      y: yCoordinates,
      z: zCoordinates,
      name: 'Artificial neural network',
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
        opacity: 10,
        size: 18,
        // color: '#00BFFF',
        color: colourArray,
        symbol: 'circle',
        line: {
          //color: '#7B68EE',
          // color: '#7B68EE',
          width: 2
        },

      },
      type: 'scatter3d',
      //scene: "scene1",

    };
    const synapses = {
      type: 'scatter3d',
      mode: 'lines',
      name: 'Synapses',
      x: xCoordinates,
      y: yCoordinates,
      z: zCoordinates,
      opacity: 1.0,
      line: {
        width: 4,
        color: colourArray,
        //color: '#7CFC00'
        //colorscale: 'Viridis'
      }
    };
    const trace2 = {

      /* x: [0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2],
      y: [0, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2],
      z: [0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2], */
      /*   x: this.xCoord,
        y: this.yCoord,
        z: this.zCoord, */

      name: 'Data 2',
      mode: 'lines+markers',
      symbol: 'circle',
      line: {
        width: 4,
        color: '#7CFC00'
      },
      marker: {
        size: 18,
        color: '#00BFFF',
        symbol: 'circle',
        line: {
          color: '#7B68EE',
          width: 2
        },
        opacity: 10
      },
      type: 'scatter3d',
      scene: "scene2",

    };
    const trace3 = {
      /*       x: [0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2],
            y: [0, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2],
            z: [0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2], */
      /*     x: this.xCoord,
          y: this.yCoord,
          z: this.zCoord, */

      name: 'Data 3',
      mode: 'lines+markers',
      symbol: 'circle',
      line: {
        width: 4,
        color: '#7CFC00'
      },
      marker: {
        size: 18,
        color: '#00BFFF',
        symbol: 'circle',
        line: {
          color: '#7B68EE',
          width: 2
        },
        opacity: 10
      },
      type: 'scatter3d',
      scene: "scene3",

    };
    const neuralChartLayout = {
      //showlegend: false, Thgis option is to show the name of legend/DataSeries 
      scene: {
        aspectmode: "manual",
        aspectratio: {
          x: 7, y: 1, z: 0.5,
        }
      },

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

      }
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

    Plotlyjs.newPlot(graphDOM, [neurons, synapses], neuralChartLayout, neuralChartConfig);
    //Plotlyjs.restyle(gd,  update, [0]);

    window.onresize = function () {
      Plotlyjs.Plots.resize(graphDOM);
    };
  }
  fillChart() {
    let model = neoCortexUtils.createModel(4, [100, 4], 6); // createModel (numberOfAreas, [xAxis, zAxis], yAxis)
    // this.opacityValues = new Array(areaSection).fill(0.5, 0, 1200).fill(1.8, 1200, 2400);
    //this.colour = new Array(areaSection).fill('#00BFFF', 0, 800).fill('#48afd1', 800, 1600).fill('#236d86', 1600, 2499);
    let xCoord = [];
    let yCoord = [];
    let zCoord = [];
    let x; let y; let z; let ai;
    for (ai = 0; ai < model.areas.length; ai++) {
      for (x = 0; x < model.areas[ai].minicolumns.length; x++) {
        for (y = 0; y < model.areas[ai].minicolumns[x].length; y++) {
          for (z = 0; z < model.areas[ai].minicolumns[x][y].cells.length; z++) {
            xCoord.push(model.areas[ai].minicolumns[x][y].cells[z].posX);
            yCoord.push(model.areas[ai].minicolumns[x][y].cells[z].posY);
            zCoord.push(model.areas[ai].minicolumns[x][y].cells[z].posZ);
          }
        }
      }

    }




    /*
        for (x = 0; x < model.areas[ai].minicolumns[0]; x++) {
          for (y = 0; y < model.settings.numLayers; y++) {
            for (z = 0; z < model.settings.minicolumnDims[1]; z++) {
    
              this.xCoord.push(x);
              this.yCoord.push(y);
              this.zCoord.push(z);
    
            }
    
          }
    
        }
        */

    //if (ai >= 0) {
    /*  x = x*20;
     y = y*2;
     z = z *20; */

    /* x = 100 +(this.xCoord[this.xCoord.length-1])
    y = 15 *(this.yCoord[this.yCoord.length-1])
    z = 11 *(this.zCoord[this.zCoord.length-1]) */

    // } 
    return [xCoord, yCoord, zCoord];

  }

  getHeatColor() {
    let colour = [];
    let colourValues0 = [];
    let colourValues1 = [];
    let getCoordLength = this.fillChart();
    let xCoordLen = getCoordLength[0].length;
    for (let i = 0.1; i < 1; i += 0.01) {
      let colorWeight = parseFloat(i.toFixed(1));
      let h = (1.0 - colorWeight) * 240;
      colourValues0.push("hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)");
      colourValues0.push("hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)");
      colourValues0.push("hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)");
      colourValues0.push("hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)");
      colourValues0.push("hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)");
      colourValues0.push("hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)");
      colourValues0.push("hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)");
      colourValues0.push("hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)");
      colourValues0.push("hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)");
      colourValues0.push("hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)","hsl(" + h + ", 100%, 50%)");
      //colourValues0 = colourValues1.concat(colourValues1, colourValues1);
    }

    for (let j = 0; j < (xCoordLen / colourValues0.length); j++) {
      for (let k = 0; k < colourValues0.length; k++) {
        colour.push(colourValues0[k] );
      }
    }
    console.log(colourValues1, 'colourValues1');
    console.log(colourValues0, 'colourValues0');
    console.log(colour);
    return colour;
  }

  gradient(startColor, endColor, steps) {
    var start = {
      'Hex': startColor,
      'R': parseInt(startColor.slice(1, 3), 16),
      'G': parseInt(startColor.slice(3, 5), 16),
      'B': parseInt(startColor.slice(5, 7), 16)
    }
    var end = {
      'Hex': endColor,
      'R': parseInt(endColor.slice(1, 3), 16),
      'G': parseInt(endColor.slice(3, 5), 16),
      'B': parseInt(endColor.slice(5, 7), 16)
    }
    let diffR = end['R'] - start['R'];
    let diffG = end['G'] - start['G'];
    let diffB = end['B'] - start['B'];

    let stepsHex = new Array();
    let stepsR = new Array();
    let stepsG = new Array();
    let stepsB = new Array();

    for (var i = 0; i <= steps; i++) {
      stepsR[i] = start['R'] + ((diffR / steps) * i);
      stepsG[i] = start['G'] + ((diffG / steps) * i);
      stepsB[i] = start['B'] + ((diffB / steps) * i);
      stepsHex[i] = '#' + Math.round(stepsR[i]).toString(16) + '' + Math.round(stepsG[i]).toString(16) + '' + Math.round(stepsB[i]).toString(16);
    }
    // this.col = stepsHex;
    /* for (let index = 0; index < (this.xCoord.length/100) ; index++) {
      for (let hexCol = 0; hexCol < stepsHex.length; hexCol++) {
        this.colour.push(stepsHex[hexCol]);
      } */

  }
  dellInvisiblepoints() {
    let removePointsByIndex = [];
    function getRandomInt(min, max) {
      min = Math.ceil(min);/*  */
      max = Math.floor(max);
      return Math.floor(Math.random() * (max - min)) + min;
    }
    for (let i = 0; i < 200; i++) {
      let randomNum = getRandomInt(0, 100);
      removePointsByIndex.push(randomNum);
    }
    for (let j = removePointsByIndex.length - 1; j >= 0; j--) {
      //this.xCoord.splice(removePointsByIndex[j], 1);

    }
    for (let k = removePointsByIndex.length - 1; k >= 0; k--) {
      //this.yCoord.splice(removePointsByIndex[k], 1);

    }
    for (let l = removePointsByIndex.length - 1; l >= 0; l--) {
      // this.zCoord.splice(removePointsByIndex[l], 1);

    }

  }

  showNeuronsByWeight(weightInput) {

    let neuronWeight = parseInt(weightInput);

    if (neuronWeight == 1) {

    }
    else if (neuronWeight == 2) {

    }
    else {
      throw "Invalid Input";

    }
  }


}
