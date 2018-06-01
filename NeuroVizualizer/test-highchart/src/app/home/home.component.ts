import { Component, OnInit } from '@angular/core';
import { Router} from '@angular/router';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  public location = '' ;
  constructor(private router: Router) { 
    
    this.location= router.url;
  }

  ngOnInit() {
  }

}
