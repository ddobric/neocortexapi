import { NgModule }             from '@angular/core';
import { BrowserModule }        from '@angular/platform-browser';
import { FormsModule }          from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AinetComponent } from './ainet/ainet.component';

const appRoutes: Routes = [
  { path: 'home', component: HomeComponent },
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