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

    settings: NeocortexSettings;

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
    miniColumnId?: number;
    Overlap: number;
    Cells: Array<Cell> = new Array();
    settings?: NeocortexSettings;

}

export class Cell {

    CellId: number;
    AreaID: number;
    Index: number;
    ParentColumnIndex: number;
    Z: number;
    incomingSynapses?: Array<Synapse> = new Array();
    outgoingSynapses?: Array<Synapse> = new Array();
    //areaID, i, cellId, parentColumnIndx, CellActivity.PredictiveCell
}


export class Synapse {

    PreSynaptic?: Cell;
    PostSynaptic?: Cell;
    PreSynapticCellId?: number;
    PostSynapticCellId?: number;
    Permanence: number;

}


export class InputModel {

    cells: Cell[][] = new Array();

}



