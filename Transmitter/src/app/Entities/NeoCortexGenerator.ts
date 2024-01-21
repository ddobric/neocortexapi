import { NeoCortexModel, Cell, NeocortexSettings, Area, Minicolumn, InputModel } from './NeoCortexModel';


export class NeoCortexGenerator {

    cellRegister: Array<Cell> = new Array();
    cellID = 0;


    /**
     * createModel (numberOfAreas/DataSeries, [xAxis, zAxis], yAxis)
     * @param areas 
     * @param miniColDims 
     * @param numLayers 
     */
    createModel(areaLevels: number[], miniColDims: number[], numLayers: number): NeoCortexModel {

        let sett: NeocortexSettings = new NeocortexSettings();
        sett.minicolumnDims = miniColDims;
        sett.areaLevels = areaLevels;
        sett.numLayers = numLayers;

        // let inpModel: InputModel = new InputModel(sett.minicolumnDims[0], sett.minicolumnDims[1]);
        //var model: NeoCortexModel = new NeoCortexModel(sett, inpModel, 0, 0, 0, []);
        //let inputModel = new InputModel();
        var Model: NeoCortexModel = this.createNeoCortexModel(sett, null);

        this.createInputModel(sett, Model);

        return Model;
    }

    createNeoCortexModel(settings: NeocortexSettings, inputModel: InputModel): NeoCortexModel {// creating areas
        var Model: NeoCortexModel = new NeoCortexModel();
        Model.Synapses = new Array();

        Model.settings = settings;
        //model.input = input;
        Model.Areas = new Array(settings.areaLevels.length);

        let areaId: number;

        for (let levelIndx = 0; levelIndx < settings.areaLevels.length; levelIndx++) {

            areaId = levelIndx;

            Model.Areas[levelIndx] = this.createArea(Model, settings, areaId, settings.areaLevels[levelIndx]);
        }

        this.createSynapses(Model);

        return Model;
    }

    createArea(model: NeoCortexModel, settings: NeocortexSettings, areaId: number, level: number): Area {// creating minicolumn
        const area = new Area();
        area.AreaId = areaId;
        area.Level = level;
        let MiniColDim0: any;
        let MiniColDim1: any;

        for (MiniColDim0 = 0; MiniColDim0 < settings.minicolumnDims[0]; MiniColDim0++) {
            let row: Array<Minicolumn> = new Array();
            for (MiniColDim1 = 0; MiniColDim1 < settings.minicolumnDims[1]; MiniColDim1++) {

                this.cellRegister = [];

                this.createCells(model, settings, areaId, MiniColDim0, MiniColDim1);
                let randomOverlap = Math.random();

                row.push({
                    miniColumnId: areaId + MiniColDim0 + MiniColDim1,
                    Cells: this.cellRegister,
                    Overlap: randomOverlap,
                });
            }

            area.Minicolumns.push(row);

        }

        return area;
    }

    createCells(Model: NeoCortexModel, settings: NeocortexSettings, areaId: number, x: number, z: number) {
        settings = settings;

        for (let layer = 0; layer < settings.numLayers; layer++) {

            this.cellRegister.push(
                {
                    cellId: this.cellID++,
                    areaIndex: areaId,
                    X: x,
                    Layer: layer,
                    Z: z,
                    incomingSynapses: [],
                    outgoingSynapses: []
                });

        }

        this.saveCells(this.cellRegister, Model);
    }

    saveCells(minicolumnCells = [], Model: NeoCortexModel) {

        for (let i = 0; i < minicolumnCells.length; i++) {
            Model.Cells.push(minicolumnCells[i]);

        }
    }

    createSynapses(Model: NeoCortexModel) {

        for (let i = 0; i < 20; i++) {

            let chooseRandomPreCell = this.getRandomInt(Model.Cells.length);
            let chooseRandomPostCell = this.getRandomInt(Model.Cells.length);
            let randomPerm = Math.random();

            const synapse = {
                permanence: randomPerm,
                preSynapticId: Model.Cells[chooseRandomPreCell].cellId,
                postSynapticId: Model.Cells[chooseRandomPostCell].cellId,

            };
            Model.Synapses.push(synapse);

            //   this.cells[chooseRandomPreCell].outgoingSynapses.push(synapse);
            //   this.cells[chooseRandomPostCell].incomingSynapses.push(synapse);

        }

    }

    getRandomInt(max: any) {

        return Math.floor(Math.random() * Math.floor(max));
    }

    createInputModel(settings: NeocortexSettings, model: NeoCortexModel) {

        for (let dim = 0; dim < settings.minicolumnDims[0]; dim++) {
            let row: Array<Cell> = new Array();

            for (let i = 0; i < settings.minicolumnDims[1]; i++) {
                //row.push(new Cell(null, dim, null, i, [], []));
                row.push(
                    {
                        areaIndex: null,
                        cellId: null,
                        X: dim,
                        Layer: null,
                        Z: i
                    });

            }
            model.input.cells.push(row);
        }

    }


}

