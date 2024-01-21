########################################################################################
# Representing of neigborhood
# Example:
# python ./NeighborhoodTest/neighborhood-test.py ./NeighborhoodTest/resultfile.csv
########################################################################################


import plotly
import csv
import sys
plotly.__version__
import numpy as np
import plotly.plotly as py
import plotly.tools as tls
import matplotlib.pyplot as plt

bubbles_mpl = plt.figure()
defColor='blue'
defActiveColor = 'green'
defCenterColor = 'black'
defBubblSize=10
yScale =1
maxRows=70 * yScale

print("Neigborhood Results")
print(len(sys.argv))

for arg in sys.argv:
     print(arg)

if len(sys.argv) != 2:
    print("WARNING: Start with argumnet. I.E.: 'python neighborhood-test.py .\resultfile.csv'")
    fileName = "./NeighborhoodTest/default.csv"
else:
    fileName = sys.argv[1]

print("Loading file '" + fileName + "'")

isFirstRow = True
i = 0
numOfPoints = 0

with open(fileName, 'r') as csvfile:
    file = csv.reader(csvfile, delimiter='|')
    for row in file:
      
        if isFirstRow:
            numOfPoints =  int(row[0])       
            colors = [defColor for point in range(numOfPoints)]        
            plt.axis([0,numOfPoints + 2, 0, maxRows + 2])
            isFirstRow=False
            plt.title('Neighborhoods. Radius = ' + row[2] )
            print("Plot for radius=" + row[2] + ", cells=" + str(numOfPoints))
            row = next(file, None)
           
        yVals = [3 + yScale*i for indx in range(numOfPoints)]   
       
        xVals = [xIndx for xIndx in range(numOfPoints)]   

        bubbleSizes = [defBubblSize for num in row]

        colors = ['blue' for num in range(numOfPoints)]

        for indx in row:
            colors[int(indx)] = defActiveColor 

        colors[int(row[0])] = defCenterColor

        plt.scatter(xVals,yVals, s=bubbleSizes, c= colors)
        i=i+1
    
plt.show()