import { Component, OnInit } from '@angular/core';
import { HighchartsStatic, HighchartsService } from 'angular2-highcharts/dist/HighchartsService';

declare var require: any;

const Highcharts = require('highcharts');
 
Highcharts.setOptions({
  //colors: ['#50B432'],
  redraw: false,
  
});

@Component({
  selector: 'app-scattered3-dchart',
  templateUrl: './scattered3-dchart.component.html',
  styleUrls: ['./scattered3-dchart.component.css']
})
export class Scattered3DchartComponent implements OnInit {
  chartOpts: any={};
  data:any=[];
  x: any= 9;
  y: any= 30;
  z: any=252;
  n:any = 10000;
  i: any;
  
    me: any;

  sampledata: any =  [
      [0, 0, 0]
    ]; 
  constructor() { 
     
    this.me = this.chartOpts;
  }

  ngOnInit() {

    this.initData(5,5,5);
    this.setOptions(null);   
  
    }

    onInputChange(event): void {
        this.setOptions(event);
    }

    setOptions(event): void {
    this.chartOpts = {
    exporting: { enabled: false },
    credits: { enabled: false },
      chart: {
          
        height: 600,
        zoomType: 'xy',
        margin: 100,
        type: 'scatter',
        animation: false,
        options3d: {
            enabled: true,
            alpha: this.x,
            beta: this.y,
            depth: this.z,
            viewDistance: 5,
            fitToPlot: false,
            frame: {
                bottom: { size: 3, color: 'rgba(0,0,0,0.02)' },
                back: { size: 3, color: 'rgba(0,0,0,0.04)' },
                side: { size: 3, color: 'rgba(0,0,0,0.06)' }
            }
        }
    },
    title: {
        text: '3D'
    },
    subtitle: {
        text: ''
    },
    plotOptions: {
        scatter: {
            width: 10,
            height: 10,
            depth: 10,
            //groupPadding: 40
        }
    },
    yAxis: {
        min: 0,
        max: 10,
        title: null
    },
    xAxis: {
        min: 0,
        max: 10,
        gridLineWidth: 1
    },
    zAxis: {
        min: 0,
        max: 10,
        showFirstLabel: false
    },
    legend: {
        enabled: false
    },
    series: [{
        name: 'Reading',
        colorByPoint: true,
        marker:{
            symbol: 'url(../../../assets/images/cylinder.png)',
            width: 30,
            height: 30
        }
       , 
       data:this.data,
        //data: [
            /*
                [0, 0, 1], [0, 0, 2], [0, 0, 3], [0, 0, 4], [0, 0, 5], [0, 0, 6], [0, 0, 7], [0, 0, 8],
                [1, 1, 1], [1, 1, 2], [1, 1, 3], [1, 1, 4], [1, 1, 5], [1, 1, 6], [1, 1, 7], [1, 1, 8],
                [2, 2, 1], [2, 2, 2], [2, 2, 3], [2, 2, 4], [2, 2, 5], [2, 2, 6], [2, 2, 7], [2, 2, 8],
                [3, 3, 1], [3, 3, 2], [3, 3, 3], [3, 3, 4], [3, 3, 5], [3, 3, 6], [3, 3, 7], [3, 3, 8],
                [4, 4, 1], [4, 4, 2], [4, 4, 3], [4, 4, 4], [4, 4, 5], [4, 4, 6], [4, 4, 7], [4, 4, 8],
                [5, 5, 1], [5, 5, 2], [5, 5, 3], [5, 5, 4], [5, 5, 5], [5, 5, 6], [5, 5, 7], [5, 5, 8],
                [6, 6, 1], [6, 6, 2], [6, 6, 3], [6, 6, 4], [6, 6, 5], [6, 6, 6], [6, 6, 7], [6, 6, 8],
                [7, 7, 1], [7, 7, 2], [7, 7, 3], [7, 7, 4], [7, 7, 5], [7, 7, 6], [7, 7, 7], [7, 7, 8],

                [1, 0, 0], [2, 0, 0], [3, 0, 0], [4, 0, 0], [5, 0, 0], [6, 0, 0], [7, 0, 0], [8, 0, 0],
                [1, 1, 0], [2, 2, 0], [3, 3, 0], [4, 4, 0], [5, 5, 0], [6, 6, 0], [7, 7, 0], [8, 8, 0],
                */
                


           /* [7, 0, 1], [2, 1, 1], [3, 2, 1], [4, 3, 1], [5, 4, 6], [6, 5, 1],
            [1, 0, 2], [2, 1, 2], [3, 2, 2], [4, 3, 2], [5, 4, 2], [6, 5, 2],
            [7, 0, 3], [2, 1, 3], [3, 2, 3], [4, 3, 3], [5, 4, 3], [6, 5, 3],
            [2, 6, 1], [8, 9, 2], [7, 6, 5], [6, 3, 1], [9, 3, 1], [8, 9, 3],
            [9, 1, 0], [3, 8, 7], [8, 0, 0], [4, 9, 7], [8, 6, 2], [4, 3, 0],
            [2, 3, 5], [9, 1, 4], [1, 1, 4], [6, 0, 2], [6, 1, 6], [3, 8, 8],
            [8, 8, 7], [5, 5, 0], [3, 9, 6], [5, 4, 3], [6, 8, 3], [0, 1, 5],
            [6, 7, 3], [8, 3, 2], [3, 8, 3], [2, 1, 6], [4, 6, 7], [8, 9, 9],
            [5, 4, 2], [6, 1, 3], [6, 9, 5], [4, 8, 2], [9, 7, 4], [5, 4, 2],
            [9, 6, 1], [2, 7, 3], [4, 5, 4], [6, 8, 1], [3, 4, 0], [2, 2, 6],
            [5, 1, 2], [9, 9, 7], [6, 9, 9], [8, 4, 3], [4, 1, 7], [6, 2, 5],
            [0, 4, 9], [3, 5, 9], [6, 9, 1], [3, 9, 2]
            */
          // ]
    }]
      
  };
  }
/*
  insertData(){
for (this.i=0; this.i < this.n; this.i += 1) {
    this.sampledata.push([
        Math.pow(Math.random(), 1) * 100,
        Math.pow(Math.random(), 2) * 100,
        Math.pow(Math.random(), 3) * 100
    ]);
this.onInputChange(null);   
  }
}

*/
initData(xDim,zDim,yDim){

    //this.chartOpts.series[0].data = [];

    var x;
    var y;
    var z;
    for (z = 0; z < zDim; z += 1) { 
     for (x = 0; x < xDim; x += 1) {
         for (y = 0; y < yDim; y += 1) {           
             this.data.push([
                 x,
                 y,
                 z
             ]);
             
             //this.onInputChange(null);      
         }
         
     }
     
   }
   //console.log('[\n    [' + this.sampledata.join('],\n    [') + ']\n]');
  
   }



}
