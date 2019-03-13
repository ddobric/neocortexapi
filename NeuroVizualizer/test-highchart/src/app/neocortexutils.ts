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


    preCell0 = new Cell(0, 0, 0, 0, [], []);
    postCell1 = new Cell(0, 0, 1, 0, [], []);

    let incomingSynap0 = new Synapse(0, preCell0, postCell1);
    let outgoingSynap1 = new Synapse(0, preCell0, postCell1);
    preCell0.outgoingSynapses.push(outgoingSynap1);
    postCell1.incomingSynapses.push(incomingSynap0);



    let preCell3: Cell;
    let postCell4: Cell;

    preCell3 = new Cell(0, 0, 3, 0, [], []);
    postCell4 = new Cell(0, 9, 0, 0, [], []);
    let incomingSynap3 = new Synapse(0, preCell3, postCell4);
    let outgoingSynap4 = new Synapse(0, preCell3, postCell4);

    preCell3.outgoingSynapses.push(outgoingSynap4);
    postCell4.incomingSynapses.push(incomingSynap3);


    let preCell2: Cell;
    let postCell3: Cell;
    preCell2 = new Cell(3, 0, 0, 0, [], []);
    postCell3 = new Cell(3, 0, 1, 0, [], []);
    let incomingSynap2 = new Synapse(0, preCell2, postCell3);
    let outgoingSynap3 = new Synapse(0, preCell2, postCell3);
    preCell2.outgoingSynapses.push(outgoingSynap3);
    postCell3.incomingSynapses.push(incomingSynap2);


    let preCell5: Cell;
    let postCell6: Cell;
    preCell5 = new Cell(1, 2, 2, 0, [], []);
    postCell6 = new Cell(3, 2, 3, 0, [], []);
    let incomingSynap5 = new Synapse(0, preCell5, postCell6);
    let outgoingSynap6 = new Synapse(0, preCell5, postCell6);
    preCell5.outgoingSynapses.push(outgoingSynap6);
    postCell6.incomingSynapses.push(incomingSynap5);

    let preCell7: Cell;
    let postCell8: Cell;
    preCell7 = new Cell(0, 0, 1, 0, [], []);
    postCell8 = new Cell(0, 0, 2, 0, [], []);
    let incomingSynap7 = new Synapse(0, preCell7, postCell8);
    let outgoingSynap8 = new Synapse(0, preCell7, postCell8);
    preCell7.outgoingSynapses.push(outgoingSynap8);
    postCell8.incomingSynapses.push(incomingSynap7);

    let preCell9: Cell;
    let postCell10: Cell;
    preCell9 = new Cell(3, 0, 1, 0, [], []);
    postCell10 = new Cell(3, 0, 2, 0, [], []);
    let incomingSynap9 = new Synapse(0.80, preCell9, postCell10);
    let outgoingSynap10 = new Synapse(0.80, preCell9, postCell10);
    preCell9.outgoingSynapses.push(outgoingSynap10);
    postCell10.incomingSynapses.push(incomingSynap9);

    let inpModel: InputModel = new InputModel(sett.minicolumnDims[0], sett.minicolumnDims[1]);

    let synaps01 = new Synapse(0, preCell0, postCell1);
    let synap23 = new Synapse(0.5, preCell2, postCell3);
    let synaps34 = new Synapse(0.25, preCell3, postCell4);
    let synap56 = new Synapse(0.65, preCell5, postCell6);
    let synap78 = new Synapse(1, preCell7, postCell8);
    let synap910 = new Synapse(0.80, preCell9, postCell10);

    let synapses: Array<Synapse> = [synaps01,synap23,synaps34,synap56,synap78, synap910];

   

    function getRandomInt(max: any) {
      return Math.floor(Math.random() * Math.floor(max));
    }

    let arrayOfPreCells: Array<Cell> = [];
    let arrayOfPostCells: Array<Cell> = [];
    
    for (let cell = 0; cell < 10; cell++) {
      let randomPreCellAreaID = getRandomInt(sett.areaLevels.length);
    let randomPreCellX = getRandomInt(sett.minicolumnDims[0]);
    let randomPreCellY = getRandomInt(sett.numLayers);
    let randomPreCellZ = getRandomInt(sett.minicolumnDims[1]);

    let randomPostCellAreaID = getRandomInt(sett.areaLevels.length);
    let randomPostCellX = getRandomInt(sett.minicolumnDims[0]);
    let randomPostCellY = getRandomInt(sett.numLayers);
    let randomPostCellZ = getRandomInt(sett.minicolumnDims[1]);
      /* arrayOfPreCells.push(new Cell(getRandomInt(5), getRandomInt(9), getRandomInt(5), getRandomInt(0), [], []));
      arrayOfPostCells.push(new Cell(getRandomInt(5), getRandomInt(9), getRandomInt(5), getRandomInt(0), [], [])); */
      arrayOfPreCells.push(new Cell(randomPreCellAreaID, randomPreCellX, randomPreCellY, randomPreCellZ, [], []));
      arrayOfPostCells.push(new Cell(randomPostCellAreaID, randomPostCellX, randomPostCellY, randomPostCellZ, [], []));
    }

    let outgoingSynapses: Array<Synapse> = [];
    let incomingSynapses: Array<Synapse> = [];
   

    for (let i = 0; i < arrayOfPreCells.length; i++) {
      outgoingSynapses.push(new Synapse(sett.defaultPermanence, arrayOfPreCells[i], arrayOfPostCells[i]));
      incomingSynapses.push(new Synapse(sett.defaultPermanence, arrayOfPreCells[i], arrayOfPostCells[i]));
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
    let randomSynapses: Array<Synapse> = [];

    for (let k = 0; k < arrayOfPreCells.length; k++) {
      let randomPermanence = Math.random();
      randomSynapses.push(new Synapse(randomPermanence, arrayOfPreCells[k], arrayOfPostCells[k]));
    }

    //var model: NeoCortexModel = new NeoCortexModel(sett, inpModel, 0, 0, 0, [synaps01, synaps34, synap23, synap56, synap78, synap910]);

    var model: NeoCortexModel = new NeoCortexModel(sett, inpModel, 0, 0, 0, synapses);

    return model;
  }

}