import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { WebsocketService } from "./websocket.service";
import { map } from "rxjs/operators";
import { Cell, Synapse, NeoCortexModel } from '../Entities/NeoCortexModel';
import { NeoCortexGenerator } from '../Entities/NeoCortexGenerator';
import { environment as env } from '../../environments/environment.prod';



export interface NeoCortexUtils {
  dataModel: any;
  clientType?: any;
  msgType?: any;
  notification?: any;


}

@Injectable({
  providedIn: 'root'
})

export class NeoCortexUtilsService {
  data: Subject<NeoCortexUtils>;
  model: any;
  notifyTyp: any;
  notifyMsg?: any;

  //{ "msgType": "init", "data": { "clientType": "NeuroVisualizer"} }
  constructor(socketService: WebsocketService) {

    this.data = <Subject<NeoCortexUtils>>socketService.connect(env.URL).pipe(map(
      (response: MessageEvent): NeoCortexUtils => {
        let JSONObject = JSON.parse(response.data);
        /*  return {
           dataModel: JSONObject
         }; */

        if (JSONObject.msgType == "init") {
          this.model = JSONObject.dataModel;
          this.createSynapses();
          return {
            dataModel: this.model,
            notification: { type: this.notifyTyp, msg: this.notifyMsg }
          }
        }
        else if (JSONObject.msgType == "updateOverlap") {
          this.updateOverlap(JSONObject.update);
          return {
            dataModel: this.model,
            notification: { type: this.notifyTyp, msg: this.notifyMsg }
          }
        }
        else if (JSONObject.msgType == "updateOrAddSynapse") {
          this.updateOrAddSynapse(JSONObject.update);
          return {
            dataModel: this.model,
            notification: { type: this.notifyTyp, msg: this.notifyMsg }
          }

        } else {

        }

      }
    ));

  }
  private updateOrAddSynapse(updateOrAddSynap) {
    this.lookUpSynapse(updateOrAddSynap);

  }

  private lookUpSynapse(searchSynapse) {
    try {

      let preCell = this.model.areas[searchSynapse.preCellAreaId].minicolumns[searchSynapse.preCell.cellX][searchSynapse.preCell.cellZ].cells[searchSynapse.preCell.cellY];
      let postCell = this.model.areas[searchSynapse.postCellAreaId].minicolumns[searchSynapse.postCell.cellX][searchSynapse.postCell.cellZ].cells[searchSynapse.postCell.cellY];


      let synapseFound = false;
      loop:
      for (let out = 0; out < preCell.outgoingSynapses.length; out++) {
        for (let inc = 0; inc < postCell.incomingSynapses.length; inc++) {

          if ((preCell.outgoingSynapses[out].postSynaptic.X === postCell.X &&
            preCell.outgoingSynapses[out].postSynaptic.Layer === postCell.Layer &&
            preCell.outgoingSynapses[out].postSynaptic.Z === postCell.Z) &&

            (postCell.incomingSynapses[inc].preSynaptic.X === preCell.X &&
              postCell.incomingSynapses[inc].preSynaptic.Layer === preCell.Layer &&
              postCell.incomingSynapses[inc].preSynaptic.Z === preCell.Z)) {

            //  console.log("Synapse Exists", "Permanence will be updated", 'info');
            this.updatePermanenceOfSynapse(searchSynapse.permanence, preCell, postCell);
            synapseFound = true;
            break loop;
          }


        }
      }
      if (synapseFound === false) {
        //Console.log("Synapse doesn't Exists", "It will be created", 'info');
        this.generateNewSynapse(searchSynapse.permanence, preCell, postCell);
      }



    } catch (error) {
      this.notifyTyp = "error";
      this.notifyMsg = error;

    }


  }
  private updatePermanenceOfSynapse(newPermanence: number, preCell: Cell, postCell: Cell) {

    for (let findSynapse = 0; findSynapse < this.model.synapses.length; findSynapse++) {

      if (this.model.synapses[findSynapse].preSynaptic.areaIndex === preCell.areaIndex &&
        this.model.synapses[findSynapse].preSynaptic.X === preCell.X &&
        this.model.synapses[findSynapse].preSynaptic.Layer === preCell.Layer &&
        this.model.synapses[findSynapse].preSynaptic.Z === preCell.Z &&

        this.model.synapses[findSynapse].postSynaptic.areaIndex === postCell.areaIndex &&
        this.model.synapses[findSynapse].postSynaptic.X === postCell.X &&
        this.model.synapses[findSynapse].postSynaptic.Layer === postCell.Layer &&
        this.model.synapses[findSynapse].postSynaptic.Z === postCell.Z) {

        this.model.synapses[findSynapse].permanence = newPermanence;
        this.notifyTyp = "success";
        this.notifyMsg = "Synapse found, permanence updated";

      }


    }

  }


  /**
   * This method creates a synapse
   * @param permanence 
   * @param preCell 
   * @param postCell 
   */
  private generateNewSynapse(synapsePermanence: number, preCell: Cell, postCell: Cell) {

    let newSynapse: Synapse = {
      permanence: synapsePermanence,
      preSynaptic: preCell,
      postSynaptic: postCell
    };


    preCell.outgoingSynapses.push(newSynapse);
    postCell.incomingSynapses.push(newSynapse);

    this.model.areas[preCell.areaIndex].minicolumns[preCell.X][preCell.Z].cells[preCell.Layer].outgoingSynapses.push(newSynapse);
    this.model.areas[postCell.areaIndex].minicolumns[postCell.X][postCell.Z].cells[postCell.Layer].incomingSynapses.push(newSynapse);

    //console.log("Synapse will be created");

    this.model.synapses.push(newSynapse);
    this.notifyTyp = "success";
    this.notifyMsg = "Synapse doesn't found, new created";

  }


  private updateOverlap(updateOverlapCo: any) {
    try {
      this.model.areas[updateOverlapCo.areaIDOfCell].minicolumns[updateOverlapCo.minColXDim][updateOverlapCo.minColZDim].overlap = updateOverlapCo.updateOverlapValue;
      this.notifyTyp = "success";
      this.notifyMsg = "Overlap Updated";

    } catch (error) {
      this.notifyTyp = "error";
      this.notifyMsg = error;

    }



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
    let synapseRegister = [];
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

      synapseRegister.push(synapse);

      preCell.outgoingSynapses.push(synapse);
      this.model.areas[preCell.areaIndex].minicolumns[preCell.X][preCell.Z].cells[preCell.Layer].outgoingSynapses.push(synapse);


      postCell.incomingSynapses.push(synapse);
      this.model.areas[postCell.areaIndex].minicolumns[postCell.X][postCell.Z].cells[postCell.Layer].incomingSynapses.push(synapse);

    }

    this.model.synapses = [];
    this.model.synapses = synapseRegister;
    this.notifyTyp = "success";
    this.notifyMsg = "New Data Model";
    return this.model;
  }

}
