import { Component, OnInit, Testability } from '@angular/core';
import { Container } from '@angular/compiler/src/i18n/i18n_ast';
import { Router} from '@angular/router';
import { TestingCompilerFactory } from '@angular/core/testing/src/test_compiler';

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
  Data1:any=[];
  Data2:any=[];
  updatedData:any=[9, 9 ,0];
  chart: any;

  constructor(private router: Router) {
    this.insertData1(10,10,10);
    this.insertData2(15.5,15.5,15.5);
    
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
        //height: '110%',
        animation: false,
        //zoomType: 'xy',
       
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
        point: {
            events: {
                update: this.updateData
            }
        },
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
        name: 'Ist Data',
        //colorByPoint: true,
        //allowPointSelect: true,
        data: this.Data1,
        marker: {
            radius: 12,
            symbol: 'url(../../../assets/images/cylinder.png)',
            width: 30,
            height: 30
          
        }

    },
    {
        name: '2nd Data',
        data: this.Data2,
        marker: {
            symbol: 'url(../../../assets/images/dbCylinder.png)',
            width: 20,
            height: 20
        }
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
    this.updateData();
  }
  updateData(){
    //this.chart.series[0].data1[0].update(this.x = 10);

    //let chart = new Highcharts.Chart({});
    //chart.series[0].data[3].update({ y: 0 });
    this.chart.update({
        chart: {
            series: [{
                name: 'Changed Data',
                data: this.updatedData,
                marker: {
                    radius: 12,
                    symbol: 'url(../../../assets/images/dbCylinder.png)',
                    width: 30,
                    height: 30
                  
                }
        
            }]
        }});



    

  }
  insertData1(xDim,zDim,yDim){
    //this.chartOpts.series[0].data = [];
    var x;
    var y;
    var z;
    
     for (x = 0; x < xDim; x += 1) {
         for (y = 0; y < yDim; y += 1) { 
            for (z = 0; z < zDim; z += 1) {           
             this.Data1.push([
                 x,
                 y,
                 z
             ]);   
             
         }
         
     }
     
   }
  
   }
   insertData2(x2,z2,y2){
    //this.chartOpts.series[0].data = [];
    var x;
    var y;
    var z;
    
     for (x = 0.5; x < x2; x += 1) {
         for (y = 0.5; y < y2; y += 1) { 
            for (z = 0.5; z < z2; z += 1) {           
             this.Data2.push([
                 x,
                 y,
                 z
             ]);   
             
         }
         
     }
     
   }
  
   }
/*
  onDrawClicked(): void{
    console.log("blah blah"); 
     // this.newData = [[0, 8, 0], [8, 4, 2], [3, 1, 3], [2, 1, 9], [4, 6, 1], [8, 12, 9]];
     this.data1 = this.insertData(5, 5, 5);
     this.data2 = this.insertData(3, 3, 3);
     
  }
  */
}

