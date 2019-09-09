import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import { WebsocketService } from "./websocket.service";
import { map } from "rxjs/operators";

const URL = "ws://localhost:5000/ws/NeuroVisualizer";

export interface NeoCortexUtils {
  dataModel: any;
}

@Injectable({
  providedIn: 'root'
})
export class NeoCortexUtilsService {
  public data: Subject<NeoCortexUtils>;

  constructor(socketService: WebsocketService) {
    this.data = <Subject<NeoCortexUtils>>socketService.connect(URL).pipe(map(
      (response: MessageEvent): NeoCortexUtils => {
        let Jdata = JSON.parse(response.data);
        return {
          dataModel: Jdata
        };
      }
    ));
   }
}
