import { Component, Input, Output, EventEmitter } from '@angular/core';
import { NeoCortexGenerator } from './Entities/NeoCortexGenerator';
import { NeoCortexUtilsService } from './Services/neocortexutils.service';
import { sendRequest } from 'selenium-webdriver/http';
import { UpdateNeuralColumn, UpdateSynapse } from './Entities/UpdateProperties';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.sass']
})
export class AppComponent {
  title = 'Transmitter';
  updateNeuralColumn: UpdateNeuralColumn;
  updateSynaspse: UpdateSynapse;

  constructor(private neoUtils: NeoCortexUtilsService) {
    this.updateNeuralColumn = new UpdateNeuralColumn();
    this.updateSynaspse = new UpdateSynapse();


  }

  sendModel() {
    //([0, 0, 0, 1, 2, 1], [10, 1], 6)
    let Model = new NeoCortexGenerator().createModel([0, 0, 0, 1, 2, 1], [10, 1], 6);

    this.neoUtils.data.next({
      MsgType: "init",
      clientType: "Transmitter",
      dataModel: Model
    });
  }

  updateOverlapInNeuralColumn() {
    // updateOverlap(areaID, miniColumnXDimension, miniColumnZDimension, newOverlapValue) {
    let areaID = parseInt(this.updateNeuralColumn.areaID);
    let miniColumnXDimension = parseInt(this.updateNeuralColumn.miniColXDim);
    let miniColumnZDimension = parseInt(this.updateNeuralColumn.miniColZDim);
    let newOverlapValue = parseFloat(this.updateNeuralColumn.updateOverlap);

    const updateOverlap = {
      areaIDOfCell: areaID,
      minColXDim: miniColumnXDimension,
      minColZDim: miniColumnZDimension,
      updateOverlapValue: newOverlapValue
    };
    this.neoUtils.data.next({
      MsgType: "updateOverlap",
      clientType: "Transmitter",
      Columns: updateOverlap
    });

  }

  // (click)='updateOrAddSynapse(preCellAreaID.value, postCellAreaID.value, pre_Cell.value, post_Cell.value, permanence.value )'>
  updateOrAddSynapse() {
    let permaValue = parseFloat(this.updateSynaspse.updatePermanence);
    let preCellArea = parseInt(this.updateSynaspse.preCellAreaID);
    let postCellArea = parseInt(this.updateSynaspse.postCellAreaID);
    let prX = parseInt(this.updateSynaspse.preCell[0]);
    let prY = parseInt(this.updateSynaspse.preCell[2]);
    let prZ = parseInt(this.updateSynaspse.preCell[4]);
    let poX = parseInt(this.updateSynaspse.postCell[0]);
    let poY = parseInt(this.updateSynaspse.postCell[2]);
    let poZ = parseInt(this.updateSynaspse.postCell[4]);

    const updateOrAddSynapse = {
      preCellAreaId: preCellArea,
      postCellAreaId: postCellArea,
      preCell:
      {
        cellX: prX,
        cellY: prY,
        cellZ: prZ,
      },
      postCell: {
        cellX: poX,
        cellY: poY,
        cellZ: poZ,
      },
      permanence: permaValue
    };
    this.neoUtils.data.next({
      MsgType: "updateOrAddSynapse",
      clientType: "Transmitter",
      Synapses: updateOrAddSynapse

    });

  }
}
