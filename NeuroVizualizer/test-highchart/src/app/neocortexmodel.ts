import { NgModule }             from '@angular/core';


export class neocortexSettings{

  public areas: number;
  public minicolumns: number[];
  public cellsInMinicolumn: number;
}

export class NeoCortexModel {

  public areas: Area[];
  
  constructor(settings:neocortexSettings ) {
     
  } 

}



export class Area {

  public minicolumns: Minicolumn[];
  
  constructor(minicolumns: number[]= [1000,3]) {
      this.minicolumns = new Array[3];
  }
  
}



export class Minicolumn {

  public Cells: Cell[];
  
  constructor(message: string) {
     
  }
  
}



export class Cell {

  public Synapses: Synapse[];
  
  public Layer: number;

  constructor(message: string) {
   
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


