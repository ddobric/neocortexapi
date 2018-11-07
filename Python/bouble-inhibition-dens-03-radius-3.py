############################################
# All columns have same overlap
# inhibition radius 3
# density takes values: 0.10, 0.20, 0.33, 0,5
############################################


import plotly
plotly.__version__
import plotly.plotly as py
import plotly.tools as tls

import matplotlib.pyplot as plt

bubbles_mpl = plt.figure()

# doubling the width of markers
x = [0,1,2,3,4,5,6,7,8,9]
y = [2,2,2,2,2,2,2,2,2,2]
y2 = [i * 2 for i in y]

k=200

s1 = [1*k,1*k,1*k,1*k,1*k,1*k,1*k,1*k,1*k,1*k]

# density 0.33
#     0    1           4    5          8   
s2 = [1*k,1*k,0*k,0*k,1*k,1*k,0*k,0*k,0*k,0*k]

# density 0.20
#     0               4               8   
s3 = [1*k,1*k,0*k,0*k,1*k,0*k,0*k,0*k,0*k,0*k]

plt.title('density=0.20, 0.33, radius=3')
plt.scatter(x,y,s1)
plt.scatter(x,y,s2)
plt.annotate('density=0.33',  xy=(3,2), xytext=(5, 3)) 

plt.scatter(x,y2,s1)
plt.scatter(x,y2,s3)
plt.annotate('density=0.20',  xy=(3,2), xytext=(5, 5)) 
plt.show()