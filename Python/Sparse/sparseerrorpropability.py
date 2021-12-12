import numpy as np
from decimal import *

def factorial(n):
    fact = 1
    for i in range(1,n+1): 
        fact = fact * i 
    
    return Decimal(fact)

def  nNadk(n, k):    
    return factorial(n)/(factorial(k)*factorial(n-k))

# a: Active bits
# n: Bits
# q: Threshold
def errProbability(n, a, q):   
    f1 = nNadk(a, q) 
    f2 = nNadk(n-a, a-q) 
    f3 = nNadk(n, a) 
    res = (f1*f2)/f3
    return res


for q in range(40, 18, -1):
     a = 40
     n = 600
     print("%d %10.3E" % (q, errProbability(n, a, q)))

f = 0.02

lines = open("./sparse.txt", 'w')

for x in range(100, 4096, 500):

    n = x
    w = int(x * f)

    res = factorial(n)/(factorial(w)*factorial(n-w))
    text = "%d | %10.3E\n" % (x, res) 
    print("%d %d %10.3E" % (x, w, res))
    lines.write(text)

lines.close()
