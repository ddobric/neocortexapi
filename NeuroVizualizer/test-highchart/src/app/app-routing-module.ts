import { NgModule }             from '@angular/core';
import { BrowserModule }        from '@angular/platform-browser';
import { FormsModule }          from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { Scattered3DchartComponent } from './scattered3-dchart/scattered3-dchart.component';
import { AiNetHighchartComponent } from './ainetHighchart/ainetHighChart.component';
import { AinetComponent } from './ainet/ainet.component';

const appRoutes: Routes = [
  { path: 'home', component: HomeComponent },
    { path: 'app-scattered3-dchart', component: Scattered3DchartComponent},
    { path: 'ainetHighchart', component: AiNetHighchartComponent},
    { path: 'app-ainet', component: AinetComponent}
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