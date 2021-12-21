import cv2
import numpy as np
from cv2 import VideoWriter, VideoWriter_fourcc
import math
import os
def triangleCoord(centerPT, R):
    pt1 = (int(centerPT[0]-R),int(centerPT[1]+R))
    pt2 = (int(centerPT[0]+R),int(centerPT[1]+R))
    pt3 = (int(centerPT[0]+R),int(centerPT[1]-R))
    pt4 = (int(centerPT[0]-R),int(centerPT[1]-R))
    triangle_cnt = np.array( [pt1, pt2, pt3, pt4] )
    return triangle_cnt


screenWidth = 120
screenHeight = 120
FPS = 24
seconds = 3
r = 20
Y = screenHeight - r  - 20
X = r + 20

vectorLength = 10
Angle = 30
vectorAngle = math.radians(Angle) #-- range 0 -> 359 degree on geometric angle--
x = int(math.cos(vectorAngle)*vectorLength)
y = -int(math.sin(vectorAngle)*vectorLength)
vectorTransform = { 'x': x, 'y': y}

experimentName = "R"+str(r)+"_Angle"+str(Angle)+"_Speed"+str(vectorLength)
fileName = experimentName+str("/")
if(not(os.path.exists(experimentName))):
    try:
        os.mkdir(experimentName)
    except OSError:
        print(OSError)

fourcc = VideoWriter_fourcc(*'MP42')
video = VideoWriter('./'+experimentName+'/rectangle.mp4', -1, float(FPS), (screenWidth, screenHeight))

for i in range(FPS*seconds):
    frame = np.zeros((screenHeight,screenWidth,3),np.uint8)
    # Reseet Frame to white
    frame[:,:] = [255, 255, 255]
    # Draw a triangle
    triag_Point = triangleCoord([X,Y],r)
    print(triag_Point)
    if(((triag_Point[2][0]+vectorTransform['x'])>screenWidth) or ((triag_Point[0][0]+vectorTransform['x'])<0)):
        vectorTransform['x'] = -vectorTransform['x']
            
    if(((triag_Point[1][1]+vectorTransform['y'])>screenHeight) or ((triag_Point[2][1]+vectorTransform['y'])<0)):
        vectorTransform['y'] = -vectorTransform['y']
    X+=vectorTransform['x']
    Y+=vectorTransform['y']
    cv2.fillPoly(frame, pts= [triag_Point], color =(0, 0, 0))
    video.write(frame)

video.release()