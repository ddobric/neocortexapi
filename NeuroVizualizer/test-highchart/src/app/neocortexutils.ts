import { NeoCortexModel, Area, Synapse, Minicolumn, Cell, NeocortexSettings, InputModel, CellId } from './neocortexmodel';


export class neoCortexUtils {



  /**
   * createModel (numberOfAreas/DataSeries, [xAxis, zAxis], yAxis)
   * @param areas 
   * @param miniColDims 
   * @param numLayers 
   */
  public static createModel(areaLevels: number[], miniColDims: number[], numLayers: number): NeoCortexModel {

    let sett: NeocortexSettings = new NeocortexSettings();
    sett.minicolumnDims = miniColDims;
    sett.areaLevels = areaLevels;
    sett.numLayers = numLayers;
    let preCell0: Cell;
    let postCell1: Cell;


    preCell0 = new Cell(sett, null, null, 0, 0, 0, [null], [null]);
    postCell1 = new Cell(sett, null, null, 0, 1, 0, [null], [null]);

    let incomingSynap0 = new Synapse(0,0, 0, preCell0, postCell1);
    let outgoingSynap1 = new Synapse(0,0, 0, preCell0, postCell1);

    preCell0 = new Cell(sett, null, null, 0, 0, 0, [null], [outgoingSynap1]);
    postCell1 = new Cell(sett, null, null, 0, 1, 0, [incomingSynap0], [null]);



    let preCell3: Cell;
    let postCell4: Cell;

    preCell3 = new Cell(sett, null, null, 0, 3, 0, [null], [null]);
    postCell4 = new Cell(sett, null, null, 9, 0, 0, [null], [null]);
    let incomingSynap3 = new Synapse(0,0, 0, preCell3, postCell4);
    let outgoingSynap4 = new Synapse(0, 0,0, preCell3, postCell4);

    preCell3 = new Cell(sett, null, null, 0, 3, 0, [null], [outgoingSynap4]);
    postCell4 = new Cell(sett, null, null, 9, 0, 0, [incomingSynap3], [null]);

 
    let preCell2: Cell;
    let postCell3: Cell;
    preCell2 = new Cell(sett, null, null, 0, 0, 0, [null], [null]);
    postCell3 = new Cell(sett, null, null, 0, 1, 0, [null], [null]);
    let incomingSynap2 = new Synapse(3,3, 0, preCell2, postCell3);
    let outgoingSynap3 = new Synapse(3,3, 0, preCell2, postCell3);
    preCell2 = new Cell(sett, null, null, 0, 0, 0, [null], [outgoingSynap3]);
    postCell3 = new Cell(sett, null, null, 0, 1, 0, [incomingSynap2], [null]); 

     
    let preCell5: Cell;
    let postCell6: Cell;
    preCell5 = new Cell(sett, null, null, 2, 2, 0, [null], [null]);
    postCell6 = new Cell(sett, null, null, 2, 3, 0, [null], [null]);
    let incomingSynap5 = new Synapse(1,3, 0, preCell5, postCell6);
    let outgoingSynap6 = new Synapse(1,3, 0, preCell5, postCell6);
    preCell5 = new Cell(sett, null, null, 2, 2, 0, [null], [outgoingSynap6]);
    postCell6 = new Cell(sett, null, null, 2, 3, 0, [incomingSynap5], [null]); 



    let inpModel: InputModel = new InputModel(sett);

    let synaps01 = new Synapse(0,0, 0, preCell0, postCell1);
    let synaps34 = new Synapse(0,0, 0.25, preCell3, postCell4);
    let synap23 = new Synapse(3,3, 0.5, preCell2, postCell3);
    let synap56 = new Synapse(3,3, 0.65, preCell5, postCell6);
    let synap561 = new Synapse(0,0, 0.80, preCell5, postCell6);
    let synap562 = new Synapse(1,3, 1, preCell5, postCell6);

    var model: NeoCortexModel = new NeoCortexModel(sett, inpModel, 0, 0, 0, [synaps01, synaps34, synap23, synap56,synap562, synap561]);


    return model;
  }


  public static addSynapse(model: NeoCortexModel, id: number, areaId: number = 1, preCell: Cell, postCell: Cell, weight: number) {

   // model.synapses.push(new Synapse(id, weight, preCell, postCell));

  }

  /*   public static updateSynapse(model: NeoCortexModel, synapseId: number, areaId: number = -1, weight: number) {
  
      let synapse = this.lookupSynapse(model, synapseId, areaId);
      if (synapse != null) {
        synapse.permanence = weight;
      }
      else
        throw "Synapse cannot be found!";
  
    } */

  /* public static updateNeuron(model: NeoCortexModel, id: number, weight: number) {


  } */


  /**
   *  Search for synapse with specified id.
   * @param model 
   * @param synapseId 
   * @param [optional] areaId.If >= 0 then restricts search for area. If not specified, the it search for synapse in all areas.
   */
  /*  public static lookupSynapse(model: NeoCortexModel, synapseId: number, areaId: number = -1): Synapse {
 
     if (areaId >= 0 && model.areas.length > areaId)
       return this.lookupSynapseInArea(model, synapseId, areaId);
 
     model.areas.forEach(area => {
       let synapse = this.lookupSynapseInArea(model, synapseId, area.id);
       if (synapse != null)
         return synapse;
     });
 
     return null;
   } */


  /**
   * Search for synapse with specified id.
   * @param model Model of AI network.
   * @param synapseId Identifier of the synapse.
   * @param areaId Restricts the search in specified area to increase performance.
   */
  /*   private static lookupSynapseInArea(model: NeoCortexModel, synapseId: number, areaId: number): Synapse {
  
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
    } */


  /**
   * Search for synapse with specified id.
   * @param model Model of AI network.
   * @param synapseId Identifier of the synapse.
   * @param areaId Restricts the search in specified area to increase performance.
   */
  private static getCell(model: NeoCortexModel, cellId: CellId): Cell {

    let area: Area = model.areas[cellId.area];

    let obj: any[] = area.minicolumns[0];

    for (let i = 1; i < area.minicolumns.length - 1; i++) {
      obj = obj[cellId.minicolumn[i]];
    }

    return obj[area.minicolumns.length - 1] as Cell;
  }
}




