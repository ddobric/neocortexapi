import { Component, OnInit } from '@angular/core';
import { HighchartsStatic, HighchartsService } from 'angular2-highcharts/dist/HighchartsService';
import { NeoCortexModel, Area, Synapse, Minicolumn, Cell, NeocortexSettings } from '../neocortexmodel';
import { neoCortexUtils} from '../neocortexutils';
import { modelGroupProvider } from '../../../node_modules/@angular/forms/src/directives/ng_model_group';


declare var require: any;

const Highcharts = require('highcharts');
Highcharts.setOptions({
  //colors: ['#50B432'],
  redraw: false,
});

@Component({
    selector: 'ainet',
    templateUrl: './ainet.component.html',
    styleUrls: ['./ainet.component.css']
})
export class AiNetComponent implements OnInit {
   
    options: any = {};
    chart: any = {};
    dataSer1: any = [];
    dataArea: any = [];
    dataSer2: any = [];
    chartInstance: Object;
    xCoordinate : any;
    yCoordinate : any;
    zCoordinate : any;

    constructor() {

        this.createChart();

        // initData function will initialize the data series 1 
       // this.initData1(100, 6, 3);
        // initData function will initialize the data series 2 
        //this.initData2(10, 6, 4);
        // generateData function will draw the chart
        this.generateChart(null);
    }


    ngOnInit() {
        
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
    initData2(xPos, yPos, zPos) {
        //this.chartOpts.series[0].data = [];
        let x;
        let y;
        let z;

        for (x = 0.5; x < xPos; x += 1) {
            for (y = 0.5; y < yPos; y += 1) {
                for (z = 0.5; z < zPos; z += 1) {
                    this.dataSer2.push([
                        x,
                        y,
                        z
                    ]);

                }

            }

        }

    }
    initData1(xPos, yPos, zPos) {
        //this.chartOpts.series[0].data = [];
        let x;
        let y;
        let z;
        for (x = 0; x < xPos; x += 1) {
            for (y = 0; y < yPos; y += 1) {
                for ( z = 0; z < zPos; z += 1) {
                    this.dataSer1.push([
                        x,
                        y,
                        z
                    ]);
                }

            }

        }

    }




    generateChart(event): void {
        let container = document.createElement("container");
        document.body.appendChild(container);

        this.options = {
            nativeChart: null,
            exporting: { enabled: false },
            credits: { enabled: false },
            chart: {
                renderTo : container,
                height: 600,
               // height: 800,
                //width: 1000,
                //zoomType: 'xy',
                margin: 100,
                type: 'scatter',
                animation: false,
                options3d: {
                    enabled: true,
                    alpha: 10,
                    beta: 30,
                    depth: 250,
                    viewDistance: 5,
                    fitToPlot: false,
                    frame: {
                        bottom: { size: 3, color: 'rgba(0,0,0,0.02)' },
                        back: { size: 3, color: 'rgba(0,0,0,0.04)' },
                        side: { size: 3, color: 'rgba(0,0,0,0.06)' }
                    }
                }
            },
            boost: {
                useGPUTranslations: true
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
                    //lineWidth: 4
                    //groupPadding: 0
                }
            },
            xAxis: {
                min: 0,
                max: 100,
                gridLineWidth: 3,
                //gridLineColor: this.lineColour
            },
            yAxis: {
                min: 0,
                max: 6,
                gridLineWidth: 3,
                title: null
            },
            zAxis: {
                min: 0,
                max: 4,
                gridLineWidth: 3,
                showFirstLabel: false
            },
            legend: {
                enabled: false
            },
            series: [{
                name: 'Ist Data',
                data: this.dataArea[0],
                color: "green",
                lineWidth: 1,
                marker: {
                    enabled: true,
                    symbol: 'url(../../../assets/images/cylinder.png)',
                    width: 25,
                    height: 25
                },
                
            },
            {
                name: '2nd Data',
                data: this.dataSer2,
                color: "orange",
                lineWidth: 5,
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
        this.chart.nativeChart = chartInstance;
        chartInstance.container.nativeChart = chartInstance;
        chartInstance.container.options = this.options;
     
        (<any>document).nativeChart = chartInstance;
         //calling mouse events to rotate the container/chart with mouse
        Highcharts.addEvent(chartInstance.container, 'mousedown', this.dragStart);
        //calling touch events to rotate the container/chart with touch
        Highcharts.addEvent(chartInstance.container, 'touchstart', this.dragStart);

    }

    /**
     * 
     * @param eStart mouse and touch events for chart/container rotation
     * 
     */
    dragStart(eStart): any {
      
        eStart = (<any>document).nativeChart.pointer.normalize(eStart);
        var posX = eStart.chartX;
        var posY = eStart.chartY;
        var alpha = this.options.chart.options3d.alpha;
        var beta = this.options.chart.options3d.beta;
        var sensitivity = 5; // lower is more sensitive

        var unbindDragMouse = Highcharts.addEvent(document, 'mousemove', dragFnc);
        Highcharts.addEvent(document, 'mouseup', unbindDragMouse);

        var unbindDragTouch = Highcharts.addEvent(document, 'touchmove', dragFnc);
        Highcharts.addEvent(document, 'touchend', unbindDragTouch);
        
       
        function dragFnc(e): any{
            // Get e.chartX and e.chartY
            e = this.nativeChart.pointer.normalize(e);
    
            this.nativeChart.update({
                chart: {
                    options3d: {
                        alpha: alpha + (e.chartY - posY) / sensitivity,
                        beta: beta + (posX - e.chartX) / sensitivity
                    }
                }
            }, undefined, undefined, false);
        }
    
    }
    
  createChart()  { 
    let model  = neoCortexUtils.createModel(2, [100, 3], 6); // createModel (numberOfAreas/DataSeries, [xAxis, zAxis], yAxis)
    let i; let j; let k;
    for ( i = 0; i < model.settings.minicolumnDims[0]; i++) {
        for ( j = 0; j < model.settings.numLayers; j ++) {
            for ( k = 0; k < model.settings.minicolumnDims[1]; k ++) {
                this.dataSer1.push([i, j, k]);
            }

        }

    }
    for (var a = 0; a < model.settings.numAreas; a++) {
      this.dataArea.push(this.dataSer1);
    }
    }

addAllSerieas(){
for(let i = 0; i < this.dataArea.length; i ++ ) {
    this.chart.nativeChart.addSeries( i, false );
}

}
}



