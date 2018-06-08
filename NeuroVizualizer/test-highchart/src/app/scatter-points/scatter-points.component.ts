import { Component, OnInit } from '@angular/core';

declare var require: any;

const Highcharts = require('highcharts');

@Component({
  selector: 'app-scatter-points',
  templateUrl: './scatter-points.component.html',
  styleUrls: ['./scatter-points.component.css']
})
export class ScatterPointsComponent implements OnInit {

  constructor() { 
    var container = document.createElement('chart');
    document.body.appendChild(container);

    var data1 = [[]],
    n = 100000,
    i;
for (i = 0; i < n; i += 1) {
    data1.push([
        Math.pow(Math.random(), 2) * 100,
        Math.pow(Math.random(), 2) * 100,
        Math.pow(Math.random(), 2) * 100
    ]);
}

    var chart = new Highcharts.Chart({
      exporting: { enabled: false },
      credits: { enabled: false },

    chart: {
      renderTo: container,
       // margin: 100,
        type: 'scatter',
        zoomType: 'xy',
        height: '110%',
        
        animation: false,
        options3d: {
            enabled: true,
            alpha: 10,
            beta: 30,
            depth: 250,
            viewDistance: 5,
            fitToPlot: false,
            
            frame: {
                bottom: { size: 1, color: 'rgba(0,0,0,0.02)' },
                back: { size: 1, color: 'rgba(0,0,0,0.04)' },
                side: { size: 1, color: 'rgba(0,0,0,0.06)' }
            }
        }
    },
    boost: {
      useGPUTranslations: true,
      usePreAllocated: true
  },
    title: {
        text: '3d Chart'
    },
    subtitle: {
        text: ''
    },
    plotOptions: {
        scatter: {
            width: 30,
            height: 20,
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
        //colorByPoint: true,
        color: 'rgba(243, 21, 117, 0.1)',
        data: data1,
        marker: {
          radius: 3
      },
      
      tooltip: {
        followPointer: false,
        pointFormat: '[{point.x:.1f}, {point.y:.1f}, {point.y:.1f}]'
    }
    }]
});



  }

  ngOnInit() {
  }

}
