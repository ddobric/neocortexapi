import os
import random
import sys
import plotly
import plotly.subplots
import itertools
import plotly.graph_objs as go
import argparse
import csv

import os

# py -m ensurepip
# pip install plotly
# Without pip: py -m pip install plotly
# py draw_figure.py -fn "C:\dev\devops-daenet\NeoCortexApi\NeoCortexApi\UnitTestsProject\bin\Debug\netcoreapp3.1\exp.csv" -gn Digit -mc 1000 -ht 15 -yt "column indicies" -xt cycle -st "Predicted/Expected/Predictive Cells" -fign CortialColumn
# py draw_figure.py -fn "C:\dev\devops-daenet\NeoCortexApi\Results\Spatial Pooler Stability\HomeostaticPlasticityActivator2\ActiveColumns_MaxBoost_5_MinOverl_1_4_plotly-input.csv" -gn Digit_4 -mc 1000 -ht 15 -yt column -xt cycle -st singlecolumn -fign CortialColumn        
# py draw_figure.py -fn "C:\dev\devops-daenet\NeoCortexApi\Results\Spatial Pooler Stability\SpStability Experiment 2 - with new born effect\ActiveColumns_MaxBoost_5_MinOverl_1_4_plotly-input.csv" -gn GRAPHNAME -mc 1000 -ht 2 -yt column -xt cycle -st singlecolumn -fign CortialColumn        
# python draw_figure.py -fn "C:\dev\git\NeoCortex\Results\Spatial Pooler Stability\SpStability Experiment 2 Boost 00 - digits 0 1 2 stable\ActiveColumns_Boost_0_0_plotly-input.csv" -gn GRAPHNAME -mc 200 -ht 2 -yt yaxis -xt xaxis -st singlecolumn -fign CortialColumn
# python draw_figure.py -fn "C:\dev\git\NeoCortex\Results\Spatial Pooler Stability\SpStability Experiment 2 Boost 00 - digits 0 1 2 stable\ActiveColumns_Boost_0_0_plotly-input.csv" -gn "Digit 0" -mc 700 -ht 320 -yt "Column Index" -xt Cycle -st "MinvOverlapCycles=0.1, first 300 cycles" -fign CortialColumn
# python draw_figure.py -fn sample.txt -gn test1 -mc 19 -ht 8 -yt yaxis -xt xaxis -min 50 -max 4000 -st 'single column' -fign CortialColumn
# python draw_figure.py -fn sample.txt -gn test1 -mc 19 -ht 8 -yt yaxis -xt xaxis -min 50 -max 4000 -st 'single column' -fign CortialColumn -a
parser = argparse.ArgumentParser(description='Draw convergence figure')
parser.add_argument('--filename', '-fn',help='Filename from which data is supposed to be red', required=True)
parser.add_argument(
    '--graphename', '-gn', help='Graphname where data is supposed to be plot', required=True)
parser.add_argument(
    '--maxcycles', '-mc', help='Number of maximum touches/iterations ', required=True, type=int)
parser.add_argument(
    '--highlighttouch', '-ht', help='Number of highlight touches', required=True, type=int)
parser.add_argument('--axis', '-a', nargs='?', default=None,
                    help='Cells are placed on desired x or y axis, enter x or y')
parser.add_argument(
    '--yaxistitle', '-yt', help='The title of the y-axis', required=True, type=str)
parser.add_argument(
    '--xaxistitle', '-xt', help='The title of the x-axis', required=True, type=str)
parser.add_argument(
    '--mincellrange', '-min', help='Minimun range of the cells', nargs='?',  type=int)
parser.add_argument(
    '--maxcellrange', '-max', help='Maximum range of the cells', nargs='?',  type=int)
parser.add_argument(
    '--subplottitle', '-st', help='The title of the subplot', required=True, type=str)
parser.add_argument(
    '--figurename', '-fign', help='The name of the figure', required=True, type=str)

args = parser.parse_args()

# Plotly requires a valid user to be able to save High Res images
plotlyUser = os.environ.get('PLOTLY_USERNAME')
plotlyAPIKey = os.environ.get('PLOTLY_API_KEY')
if plotlyAPIKey is not None:
    plotly.plotly.sign_in(plotlyUser, plotlyAPIKey)

#text = args.   # Neuron #
maxcycles = args.maxcycles
highlight_touch = args.highlighttouch
yAxisTitle = args.yaxistitle
xAxisTitle = args.xaxistitle
minCellRange = args.mincellrange
maxCellRange = args.maxcellrange
subPlotTitle = args.subplottitle
figureName = args.graphename
filename = args.filename

print (filename)
# os.path.realpath(__file__)


def plotActivityVertically(activeCellsColumn, highlightTouch):
    numTouches = min(maxcycles, len(activeCellsColumn))
    numColumns = len(activeCellsColumn[0])
    fig = plotly.subplots.make_subplots(
        rows=1, cols=numColumns, shared_yaxes=True,
        subplot_titles=(subPlotTitle, 'Column 2', 'Column 3')[0:numColumns]
        #subplot_titles=('Column 1', 'Column 2', 'Column 3')[0:numColumns]
    )

    data = go.Scatter(x=[], y=[])

    shapes = []
    for t, sdrs in enumerate(activeCellsColumn):
        if t <= numTouches:
            for c, activeCells in enumerate(sdrs):
                for cell in activeCells:
                    shapes.append(
                        {
                            'type': 'rect',
                            'xref': 'x' + str((c + 1)),
                            'yref': 'y1',
                            'x0': t,
                            'x1': t + 0.6,
                            'y0': cell,
                            'y1': cell + 1,
                            'line': {
                                # 'color': 'rgba(128, 0, 128, 1)',
                                'width': 2,
                            },
                            # 'fillcolor': 'rgba(128, 0, 128, 0.7)',
                        },
                    )
                if t == highlightTouch:
                    # Add red rectangle
                    shapes.append(
                        {
                            'type': 'rect',
                            'xref': 'x' + str((c + 1)),
                            'x0': t,
                            'x1': t + 0.6,
                            'y0': -95,
                            'y1': 4100,
                            'line': {
                                'color': 'rgba(255, 0, 0, 0.5)',
                                'width': 3,
                            },
                        },
                    )

    # Legend for x-axis and appropriate title
    
    # depending on the python version you might have to remove the list(..)
    list(fig['layout']['annotations']).append({
        'font': {'size': 20},
        'xanchor': 'center',
        'yanchor': 'bottom',
        # 'text': 'Number of touches',
        'text': xAxisTitle,
        'xref': 'paper',
        'yref': 'paper',
        'x': 0.5,
        'y': -0.15,
        'showarrow': False,
    })

    # depending on the python version you might have to remove the list(..)
    list(fig['layout']['annotations']).append({
        'font': {'size': 24},
        'xanchor': 'center',
        'yanchor': 'bottom',
        # 'text': ['', '<b>One cortical column</b>', '', '<b>Three cortical columns</b>'][numColumns],
        'text': ['', figureName, '',
                 '<b>Three cortical columns</b>'][numColumns],
        'xref': 'paper',
        'yref': 'paper',
        'x': 0.5,
        'y': 1.1,
        'showarrow': False,
    })
    layout = {
        'height': 600,
        'font': {'size': 18},
        'yaxis': {
            # 'title': "Neuron #",
            'title': yAxisTitle,
            # 'range': [-100, 4201],
            'range': [minCell, maxCell],
            'showgrid': False,
        },
        'shapes': shapes,
    }

    if numColumns == 1:
        layout.update(width=320)
    else:
        layout.update(width=700)

    for c in range(numColumns):
        fig.append_trace(data, 1, c + 1)
        fig['layout']['xaxis' + str(c + 1)].update({
            'title': "",
            'range': [0, numTouches],
            'showgrid': False,
            'showticklabels': True,
        }),

    fig['layout'].update(layout)

    # Save plots as HTM and/or PDF
    basename = args.graphename + str(numColumns)
    plotly.offline.plot(fig, filename=basename + '.html', auto_open=True)

    # Can't save image files in offline mode
    if plotlyAPIKey is not None:
        plotly.plotly.image.save_as(fig, filename=basename + '.pdf', scale=4)


def plotActivityHorizontally(activeCellsColumn, highlightTouch):
    numTouches = min(maxcycles, len(activeCellsColumn))
    numColumns = len(activeCellsColumn[0])
    fig = plotly.subplots.make_subplots(
        rows=1, cols=numColumns, shared_yaxes=True,
        subplot_titles=(subPlotTitle, 'Column 2', 'Column 3')[0:numColumns]
    )

    data = go.Scatter(x=[], y=[])

    shapes = []
    for t, sdrs in enumerate(activeCellsColumn):
        if t <= numTouches:
            for c, activeCells in enumerate(sdrs):
                for cell in activeCells:
                    shapes.append(
                        {
                            'type': 'rect',
                            'xref': 'x1',
                            'yref': 'y' + str((c + 1)),
                            'x0': cell,
                            'x1': cell + 1,
                            'y0': t,
                            'y1': t + 0.6,
                            'line': {
                                # 'color': 'rgba(128, 0, 128, 1)',
                                'width': 2,
                            },
                            # 'fillcolor': 'rgba(128, 0, 128, 0.7)',
                        },
                    )
                if t == highlightTouch:
                    # Add red rectangle
                    shapes.append(
                        {
                            'type': 'rect',
                            'yref': 'y' + str((c + 1)),
                            'x0': -95,
                            'x1': 4100,
                            'y0': t,
                            'y1': t + 0.6,
                            'line': {
                                'color': 'rgba(255, 0, 0, 0.5)',
                                'width': 3,
                            },
                        },
                    )

    # Legend for y-axis and appropriate title
    fig['layout']['annotations'].append({
        'font': {'size': 24},
        'xanchor': 'center',
        'yanchor': 'bottom',
        'text': ['', figureName, '',
                 '<b>Three cortical columns</b>'][numColumns],
        'xref': 'paper',
        'yref': 'paper',
        'x': 0.5,
        'y': 1.1,
        'showarrow': False,
    })
    fig['layout']['annotations'].append({
        'font': {'size': 20},
        'xanchor': 'center',
        'yanchor': 'bottom',
        'text': xAxisTitle,  # also checked
        'xref': 'paper',
        'yref': 'paper',
        'x': 0.5,
        'y': -0.15,
        'showarrow': False,
    })
    layout = {
        'width': 600,
        'font': {'size': 18},
        'yaxis': {
            'title': yAxisTitle,  # checked
            # 'title': numTouches,  # checked
            'range': [0, numTouches],
            'showgrid': False,
        },
        'shapes': shapes,
    }

    if numColumns == 1:
        layout.update(height=320)
    else:
        layout.update(height=700)

    for c in range(numColumns):
        fig.append_trace(data, 1, c + 1)
        fig['layout']['yaxis' + str(c + 1)].update({
            'title': "",
            'range': [minCell, maxCell],
            'showgrid': False,
            'showticklabels': True,
        }),

    fig['layout'].update(layout)

    # Save plots as HTM and/or PDF
    basename = args.graphename + str(numColumns)
    plotly.offline.plot(fig, filename=basename + '.html', auto_open=True)

    # Can't save image files in offline mode
    if plotlyAPIKey is not None:
        plotly.plotly.image.save_as(fig, filename=basename + '.pdf', scale=4)


dataSets = []
allCells = []
cell = []
# with open("C:\\Users\\ataul\\source\\repos\\NeoCortex\\Python\\ColumnActivityDiagram\\sampleOne.txt") as datafile:
with open(filename, 'r') as datafile:
    csv_reader = csv.reader(datafile, skipinitialspace=False,
                            delimiter=',', quoting=csv.QUOTE_NONE)
    for row in csv_reader:
        
        if len(row)>0 and (row[-1] == '' or row[-1] == ' ' or row[-1] == ','):
            del row[-1]
        for i in row:
            if i == '':
                del row[i]
            j = i.strip()
            cell.append(int(j))
            allCells.append(int(j))
        dataSets.append([set(cell)])
        cell = []


maxCell = max(allCells)+100
minCell = min(allCells)-100

# print(len(dataSets))

if args.axis == 'x':
    plotActivityHorizontally(dataSets, highlight_touch)
else:
    plotActivityVertically(dataSets, highlight_touch)

# if args.maxCellRange:
