import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AinetComponent } from './ainet/ainet.component';

const routes: Routes = [
  { path: 'home', component: HomeComponent },
    { path: 'app-ainet', component: AinetComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
