import numpy as np

def factorial(n):
    fact = 1
    for i in range(1,n+1): 
        fact = fact * i 
    
    return fact

def  nNadk(n, k):    
    return factorial(n)/(factorial(k)*factorial(n-k))

def errProbability(n, a, q):   
    f1 = nNadk(a, q) 
    f2 = nNadk(n-a, a-q) 
    f3 = nNadk(n, a) 
    res = (f1*f2)/f3
    return res

# print("%d " % factorial(0))

# for x in range(0, 100, 1):
#      res = factorial(x)/(factorial(x)*factorial(0))
#      print("%d %d " % (x, res))

for q in range(40, 18, -1):
     a = 40
     n = 600
     print("%d %10.3E" % (q, errProbability(n, a, q)))

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
