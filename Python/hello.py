# https://code.visualstudio.com/docs/python/python-tutorial#_select-a-python-interpreter
# py -3 -m venv .venv
import matplotlib.pyplot as plt
import numpy as np

msg = "Hello World"
print(msg)

x = np.linspace(0, 20, 100)  # Create a list of evenly-spaced numbers over the range
plt.plot(x, np.sin(x))       # Plot the sine of each x point
plt.show()                   # Display the plot