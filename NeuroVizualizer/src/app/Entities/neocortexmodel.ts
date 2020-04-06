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

    synapses: Array<Synapse>;

    settings: NeocortexSettings;

    cells: Array<Cell> = new Array();

    /**
     * Multidimensional sensory input.
     */
    input: InputModel = new InputModel();
}


export class Area {

    Minicolumns: Minicolumn[][] = new Array();
    level: number;
    areaId: number;

}

export class Minicolumn {
    miniColumnId?: number;
    overlap: number;
    Cells: Array<Cell> = new Array();
    settings?: NeocortexSettings;

}

export class Cell {

    cellId?: number;
    areaIndex?: number;
    X?: number;
    Layer?: number;
    Z?: number;
    Index?: number;
    ParentColumnIndex?: number;
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



