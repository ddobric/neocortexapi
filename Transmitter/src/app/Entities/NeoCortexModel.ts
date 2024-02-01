export class CellId {

    area: number;
    minicolumn: number;
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

    Areas: Array<Area>;

    Synapses: Array<Synapse>;

    settings: NeocortexSettings;

    Cells: Array<Cell> = new Array();

    /**
     * Multidimensional sensory input.
     */
    input: InputModel = new InputModel();
}


export class Area {

    Minicolumns: Minicolumn[][] = new Array();
    Level: number;
    AreaId: number;

}

export class Minicolumn {
    miniColumnId?: number;
    Overlap: number;
    Cells: Array<Cell> = new Array();
    settings?: NeocortexSettings;

}

export class Cell {

    cellId: number;
    areaIndex: number;
    X: number;
    Layer: number;
    Z: number;
    incomingSynapses?: Array<Synapse> = new Array();
    outgoingSynapses?: Array<Synapse> = new Array();
}


export class Synapse {

    preSynaptic?: Cell;
    postSynaptic?: Cell;
    preSynapticId?: number;
    postSynapticId?: number;
    permanence: number;

}


export class InputModel {

    cells: Cell[][] = new Array();

}



