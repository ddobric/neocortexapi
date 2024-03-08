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
        var model: NeoCortexModel = this.createNeoCortexModel(sett, null);

        this.createInputModel(sett, model);

        return model;
    }

    createNeoCortexModel(settings: NeocortexSettings, inputModel: InputModel): NeoCortexModel {// creating areas
        var model: NeoCortexModel = new NeoCortexModel();
        model.synapses = new Array();

        model.settings = settings;
        //model.input = input;
        model.Areas = new Array(settings.areaLevels.length);

        let areaId: number;

        for (let levelIndx = 0; levelIndx < settings.areaLevels.length; levelIndx++) {

            areaId = levelIndx;

            model.Areas[levelIndx] = this.createArea(model, settings, areaId, settings.areaLevels[levelIndx]);
        }

        this.createSynapses(model);

        return model;
    }

    createArea(model: NeoCortexModel, settings: NeocortexSettings, areaId: number, level: number): Area {// creating minicolumn
        const area = new Area();
        area.areaId = areaId;
        area.level = level;
        let miniColDim0: any;
        let miniColDim1: any;

        for (miniColDim0 = 0; miniColDim0 < settings.minicolumnDims[0]; miniColDim0++) {
            let row: Array<Minicolumn> = new Array();
            for (miniColDim1 = 0; miniColDim1 < settings.minicolumnDims[1]; miniColDim1++) {

                this.cellRegister = [];

                this.createCells(model, settings, areaId, miniColDim0, miniColDim1);
                let randomOverlap = Math.random();

                row.push({
                    miniColumnId: areaId + miniColDim0 + miniColDim1,
                    Cells: this.cellRegister,
                    overlap: randomOverlap,
                });
            }

            area.Minicolumns.push(row);

        }

        return area;
    }

    createCells(model: NeoCortexModel, settings: NeocortexSettings, areaId: number, x: number, z: number) {
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

        this.saveCells(this.cellRegister, model);
    }

    saveCells(minicolumnCells = [], model: NeoCortexModel) {

        for (let i = 0; i < minicolumnCells.length; i++) {
            model.cells.push(minicolumnCells[i]);

        }
    }

    createSynapses(model: NeoCortexModel) {

        for (let i = 0; i < 10; i++) {

            let chooseRandomPreCell = this.getRandomInt(model.cells.length);
            let chooseRandomPostCell = this.getRandomInt(model.cells.length);
            let randomPerm = Math.random();

            const synapse = {
                permanence: randomPerm,
                preSynapticId: model.cells[chooseRandomPreCell].cellId,
                postSynapticId: model.cells[chooseRandomPostCell].cellId,

            };
            model.synapses.push(synapse);

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

