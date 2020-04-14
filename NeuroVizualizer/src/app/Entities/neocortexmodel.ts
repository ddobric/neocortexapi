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

    Synapse: Array<Synapse>;

    Settings: NeocortexSettings;

    Cells: Array<Cell> = new Array();

    /**
     * Multidimensional sensory input.
     */
    input: InputModel = new InputModel();
}


export class Area {

    MiniColumns: Minicolumn[][] = new Array();
    Level: number;
    AreaId: number;

}

export class Minicolumn {
    MiniColumnId?: number;
    Overlap: number;
    Cells: Array<Cell> = new Array();
    Settings?: NeocortexSettings;

}

export class Cell {

    /* cellId?: number;
    areaIndex?: number;
    X?: number;
    Layer?: number;
    Z?: number;
    Index?: number;
    ParentColumnIndex?: number;
    incomingSynapses?: Array<Synapse> = new Array();
    outgoingSynapses?: Array<Synapse> = new Array();
    areaID: any; */
    CellId: number;
    AreaID: number;
    Index: number;
    ParentColumnIndex: number;
    Z: number;
    incomingSynapses?: Array<Synapse> = new Array();
    outgoingSynapses?: Array<Synapse> = new Array();
}


export class Synapse {

    PreSynaptic?: Cell;
    PostSynaptic?: Cell;
    PreSynapticCellId?: number;
    PostSynapticCellId?: number;
    Permanence: number;

}


export class InputModel {

    Cells: Cell[][] = new Array();

}



