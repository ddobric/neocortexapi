import { Component, ElementRef, OnInit, ViewChild, AfterViewInit } from '@angular/core';
//import * as Plotly from 'plotly.js';
//import * as Plotlyjs from 'plotly.js/dist/plotly;
import * as Plotlyjs from 'plotly.js/dist/plotly';

@Component({
  selector: 'app-plotly',
  templateUrl: './plotly.component.html',
  styleUrls: ['./plotly.component.css']
})
export class PlotlyComponent implements OnInit, AfterViewInit {

  data: any;
  layout: any;
 
  constructor() {
    
  }
  ngOnInit() {
  }
  ngAfterViewInit() {
    this.createChart();
  }
  createChart() {
    let xx = [];
    let yy = [];
    let zz = [];
    let x;
    let y;
    let z;
    for (x = 0; x < 10; x++) {
      for (y = 0; y < 6; y++) {
        for (z = 0; z < 4; z++) {
          xx.push(x);
          yy.push(y);
          zz.push(z);
        }

      }

    }
    let graph = document.getElementById('graph');
    const trace1 = {
      /*       x: [0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2],
            y: [0,0,0,1,2,1,2,1,2,0,0,0,1,2,1,2,1,2,0,0,0,1,2,1,2,1,2],
            z: [0,1,2,0,0,1,1,2,2,0,1,2,0,0,1,1,2,2,0,1,2,0,0,1,1,2,2], */
            x: xx,
            y: yy,
            z: zz,
      mode: 'markers',
      marker: {
        size: 12,
        
        //color: '#00FFFF',
        symbol: 'circle',
                  line: {
                  color: '#00FFFF',
                  width: 2
                } , 
        opacity: 0.8
      },
      type: 'scatter3d'
    
    };

    this.data = [trace1];
    this.layout = {
      dragmode: false,
      autosize: false,
      width: 800,
      height: 800,
      margin: {
        l: 0,
        r: 0,
        b: 0,
        t: 0,
        pad: 4
      }
    };
    Plotlyjs.newPlot(graph, this.data, this.layout);
  }

}
