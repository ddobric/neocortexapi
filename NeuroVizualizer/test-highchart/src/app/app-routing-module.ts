import { NgModule }             from '@angular/core';
import { BrowserModule }        from '@angular/platform-browser';
import { FormsModule }          from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';

import { GraphComponent } from './graph/graph.component';
import { HomeComponent } from './home/home.component';
import { Rotation3DComponent } from './rotation3-d/rotation3-d.component';
import { Scattered3DchartComponent } from './scattered3-dchart/scattered3-dchart.component';
import { ZoomGraphComponent } from './zoom-graph/zoom-graph.component';
import { ScatterPointsComponent } from './scatter-points/scatter-points.component';
import { AiNetComponent } from './ainet/ainet.component';

const appRoutes: Routes = [
  { path: 'home', component: HomeComponent },
    { path: 'app-graph', component: GraphComponent },
    { path: 'app-rotation3-d', component: Rotation3DComponent },
    { path: 'app-scattered3-dchart', component: Scattered3DchartComponent},
    { path: 'app-zoom-graph', component: ZoomGraphComponent},
    { path: 'app-scatter-points', component: ScatterPointsComponent},
    { path: 'ainet', component: AiNetComponent}
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