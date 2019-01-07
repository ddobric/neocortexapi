import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {ChartModule} from 'angular2-highcharts';
import { AppComponent } from './app.component';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AppRoutingModule } from './app-routing-module';
import { HighchartsStatic } from 'angular2-highcharts/dist/HighchartsService';
import { AinetComponent } from './ainet/ainet.component';
import { SimpleNotificationsModule } from 'angular2-notifications';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

//declare var require: any;
declare var require: any;
export function highchartsFactory() {
  //return require('highcharts/highmaps');
  return require('highcharts');
}
@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    AinetComponent,
    
  ],
  imports: [
    BrowserModule,
    FormsModule,
    AppRoutingModule,
    RouterModule,
    SimpleNotificationsModule.forRoot(),
    BrowserAnimationsModule,
    ChartModule.forRoot(
    require('highcharts'),
    require('highcharts/modules/exporting'),
    require('highcharts/highcharts-3d')  
  )
  ],
  providers: [{provide: HighchartsStatic,
    useFactory: highchartsFactory,}],
  bootstrap: [AppComponent]
})
export class AppModule { }
