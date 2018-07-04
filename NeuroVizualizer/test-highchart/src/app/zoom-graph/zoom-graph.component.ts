import { Component, OnInit, Testability } from '@angular/core';
import { Router} from '@angular/router';
//import { Chart }            from 'angular2-highcharts'; 

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
  editedData:any=[[0, 4, 0]];
  chart: any = {};



  constructor(private router: Router) {
    
    this.insertData1(7, 7, 7);
    this.insertData2(3.5, 4.5, 5.5);
    //this.insertData1(10,10,10);
   // this.insertData2(15.5,15.5,15.5);
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
            spacingLeft: 50,
            spacingRight: 10,
            marginLeft: 170,
            margin: [70, 50, 60, 80],
            // Explicitly tell the width and height of a chart
            //width: null,
            width: 1200,
            height: 450,
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
               // update: this.updateData
            }
        },
        scatter: {
           name: 'Clicked Data',
            lineWidth:4
        }
    },
    xAxis: {
        min: 0,
        max: 15,
        gridLineWidth: 3
    },
    yAxis: {
        min: 0,
        max: 15,
        title: null
    },
    zAxis: {
        min: 0,
        max: 15,
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
            symbol: 'url(../../../assets/images/cylinder.png)',
            width: 25,
            height: 25 
        },
        // to changes the color of the reading bulbble
        //zoneAxis: 'y',
       // zones: [{
           // value: 5,
          ////  color: 'red',
       // }],

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
        name: 'Clicked Data',
        data: this.editedData,
        marker: {
            symbol: 'url(../../../assets/images/edited.png)',
            width: 30,
            height: 30
        }
    },
    
 {
    
    }    
]
});
/*
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

*/
 /*
 // This works fine
chart.series[0].update({
    marker: {
        radius:30,
        symbol: 'url(../../../assets/images/cylinder.png)',
        width: 70,
        height: 70
        
    }
});
*/

/*
// this works too fine to add a single point in graph
chart.series[0].data[0].update({
    x:20,
    marker: {
        symbol: 'url(../../../assets/images/cylinder.png)',
        width: 40,
        height: 40
        
    }
});
*/
   }
  ngOnInit() {
    this.updateData();
  }
  updateData(){
    this.chart.series.push({
        name: 'Clicked Data',
            data: this.editedData,
            marker: {
                symbol: 'url(../../../assets/images/edited.png)',
                width: 60,
                height: 60
            }

    });
  }
  insertData1(xDim,zDim,yDim){
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
                 x+1,
                 y+1,
                 z+1
             ]);   
             
         }
         
     }
     
   }
  
   }
 

}

