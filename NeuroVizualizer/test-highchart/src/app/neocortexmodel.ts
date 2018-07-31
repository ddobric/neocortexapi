import { NgModule }             from '@angular/core';



export class neocortexSettings{

  public numAreas: number;
  public minicolumnDims: number[];
  public numCellsInMinicolumn: number;
}

export class NeoCortexModel {

  public areas: Array<Area>;
  
  constructor(settings:neocortexSettings ) {
     this.areas = new Array(settings.numAreas);
     for(var i=0;i<settings.numAreas;i++)
      this.areas[i] = new Area(settings.minicolumnDims, settings.numCellsInMinicolumn);
  } 

}



export class Area {

  public minicolumns: Minicolumn[][] = new Array();
  
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



export class Minicolumn {

  public Cells: Cell[];
  
  constructor(numCells: number) {
     
  }
  
}

export class location{

  posX:number;
  posY:number;
  posZ:number;

  constructor(posX:number = 0, posY:number=0, posZ:number=0) {
    
  }  

  
}

export class Cell extends location {

  public Synapses: Synapse[];
  
  public Layer: number;

  constructor(posX:number = 0, posY:number=0, posZ:number=0) {
   super(posX, posY, posY);
  }    
}


export class Synapse {

  public preSynaptic: Cell;

  public postSynaptic: Cell;
  
  constructor(preSynaptic: Cell, postSynaptic: Cell) {
      this.preSynaptic = preSynaptic;
      this.postSynaptic = postSynaptic;
  }
  
}




