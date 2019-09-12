

export class CellId {

  area: number;

  minicolumn: number[];

  layer: number;
}

export class NeocortexSettings {

  areaLevels: Array<number> = [];
  minicolumnDims: number[];
  numLayers: number;
  defaultOverlapValue: number = 0;
  defaultPermanence: number = 0;

}

export class NeoCortexModel {

  areas: Array<Area>;

  synapses: Array<Synapse>;

  settings: NeocortexSettings;

  /**
   * Multidimensional sensory input.
   */
  input: InputModel;


  constructor(settings: NeocortexSettings = null, input: InputModel = null, X = 0, layer = 0, Z = 0, synapses = []) {

    this.synapses = new Array();
    this.settings = settings;
    this.input = input;
    this.areas = new Array(settings.areaLevels.length);
    this.synapses = synapses;

    let areaId: number = 0;
    for (var levelIndx = 0; levelIndx < settings.areaLevels.length; levelIndx++) {
      areaId = levelIndx;
      this.areas[levelIndx] = new Area(settings, areaId, settings.areaLevels[levelIndx], X, layer, Z);

    }

    for (let i = 0; i < this.synapses.length; i++) {
      const a = this.synapses[i];
      this.areas[a.preSynaptic.areaIndex].minicolumns[a.preSynaptic.X][a.preSynaptic.Z].cells[a.preSynaptic.Layer].outgoingSynapses.push(this.synapses[i]);
      this.areas[a.postSynaptic.areaIndex].minicolumns[a.postSynaptic.X][a.postSynaptic.Z].cells[a.postSynaptic.Layer].incomingSynapses.push(this.synapses[i]);
    }

  }

}


export class Area {

  minicolumns: Minicolumn[][] = new Array();
  level: number;
  id: number;

  constructor(settings: NeocortexSettings, areaId: number, level: number, X: number, layer: number, Z: number) {

    this.id = areaId;
    this.level = level;
    let miniColDim0: any;
    let miniColDim1: any;

    for (miniColDim0 = 0; miniColDim0 < settings.minicolumnDims[0]; miniColDim0++) {
      let row: Array<Minicolumn> = new Array();
      for (miniColDim1 = 0; miniColDim1 < settings.minicolumnDims[1]; miniColDim1++) {
        let randomOverlap = Math.random();
        //row.push(new Minicolumn(settings, areaId, [this.id, miniColDim0, miniColDim1], settings.defaultOverlapValue, (miniColDim0 + X), (miniColDim1 + Z)));
        row.push(new Minicolumn(settings, areaId, [this.id, miniColDim0, miniColDim1], randomOverlap, (miniColDim0 + X), (miniColDim1 + Z)));
      }


      this.minicolumns.push(row);

    }
  }
}

export class Minicolumn {

  overlap: number;

  cells: Array<Cell> = new Array();

  id: number[];

  areaId: number;

  settings: NeocortexSettings;

  constructor(settings: NeocortexSettings, areaId: number, miniColId: number[], overlap: number, X: number, Z: number) {

    this.areaId = areaId;
    this.overlap = overlap;
    this.id = miniColId;
    this.settings = settings;

    for (let layer = 0; layer < settings.numLayers; layer++) {

      let cell: Cell = new Cell(areaId, X, layer, Z, [], []);

      this.cells.push(cell);
    }

  }
}


/**
 * 
 */
export class Cell {

  Id: number;
  X: number;
  Layer: number;
  Z: number;
  areaIndex: number;


  incomingSynapses: Array<Synapse> = new Array();
  outgoingSynapses: Array<Synapse> = new Array();

  /**
   * 
   * @param layer Optional for input model.
   * @param posX 
   * @param posY 
   * @param posZ 
   */
  constructor(areaIndex: number, X: number, layer: number, Z: number, incomingSynap: Array<Synapse>, outgoingSynap: Array<Synapse>) {
    this.Layer = layer;
    this.X = X;
    this.Z = Z;
    this.areaIndex = areaIndex;
    this.incomingSynapses = incomingSynap;
    this.outgoingSynapses = outgoingSynap;

  }
}


export class Synapse {

  preSynaptic: Cell;

  postSynaptic: Cell;

  permanence: number;

  constructor(permanence: number = 0, preSynaptic: Cell, postSynaptic: Cell) {
    this.preSynaptic = preSynaptic;
    this.postSynaptic = postSynaptic;
    this.permanence = permanence;

  }
}


export class InputModel {

  cells: Cell[][] = new Array();

  constructor(cellDim0: any, cellDim1: any) {

    this.cells = new Array();

    /*    for (let dim = 0; dim < cellDim0; dim++) {
         let row: Array<Cell> = new Array();
   
         for (let i = 0; i < cellDim1; i++) {
           row.push(new Cell(null, dim, null, i, [], []));
   
         }
         this.cells.push(row);
   
       } */

  }
}



