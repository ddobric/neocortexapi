import { Component, OnInit, AfterViewInit } from '@angular/core';
import * as Plotlyjs from 'plotly.js/dist/plotly';
// import { neoCortexUtils } from '../neocortexutils';

export const environment = {

    //change numberOfColours to specify the total numbers of colours for neurons
    //The colour scale lasts from 0 to 1, 0 == blue and 1 == red 
    numberOfColours: 600,

    // with X,Y, Z Ratio change the appearance of graph
    xRatio: 7,
    yRatio: 1,
    zRatio: 0.5,


}