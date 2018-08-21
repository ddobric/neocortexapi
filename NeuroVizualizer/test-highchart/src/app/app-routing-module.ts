import { NgModule }             from '@angular/core';
import { BrowserModule }        from '@angular/platform-browser';
import { FormsModule }          from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { Scattered3DchartComponent } from './scattered3-dchart/scattered3-dchart.component';
import { AiNetComponent } from './ainet/ainet.component';
import { PlotlyComponent } from './plotly/plotly.component';

const appRoutes: Routes = [
  { path: 'home', component: HomeComponent },
    { path: 'app-scattered3-dchart', component: Scattered3DchartComponent},
    { path: 'ainet', component: AiNetComponent},
    { path: 'app-plotly', component: PlotlyComponent}
  ];
  
  @NgModule({
    imports: [
      RouterModule.forRoot(
        appRoutes,
        { enableTracing: true } // <-- debugging purposes only
      )
    ]
  })
  export class AppRoutingModule { }