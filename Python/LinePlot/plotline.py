import plotly
import numpy as np
import matplotlib.pyplot as plt
import numpy as np
import sys

delimiter = "|"

if len(sys.argv) <= 3:
    print("WARNING: Invalid arguments.")
    print("python plotline.py <title> <CSV file with data> <output picture name>")
    print("python ./plotline.py title ./sample.txt ./figure1.png")
    print("Data sample")
    print("10|11")
    print("20|11")
    print("30|11")
    print("40|11")
    print("...")
    print("10|11")
    sys.argv = "./LinePlot/plotline.py", "False positive error probability", "./LinePlot/sparse16-1024.txt", "./figure.png"

print(sys.argv)
positions = []
fileName = sys.argv[2]

titles = []
traces = []

lines = open(fileName, 'r')

titles.append(sys.argv[1])

minimum = sys.float_info.max

xNums = []
yNums = []

for line in lines:
    tokenCnt = 0

    tokens = line.split(delimiter)

    xNums.append(float(tokens[0]))
    yNums.append(float(tokens[1]))
   
 
ax1 = plt.subplots()
plt.title(sys.argv[1])
plt.plot(xNums, yNums, lw=2)
plt.show()
plt.close()

