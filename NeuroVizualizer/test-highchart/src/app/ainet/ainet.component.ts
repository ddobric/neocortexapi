import { Component, OnInit, AfterViewInit } from '@angular/core';
//import * as Plotly from 'plotly.js';
import * as Plotlyjs from 'plotly.js/dist/plotly';
import { neoCortexUtils } from '../neocortexutils';
import { color } from 'd3';
import { environment as env } from "../environments/environment";


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
    console.log(xCoordinates, "X");
    console.log(yCoordinates, "Y");
    console.log(zCoordinates, "Z");

    let colourArray = this.getHeatColor();
    let cellColours = colourArray[0];
    let weights = colourArray[1];
    //colourArray1.splice(-1, 1);

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
      text: weights,
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
        opacity: 1,
        size: 18,
        // color: '#00BFFF',
        color: cellColours,
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
      x: xCoordinates,
      y: yCoordinates,
      z: zCoordinates,
      text: weights,
      opacity: 1.0,
      line: {
        width: 4,
        color: cellColours,

        //color: '#7CFC00'
        //colorscale: 'Viridis'
      }
    };
    console.log("second");
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
          x: env.xRatio, y: env.yRatio, z: env.zRatio,
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
    let model = neoCortexUtils.createModel(2, [100, 6], 10); // createModel (numberOfAreas, [xAxis, zAxis], yAxis)
    // this.opacityValues = new Array(areaSection).fill(0.5, 0, 1200).fill(1.8, 1200, 2400);
    //this.colour = new Array(areaSection).fill('#00BFFF', 0, 800).fill('#48afd1', 800, 1600).fill('#236d86', 1600, 2499);
    let xCoord = [];
    let yCoord = [];
    let zCoord = [];
    let numOfAreas = model.areas;
    let ai;
    for (ai = 0; ai < model.areas.length; ai++) {
      for (let i = 0; i < model.areas[ai].minicolumns[0].length; i++) {
        xCoord.push(model.areas[ai].minicolumns[0][i].posX);
        yCoord.push(model.areas[ai].minicolumns[0][i].posY);
        zCoord.push(model.areas[ai].minicolumns[0][i].posZ);
      }
    }

    /*         for (x = 0; x < model.areas[ai].minicolumns.length; x++) {
          for (y = 0; y < model.areas[ai].minicolumns[x].length; y++) {
            for (z = 0; z < model.areas[ai].minicolumns[x][y].cells.length; z++) {
               xCoord.push(model.areas[ai].minicolumns[x][y].cells[z].posX);
              yCoord.push(model.areas[ai].minicolumns[x][y].cells[z].posY);
              zCoord.push(model.areas[ai].minicolumns[x][y].cells[z].posZ); 
            
 
            }
          }
        } */
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
    /* x = 100 +(this.xCoord[this.xCoord.length-1])
    y = 15 *(this.yCoord[this.yCoord.length-1])
    z = 11 *(this.zCoord[this.zCoord.length-1]) */
    return [xCoord, yCoord, zCoord, numOfAreas];

  }

  getHeatColor() {

    let colourScheme = [];
    let colourCodingSegment = [];
    let colourCoding = [];
    let weights = [];
    let neuronsWeightSegment = [];
    let allNeuronsWeight = [];

    let getCoordLength = this.fillChart();
    let xCoordLen = getCoordLength[0].length;
    let totalAreas = getCoordLength[3].length;

    /*   for (let neuronWeight = 0; neuronWeight < 1; neuronWeight = neuronWeight + (1 / (xCoordLen / totalAreas)) ) {
        let H = (1.0 - neuronWeight) * 240;
            colourScheme.push("hsl(" + H + ", 100%, 50%)")
      }
      for (let hsl = 0; hsl < (xCoordLen / colourScheme.length); hsl++) {
        for (let colourCode = 0; colourCode < colourScheme.length; colourCode++) {
          colourCoding.push(colourScheme[colourCode]);
  
        }
      } */
    for (let nW = 0; nW < 1; nW = nW + (1 / (env.numberOfColours))) {
      let H = (1.0 - nW) * 240;

      colourScheme = Array((xCoordLen / totalAreas) / (env.numberOfColours)).fill("hsl(" + H + ", 100%, 50%)");
      weights = Array((xCoordLen / totalAreas) / (env.numberOfColours)).fill(nW);

      for (let hsl = 0; hsl < (colourScheme.length); hsl++) {
        colourCodingSegment.push(colourScheme[hsl]);

      }

      for (let w = 0; w < (weights.length); w++) {
       // allNeuronsWeight.push(neuronsWeightSegment[w]);
        neuronsWeightSegment.push(weights[w]);
      }
    }

    for (let i = 0; i < totalAreas; i++) {
      for (let j = 0; j < colourCodingSegment.length; j++) {
        colourCoding.push(colourCodingSegment[j]);
      }

      for (let k = 0; k < totalAreas; k++) {
        for (let l = 0; l < neuronsWeightSegment.length; l++) {
          allNeuronsWeight.push(neuronsWeightSegment[l]);
        }

      }


    }
    console.log("neuronsWeightArray", allNeuronsWeight);

    console.log("ColorCodingSegment", colourCodingSegment);
    console.log("ColorCoding", colourCoding);
    /*  let neuronsWeight = [0, 0.25, 0.5, 0.75, 1];
 
     let zeroWeightNeurons = ((xCoordLen / totalAreas) / 100) * 23;
     let quarterWeightNeurons = ((xCoordLen / totalAreas) / 100) * 8;
     let halfWeightNeurons = ((xCoordLen / totalAreas) / 100) * 14;
     let quarterThirdWeightNeurons = ((xCoordLen / totalAreas) / 100) * 24;
     let oneWeightNeurons = ((xCoordLen / totalAreas) / 100) * 28; */



    /* let segmentA = Math.round(zeroWeightNeurons / (240 / 5));
    let segmentB = Math.round(quarterWeightNeurons / (240 / 5));;
    let segmentC = Math.round(halfWeightNeurons / (240 / 5));
    let segmentD = Math.round(quarterThirdWeightNeurons / (240 / 5));
    let segmentE = Math.round(oneWeightNeurons / (240 / 5)); */
    /* 
        let segmentA = zeroWeightNeurons / (240 / 5);
        let segA = segmentA.toFixed(0);
        let segmentB = quarterWeightNeurons / (240 / 5);
        let segB = segmentB.toFixed(0);
        let segmentC = halfWeightNeurons / (240 / 5);
        let segC = segmentC.toFixed(0);
        //parseInt(segmentC);
        let segmentD = quarterThirdWeightNeurons / (240 / 5);
        let segD = segmentD.toFixed(0);
        let segmentE = oneWeightNeurons / (240 / 5);
        let segE = segmentE.toFixed(0); 
    
        
        let segmentWiseColors = [];
       
    
        for (let a = 0; a < 0.20; a = a + (1 / 240)) {
          let newH = (1.0 - a) * 240;
          segmentWiseColors.push("hsl(" + newH + ", 100%, 50%)")
          segmentWiseColors = Array(parseInt(segA)).fill("hsl(" + newH + ", 100%, 50%)");
          for (let b = 0; b < segmentWiseColors.length; b++) {
            allSegmentColors.push(segmentWiseColors[b]);
          }
        }
        for (let ab = 0.20; ab < 0.40; ab = ab + (1 / 240)) {
          let newH = (1.0 - ab) * 240;
          segmentWiseColors = Array(parseInt(segB)).fill("hsl(" + newH + ", 100%, 50%)");
          for (let bc = 0; bc < segmentWiseColors.length; bc++) {
            allSegmentColors.push(segmentWiseColors[bc]);
          }
        }
        for (let c = 0.40; c < 0.60; c = c + (1 / 240)) {
          let newH = (1.0 - c) * 240;
          segmentWiseColors = Array(parseInt(segC)).fill("hsl(" + newH + ", 100%, 50%)");
          for (let d = 0; d < segmentWiseColors.length; d++) {
            allSegmentColors.push(segmentWiseColors[d]);
          }
        }
        for (let e = 0.60; e < 0.80; e = e + (1 / 240)) {
          let newH = (1.0 - e) * 240;
          segmentWiseColors = Array(parseInt(segD)).fill("hsl(" + newH + ", 100%, 50%)");
          for (let f = 0; f < segmentWiseColors.length; f++) {
            allSegmentColors.push(segmentWiseColors[f]);
          }
        }
        for (let g = 0.80; g < 1; g = g + (1 / 240)) {
          let newH = (1.0 - g) * 240;
          segmentWiseColors = Array(parseInt(segE)).fill("hsl(" + newH + ", 100%, 50%)");
          for (let hi = 0; hi < segmentWiseColors.length; hi++) {
            allSegmentColors.push(segmentWiseColors[hi]);
          }
        } 
        console.log("segmentWiseColors", segmentWiseColors);
        */


    /*  neuronsWeight.forEach(weight => {
       let h = (1.0 - weight) * 240;
       if (h == 240) {
         // colour =  Array(zeroWeightNeurons).fill("hsl(" + h + ", 100%, 50%)");
         for (let i = 0; i < zeroWeightNeurons; i++) {
           colourCoding.push("hsl(" + h + ", 100%, 50%)")
 
         }
       }
       if (h == 180) {
         for (let j = 0; j < quarterWeightNeurons; j++) {
           colourCoding.push("hsl(" + h + ", 100%, 50%)")
 
         }
       }
       if (h == 120) {
         for (let k = 0; k < halfWeightNeurons; k++) {
           colourCoding.push("hsl(" + h + ", 100%, 50%)")
 
         }
       }
       if (h == 60) {
         for (let l = 0; l < quarterThirdWeightNeurons; l++) {
           colourCoding.push("hsl(" + h + ", 100%, 50%)")
 
         }
       }
       if (h == 0) {
         for (let m = 0; m < oneWeightNeurons; m++) {
           colourCoding.push("hsl(" + h + ", 100%, 50%)")
 
         }
       }
     }); */



    /*  for (let l = 0; l < (xCoordLen / (colourValues.length + colourValues.length)); l++) {
       for (let m = 0; m < colourValues.length; m++) {
         synapseColour.push(colourValues[m], colourValues[m]);
       }
     } */

    /*  for (let neuronWeight = 1; neuronWeight > 0; neuronWeight = neuronWeight - (1 / (xCoordLen / totalAreas)) ) {
      let H = (1.0 - neuronWeight) * 240;
          colourSchemeA.push("hsl(" + H + ", 100%, 50%)")
    }
    for (let hsl = 0; hsl < (xCoordLen / colourSchemeA.length); hsl++) {
      for (let colourCode = 0; colourCode < colourSchemeA.length; colourCode++) {
        colourCodingA.push(colourSchemeA[colourCode]);

      }
    } */

    return [colourCoding, allNeuronsWeight, colourCodingSegment,];
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
