import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {ChartModule} from 'angular2-highcharts';
import { AppComponent } from './app.component';
import { RouterModule, Routes } from '@angular/router';
import { GraphComponent } from './graph/graph.component';
import { Rotation3DComponent } from './rotation3-d/rotation3-d.component';
import { Scattered3DchartComponent } from './scattered3-dchart/scattered3-dchart.component';
import { ZoomGraphComponent } from './zoom-graph/zoom-graph.component';
import { HomeComponent } from './home/home.component';
import { AppRoutingModule } from './app-routing-module';
import { ScatterPointsComponent } from './scatter-points/scatter-points.component';
declare var require: any;

@NgModule({
  declarations: [
    AppComponent,
    GraphComponent,
    Rotation3DComponent,
    Scattered3DchartComponent,
    ZoomGraphComponent,
    HomeComponent,
    ScatterPointsComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    AppRoutingModule,
    RouterModule,
    ChartModule.forRoot(
    require('highcharts'),
    require('highcharts/modules/exporting'),
    require('highcharts/highcharts-3d')
    
    
  )
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
