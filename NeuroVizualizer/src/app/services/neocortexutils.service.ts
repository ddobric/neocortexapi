import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { WebsocketService } from "./websocket.service";
import { map } from "rxjs/operators";
import { Cell, Synapse, NeoCortexModel } from '../Entities/NeoCortexModel';
import { environment as env } from '../../environments/environment.prod';




export class NeoCortexUtils {
  dataModel: NeoCortexModel;
  clientType?: any;
  msgType?: any;
  //notification?: any;
  notification?= {
    msg: "",
    title: "",
    type: ""
  }

}

@Injectable({
  providedIn: 'root'
})

export class NeoCortexUtilsService {

  data: Subject<NeoCortexUtils>;
  Model: any;
  notifyTyp: any;
  notifyMsg: any;
  notifyTitle: any;

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
          if (this.Model.Cells[0].Z == "undefined") {
            this.addPlaceholder();
          }
          this.createSynapses();
          return {
            dataModel: this.Model,
            notification: { type: this.notifyTyp, msg: this.notifyMsg, title: this.notifyTitle }
          }
        }
        else if (JSONObject.MsgType == "updateColumn") {
          this.updateOverlap(JSONObject.Columns);
          return {
            dataModel: this.Model,
            notification: { type: this.notifyTyp, msg: this.notifyMsg, title: this.notifyTitle }
          }
        }
        else if (JSONObject.MsgType == "updateOrAddSynapse") {
          this.updateOrAddSynapse(JSONObject.Synapses);
          return {
            dataModel: this.Model,
            notification: { type: this.notifyTyp, msg: this.notifyMsg, title: this.notifyTitle }
          }
        }
        else if (JSONObject.MsgType == "updateCells") {
          this.updateCells(JSONObject.Cells);
          return {
            dataModel: this.Model,
            notification: { type: this.notifyTyp, msg: this.notifyMsg, title: this.notifyTitle }
          }

        } else {

        }

      }
    ));

  }

  updateCells(Cells: any) {
    throw new Error("Method not implemented.");
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

        let preCell = this.Model.Areas[searchSynapse[i].preCellAreaId].MiniColumns[searchSynapse[i].preCell.cellX][searchSynapse[i].preCell.cellZ].Cells[searchSynapse[i].preCell.cellY];
        let postCell = this.Model.Areas[searchSynapse[i].postCellAreaId].MiniColumns[searchSynapse[i].postCell.cellX][searchSynapse[i].postCell.cellZ].Cells[searchSynapse[i].postCell.cellY];


        let synapseFound = false;
        loop:
        for (let out = 0; out < preCell.outgoingSynapses.length; out++) {
          for (let inc = 0; inc < postCell.incomingSynapses.length; inc++) {

            if ((preCell.outgoingSynapses[out].PostSynaptic.Index === postCell.Index &&
              preCell.outgoingSynapses[out].PostSynaptic.ParentColumnIndex === postCell.ParentColumnIndex &&
              preCell.outgoingSynapses[out].PostSynaptic.Z === postCell.Z) &&

              (postCell.incomingSynapses[inc].PreSynaptic.Index === preCell.Index &&
                postCell.incomingSynapses[inc].PreSynaptic.ParentColumnIndex === preCell.ParentColumnIndex &&
                postCell.incomingSynapses[inc].PreSynaptic.Z === preCell.Z)) {

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


    } catch (ex) {
      this.notifyTyp = "error";
      this.notifyMsg = ex;
      this.notifyTitle = "Error";

    }


  }
  private updatePermanenceOfSynapse(newPermanence: number, preCell: Cell, postCell: Cell) {

    for (let findSynapse = 0; findSynapse < this.Model.Synapse.length; findSynapse++) {

      if (this.Model.Synapse[findSynapse].PreSynaptic.AreaID === preCell.AreaID &&
        this.Model.Synapse[findSynapse].PreSynaptic.Index === preCell.Index &&
        this.Model.Synapse[findSynapse].PreSynaptic.ParentColumnIndex === preCell.ParentColumnIndex &&
        this.Model.Synapse[findSynapse].PreSynaptic.Z === preCell.Z &&

        this.Model.Synapse[findSynapse].PostSynaptic.AreaID === postCell.AreaID &&
        this.Model.Synapse[findSynapse].PostSynaptic.Index === postCell.Index &&
        this.Model.Synapse[findSynapse].PostSynaptic.ParentColumnIndex === postCell.ParentColumnIndex &&
        this.Model.Synapse[findSynapse].PostSynaptic.Z === postCell.Z) {

        this.Model.Synapse[findSynapse].Permanence = newPermanence;
        this.notifyTyp = "info";
        this.notifyMsg = "Synapse found";
        this.notifyTitle = "Permanence updated";

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
      Permanence: synapsePermanence,
      PreSynaptic: preCell,
      PostSynaptic: postCell
    };


    preCell.outgoingSynapses.push(newSynapse);
    postCell.incomingSynapses.push(newSynapse);

    /*  this.Model.Areas[preCell.AreaID].Minicolumns[preCell.X][preCell.Z].Cells[preCell.Layer].outgoingSynapses.push(newSynapse);
     this.Model.Areas[postCell.AreaID].Minicolumns[postCell.X][postCell.Z].Cells[postCell.Layer].incomingSynapses.push(newSynapse); */

    this.Model.Areas[preCell.AreaID].MiniColumns[preCell.Index][preCell.Z].Cells[preCell.ParentColumnIndex].outgoingSynapses.push(newSynapse);
    this.Model.Areas[postCell.AreaID].MiniColumns[postCell.Index][postCell.Z].Cells[postCell.ParentColumnIndex].incomingSynapses.push(newSynapse);

    //console.log("Synapse will be created");

    this.Model.Synapse.push(newSynapse);
    this.notifyTyp = "success";
    this.notifyMsg = "Synapse doesn't found";
    this.notifyTitle = "New Synapse created";

  }


  private updateOverlap(updateOverlap: any) {
    try {

      for (let i = 0; i < updateOverlap.length; i++) {
        this.Model.Areas[updateOverlap[i].areaIDOfCell].MiniColumns[updateOverlap[i].minColXDim][updateOverlap[i].minColZDim].overlap = updateOverlap[i].updateOverlapValue;
        this.notifyTyp = "success";
        this.notifyMsg = "Overlap";
        this.notifyTitle = "Update";
      }


    } catch (ex) {
      this.notifyTyp = "error";
      this.notifyMsg = ex;
      this.notifyTitle = "Error";

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
      let preCell: Cell = this.binaryCellSearch(this.Model.Synapse[i].PreSynapticCellId, 0, upper);
      let postCell: Cell = this.binaryCellSearch(this.Model.Synapse[i].PostSynapticCellId, 0, upper);


      let synapse: Synapse = {
        Permanence: perm,
        PreSynaptic: preCell,
        PostSynaptic: postCell
      };

      synapseRegister.push(synapse);

      preCell.outgoingSynapses.push(synapse);
      this.Model.Areas[preCell.AreaID].MiniColumns[preCell.Index][preCell.Z].Cells[preCell.ParentColumnIndex].outgoingSynapses.push(synapse);


      postCell.incomingSynapses.push(synapse);
      this.Model.Areas[postCell.AreaID].MiniColumns[postCell.Index][postCell.Z].Cells[postCell.ParentColumnIndex].incomingSynapses.push(synapse);

    }

    this.Model.Synapse = [];
    this.Model.Synapse = synapseRegister;
    this.notifyTyp = "success";
    this.notifyMsg = "New Data";
    this.notifyTitle = "Model";
    return this.Model;
  }

}
