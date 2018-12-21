import { Component, OnInit, AfterViewInit } from '@angular/core';
import * as Plotlyjs from 'plotly.js/dist/plotly';
// import { neoCortexUtils } from '../neocortexutils';

export const environment = {

    //change numberOfColours to specify the total numbers of colours for neurons
    //The colour scale lasts from 0 to 1, 0 == blue and 1 == red 
    numberOfColours: 500,

    // with X,Y, Z Ratio change the appearance of graph
    /* xRatio: 7,
    yRatio: 1,
    zRatio: 0.5, */
    xRatio: 7,
    yRatio: 1,
    zRatio: 0.5,
    sizeOfNeuron: 15,
    opacityOfNeuron: 1,
    opacityOfSynapse: 1,
    lineWidthOfSynapse: 4,
    cellHeightInMiniColumn: 5,
    miniColumnWidth: 5




}