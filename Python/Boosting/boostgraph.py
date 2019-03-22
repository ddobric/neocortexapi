import plotly
import numpy as np
import matplotlib.pyplot as plt
import numpy as np
import sys


def calcBostFactors():
    xDutyCycles = []
    yBostFactors = []

    maxBoost = 5

    for minActDuty in range(1, 10):
        isWriten = False
        
        for actDutyCycle in range(0, 10):
            xDutyCycles.append(actDutyCycle)
            
            if(actDutyCycle < minActDuty):
                yBostFactors.append(((1-maxBoost)/minActDuty) * actDutyCycle + maxBoost)           
            else:
                yBostFactors.append(1)
                if(isWriten==False):
                    plt.text(actDutyCycle, 1, "%d" % (minActDuty))
                    isWriten = True

        plt.text(0, 1, "minDuty")       
        plt.title("maxBoost %d | minActDuty %d\n" % (maxBoost, minActDuty))
        plt.plot(xDutyCycles, yBostFactors, lw=2)


    plt.show()
    plt.close()


def calcDutyCycle():
   
    #period = 5
    for period in range(1, 1000, 100):
        for overlap in range(50, 150, 50):
            xCycles = [0]
            yDutyCycles = [0]

            for dutyCycle in range(1, 200):        
                xCycles.append(dutyCycle)
                yDutyCycles.append((((period-1)*yDutyCycles[dutyCycle-1]) + overlap) / period)           
            
            plt.text(xCycles[int(len(xCycles)/2)], yDutyCycles[int(len(yDutyCycles)/2)], "o,p=(%d,%d)" % (overlap, period))     
            plt.title("overlap %d" % (overlap))
            plt.plot(xCycles, yDutyCycles, lw=2)

    plt.show()
    plt.close()

#calcBostFactors()
calcDutyCycle()
