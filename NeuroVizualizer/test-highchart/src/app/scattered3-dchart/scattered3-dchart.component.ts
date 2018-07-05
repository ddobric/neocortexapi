import { Component, OnInit } from '@angular/core';
import { HighchartsStatic, HighchartsService } from 'angular2-highcharts/dist/HighchartsService';

declare var require: any;

/*
const Highcharts = require('highcharts');
Highcharts.setOptions({
  //colors: ['#50B432'],
  redraw: false,
});
*/
@Component({
    selector: 'app-scattered3-dchart',
    templateUrl: './scattered3-dchart.component.html',
    styleUrls: ['./scattered3-dchart.component.css']
})
export class Scattered3DchartComponent implements OnInit {
    //chartOpts: any={};
    options: any = {};
    chart: any = {};
    editedData: any = [[2, 4, 0]];
    dataSer1: any = [];
    x: any = 9;
    y: any = 30;
    z: any = 252;
    dataSer2: any = [];
    //me: any;
    chartInstance: Object;
    posX: any;
    posY: any;
    alpha: any;
    beta: any;
    constructor() {
        this.initData(3, 3, 3);
        this.insertData2(4, 4, 5);
        this.generateChart(null);
        //this.me = this.chartOpts;
        //this.options = Object;
        // this.saveInstance(this.chartInstance, this.chart);
        // this.chartOpts.chart.redraw = false;
    }
    ngOnInit() {
        //this.ngAfterViewInit();
        ;
    }


    onInputChange(event): void {
        this.generateChart(event);
    }

    addPoints(points: String) {


        // this.dataSer1[0] = eval("[" + points + "]");
        this.dataSer1[3] = [6,6, 6];

        // ,
        //     marker: {
        //         enabled: true,
        //         symbol: 'url(../../../assets/images/edited.png)',
        //         width: 40,
        //         height: 40
        //     }

        this.chart.nativeChart.series[0].data[3].update({y:6, marker:{color: 'rgba(0,0,0,0.02)', width:100}});

        //this.chart.nativeChart.series[0].setData(this.dataSer1);
        //this.options.series[0].marker.width = 80;
        this.chart.nativeChart.update(this.options);
        //this.chart.nativeChart.series.dataSer1[0].marker.width= 80;
        // this.chart.nativeChart.series[0].setData(this.options.data.marker.width= 80);
        //this.chart.nativeChart.series.SVGRenderer.symbol.width= 80;
        /*
        //doesnt change the size
        this.options.series[this.dataSer1[0]]= [{
          marker:{width : 80}
        }];
        */
        /*
         this.options.series[0]= [{
             data: this.dataSer1[0],
             marker:{width : 80}
           }];
           */
        /*
      this.options.series[0]= [{
          dataSer1: {
          marker : {
            enabled: true,
              width : 80
            }
        }
        }];*/


        //this.chart.nativeChart.series.dataSer1[0].marker.width= 80;
        /* this woks fine for whole series
          this.options.series = [{
           marker:{
               width: 80
             }}];
             */
        //works fine fora series
        // this.options.series[0].marker.width = 80;
    }

    generateChart(event): void {
        this.options = {
            nativeChart: null,
            exporting: { enabled: false },
            credits: { enabled: false },
            chart: {
                //renderTo: 'container',

                height: 600,
                //zoomType: 'xy',
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
                series: {
                    marker: {
                        enabled: true
                    }
                },
                scatter: {
                    width: 10,
                    height: 10,
                    depth: 10,
                    lineWidth: 4
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
                name: 'Ist Data',
                colorByPoint: true,
                redraw: false,
                marker: {
                    enabled: true,
                    symbol: 'url(../../../assets/images/cylinder.png)',
                    width: 30,
                    height: 30
                },
                data: this.dataSer1,
            },
            {
                name: '2nd Data',
                data: this.dataSer2,
                marker: {
                    enabled: true,
                    symbol: 'url(../../../assets/images/dbCylinder.png)',
                    width: 20,
                    height: 20
                }
            },]

        };
    }
    saveInstance(chartInstance, chart) {
        chart.nativeChart = chartInstance;
    }

    insertData2(xDim, yDim, zDim) {
        //this.chartOpts.series[0].data = [];
        var x;
        var y;
        var z;

        for (x = 0.5; x < xDim; x += 1) {
            for (y = 0.5; y < yDim; y += 1) {
                for (z = 0.5; z < zDim; z += 1) {
                    this.dataSer2.push([
                        x,
                        y,
                        z
                    ]);

                }

            }

        }

    }
    initData(xDim, zDim, yDim) {
        //this.chartOpts.series[0].data = [];
        var x;
        var y;
        var z;
        for (z = 0; z < zDim; z += 1) {
            for (x = 0; x < xDim; x += 1) {
                for (y = 0; y < yDim; y += 1) {
                    this.dataSer1.push([
                        x,
                        y,
                        z
                    ]);
                }

            }

        }

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



