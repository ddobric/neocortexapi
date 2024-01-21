import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AinetComponent } from './ainet/ainet.component';
import { HomeComponent } from './home/home.component';
import { SimpleNotificationsModule } from 'angular2-notifications';
import { SenderComponent } from './sender/sender.component';
import { NotifierModule } from "angular-notifier";

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
    NotifierModule.withConfig({
      // Custom options in here
      behaviour: {

        /**
         * Defines whether each notification will hide itself automatically after a timeout passes
         * @type {number | false}
         */
        autoHide: 1000,

        /**
         * Defines what happens when someone clicks on a notification
         * @type {'hide' | false}
         */
        onClick: false,

        /**
         * Defines what happens when someone hovers over a notification
         * @type {'pauseAutoHide' | 'resetAutoHide' | false}
         */
        onMouseover: 'pauseAutoHide',

        /**
         * Defines whether the dismiss button is visible or not
         * @type {boolean} 
         */
        showDismissButton: true,

        /**
         * Defines whether multiple notification will be stacked, and how high the stack limit is
         * @type {number | false}
         */
        stacking: 4

      }
      ,
      position: {

        horizontal: {

          /**
           * Defines the horizontal position on the screen
           * @type {'left' | 'middle' | 'right'}
           */
          position: 'right',

          /**
           * Defines the horizontal distance to the screen edge (in px)
           * @type {number} 
           */
          distance: 12

        },

        vertical: {

          /**
           * Defines the vertical position on the screen
           * @type {'top' | 'bottom'}
           */
          position: 'top',

          /**
           * Defines the vertical distance to the screen edge (in px)
           * @type {number} 
           */
          distance: 12,

          /**
           * Defines the vertical gap, existing between multiple notifications (in px)
           * @type {number} 
           */
          gap: 10

        }

      }
    }),
    BrowserModule,
    FormsModule,
    PlotlyModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    SimpleNotificationsModule.forRoot()
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
