import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AinetComponent } from './ainet/ainet.component';
import { HomeComponent } from './home/home.component';

import { SenderComponent } from './sender/sender.component';
import { ToastrModule } from 'ngx-toastr';

import * as PlotlyJS from 'plotly.js/dist/plotly.js';
import { PlotlyModule } from 'angular-plotly.js';
PlotlyModule.plotlyjs = PlotlyJS;

@NgModule({
  declarations: [
    AppComponent,
    AinetComponent,
    HomeComponent,
    SenderComponent

  ],
  imports: [
    BrowserModule,
    FormsModule,
    PlotlyModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    ToastrModule.forRoot()
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
