import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { WebsocketService } from "./websocket.service";
import { map } from "rxjs/operators";
import { Cell, Synapse } from '../Entities/NeoCortexModel';
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
  Model: any;
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

        if (JSONObject.msgType == "init" || JSONObject.MsgType == "init") {
          this.Model = JSONObject.Model;
          if (!this.Model.Cells[0].Z) {
            this.addPlaceholder();
          }
          this.createSynapses();
          return {
            dataModel: this.Model,
            notification: { type: this.notifyTyp, msg: this.notifyMsg }
          }
        }
        else if (JSONObject.msgType == "updateOverlap" || JSONObject.MsgType == "updateOverlap") {
          this.updateOverlap(JSONObject.Columns);
          return {
            dataModel: this.Model,
            notification: { type: this.notifyTyp, msg: this.notifyMsg }
          }
        }
        else if (JSONObject.msgType == "updateOrAddSynapse" || JSONObject.MsgType == "updateOrAddSynapse") {
          this.updateOrAddSynapse(JSONObject.Synapses);
          return {
            dataModel: this.Model,
            notification: { type: this.notifyTyp, msg: this.notifyMsg }
          }

        } else {

        }

      }
    ));

  }
  addPlaceholder() {
    for (let i = 0; i < this.Model.Areas.length; i++) {
      for (let j = 0; j < this.Model.Areas[i].MiniColumns.length; j++) {
        for (let k = 0; k < this.Model.Areas[i].MiniColumns[j][0].Cells.length; k++) {
          let cell = {
            CellId: this.Model.Areas[i].MiniColumns[j][0].Cells[k].CellId,
            Index: this.Model.Areas[i].MiniColumns[j][0].Cells[k].Index,
            ParentColumnIndex: this.Model.Areas[i].MiniColumns[j][0].Cells[k].ParentColumnIndex,
            Z: 0
          };
          this.Model.Areas[i].MiniColumns[j][0].Cells[k] = cell;
        }

      }

    }
    for (let l = 0; l < this.Model.Cells.length; l++) {
      let cell1 = {
        CellId: this.Model.Cells[l].CellId,
        Index: this.Model.Cells[l].Index,
        ParentColumnIndex: this.Model.Cells[l].ParentColumnIndex,
        Z: 0
      };
      this.Model.Cells[l] = cell1;
    }
  }
  private updateOrAddSynapse(updateOrAddSynap) {
    this.lookUpSynapse(updateOrAddSynap);

  }

  private lookUpSynapse(searchSynapse) {
    try {

      for (let i = 0; i < searchSynapse.length; i++) {

        let preCell = this.Model.Areas[searchSynapse[i].preCellAreaId].Minicolumns[searchSynapse[i].preCell.cellX][searchSynapse[i].preCell.cellZ].Cells[searchSynapse[i].preCell.cellY];
        let postCell = this.Model.Areas[searchSynapse[i].postCellAreaId].Minicolumns[searchSynapse[i].postCell.cellX][searchSynapse[i].postCell.cellZ].Cells[searchSynapse[i].postCell.cellY];


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
              this.updatePermanenceOfSynapse(searchSynapse[i].permanence, preCell, postCell);
              synapseFound = true;
              break loop;
            }


          }
        }
        if (synapseFound === false) {
          //Console.log("Synapse doesn't Exists", "It will be created", 'info');
          this.generateNewSynapse(searchSynapse[i].permanence, preCell, postCell);
        }

      }


    } catch (error) {
      this.notifyTyp = "error";
      this.notifyMsg = error;

    }


  }
  private updatePermanenceOfSynapse(newPermanence: number, preCell: Cell, postCell: Cell) {

    for (let findSynapse = 0; findSynapse < this.Model.Synapses.length; findSynapse++) {

      if (this.Model.Synapses[findSynapse].preSynaptic.areaIndex === preCell.areaIndex &&
        this.Model.Synapses[findSynapse].preSynaptic.X === preCell.X &&
        this.Model.Synapses[findSynapse].preSynaptic.Layer === preCell.Layer &&
        this.Model.Synapses[findSynapse].preSynaptic.Z === preCell.Z &&

        this.Model.Synapses[findSynapse].postSynaptic.areaIndex === postCell.areaIndex &&
        this.Model.Synapses[findSynapse].postSynaptic.X === postCell.X &&
        this.Model.Synapses[findSynapse].postSynaptic.Layer === postCell.Layer &&
        this.Model.Synapses[findSynapse].postSynaptic.Z === postCell.Z) {

        this.Model.Synapses[findSynapse].permanence = newPermanence;
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

    this.Model.Areas[preCell.areaIndex].Minicolumns[preCell.X][preCell.Z].Cells[preCell.Layer].outgoingSynapses.push(newSynapse);
    this.Model.Areas[postCell.areaIndex].Minicolumns[postCell.X][postCell.Z].Cells[postCell.Layer].incomingSynapses.push(newSynapse);

    //console.log("Synapse will be created");

    this.Model.Synapses.push(newSynapse);
    this.notifyTyp = "success";
    this.notifyMsg = "Synapse doesn't found, new created";

  }


  private updateOverlap(updateOverlapCo: any) {
    try {
      this.Model.Areas[updateOverlapCo.areaIDOfCell].Minicolumns[updateOverlapCo.minColXDim][updateOverlapCo.minColZDim].overlap = updateOverlapCo.updateOverlapValue;
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

    if (this.Model.Cells[midElement].CellId == synapticId) {
      cell = this.Model.Cells[midElement];
      return cell;
    }
    else {
      if (synapticId < this.Model.Cells[midElement].CellId) {
        return this.binaryCellSearch(synapticId, lower, midElement - 1);

      }
      else {
        return this.binaryCellSearch(synapticId, midElement + 1, upper);
      }
    }

  }

  private createSynapses() {
    let synapseRegister = [];
    let upper = this.Model.Cells.length;

    for (let i = 0; i < this.Model.Synapse.length; i++) {

      let perm = this.Model.Synapse[i].Permanence;
      let preCell: Cell = this.binaryCellSearch(this.Model.Synapse[i].PreSynapticCellIndex, 0, upper);
      let postCell: Cell = this.binaryCellSearch(this.Model.Synapse[i].PostSynapticCellIndex, 0, upper);


      let synapse: Synapse = {
        permanence: perm,
        preSynaptic: preCell,
        postSynaptic: postCell
      };

      synapseRegister.push(synapse);

      preCell.outgoingSynapses.push(synapse);
      this.Model.Areas[preCell.ParentColumnIndex].MiniColumns[preCell.Index][preCell.Z].Cells[preCell.Index].outgoingSynapses.push(synapse);


      postCell.incomingSynapses.push(synapse);
      this.Model.Areas[postCell.ParentColumnIndex].MiniColumns[postCell.Index][postCell.Z].Cells[postCell.Index].incomingSynapses.push(synapse);

    }

    this.Model.Synapses = [];
    this.Model.Synapses = synapseRegister;
    this.notifyTyp = "success";
    this.notifyMsg = "New Data Model";
    return this.Model;
  }

}
