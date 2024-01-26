import numpy as np
from decimal import *

def factorial(n):
    fact = 1
    for i in range(1,n+1): 
        fact = fact * i 
    
    return Decimal(fact)

f = 0.02

lines = open("./sparse.txt", 'w')

n=16
w=1
print(factorial(n)/(factorial(w)*factorial(n-w)))

for x in range(500, 4096, 100):

    n = x
    w = int(x * f)

    res = factorial(n)/(factorial(w)*factorial(n-w))
    text = "%d | %10.3E\n" % (x, res) 
    print("%d %d %10.3E" % (x, w, res))
    lines.write(text)

lines.close()