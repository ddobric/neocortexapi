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
    let model = new NeoCortexGenerator().createModel([0, 0, 0, 1, 2, 1], [10, 1], 6);

    this.neoUtils.data.next({
      msgType: "init",
      clientType: "Transmitter",
      dataModel: model
    });
  }
  //updateOverlap(areaID.value, miniColumnXDimension.value,miniColumnZDimension.value,newOverlapValue.value)'>Update
  updateOverlap(areaIDOfCell: any, minColXDim: any, minColZDim: any, updateOverlapValue: any) {
    // updateOverlap(areaID, miniColumnXDimension, miniColumnZDimension, newOverlapValue) {

    let areaID = parseInt(areaIDOfCell);
    let miniColumnXDimension = parseInt(minColXDim);
    let miniColumnZDimension = parseInt(minColZDim);
    let newOverlapValue = parseFloat(updateOverlapValue);

    const updateOverlap = {
      areaIDOfCell: areaID,
      minColXDim: miniColumnXDimension,
      minColZDim: miniColumnZDimension,
      updateOverlapValue: newOverlapValue
    };
    this.neoUtils.data.next({
      msgType: "updateOverlap",
      clientType: "Transmitter",
      update: updateOverlap
    });

  }

  // (click)='updateOrAddSynapse(preCellAreaID.value, postCellAreaID.value, pre_Cell.value, post_Cell.value, permanence.value )'>
  updateOrAddSynapse(preCellAreaID: any, postCellAreaID: any, preCell: any, postCell: any, permanence: any) {
    let permaValue = parseFloat(permanence);
    let preCellArea = parseInt(preCellAreaID);
    let postCellArea = parseInt(postCellAreaID);
    let prX = parseInt(preCell[0]);
    let prY = parseInt(preCell[2]);
    let prZ = parseInt(preCell[4]);
    let poX = parseInt(postCell[0]);
    let poY = parseInt(postCell[2]);
    let poZ = parseInt(postCell[4]);

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
      msgType: "updateOrAddSynapse",
      clientType: "Transmitter",
      update: updateOrAddSynapse

    });

  }
}
