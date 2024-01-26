import plotly
import plotly.plotly as py
import plotly.graph_objs as go
import numpy as np
import sys

#plotly.tools.set_credentials_file(username='ddobric', api_key='lr1c37zw81')
#plotly.tools.set_config_file(world_readable=False, sharing='private')

#py.sign_in()

delimiter = "|"

if len(sys.argv) <= 3:
    print("WARNING: Invalid arguments.")
    print("python lineplot.py <title> <CSV file with data> <output picture name>")
    print("python ./boxplot.py title ./sample.txt ./figure1.png")
    print("Data sample")
    print("10|11")
    print("20|11")
    print("30|11")
    print("40|11")
    print("...")
    print("10|11")
    sys.argv = "./lineplot.py", "sample graph", "./sparse.txt", "./figure.png"

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
    
  
trace = go.Scatter(
    x = xNums,
    y = yNums,
    mode = 'lines',
    name = titles[0]
)


traces.append(trace)

layout = go.Layout(
    xaxis=dict(
        autorange=True,
        showgrid=False,
        zeroline=False,
        showline=False,
        ticks='',
        showticklabels=False
    ),
    yaxis=dict(
       tickmode='linear',
        ticks='outside',
        tick0=0,
        dtick=0.25,
        ticklen=8,
        tickwidth=4,
        tickcolor='#000'
    )
)

py.iplot(traces, filename='line-mode', layout=layout)

