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

  constructor(posX: number, posY: number, posZ: number) {
    this.posX = posX;
    this.posY = posY;
    this.posZ = posZ;
  }
}

export class NeocortexSettings {

  public cellHeightInMiniColumn: number = 5;
  public miniColumnWidth: number = 5;
  public areaLevels: Location[];
  public minicolumnDims: number[];
  public numLayers: number;

}

export class NeoCortexModel {

  public areas: Array<Area>;

  public synapses: Array<Synapse>;

  public settings: NeocortexSettings;

  /**
   * Multidimensional sensory input.
   */
  public input: InputModel;


  constructor(settings: NeocortexSettings, input: InputModel, posX = 0, posY = 0, posZ = 0) {

    this.synapses = new Array();
    this.settings = settings;
    this.input = input;
    this.areas = new Array(settings.areaLevels.length);

    let areaId : number = 0;
    for (var areaLevel = 0; areaLevel < settings.areaLevels.length; areaLevel++) {
     
      this.areas[areaLevel] = new Area(settings, areaId, areaLevel, posX, posY, posZ);/// change at this position to chnage the area
      areaId++;
      posX = posX + 50;//posX = posX + sett.areaXDistance  ;
      posY = posY + 10; //posY = posY + sett.areaXDistance * settings.areas[i];
      posZ = posZ + 7;

    }
  }
}



export class Area extends Location {

  public minicolumns: Minicolumn[][] = new Array();

  public level: number;
  public id: number;
  public overlap: Array<any> = new Array();
  public oL: Array<any> = new Array();
  // public overlap: number[] = new Array();
  private settings: NeocortexSettings;

  constructor(settings: NeocortexSettings, areaId: number, level: number, posX: number, posY: number, posZ: number, ) {
    super(posX, posY, posZ); {

      this.id = areaId;
      this.level = level;
      //this.overlap = overlap;

      this.settings = settings;

      this.minicolumns = new Array();
      /*    for (var i = 0; i < settings.minicolumnDims[0]; i++) {
   
           let row: Array<Minicolumn> = new Array();
   
           for (var j = 0; j < settings.minicolumnDims[1]; j++) {
   
             for (let k = 0; k < settings.areaLocations.length; k++) {
               row.push(new Minicolumn(settings, areaId, [i, j], (posX+i+j + (k*i * this.settings.miniColumnWidth)), (posY * k+i + this.settings.cellHeightInMiniColumn), (posZ + j+k)));
               
             }
             
           }
   
           this.minicolumns.push(row);
         } */
      let row: Array<Minicolumn> = new Array();
      let i; let j; let k;
      let totalNumberOfNeurons = (settings.minicolumnDims[0] * settings.minicolumnDims[1] * settings.numLayers);

      //TODO..
      // this.oL.push(overlap);
      for (let nW = 0; nW < 1; nW = nW + (1 / (totalNumberOfNeurons / settings.numLayers))) { //totalNumberOfNeurons/settings.numLayers to get each minicolumn
        for (let l = 0; l < settings.numLayers; l++) {
          let roundNW = nW.toFixed(3);
          this.oL.push(parseFloat(roundNW));
        }
      }

          for (let totalAreas = 0; totalAreas < settings.areaLevels.length; totalAreas++) {
            for (let oLArray = 0; oLArray < this.oL.length; oLArray++) {
              this.overlap.push(this.oL[oLArray]);
            }
          }


       
      for (i = 0; i < settings.minicolumnDims[0]; i++) {
        for (j = 0; j < settings.numLayers; j++) {
          for (k = 0; k < settings.minicolumnDims[1]; k++) {
            row.push(new Minicolumn(settings, areaId, [0], (posX + i), (posY + j), (posZ + k), this.overlap));
            this.minicolumns.push(row);
          }
        }
      }
    }
  }
}


export class Minicolumn extends Location {

  public overlap = [];

  public cells: Array<Cell> = new Array();

  public id: number[];

  public areaId: number;

  private settings: NeocortexSettings;

  constructor(settings: NeocortexSettings, areaId: number, miniColId: number[], posX: number, posY: number, posZ: number, overlap = []) {
    super(posX, posY, posZ);

    this.areaId = areaId;
    this.overlap = overlap;

    this.id = miniColId;

    this.settings = settings;

    for (let layer = 0; layer < settings.numLayers; layer++) {

      let cell: Cell = new Cell(settings, areaId, miniColId, layer, this.posX, this.posY, this.posZ)
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

  public isActive: boolean;

  public isPredictiv: boolean;

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

  constructor(settings: NeocortexSettings, cellDims: number[] = []) {

    this.cells = new Array();

    //TODO. Exception if cellDims > 2

    for (var i = 0; i < cellDims[0]; i++) {

      let row: Array<Cell> = new Array();

      for (var j = 0; j < cellDims[1]; j++) {

        row.push(new Cell(settings, 0, [i, j], 0));
      }

      this.cells.push(row);
    }
  }
}



