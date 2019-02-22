############################################
# All columns have same overlap
# inhibition radius 3
# density takes values: 0.10, 0.20, 0.33, 0,5
############################################


import plotly
import sys
plotly.__version__
import numpy as np
import plotly.plotly as py
import plotly.tools as tls
import matplotlib.pyplot as plt

if len(sys.argv) <= 2:
    print("WARNING: Start with argumnet. I.E.: 'python bubles.py title, data1, .\dataFile1.txt, data2, .\dataFile2.txt'")
    print("'python bubbles.py <title> <Number of Points on X>, <name of data> <data file1>,.., <name of data> <data file N>")
    sys.argv = "./boubles.py", "graph1", "data1", "./Boubles/synapses.txt"

print(sys.argv)

data = []
row = []

for x in range(2, len(sys.argv)):
    n=(x-2) % 2
    row.append(sys.argv[x])
    if n==1:
        data.append(row)
        row = []

bubbles_mpl = plt.figure()

delimiter = ","
bubleSize = 10

yScale = 1
i = 0
for f in data:  
    fileName = f[1]
    lines = open(fileName, 'r')

    colors = ['blue','green', 'red', 'black']

    for line in lines:

        if(line != '\n' ):
            numbers = []
            tokens = line.split(delimiter)
            for token in tokens:
                if( token.strip()):
                    numbers.append(float(token))
            
            y = 0 + yScale * i
            #plt.annotate("%01d" % (i-1), xy=(-25, y))
            yVals = np.empty(len(numbers))
            yVals.fill(y)   

            boubleSizes = []
            boubleSizes = np.empty(len(numbers))
            boubleSizes.fill(bubleSize)   

            clr = [colors[i%4] for num in numbers]

            plt.scatter(numbers,yVals, s=boubleSizes, c= clr)
            i=i+1

    plt.show()

    

    
