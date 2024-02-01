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
    a=int(a)
    f1 = nNadk(a, q) 
    f2 = nNadk(n-a, a-q) 
    f3 = nNadk(n, a) 
    res = (f1*f2)/f3
    return res

print(" EXPERIMENT False Positives ")

f = 0.02
 

for n in range(512, 5000, 512):
    print("--------------- %d --------------", n)
    a = int(n * f)
    b = int(a/3)
    # 30% of possible active bits are used as a threshold. 
    for q in range(a, b, -1):
       #print("%d %d %d %10.3E" % (n, a, q, errProbability(n, a, q)))
       print("%d %d %10.3E" % (n, q, errProbability(n, a, q)))
    
lines = open("./sparse.txt", 'w')

for x in range(100, 4096, 500):

    n = x
    w = int(x * f)

    res = factorial(n)/(factorial(w)*factorial(n-w))
    text = "%d | %10.3E\n" % (x, res) 
    print("%d %d %10.3E" % (x, w, res))
    lines.write(text)

lines.close()
