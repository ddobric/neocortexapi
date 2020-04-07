import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { WebsocketService } from "./websocket.service";
import { map } from "rxjs/operators";
import { Cell, Synapse } from '../Entities/NeoCortexModel';

const URL = "ws://localhost:5000/ws/transmitter";

export interface NeoCortexUtils {
  dataModel?: any;
  clientType?: any;
  MsgType?: any;
  Columns?: any;
  Synapses?: any;


}

@Injectable({
  providedIn: 'root'
})
/* this.neoUtilsService.data.subscribe(a => {
  console.log(a);
}); */
export class NeoCortexUtilsService {
  data: Subject<NeoCortexUtils>;

  //{ "msgType": "init", "data": { "clientType": "NeuroVisualizer"} }
  constructor(socketService: WebsocketService) {
    this.data = <Subject<NeoCortexUtils>>socketService.connect(URL).pipe(map(
      (response: MessageEvent): NeoCortexUtils => {
        let JSONObject = JSON.parse(response.data);
        return {
          dataModel: JSONObject
        };

      }
    ));

  }




}
