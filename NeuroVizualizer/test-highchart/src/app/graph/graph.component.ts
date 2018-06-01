import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-graph',
  templateUrl: './graph.component.html',
  styleUrls: ['./graph.component.css']
})

export class GraphComponent implements OnInit {
  constructor() {
    this.options = {
      exporting: { enabled: false },
      credits: { enabled: false },
      title : { text : 'chart selection event example' },
      chart: { 
        zoomType: 'xy',
        type: 'spline',
        scrollablePlotArea: {
            minWidth: 700,
            scrollPositionX: 1
      }
      },
      
     // series: [{ data: [29.9, 71.5, 106.4, 129.2, 45,13,120, 29.9, 71.5, 106.4, 129.2, 45,13,120], }]
   //  series: [{
    //  data: [29.9, 71.5, 106.4, 129.2, 144.0, 176.0, 135.6, 148.5,
      //      { y: 216.4, color: '#BF0B23'}, 194.1, 95.6, 54.4]
 // }]
 series: [{
  data: [29.9, 71.5, 106.4, 129.2, 144.0, 176.0, 135.6, 148.5,
  {y: 216.4, marker: { fillColor: '#BF0B23', radius: 10 } }, 194.1, 95.6, 54.4, 9.9, 71.5, 106.4, 129.2, 45,13,120, 29.9, 71.5, 106.4, 129.2, 45,13,120]
}]

  };
   }

  options:any;
  from:any;
  to: any;
  onChartSelection (e) {
    this.from = e.originalEvent.xAxis[0].min.toFixed(2);
    this.to = e.originalEvent.xAxis[0].max.toFixed(2);
  }
  ngOnInit() {
  }

}
