############################################
# All columns have same overlap
# inhibition radius 3
# density takes values: 0.10, 0.20, 0.33, 0,5
############################################


import plotly
plotly.__version__
import numpy as np
import plotly.plotly as py
import plotly.tools as tls
import matplotlib.pyplot as plt

bubbles_mpl = plt.figure()

k=90
yScale =3
x = [0,1,2,3,4,5,6,7,8,9]

# TEST 1
inhibits = [1, 2, 7, 0.1, 3, 4, 16, 1, 1.5, 1.7]
res1 = [0.10, [2, 6]]
res2 = [0.20, [2,6]]
res3 = [0.30, [2,6]]
res4 = [0.40, [2,5,6,9]]
res5 = [0.50, [1,2,4,5,6,9]]

# TEST 2
# inhibits = [ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 ]
# res1 = [0.10, [1, 5]]
# res2 = [0.20, [0,4,8]]
# res3 = [0.30, [0,1,4,5,8]]
# res4 = [0.40, [0,1,3,4,5,8,9]]
# res5 = [0.50, [0,1,2,3,4,5,6,8]]

res = [res1,res2,res3, res4, res5]

clr = ['blue' for num in x]

sizedIhibits = [num * k for num in inhibits]

plt.axis([-1, len(inhibits) + 1, 0, yScale * len(res) + 2])

plt.title('radius=3')

i = 0
for r in res:    
    plt.annotate("density=%f" % r[0], xy=(len(res),yScale *i + 4.4))
    yVals = np.empty(len(inhibits))
    yVals.fill(3 + yScale*i)   

    clr = ['blue' for num in x]

    print(res[0])

    for indx in r[1]:
        clr[indx] = 'green' 

    plt.scatter(x,yVals, s=sizedIhibits, c= clr)
    i=i+1

    
plt.show()