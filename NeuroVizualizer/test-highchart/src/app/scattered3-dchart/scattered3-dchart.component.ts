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
    options: any = {};
    chart: any = {};
    dataSer1: any = [];
    x: any = 9;
    y: any = 30;
    z: any = 252;
    dataSer2: any = [];
    chartInstance: Object;
    xCoordinate : any;
    yCoordinate : any;
    zCoordinate : any;
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
    }

    onInputChange(event): void {
        this.generateChart(event);
    }
    xAxisColour(xAxisColor: String){
       // this.lineColour = colour;
        //this.chart.nativeChart.options.xAxis.gridLineColor= "red";
        this.options.xAxis.gridLineColor = xAxisColor;
        this.chart.nativeChart.update(this.options);

    }
    yAxisColour(yAxisColor: string){
        this.options.yAxis.gridLineColor = yAxisColor;
        this.chart.nativeChart.update(this.options);
    }

    addPoints(points: String) {
         this.dataSer1[0] = eval("[" + points + "]");
        var str = this.dataSer1[0]; 
        var xCoordinateStr = str.slice(0, 1)//x
        var yCoordinateStr = str.slice(1, 2); //y
        var zCoordinateStr = str.slice(2, 3)//z
        this.xCoordinate = parseInt(xCoordinateStr);
        this.yCoordinate = parseInt(yCoordinateStr);
        this.zCoordinate = parseInt(zCoordinateStr);
        //this.chart.nativeChart.series[0].data[0].update({y:this.yCoordinate, marker:{symbol:'url(../../../assets/images/edited.png)', width:10}});
        this.chart.nativeChart.series[0].data[0].update({x:this.xCoordinate, y:this.yCoordinate, z:this.zCoordinate,marker:{symbol:'url(../../../assets/images/edited.png)', width:50, height:50}});
        //this.chart.nativeChart.series[0].setData(this.dataSer1);
        this.chart.nativeChart.update(this.options);

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
                // when you render the chart to container then container should be added in chart tag in html file
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
            xAxis: {
                min: 0,
                max: 10,
                gridLineWidth: 3,
                //gridLineColor: this.lineColour
            },
            yAxis: {
                min: 0,
                max: 10,
                gridLineWidth: 3,
                title: null
            },
            zAxis: {
                min: 0,
                max: 10,
                gridLineWidth: 3,
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



