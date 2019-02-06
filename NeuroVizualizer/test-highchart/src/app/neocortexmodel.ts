import { NgModule } from '@angular/core';
import { LOCATION_INITIALIZED } from '@angular/common';


export class CellId {

  public area: number;

  public minicolumn: number[];

  public layer: number;
}

export class Location {

  posX: number;
  posY: number;
  posZ: number;

  public isVisible: boolean;

  constructor(posX: number, posY: number, posZ: number) {
    this.posX = posX;
    this.posY = posY;
    this.posZ = posZ;
  }
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
  public synapsesArray = [];

  /**
   * Multidimensional sensory input.
   */
  public input: InputModel;


  constructor(settings: NeocortexSettings = null, input: InputModel = null, posX = 0, posY = 0, posZ = 0, synapses = []) {

    this.synapses = new Array();
    this.settings = settings;
    this.input = input;
    this.areas = new Array(settings.areaLevels.length);
    this.synapses = synapses;

    let areaId: number = 0;
    for (var levelIndx = 0; levelIndx < settings.areaLevels.length; levelIndx++) {

      this.areas[levelIndx] = new Area(settings, areaId, settings.areaLevels[levelIndx], posX, posY, posZ);

      posX = posX + settings.xAreaDistance;
      posY = posY + settings.yAreaDistance;
      posZ = posZ + settings.zAreaDistance;

    }
   /*  let preCellIn: any;
    let postCellIn: any;
    for (preCellIn = 0; preCellIn < preCellsArray.length; preCellIn++) {
      for (postCellIn = 0; postCellIn < postCellsArray.length; postCellIn++) {

        this.synapses[preCellIn] = new Synapse(null, 0, preCellsArray[preCellIn], postCellsArray[preCellIn]);

      }

    } */

  }
}



export class Area extends Location {

  public minicolumns: Minicolumn[][] = new Array();
  public level: number;
  public id: number;

  constructor(settings: NeocortexSettings, areaId: number, level: number, posX: number, posY: number, posZ: number) {
    super(posX, posY, posZ); {

      this.id = areaId;
      this.level = level;
      let miniColDim0; let miniColDim1;

      for (miniColDim0 = 0; miniColDim0 < settings.minicolumnDims[0]; miniColDim0++) {
        let row: Array<Minicolumn> = new Array();
        for (miniColDim1 = 0; miniColDim1 < settings.minicolumnDims[1]; miniColDim1++) {
          row.push(new Minicolumn(settings, areaId, [this.id, miniColDim0, miniColDim1], settings.defaultOverlapValue, (posX + miniColDim0), (posY), (posZ + miniColDim1)));
        }


        this.minicolumns.push(row);

      }
    }
  }
}


export class Minicolumn extends Location {

  public overlap: number;

  public cells: Array<Cell> = new Array();

  public id: number[];

  public areaId: number;
  preSynapse: Synapse;
  postSynapse: Synapse;

  private settings: NeocortexSettings;

  constructor(settings: NeocortexSettings, areaId: number, miniColId: number[], overlap: number, posX: number, posY: number, posZ: number) {
    super(posX, posY, posZ);

    this.areaId = areaId;
    this.overlap = overlap;
    this.id = miniColId;
    this.settings = settings;

    for (let layer = 0; layer < settings.numLayers; layer++) {

      let cell: Cell = new Cell(settings, areaId, miniColId, layer, this.posX, this.posY, this.posZ, this.preSynapse, this.postSynapse)

      this.cells.push(cell);
    }
  }
}


/**
 * 
 */
export class Cell extends Location {

  public id: CellId;
  public Layer: number;




  /**
   * 
   * @param layer Optional for input model.
   * @param posX 
   * @param posY 
   * @param posZ 
   */
  constructor(settings: NeocortexSettings, areaId: number, miniColId: number[], layer: number, posX: number = 0, posY: number = 0, posZ: number = 0, preSynapse: Synapse, postSynapse: Synapse) {
    super(posX, posY, posZ);
    this.Layer = layer;
    this.id = { area: areaId, minicolumn: miniColId, layer: layer };

  }
}


export class Synapse {

  public id: number;

  public preSynaptic: Cell;

  public postSynaptic: Cell;

  public permanence: number;

  constructor(id: number, permanence: number = 0, preSynaptic: Cell, postSynaptic: Cell) {
    this.preSynaptic = preSynaptic;
    this.postSynaptic = postSynaptic;
    this.permanence = permanence;
    this.id = id;
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
          row.push(new Cell(settings, this.id, [dim, i], null, null, null, null, null, null));
        }

      }

      for (var dim = 0; dim < cellDims[0]; dim++) {

        let row: Array<Cell> = new Array();

        for (var j = 0; j < cellDims[1]; j++) {

          //row.push(new Cell(settings, 0, [i, j], 0));
          row.push(new Cell(settings, this.id, [dim, j], null, null, null, null, null, null));
        }

        this.cells.push(row);
      }
    }
    catch (e) {
      console.log(e);
    }


  }
}



