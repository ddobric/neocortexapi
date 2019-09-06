import { Component, OnInit } from '@angular/core';
import { NotificationsService } from 'angular2-notifications';
import { NeoCortexUtilsService } from '../services/neo-cortex-utils.service';
import { neoCortexUtils } from '../Entities/neocortexutils';
import { NeoCortexModel, Cell } from '../Entities/neocortexmodel';

@Component({
  selector: 'app-sender',
  templateUrl: './sender.component.html',
  styleUrls: ['./sender.component.scss']
})
export class SenderComponent implements OnInit {
  model: NeoCortexModel;

  constructor(private _service: NotificationsService, private neoUtils: NeoCortexUtilsService) { }

  ngOnInit() {
  }

  sendModel() {

    this.model = neoCortexUtils.createModel([0, 0, 0, 1, 2, 1], [2, 1], 3);
    this.neoUtils.data.next({ dataModel: this.model });
  }
}
