import { NeoCortexModel, Area, Synapse, Minicolumn, Cell, NeocortexSettings, InputModel, CellId } from './neocortexmodel';
import { all } from 'q';


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


    preCell0 = new Cell(0, 0, 0, 0, [null], [null]);
    postCell1 = new Cell(0, 0, 1, 0, [null], [null]);

    let incomingSynap0 = new Synapse(0, preCell0, postCell1);
    let outgoingSynap1 = new Synapse(0, preCell0, postCell1);

    preCell0 = new Cell(0, 0, 0, 0, [null], [outgoingSynap1]);
    postCell1 = new Cell(0, 0, 1, 0, [incomingSynap0], [null]);



    let preCell3: Cell;
    let postCell4: Cell;

    preCell3 = new Cell(0, 0, 3, 0, [null], [null]);
    postCell4 = new Cell(0, 9, 0, 0, [null], [null]);
    let incomingSynap3 = new Synapse(0, preCell3, postCell4);
    let outgoingSynap4 = new Synapse(0, preCell3, postCell4);

    preCell3 = new Cell(0, 0, 3, 0, [null], [outgoingSynap4]);
    postCell4 = new Cell(0, 9, 0, 0, [incomingSynap3], [null]);


    let preCell2: Cell;
    let postCell3: Cell;
    preCell2 = new Cell(3, 0, 0, 0, [null], [null]);
    postCell3 = new Cell(3, 0, 1, 0, [null], [null]);
    let incomingSynap2 = new Synapse(0, preCell2, postCell3);
    let outgoingSynap3 = new Synapse(0, preCell2, postCell3);
    preCell2 = new Cell(3, 0, 0, 0, [null], [outgoingSynap3]);
    postCell3 = new Cell(3, 0, 1, 0, [incomingSynap2], [null]);


    let preCell5: Cell;
    let postCell6: Cell;
    preCell5 = new Cell(1, 2, 2, 0, [null], [null]);
    postCell6 = new Cell(3, 2, 3, 0, [null], [null]);
    let incomingSynap5 = new Synapse(0, preCell5, postCell6);
    let outgoingSynap6 = new Synapse(0, preCell5, postCell6);
    preCell5 = new Cell(1, 2, 2, 0, [null], [outgoingSynap6]);
    postCell6 = new Cell(3, 2, 3, 0, [incomingSynap5], [null]);

    let preCell7: Cell;
    let postCell8: Cell;
    preCell7 = new Cell(0, 0, 1, 0, [null], [null]);
    postCell8 = new Cell(0, 0, 2, 0, [null], [null]);
    let incomingSynap7 = new Synapse(0, preCell7, postCell8);
    let outgoingSynap8 = new Synapse(0, preCell7, postCell8);
    preCell7 = new Cell(0, 0, 1, 0, [null], [outgoingSynap8]);
    postCell8 = new Cell(0, 0, 2, 0, [incomingSynap7], [null]);

    let preCell9: Cell;
    let postCell10: Cell;
    preCell9 = new Cell(3, 0, 1, 0, [null], [null]);
    postCell10 = new Cell(3, 0, 2, 0, [null], [null]);
    let incomingSynap9 = new Synapse(0.80, preCell9, postCell10);
    let outgoingSynap10 = new Synapse(0.80, preCell9, postCell10);
    preCell9 = new Cell(3, 0, 1, 0, [null], [outgoingSynap10]);
    postCell10 = new Cell(3, 0, 2, 0, [incomingSynap9], [null]);

    let inpModel: InputModel = new InputModel(sett);

    let synaps01 = new Synapse(0, preCell0, postCell1);
    let synap23 = new Synapse(0.5, preCell2, postCell3);
    let synaps34 = new Synapse(0.25, preCell3, postCell4);
    let synap56 = new Synapse(0.65, preCell5, postCell6);
    let synap78 = new Synapse(1, preCell7, postCell8);
    let synap910 = new Synapse(0.80, preCell9, postCell10);

    function getRandomInt(max) {
      return Math.floor(Math.random() * Math.floor(max));
    }

    let arrayOfPreCells: Array<Cell> = [];
    let arrayOfPostCells: Array<Cell> = [];
    let preCellAreaID = getRandomInt(5);
    let preCellX = getRandomInt(9);
    let preCellY = getRandomInt(5);
    let preCellZ = getRandomInt(0);

    let postCellAreaID = getRandomInt(5);
    let postCellX = getRandomInt(9);
    let postCellY = getRandomInt(5);
    let postCellZ = getRandomInt(0);
    for (let cell = 0; cell < 100; cell++) {
      arrayOfPreCells.push(new Cell(getRandomInt(5), getRandomInt(9), getRandomInt(5), getRandomInt(0), [], []));
      arrayOfPostCells.push(new Cell(getRandomInt(5), getRandomInt(9), getRandomInt(5), getRandomInt(0), [], []));
    }

    let outgoingSynapses: Array<Synapse> = [];
    let incomingSynapses: Array<Synapse> = [];
    let randomPermanence = Math.random();
    let defaultPermanence = 0;

    for (let i = 0; i < arrayOfPreCells.length; i++) {
      outgoingSynapses.push(new Synapse(defaultPermanence, arrayOfPreCells[i], arrayOfPostCells[i]));
      incomingSynapses.push(new Synapse(defaultPermanence, arrayOfPreCells[i], arrayOfPostCells[i]));
    }
 

    for (let j = 0; j < arrayOfPreCells.length; j++) {

      let numberOfOutSynapses = getRandomInt(3);
      let numberOfInSynapses = getRandomInt(3);
      let outsynap: number;
      let outsynapArr = [];
      for (let synapMaxIndex = 0; synapMaxIndex < numberOfOutSynapses; synapMaxIndex++) {
        outsynapArr.push(outsynap = getRandomInt(outgoingSynapses.length));
      }
      let insynap: number;
      let insynapArr = [];
      for (let insynapMaxIndex = 0; insynapMaxIndex < numberOfInSynapses; insynapMaxIndex++) {
        insynapArr.push(insynap = getRandomInt(incomingSynapses.length));
      }

      for (let numOfOutSynap = 0; numOfOutSynap < outsynapArr.length; numOfOutSynap++) {
        //addoutgoingSynapse = outgoingSynapses[outsynapArr[numOfOutSynap]];
        arrayOfPreCells[j].outgoingSynapses.push(outgoingSynapses[outsynapArr[numOfOutSynap]]);
      }
      for ( let numOfInSynap = 0; numOfInSynap < insynapArr.length; numOfInSynap++) {
        //addoutgoingSynapse = outgoingSynapses[outsynapArr[numOfOutSynap]];
        arrayOfPostCells[j].incomingSynapses.push(incomingSynapses[insynapArr[numOfInSynap]]);
      }
      //let addoutgoingSynapse: Synapse = outgoingSynapses[j];
    
     
      
    }
    let allSynapses: Array<Synapse> = [];

    for (let k = 0; k < arrayOfPreCells.length; k++) {
      allSynapses.push(new Synapse(Math.random(), arrayOfPreCells[k], arrayOfPostCells[k]));
    }

    //var model: NeoCortexModel = new NeoCortexModel(sett, inpModel, 0, 0, 0, [synaps01, synaps34, synap23, synap56, synap78, synap910]);

    var model: NeoCortexModel = new NeoCortexModel(sett, inpModel, 0, 0, 0, allSynapses);

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




