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
  options: any;
  x: any= 9;
  y: any= 30;
  z: any=252; 
  constructor() { 
      
    this.onInputChange(null);
  }
  onInputChange(event): void {
    this.options = {
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
            depth: 10
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
            symbol: 'url(../../../assets/images/image1.png)',
        },
        data: [
            [7, 0, 1], [2, 1, 1], [3, 2, 1], [4, 3, 1], [5, 4, 6], [6, 5, 1],
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
           ]
    }]
      
  };
  }

  ngOnInit() {
  }
 

}
