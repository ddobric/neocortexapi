import { Component, OnInit } from '@angular/core';
import { ChartSelectionEvent } from 'highcharts';
declare var require: any;

const Highcharts = require('highcharts');
 
@Component({
  selector: 'app-rotation3-d',
  templateUrl: './rotation3-d.component.html',
  styleUrls: ['./rotation3-d.component.css']
})
export class Rotation3DComponent implements OnInit {
options: any;
x: any = 45;
y: any = 15;
z: any = 30;

  constructor() { 
    Highcharts.setOptions({
      //colors: ['#50B432'],
      //colors: ['yellow'],
      chart: {
        redraw:['false']
      }
    });
    
   this.onInputChange(null);
    
    
  }


  onInputChange(event): void {
     console.log("blah blah" + event)
     this.options = {
      exporting: { enabled: false },
      credits: { enabled: false },
      
      chart: {
        type: 'column',
        margin: 75,
        zoomType: 'xy',
        redraw:{enabled: false},

        options3d: {
            enabled: true,
            alpha: this.x,
            beta: this.y,
            depth: this.z,
            viewDistance: 25,
        },
    },
    plotOptions: {
        column: {
            depth: 25
        }
    },
    series: [{
        data: [29.9, 71.5, 106.4, 129.2, 144.0, 176.0, 135.6, 148.5, 216.4, 194.1, 95.6, 54.4]
    }],
   // redraw: false
    
     }

    // this.showValue();
  }
  
  //showValue(): void{
    //document.getElementById("x-value").innerHTML;
  //}
  //onInputBchange(event1): void{
  //  this.y = this.options.chart.options3d.beta;
   // console.log("b_changed");
  //}
  ngOnInit() {
  }

}
