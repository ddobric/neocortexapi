import os
import random

import plotly
import plotly.graph_objs as go
import argparse
#import sys
import csv


# if len(sys.argv) < 2:
#print("WARNING: Invalid arguments.")
#print("python draw_figure.py <title> <CSV file with data>")
# print("----------------------------------------------------")

#sys.argv = "./draw_figure.py", "./sample.txt"

# print(sys.argv)
#fileName = sys.argv[1]

#inputFile = open(fileName)
# for line in inputFile:
# print(line)

parser = argparse.ArgumentParser(description='Draw convergence figure')
parser.add_argument(
    '--filename', '-f', help='Filename from which data is supposed to be red')
parser.add_argument(
    '--axis', '-a', help='Cells are placed on desired x or y axis')
args = parser.parse_args()

# Plotly requires a valid user to be able to save High Res images
plotlyUser = os.environ.get('PLOTLY_USERNAME')
plotlyAPIKey = os.environ.get('PLOTLY_API_KEY')
if plotlyAPIKey is not None:
    plotly.plotly.sign_in(plotlyUser, plotlyAPIKey)


def plotActivity(l2ActiveCellsMultiColumn, highlightTouch):
    maxTouches = 15
    numTouches = min(maxTouches, len(l2ActiveCellsMultiColumn))
    numColumns = len(l2ActiveCellsMultiColumn[0])
    fig = plotly.tools.make_subplots(
        rows=1, cols=numColumns, shared_yaxes=True,
        subplot_titles=('Column 1', 'Column 2', 'Column 3')[0:numColumns]
    )

    data = go.Scatter(x=[], y=[])

    shapes = []
    for t, sdrs in enumerate(l2ActiveCellsMultiColumn):
        if t <= numTouches:
            for c, activeCells in enumerate(sdrs):
                # print t, c, len(activeCells)
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
    fig['layout']['annotations'].append({
        'font': {'size': 20},
        'xanchor': 'center',
        'yanchor': 'bottom',
        'text': 'Number of touches',
        'xref': 'paper',
        'yref': 'paper',
        'x': 0.5,
        'y': -0.15,
        'showarrow': False,
    })
    fig['layout']['annotations'].append({
        'font': {'size': 24},
        'xanchor': 'center',
        'yanchor': 'bottom',
        'text': ['', '<b>One cortical column</b>', '',
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
            'title': "Neuron #",
            'range': [-100, 4201],
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
    basename = 'plots/activity_c' + str(numColumns)
    plotly.offline.plot(fig, filename=basename + '.html', auto_open=True)

    # Can't save image files in offline mode
    if plotlyAPIKey is not None:
        plotly.plotly.image.save_as(fig, filename=basename + '.pdf', scale=4)


dataSets = []
# with open("C:\\Users\\ataul\\source\\repos\\NeoCortex\\Python\\ColumnActivityDiagram\\sample.txt") as datafile:
with open(args.filename) as datafile:
    csv_reader = csv.reader(datafile, delimiter=';')
    for row in csv_reader:
        cells = map(int, row)
        dataSets.append([set(cells)])
    print(len(dataSets))


plotActivity(dataSets, 7)
