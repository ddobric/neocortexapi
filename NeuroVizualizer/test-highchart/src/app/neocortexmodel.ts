
export class CellId {

  public area: number;

  public minicolumn: number[];

  public layer: number;
}

export class NeocortexSettings {

  public cellHeightInMiniColumn: number = 5;
  public miniColumnWidth: number = 5;
  public areaLevels: Array<number> = [0, 0, 0, 1, 1, 2];
  public minicolumnDims: number[];
  public numLayers: number;
  public xAreaDistance: number = 30;
  public yAreaDistance: number = 10;
  public zAreaDistance: number = 1;
  public defaultOverlapValue: number = 0;

}

export class NeoCortexModel {

  public areas: Array<Area>;

  public synapses: Array<Synapse>;

  public settings: NeocortexSettings;

  /**
   * Multidimensional sensory input.
   */
  public input: InputModel;


  constructor(settings: NeocortexSettings = null, input: InputModel = null, X = 0, layer = 0, Z = 0, synapses = []) {

    this.synapses = new Array();
    this.settings = settings;
    this.input = input;
    this.areas = new Array(settings.areaLevels.length);
    this.synapses = synapses;

    let areaId: number =0;
    for (var levelIndx = 0; levelIndx < settings.areaLevels.length; levelIndx++) {
      areaId = levelIndx;
      this.areas[levelIndx] = new Area(settings, areaId, settings.areaLevels[levelIndx], X, layer, Z);
      //areaId = levelIndx + areaId;
      // posX = posX + settings.xAreaDistance;
      // posY = posY + settings.yAreaDistance;
      // posZ = posZ + settings.zAreaDistance;

    }


     
 /*   this.synapses.forEach(syn => {
     this.areas[syn.preSynaptic.id.area].minicolumns[syn.preSynaptic.X][syn.preSynaptic.Z].cells[syn.preSynaptic.Layer].outgoingSynapses.push(syn);
     this.areas[syn.postSynaptic.id.area].minicolumns[syn.postSynaptic.X][syn.postSynaptic.Z].cells[syn.postSynaptic.Layer].incomingSynapses.push(syn);
   
    });  */

    for (let i = 0; i < this.synapses.length; i++) {
      const a = this.synapses[i];
      this.areas[a.preSynaptic.areaIndex].minicolumns[a.preSynaptic.X][a.preSynaptic.Z].cells[a.preSynaptic.Layer].outgoingSynapses.push(this.synapses[i]);
      this.areas[a.postSynaptic.areaIndex].minicolumns[a.postSynaptic.X][a.postSynaptic.Z].cells[a.postSynaptic.Layer].incomingSynapses.push(this.synapses[i]);
    } 
  
  }
}


export class Area {

  minicolumns: Minicolumn[][] = new Array();
  public level: number;
  public id: number;

  constructor(settings: NeocortexSettings, areaId: number, level: number, X: number, layer: number,Z: number) {

    this.id = areaId;
    this.level = level;
    let miniColDim0: any; 
    let miniColDim1: any;

    for (miniColDim0 = 0; miniColDim0 < settings.minicolumnDims[0]; miniColDim0++) {
      let row: Array<Minicolumn> = new Array();
      for (miniColDim1 = 0; miniColDim1 < settings.minicolumnDims[1]; miniColDim1++) {
        row.push(new Minicolumn(settings, areaId, [this.id, miniColDim0, miniColDim1], settings.defaultOverlapValue, (miniColDim0+X), layer, (miniColDim1+Z)));
      }


      this.minicolumns.push(row);

    }
  }
}

export class Minicolumn {

  public overlap: number;

  public cells: Array<Cell> = new Array();

  public id: number[];

  public areaId: number;

  private settings: NeocortexSettings;

  constructor(settings: NeocortexSettings, areaId: number, miniColId: number[], overlap: number, X: number, layer: number, Z: number) {

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

  public X: number;
  public Layer: number;
  public Z: number;
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
  constructor( areaIndex: number, X: number , layer: number,  Z: number, incomingSynap :Array<Synapse>, outgoingSynap : Array<Synapse>) {
    this.Layer = layer;
    this.X = X;
    this.Z = Z; 
    this.areaIndex = areaIndex;
    this.incomingSynapses = incomingSynap;
    this.outgoingSynapses = outgoingSynap;

  }
}


export class Synapse {

  public preSynaptic: Cell;

  public postSynaptic: Cell;

  public permanence: number;

  constructor( permanence: number = 0, preSynaptic: Cell, postSynaptic: Cell) {
    this.preSynaptic = preSynaptic;
    this.postSynaptic = postSynaptic;
    this.permanence = permanence;

  }
}


export class InputModel {

  public cells: Cell[][] = new Array();

  public id: number;

  constructor(settings: NeocortexSettings, cellDims: number[] = []) {

    this.cells = new Array();

    //TODO. Exception if cellDims > 2
    try {

      for (var dim = 0; dim < cellDims.length; dim++) {
        let row: Array<Cell> = new Array();
        for (var i = 0; i < cellDims[dim]; i++) {
          row.push(new Cell(null,null, null, null, null, null));
        }

      }

      for (var dim = 0; dim < cellDims[0]; dim++) {

        let row: Array<Cell> = new Array();

        for (var j = 0; j < cellDims[1]; j++) {

          //row.push(new Cell(settings, 0, [i, j], 0));
          row.push(new Cell( null, null, null,null, null, null));
        }

        this.cells.push(row);
      }
    }
    catch (e) {
      console.log(e);
    }


  }
}



