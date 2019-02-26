import numpy as np

def factorial(n):
    fact = 1
    for i in range(1,n+1): 
        fact = fact * i 
    
    return fact;

f = 0.2

lines = open("./sparse.txt", 'w')

for x in range(16, 128, 10):

    n = x
    w = int(x * f)

    res = factorial(n)/(factorial(w)*factorial(n-w))
    text = "%d | %10.3E\n" % (x, res) 
    print("%d %d %10.3E" % (x, w, res))
    lines.write(text)

lines.close()
