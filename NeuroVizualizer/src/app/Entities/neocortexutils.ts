import { NeoCortexModel, Cell, NeocortexSettings, Area, Minicolumn, InputModel } from './neocortexmodel';


export class neoCortexUtils {

    static cells: Array<Cell> = new Array();
    static cellRegister: Array<Cell> = new Array();
    static cellID = 0;
    static areaLevelIndex: number;


    /**
     * createModel (numberOfAreas/DataSeries, [xAxis, zAxis], yAxis)
     * @param areas 
     * @param miniColDims 
     * @param numLayers 
     */
    static createModel(areaLevels: number[], miniColDims: number[], numLayers: number): NeoCortexModel {

        let sett: NeocortexSettings = new NeocortexSettings();
        sett.minicolumnDims = miniColDims;
        sett.areaLevels = areaLevels;
        sett.numLayers = numLayers;

        // let inpModel: InputModel = new InputModel(sett.minicolumnDims[0], sett.minicolumnDims[1]);
        //var model: NeoCortexModel = new NeoCortexModel(sett, inpModel, 0, 0, 0, []);

        var model: NeoCortexModel = this.createNeoCortexModel(sett, 0, 0, 0);

        return model;
    }

    static createNeoCortexModel(settings: NeocortexSettings, X: number, layer: number, Z: number): NeoCortexModel {// creating areas
        var model: NeoCortexModel = new NeoCortexModel();
        model.synapses = new Array();

        model.settings = settings;
        //model.input = input;
        model.areas = new Array(settings.areaLevels.length);

        let areaId: number;

        for (let levelIndx = 0; levelIndx < settings.areaLevels.length; levelIndx++) {
            this.areaLevelIndex = levelIndx;
            areaId = levelIndx;

            model.areas[levelIndx] = this.createArea(settings, areaId, settings.areaLevels[levelIndx]);
        }


        return model;
    }

    static createArea(settings: NeocortexSettings, areaId: number, level: number): Area {// creating minicolumn
        const area = new Area();
        area.areaId = areaId;
        area.level = level;
        let miniColDim0: any;
        let miniColDim1: any;

        for (miniColDim0 = 0; miniColDim0 < settings.minicolumnDims[0]; miniColDim0++) {
            let row: Array<Minicolumn> = new Array();
            for (miniColDim1 = 0; miniColDim1 < settings.minicolumnDims[1]; miniColDim1++) {

                this.cellRegister = [];

                this.createCells(settings, areaId, miniColDim0, miniColDim1);
                let randomOverlap = Math.random();

                row.push({
                    miniColumnId: areaId + miniColDim0 + miniColDim1,
                    cells: this.cellRegister,
                    overlap: randomOverlap,
                });
            }

            area.minicolumns.push(row);

        }

        return area;
    }

    static createCells(settings: NeocortexSettings, areaId: number, x: number, z: number) {
        settings = settings;

        for (let layer = 0; layer < settings.numLayers; layer++) {

            this.cellRegister.push(
                {
                    cellId: this.cellID++,
                    areaIndex: areaId,
                    X: x,
                    Layer: layer,
                    Z: z
                });

        }

        this.saveCells(this.cellRegister);
    }

    static saveCells(minicolumnCells = []) {

        for (let i = 0; i < minicolumnCells.length; i++) {
            this.cells.push(minicolumnCells[i]);

        }
        console.log(this.cells);
    }

    static createSynapses(model: NeoCortexModel) {

        for (let i = 0; i < 10; i++) {

            let chooseRandomPreCell = this.getRandomInt(this.cells.length);
            let chooseRandomPostCell = this.getRandomInt(this.cells.length);

            const synapse = {
                permanence: 0,
                preSynapticId: this.cells[chooseRandomPreCell].cellId,
                postSynapticId: this.cells[chooseRandomPostCell].cellId,

            };
            model.synapses.push(synapse);

            //   this.cells[chooseRandomPreCell].outgoingSynapses.push(synapse);
            //   this.cells[chooseRandomPostCell].incomingSynapses.push(synapse);


        }




    }

    static getRandomInt(max: any) {

        return Math.floor(Math.random() * Math.floor(max));
    }

    createInputModel(cellDim0: any, cellDim1: any, inputModel: InputModel) {

        for (let dim = 0; dim < cellDim0; dim++) {
            let row: Array<Cell> = new Array();

            for (let i = 0; i < cellDim1; i++) {
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
            inputModel.cells.push(row);

        }

    }


}

