import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {ChartModule} from 'angular2-highcharts';
import { AppComponent } from './app.component';
import { RouterModule, Routes } from '@angular/router';
import { Scattered3DchartComponent } from './scattered3-dchart/scattered3-dchart.component';
import { HomeComponent } from './home/home.component';
import { AppRoutingModule } from './app-routing-module';
import { HighchartsStatic } from 'angular2-highcharts/dist/HighchartsService';
import { AiNetComponent } from './ainet/ainet.component';
import { PlotlyComponent } from './plotly/plotly.component';

//declare var require: any;
declare var require: any;
export function highchartsFactory() {
  //return require('highcharts/highmaps');
  return require('highcharts');
}
@NgModule({
  declarations: [
    AppComponent,
    Scattered3DchartComponent,
    HomeComponent,
    AiNetComponent,
    PlotlyComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    AppRoutingModule,
    RouterModule,
    //ChartModule,
    ChartModule.forRoot(
    require('highcharts'),
   // require('highcharts/modules/map'),
    require('highcharts/modules/exporting'),
    require('highcharts/highcharts-3d')
    
    
  )
  ],
  providers: [{provide: HighchartsStatic,
    useFactory: highchartsFactory,}],
  bootstrap: [AppComponent]
})
export class AppModule { }
