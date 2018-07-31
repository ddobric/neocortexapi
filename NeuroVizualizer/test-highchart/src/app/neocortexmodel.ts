import { NgModule }             from '@angular/core';



export class neocortexSettings{

  public numAreas: number;
  public minicolumnDims: number[];
  public numCellsInMinicolumn: number;
  public inputModel : InputModel;
}

export class NeoCortexModel {

  public areas: Array<Area>;
  
  public synapses: Array<Synapse>;
  
  public cells: Array<Cell>;
  
  public input: InputModel;

  constructor(settings:neocortexSettings ) {
     this.areas = new Array(settings.numAreas);
     for(var i=0;i<settings.numAreas;i++)
      this.areas[i] = new Area(settings.minicolumnDims, settings.numCellsInMinicolumn);
  } 

}



export class Area {

  public minicolumns: Minicolumn[][] = new Array();
  
  public id:number;

  constructor(minicolumns: number[]= [3,2048], cellsInMinicolumn:number) {

    //this.minicolumns = new Minicolumn[minicolumns[0]][minicolumns[1]];
    this.minicolumns = new Array();

    for (var i = 0; i < minicolumns[0]; i++) {

      let row : Array<Minicolumn> = new Array();
      
      for (var j = 0; j < minicolumns[1]; j++) {
        row.push(new Minicolumn(cellsInMinicolumn));       
      }

      this.minicolumns.push(row);
    }
  }  
}



export class Minicolumn extends Location{

  public cells: Array<Cell> = new Array();
  
  constructor(numCells: number) {
    super();
    //todo. create cells and push them to the list.
   // let cell : Cell = new Cell(1);
    //this.cells.push(cell);
    for (let x = 0; x < 1000; x++) {
      for (let y = 0; y < 6; y++) {
        for (let z = 0; z < 3; z++) {
          let cell : Cell = new Cell(x, y, z)
          this.cells.push(cell);
          
        }
        
      }
      
    }
    
  }
  
}

export class location{

  posX:number;
  posY:number;
  posZ:number;

  constructor(posX:number = 0, posY:number=0, posZ:number=0) {
 
    
  }  

  
}


/**
 * 
 */
export class Cell extends location {

  public Synapses: Synapse[];
  
  public id: number;
  
  public Layer: number;

/**
 * 
 * @param layer Optional for input model.
 * @param posX 
 * @param posY 
 * @param posZ 
 */
  constructor(layer:number=0,posX:number = 0, posY:number=0, posZ:number=0) {
       super(posX, posY, posZ);
       this.Layer=layer;
  }    
}


export class Synapse {

  public id:number;

  public preSynaptic: Cell;

  public postSynaptic: Cell;
  
  public permanence: number;

  constructor(id:number, permanence:number = 0, preSynaptic: Cell, postSynaptic: Cell) {
      this.preSynaptic = preSynaptic;
      this.postSynaptic = postSynaptic;
  }
  
}


export class InputModel {

  public cells: Cell[][] = new Array();
  
  public id:number;

  constructor(cellDims: number[]= [1,2048]) {

    //this.minicolumns = new Minicolumn[minicolumns[0]][minicolumns[1]];
    this.cells = new Array();

    for (var i = 0; i < cellDims[0]; i++) {

      let row : Array<Cell> = new Array();
      
      for (var j = 0; j < cellDims[1]; j++) {
        row.push(new Cell());       
      }

      this.cells.push(row);
    }
  }  
}



