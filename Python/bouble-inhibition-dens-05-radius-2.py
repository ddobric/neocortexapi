import plotly
plotly.__version__
import plotly.plotly as py
import plotly.tools as tls

import matplotlib.pyplot as plt

bubbles_mpl = plt.figure()

# doubling the width of markers
x = [0,1,2,3,4,5,6,7,8,9]
y = [2,2,2,2,2,2,2,2,2,2]
k=200

s1 = [1*k,2*k,7*k,0*k,3*k,4*k,16*k,1*k,1.5*k,1.7*k]

#         1    2           5   6             9
s2 = [0*k,2*k,7*k,0*k,0*k,4*k,16*k,0*k,0*k,1.7*k]

plt.title('density=50%, radius=2')
plt.scatter(x,y,s1)
plt.scatter(x,y,s2)

plt.show()