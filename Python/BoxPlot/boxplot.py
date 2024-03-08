########################################################################################
# Representing of neigborhood
# Examples:
# python Title ./BoxPlot/boxplot.py ./BoxPlot/data.csv
# python ./boxplot.py "Many messages, Page=5000, Single device" "./query 5000 large results.txt" "./query 5000 large results.png"
# python "BoxPlot/boxplot.py" "Title" "BoxPlot/SpatialPoolerInitTimeNoAkka.txt" "BoxPlot/Spatial Pooler init.png" "xLabel" "yLabel"
########################################################################################


import plotly.plotly as py
import matplotlib.pyplot as plt
import plotly.tools as tls
import numpy as np
import plotly
import csv
import sys
plotly.__version__

delimiter = "|"

if len(sys.argv) <= 3:
    print("WARNING: Invalid arguments.")
    print("python create-histogram.py <title> <CSV file with data> <output picture name>")
    print("python ./boxplot.py title ./boxplotsample.txt ./figure1.png" "xLabel" "yLabel")
    print("File example:")
    print("100 | 1 | 21.5 | 24.5 | 2 | 4 | 5 | 6 | 7 | 7 | 9 ")
    print("First number is number on X axiss.")
    sys.argv = "./BoxPlot/boxplot.py", "sample graph", "./BoxPlot/boxplotsample.txt", "./figure.png", "xLabel", "yLabel"

print(sys.argv)

fileName = sys.argv[2]

lines = open(fileName, 'r')

data = []
titles = []
minimum = sys.float_info.max
positions = []

for line in lines:
    tokenCnt = 0
    numbers = []
    tokens = line.split(delimiter)

    for token in tokens:
        if tokenCnt == 0:
            # first word is title of sequence.
            titles.append(token)
            #positions.append(int(token))
        else:
            # here we read numbers in sequence
            numbers.append(float(token))
            if minimum > min(numbers):
                minimum = min(numbers)

        tokenCnt = tokenCnt + 1

    data.append(numbers)

fig1, ax1 = plt.subplots()
ax1.set_title(sys.argv[1])
positions = np.array(range(len(data)))
ax1.boxplot(data, positions=positions)

pos = 0
for label in titles:
    ax1.text(positions[pos],  minimum - 5000, label,
             horizontalalignment='center', size='x-small', weight=5,
             color="blue")
    pos = pos + 1

plt.xlabel(sys.argv[4], fontsize=13)
plt.ylabel(sys.argv[5], fontsize=13)

plt.savefig(sys.argv[3])
plt.show()
plt.close()
