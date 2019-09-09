import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AinetComponent } from './ainet/ainet.component';
import { SenderComponent } from './sender/sender.component';

const routes: Routes = [
  { path: 'home', component: HomeComponent },
    { path: 'app-ainet', component: AinetComponent},
    { path: 'app-sender', component: SenderComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
