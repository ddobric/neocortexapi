import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { WebsocketService } from "./websocket.service";
import { map } from "rxjs/operators";
import { Cell, Synapse, NeoCortexModel } from '../Entities/NeoCortexModel';

const URL = "ws://localhost:5000/ws/NeuroVisualizer";

export interface NeoCortexUtils {
  dataModel: any;
  clientType?: any;
  msgType?: any;

}

@Injectable({
  providedIn: 'root'
})
/* this.neoUtilsService.data.subscribe(a => {
  console.log(a);
}); */
export class NeoCortexUtilsService {
  data: Subject<NeoCortexUtils>;
  model: any = null;
  JSONObject: any = null;
  //{ "msgType": "init", "data": { "clientType": "NeuroVisualizer"} }
  constructor(socketService: WebsocketService) {
    this.data = <Subject<NeoCortexUtils>>socketService.connect(URL).pipe(map(
      (response: MessageEvent): NeoCortexUtils => {
        this.JSONObject = JSON.parse(response.data);
        /*  return {
           dataModel: JSONObject
         }; */
        console.log(this.JSONObject);
        if (this.JSONObject.msgType == "init") {
          this.model = this.JSONObject.dataModel;
          this.createSynapses();
          return { dataModel: this.model }
        }
        else if (this.JSONObject.msgType == "updateOverlap") {

        }
        else if (this.JSONObject.msgType == "updateSynapses") {

        } else {

        }

      }
    ));

  }

  private binaryCellSearch(synapticId: number, lower: number, upper: number): Cell {
    let cell: Cell = null;

    if (upper < lower) {
      console.log("The cell with the following synapticId", synapticId, "not found");
      return null;
    }

    let midElement = Math.round((lower + upper) / 2);

    if (this.model.cells[midElement].cellId == synapticId) {
      cell = this.model.cells[midElement]
      return cell;
    }
    else {
      if (synapticId < this.model.cells[midElement].cellId) {
        return this.binaryCellSearch(synapticId, lower, midElement - 1);

      }
      else {
        return this.binaryCellSearch(synapticId, midElement + 1, upper);
      }
    }

  }

  private createSynapses() {
    let synapses = [];
    let upper = this.model.cells.length;

    for (let i = 0; i < this.model.synapses.length; i++) {

      let perm = this.model.synapses[i].permanence;
      let preCell: Cell = this.binaryCellSearch(this.model.synapses[i].preSynapticId, 0, upper);
      let postCell: Cell = this.binaryCellSearch(this.model.synapses[i].postSynapticId, 0, upper);


      let synapse: Synapse = {
        permanence: perm,
        preSynaptic: preCell,
        postSynaptic: postCell
      };

      synapses.push(synapse);

      preCell.outgoingSynapses.push(synapse);

      postCell.incomingSynapses.push(synapse);


    }
    this.model.synapses = [];
    this.model.synapse = synapses;
    console.log(this.model.synapses);

    return this.model;

  }

}
