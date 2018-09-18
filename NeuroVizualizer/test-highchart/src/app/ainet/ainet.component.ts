import { Component, OnInit, AfterViewInit } from '@angular/core';
//import * as Plotly from 'plotly.js';
import * as Plotlyjs from 'plotly.js/dist/plotly';
import { neoCortexUtils } from '../neocortexutils';

@Component({
  selector: 'app-ainet',
  templateUrl: './ainet.component.html',
  styleUrls: ['./ainet.component.css']
})
export class AinetComponent implements OnInit, AfterViewInit {


  xCoord = [];
  yCoord = [];
  zCoord = [];
  colour = [];
  opacityValues = [];



  constructor() {

  }
  ngOnInit() {
  }
  ngAfterViewInit() {
    this.fillChart();
    //this.dellInvisiblepoints();
    this.createChart();
  }
  createChart() {
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
      x: this.xCoord,
      y: this.yCoord,
      z: this.zCoord,
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
        color: '#00BFFF',
        symbol: 'circle',
        line: {
          color: '#7B68EE',
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
      x: this.xCoord,
      y: this.yCoord,
      z: this.zCoord,
      opacity: 1.0,
      line: {
        width: 4,
        color: '#7CFC00',
        colorscale: 'Viridis'
      }
    };
    const trace2 = {

      /* x: [0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2],
      y: [0, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2, 0, 0, 0, 1, 2, 1, 2, 1, 2],
      z: [0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2, 0, 1, 2, 0, 0, 1, 1, 2, 2], */
      x: this.xCoord,
      y: this.yCoord,
      z: this.zCoord,

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
      x: this.xCoord,
      y: this.yCoord,
      z: this.zCoord,

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

    let x; let y; let z; let ai;
    for (ai = 0; ai < model.areas.length; ai++) {
       if (ai >= 1) {
        /* x = x*20;
        y = y*2;
        z = z *20; */
        
        /* x = 100 *(this.xCoord[this.xCoord.length-1])
        y = 15 *(this.yCoord[this.yCoord.length-1])
        z = 11 *(this.zCoord[this.zCoord.length-1])
         */
      } 
      for (x = 0; x < model.areas[ai].minicolumns.length; x++) {
        for (y = 0; y < model.areas[ai].minicolumns[x].length; y++) {
          for (z = 0; z < model.areas[ai].minicolumns[x][y].cells.length; z++) {
            this.xCoord.push(model.areas[ai].minicolumns[x][y].cells[z].posX);
            this.yCoord.push(model.areas[ai].minicolumns[x][y].cells[z].posY);
            this.zCoord.push(model.areas[ai].minicolumns[x][y].cells[z].posZ);
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
      }
    }
    console.log(this.xCoord);
  }
  dellInvisiblepoints() {
    let removePointsByIndex = [];
    function getRandomInt(min, max) {
      min = Math.ceil(min);
      max = Math.floor(max);
      return Math.floor(Math.random() * (max - min)) + min;
    }
    for (let i = 0; i < 200; i++) {
      let randomNum = getRandomInt(0, 100);
      removePointsByIndex.push(randomNum);
    }
    for (let j = removePointsByIndex.length - 1; j >= 0; j--) {
      this.xCoord.splice(removePointsByIndex[j], 1);

    }
    for (let k = removePointsByIndex.length - 1; k >= 0; k--) {
      this.yCoord.splice(removePointsByIndex[k], 1);

    }
    for (let l = removePointsByIndex.length - 1; l >= 0; l--) {
      this.zCoord.splice(removePointsByIndex[l], 1);

    }

  }


}
