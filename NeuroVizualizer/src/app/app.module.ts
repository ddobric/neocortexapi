import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AinetComponent } from './ainet/ainet.component';
import { HomeComponent } from './home/home.component';
import { SimpleNotificationsModule } from 'angular2-notifications';
import { SenderComponent } from './sender/sender.component';

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
    AppRoutingModule,
    SimpleNotificationsModule.forRoot()
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
