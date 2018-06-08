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
  //redraw: boolean;
  
    me: any;

  sampledata: any =  [
      [0, 0, 0]
    ]; 
  constructor() { 
     
    this.me = this.chartOpts;
     // this.chartOpts.chart.redraw = false;
  }

  ngOnInit() {

    this.initData(10,10,10);
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
            //groupPadding: 0
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
        redraw: false,
        marker:{
            symbol: 'url(../../../assets/images/cylinder.png)',
            width: 30,
            height: 30
        }
       , 
       data:this.data,
    }]
      
  };
  }
 

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
         }
         
     }
     
   }
  
   }
}


