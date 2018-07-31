import { NeoCortexModel, Area, Synapse, Minicolumn, Cell, neocortexSettings } from './neocortexmodel';




export class neoCortexUtils {

 
  /**
   * 
   * @param areas 
   * @param minicolumns 
   * @param cellsInMinicolumn 
   */
  public static createModel(areas: number=1, minicolumns: number[]= [1000,3], cellsInMinicolumn: number=6) : NeoCortexModel { 

    var  sett :  neocortexSettings = { numAreas: areas, minicolumnDims: minicolumns, numCellsInMinicolumn: cellsInMinicolumn  };

    var  model:NeoCortexModel = new NeoCortexModel(sett);

    return model; 
  }


  public static updateSynapse(posX : number, posY: number, permanence : number)  { 

    
  }
}




