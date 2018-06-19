import { Component, OnInit } from '@angular/core';
import { Container } from '@angular/compiler/src/i18n/i18n_ast';
import { Router} from '@angular/router';

declare var require: any;
const Highcharts = require('highcharts');
Highcharts.setOptions({
    colors: ['lightblue'],
    redraw: false
    
  });

@Component({
  selector: 'app-zoom-graph',
  templateUrl: './zoom-graph.component.html',
  styleUrls: ['./zoom-graph.component.css']
})


export class ZoomGraphComponent implements OnInit {
  x: any;
  y: any;
  z: any;
  msg: any ="test";
  public location = '' ;
  fn:any;
  newData:any=[];

  constructor(private router: Router) {
    this.location= router.url; 

    let container = document.createElement('chart');
    document.activeElement.appendChild(container);

    let chart = new Highcharts.Chart({
      exporting: { enabled: false },
      credits: { enabled: false },
    
        chart: {
            // Edit chart spacing
            spacingBottom: 15,
            spacingTop: 10,
            spacingLeft: 10,
            spacingRight: 10,
            
            // Explicitly tell the width and height of a chart
            width: null,
            height: null,
    
      renderTo: container,
       // margin: 50,
        
        type: 'scatter',
        //zoomType: 'xy',
       // height: '110%',
        animation: false,
        options3d: {
            enabled: true,
            alpha: 10,
            beta: 30,
            depth: 250,
            viewDistance: 5,
            fitToPlot: false,
            
            frame: {
                bottom: { size: 2, color: 'rgba(0,0,0,0.02)' },
                back: { size: 15, color: 'rgba(0,0,0,0.04)' },
                side: { size: 1, color: 'rgba(0,0,0,0.06)' }
            }
        }
    },

    title: {
        text: '3d Chart'
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
        allowPointSelect: true,
        marker: {
            radius: 12,
            symbol: 'url(../../../assets/images/cylinder.png)',
            width: 30,
            height: 30
          
        },
        data: this.newData
    
    },
    
 {
    
}
]
});

(function (H) {
  function dragStart(eStart) {
      eStart = chart.pointer.normalize(eStart);

      var posX = eStart.chartX,
          posY = eStart.chartY,
          alpha = chart.options.chart.options3d.alpha,
          beta = chart.options.chart.options3d.beta,
          sensitivity = 5; // lower is more sensitive

      function drag(e) {
          // Get e.chartX and e.chartY
          e = chart.pointer.normalize(e);

          chart.update({
              chart: {
                  options3d: {
                      alpha: alpha + (e.chartY - posY) / sensitivity,
                      beta: beta + (posX - e.chartX) / sensitivity
                  }
              }
          }, undefined, undefined, false);
      }

      chart.unbindDragMouse = H.addEvent(document, 'mousemove', drag);
      chart.unbindDragTouch = H.addEvent(document, 'touchmove', drag);

      H.addEvent(document, 'mouseup', chart.unbindDragMouse);
      H.addEvent(document, 'touchend', chart.unbindDragTouch);
  }
  H.addEvent(chart.container, 'mousedown', dragStart);
  H.addEvent(chart.container, 'touchstart', dragStart);
}(Highcharts));
 
   }

  ngOnInit() {
    this.insertData(10,10,10);
  }
  insertData(xDim,zDim,yDim){
    //this.chartOpts.series[0].data = [];
    var x;
    var y;
    var z;
    
     for (x = 0; x < xDim; x += 1) {
         for (y = 0; y < yDim; y += 1) { 
            for (z = 0; z < zDim; z += 1) {           
             this.newData.push([
                 x,
                 y,
                 z
             ]);     
         }
         
     }
     
   }
  
   }

  onDrawClicked(): void{
    console.log("blah blah"); 
     // this.newData = [[0, 8, 0], [8, 4, 2], [3, 1, 3], [2, 1, 9], [4, 6, 1], [8, 12, 9]];
     this.newData = this.insertData(5, 5, 5);
     
  }
}

