import { NgModule } from '@angular/core';


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

  constructor(posX: number = 0, posY: number = 0, posZ: number = 0) {
    this.posX=posX;
    this.posY=posY;
    this.posZ=posZ;



  }
}

export class NeocortexSettings {

  public numAreas: number;
  public minicolumnDims: number[];
  public numLayers: number;

  public cellHeightInMiniColumn: number = 5;
  public miniColumnWidth: number = 5;
}

export class NeoCortexModel {

  public areas: Array<Area>;

  public synapses: Array<Synapse>;

  public settings: NeocortexSettings;
  
  /**
   * Multidimensional sensory input.
   */
  public input: InputModel;

  constructor(settings: NeocortexSettings, input: InputModel) {

    this.synapses = new Array();
    this.settings = settings;
    this.input = input;
    this.areas = new Array(settings.numAreas);
    for (var i = 0; i < settings.numAreas; i++)
      this.areas[i] = new Area(settings, i);
  }
}



export class Area {

  public minicolumns: Minicolumn[][] = new Array();

  public id: number;

  private settings: NeocortexSettings;

  constructor(settings: NeocortexSettings, areaId: number) {

    this.id = areaId;

    this.settings = settings;

    this.minicolumns = new Array();

    for (var i = 0; i < settings.minicolumnDims[1]; i++) {

      let row: Array<Minicolumn> = new Array();

      for (var j = 0; j < settings.minicolumnDims[0]; j++) {
        row.push(new Minicolumn(settings, areaId, [i, j]));
      }

      this.minicolumns.push(row);
    }
  }
}



export class Minicolumn extends Location {

  public cells: Array<Cell> = new Array();

  public id: number[];

  public areaId: number;

  private settings: NeocortexSettings;

  constructor(settings: NeocortexSettings, areaId: number, miniColId: number[], posX: number = 0, posY: number = 0, posZ: number = 0) {
    super(posX, posY, posZ);

    this.areaId = areaId;

    this.id = miniColId;

    this.settings = settings;

    for (let layer = 0; layer < settings.numLayers; layer++) {

      let cell: Cell = new Cell(settings, areaId, miniColId, layer, this.posX, this.posY + layer * this.settings.cellHeightInMiniColumn, this.posZ)
      //let cell: Cell = new Cell(settings, areaId, miniColId, layer, this.posX, this.posY + layer, this.posZ)
      this.cells.push(cell);
    }
  }
}


/**
 * 
 */
export class Cell extends Location {

  public id: CellId;

  public Synapses: Synapse[];


  public Layer: number;

  /**
   * 
   * @param layer Optional for input model.
   * @param posX 
   * @param posY 
   * @param posZ 
   */
  constructor(settings: NeocortexSettings, areaId: number, miniColId: number[], layer: number, posX: number = 0, posY: number = 0, posZ: number = 0) {
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
  }
}


export class InputModel {

  public cells: Cell[][] = new Array();

  public id: number;

  constructor(settings: NeocortexSettings, cellDims: number[] = [1, 2048]) {

    this.cells = new Array();

    for (var i = 0; i < cellDims[0]; i++) {

      let row: Array<Cell> = new Array();

      for (var j = 0; j < cellDims[1]; j++) {

        row.push(new Cell(settings, 0, [i, j], 0));
      }

      this.cells.push(row);
    }
  }
}



