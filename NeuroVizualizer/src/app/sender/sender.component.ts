import { Component, OnInit } from '@angular/core';
import { NotificationsService } from 'angular2-notifications';
import { NeoCortexUtilsService } from '../services/neocortexutils.service';
import { NeoCortexGenerator } from '../Entities/NeoCortexGenerator';
import { NeoCortexModel } from '../Entities/NeoCortexModel';

@Component({
  selector: 'app-sender',
  templateUrl: './sender.component.html',
  styleUrls: ['./sender.component.scss']
})
export class SenderComponent implements OnInit {

  constructor(private _service: NotificationsService, private neoUtils: NeoCortexUtilsService) {
  }

  ngOnInit() {
  }

  sendModel() {
    let model = new NeoCortexGenerator().createModel([0, 0, 0, 1, 2, 1], [10, 1], 6);
    //this.neoUtils.data.next({ dataModel: model });

    this.neoUtils.data.next({
      msgType: "init",
      clientType: "NeuroVisualizer",
      dataModel: model
    });
  }
}
/* {
  "msgType": "init",
  "data": {
     "clientType": "NeuroVisualizer"
  }

} */
