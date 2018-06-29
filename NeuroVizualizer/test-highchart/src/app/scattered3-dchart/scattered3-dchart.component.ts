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
  //chartOpts: any={};
  options: any= {};
  chart:any ={};
  editedData:any=[[2, 4, 0]];
  data:any=[];
  x: any= 9;
  y: any= 30;
  z: any=252;
  n:any = 10000;
  i: any;
  Data2:any=[];
  series: any = [];
  //redraw: boolean;
    //me: any;
    chartInstance: Object;
    posX: any;
    posY: any;
    alpha: any;
    beta: any;
   
  constructor() { 
     
    //this.me = this.chartOpts;
    
    this.initData(5.5,6.2,5.8);
    this.insertData2(6.4, 7.2, 5.9);
    this.options = Object;
    this.generateChart(null);  
    this.saveInstance(this.chartInstance);
   // this.updateData();
     // this.chartOpts.chart.redraw = false;
     
    
  }

  ngOnInit() {

    }

    onInputChange(event): void {
        this.generateChart(event);
    }

    generateChart(event): void {
    this.options = {
    exporting: { enabled: false },
    credits: { enabled: false },
      chart: {
        //renderTo: 'container',
        
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
            lineWidth:4
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
    },
    {
        name: '2nd Data',
        data: this.Data2,
        marker: {
            symbol: 'url(../../../assets/images/dbCylinder.png)',
            width: 20,
            height: 20
        }
    },]
      
  };
  }
  saveInstance(chartInstance) {
    this.chart = chartInstance;
}
/*updateData(){
    this.chart.series.push({
        name: 'Clicked Data',
            data: this.editedData,
            marker: {
                symbol: 'url(../../../assets/images/edited.png)',
                width: 60,
                height: 60
            }

    });
  }*/
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

/*
  dragStart(eStart){
    eStart = this.chartOpts.pointer.normalize(eStart);
    this.posX = eStart.chartX,
    this.posY = eStart.chartY,
    this.alpha = this.chartOpts.options.chart.options3d.alpha,
    this.beta = this.chartOpts.options.chart.options3d.beta,
    this.sensitivity = 5;

  }
  drag(e) {
    e = this.chartOpts.pointer.normalize(e);

    this.chartOpts.update({
        chart: {
            options3d: {
                alpha: this.alpha + (e.chartY - this.posY) / this.sensitivity,
                beta: this.beta + (this.posX - e.chartX) / this.sensitivity
            }
        }
    });
   
  }
 */
 


}



