import { NeoCortexModel, Area, Synapse, Minicolumn, Cell, NeocortexSettings, InputModel, CellId } from './neocortexmodel';


export class neoCortexUtils {


  // public static createId(settings:neocortexSettings, areaIndx:number = 0, miniColIndx:number[], layer:number) : number
  // {
  //   let cellSegmentSz = 1;
  //   for (let i = 0; i < settings.minicolumnDims.length; i++) {
  //     cellSegmentSz = cellSegmentSz * settings.minicolumnDims[i];      
  //   }

  //   let multiDimSegmentSize = settings.numAreas * cellSegmentSz * settings.numLayers;

  //   let flatIndx = 0;
  //   for (let i = 0; i < miniColIndx.length -1; i++) {

  //     flatIndx = flatIndx + settings.minicolumnDims[i] * miniColIndx[i];    
  //   }
  //    let id = areaIndx * multiDimSegmentSize +  cellSegmentSz * 

  //    return id;
  // }

  /**
   * createModel (numberOfAreas/DataSeries, [xAxis, zAxis], yAxis)
   * @param areas 
   * @param miniColDims 
   * @param numLayers 
   */
  //public static createModel(areas: number = 1, miniColDims: number[] = [1000, 3], numLayers: number = 6): NeoCortexModel {
  public static createModel(numOfAreas, miniColDims, numLayers): NeoCortexModel {

    const sensoryAreaId = 0;
    const sensoryLayer = 3;

    let sett: NeocortexSettings = {
      numAreas: numOfAreas, minicolumnDims: miniColDims, numLayers: numLayers,
      cellHeightInMiniColumn: 5, miniColumnWidth: 5
    };

    let inpModel: InputModel = new InputModel(sett, [3, 3]);

    var model: NeoCortexModel = new NeoCortexModel(sett, inpModel);

    let idCnt: number = 0;

    for (let arrIndx = 0; arrIndx < sett.numAreas; arrIndx++) {
      
      model.areas[arrIndx].minicolumns.forEach(miniColRow => {
        miniColRow.forEach(miniCol => {

          // Selecting random input cell to cennect.
          let rndInpRowIndx = Math.floor(Math.random() * inpModel.cells.length);
          let rndInpCellIndx = Math.floor(Math.random() * inpModel.cells[rndInpRowIndx].length);

          for (let miniColIndx = 0; miniColIndx < inpModel.cells[rndInpRowIndx].length * 0.2; miniColIndx++) {
            this.addSynapse(model, ++idCnt, model.areas[arrIndx].id, model.input.cells[rndInpRowIndx][rndInpCellIndx], miniCol.cells[sensoryLayer], 0)
          }        
           
        });
      });

    }

    return model;
  }


  public static addSynapse(model: NeoCortexModel, id: number, areaId: number = -1, preCell: Cell, postCell: Cell, weight: number) {

    model.synapses.push(new Synapse(id, weight, preCell, postCell));
    
  }

  public static updateSynapse(model: NeoCortexModel, synapseId: number, areaId: number = -1, weight: number) {

    let synapse = this.lookupSynapse(model, synapseId, areaId);
    if (synapse != null) {
      synapse.permanence = weight;
    }
    else
      throw "Synapse cannot be found!";

  }

  public static updateNeuron(model: NeoCortexModel, id: number, weight: number) {


  }


  /**
   *  Search for synapse with specified id.
   * @param model 
   * @param synapseId 
   * @param [optional] areaId.If >= 0 then restricts search for area. If not specified, the it search for synapse in all areas.
   */
  public static lookupSynapse(model: NeoCortexModel, synapseId: number, areaId: number = -1): Synapse {

    if (areaId >= 0 && model.areas.length > areaId)
      return this.lookupSynapseInArea(model, synapseId, areaId);

    model.areas.forEach(area => {
      let synapse = this.lookupSynapseInArea(model, synapseId, area.id);
      if (synapse != null)
        return synapse;
    });

    return null;
  }


  /**
   * Search for synapse with specified id.
   * @param model Model of AI network.
   * @param synapseId Identifier of the synapse.
   * @param areaId Restricts the search in specified area to increase performance.
   */
  private static lookupSynapseInArea(model: NeoCortexModel, synapseId: number, areaId: number): Synapse {

    model.areas[areaId].minicolumns.forEach(minColRow => {
      minColRow.forEach(miniColumn => {
        miniColumn.cells.forEach(cell => {
          cell.Synapses.forEach(synapse => {
            if (synapse.id == synapseId)
              return synapse;
          });
        });
      });
    });

    return null;
  }

  
  /**
   * Search for synapse with specified id.
   * @param model Model of AI network.
   * @param synapseId Identifier of the synapse.
   * @param areaId Restricts the search in specified area to increase performance.
   */
  private static getCell(model: NeoCortexModel, cellId:CellId): Cell {

    let area : Area = model.areas[cellId.area];
    
    let obj:any[] =  area.minicolumns[0];
    
    for (let i = 1; i < area.minicolumns.length-1; i++) {
      obj =  obj[cellId.minicolumn[i]];
    }

    return obj[area.minicolumns.length-1] as Cell;
  }
}




