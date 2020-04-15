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
    Input: InputModel = new InputModel();
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
    ColActivity: ColumnActivity;

}

export class Cell {

    CellId: number;
    AreaID: number;
    Index: number;
    ParentColumnIndex: number;
    Z: number;
    CellActivity: CellActivity;
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
export enum ColumnActivity {

    Inactive,
    Active,
}

export enum CellActivity {

    ActiveCell,

    PredictiveCell,

    WinnerCell,
}

export class InputModel {

    Cells: Cell[][] = new Array();

}



