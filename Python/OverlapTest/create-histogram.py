####################################################################################################################################
# Creating and saving of histogram diagram
# This code loads list of specified files and creates histogram. 
# 'python create-histogram.py <Number of Points on X>, <name of data> <data file1>,.., <name of data> <data file N>"
# Example: 
# python ./OverlapTest/create-histogram.py graph1 28 data1 ./OverlapTest/default1-data.txt data2 ./OverlapTest/default2-data.txt
####################################################################################################################################


import plotly
import csv
import sys
import numpy as np
import plotly.plotly as py
import plotly.tools as tls
import matplotlib.pyplot as plt


fileArgs = []
numOfPoints = 10


if len(sys.argv) <= 2:
    print("WARNING: Start with argumnet. I.E.: 'python create-histogram.py 28, data1, .\dataFile1.txt, data2, .\dataFile2.txt'")
    print("'python create-histogram.py <title> <Number of Points on X>, <name of data> <data file1>,.., <name of data> <data file N>")
    sys.argv = "./OverlapTest/create-histogram.py", "graph1", "28", "data1", "./OverlapTest/default1-data.txt", "data2", "./OverlapTest/default2-data.txt"

print(sys.argv)

numOfPoints = int(sys.argv[2])
paramIndx = 0
currTuppleArr = None

for i in range(3, len(sys.argv)):
    prm = paramIndx % 2
    if prm == 0:
        currTuppleArr = []
    
    currTuppleArr.append(sys.argv[i])

    if prm == 1:   
        tpl = (currTuppleArr[0], currTuppleArr[1])              
        fileArgs.append(tpl)
        currTuppleArr = None

    paramIndx += 1


isFirst = True
bins = None
for file in fileArgs:
    dt = np.loadtxt(file[1], comments="#", delimiter=",", unpack=False)
    
    if isFirst:
        bins = np.linspace(min(dt), max(dt), numOfPoints)             
        isFirst = False

    print(dt)
    plt.hist(dt, bins, alpha=0.5, label=file[0])


plt.legend(loc='upper right')
plt.xlabel("Values")
plt.ylabel("Frequency")
plt.title(sys.argv[1])
plt.show()
plt.savefig("figure")
plt.close()


