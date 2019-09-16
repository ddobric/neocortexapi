import { Component } from '@angular/core';
import { NeoCortexGenerator } from './Entities/NeoCortexGenerator';
import { NeoCortexUtilsService } from './Services/neocortexutils.service';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.sass']
})
export class AppComponent {
  title = 'Transmitter';
  constructor(private neoUtils: NeoCortexUtilsService) {
  }
  sendModel() {
    let model = new NeoCortexGenerator().createModel([0, 0, 0, 1, 2, 1], [10, 2], 6);

    this.neoUtils.data.next({
      msgType: "init",
      clientType: "Sender",
      dataModel: model
    });
  }
}
