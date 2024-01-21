import { NeoCortexModel, Cell, NeocortexSettings, InputModel } from './neocortexmodel';


export class neoCortexUtils {

  cells: Array<Cell>;

  /**
   * createModel (numberOfAreas/DataSeries, [xAxis, zAxis], yAxis)
   * @param areas 
   * @param miniColDims 
   * @param numLayers 
   */
  static createModel(areaLevels: number[], miniColDims: number[], numLayers: number): NeoCortexModel {
    // createModel (numberOfAreas, [xAxis, zAxis], yAxis)

    let sett: NeocortexSettings = new NeocortexSettings();
    sett.minicolumnDims = miniColDims;
    sett.areaLevels = areaLevels;
    sett.numLayers = numLayers;

    let inpModel: InputModel = new InputModel(sett.minicolumnDims[0], sett.minicolumnDims[1]);
    var model: NeoCortexModel = new NeoCortexModel(sett, inpModel, 0, 0, 0, []);


    return model;
  }

}