import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NeoCortexUtilsService } from '../services/neocortexutils.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  public location = '';
  constructor(public router: Router, private neoUtilsService: NeoCortexUtilsService) {

    this.location = router.url;
  }
  ngOnInit() {
  }

}
