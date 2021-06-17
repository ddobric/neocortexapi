# Simple pygame program


# Import and initialize the pygame library
import math
import os
import pygame
import time

pygame.init()


#----------------------------------------------------------------------------
#Declare XY Plane
screenHeight = 120
screenWidth  = 120
screen = pygame.display.set_mode([screenWidth, screenHeight])
#----------------------------------------------------------------------------
#Declare Circle - Object
r = 40
#Initial Position DOWN LEFT
X = r
Y = screenHeight - r 
#----------------------------------------------------------------------------
# Run until the user asks to quit
running = True
#----------------------------------------------------------------------------
#Declaring a Vector
vectorLength = 10
Angle = -100
vectorAngle = math.radians(Angle) #-- range 0 -> 359 degree on geometric angle--
x = int(math.cos(vectorAngle)*vectorLength)
y = -int(math.sin(vectorAngle)*vectorLength)
vectorTransform = { 'x': x, 'y': y}
#----------------------------------------------------------------------------
#Making Picture for learning 
count = 0
countLimit = 1000 #No of Pictures taken for data
#----------------------------------------------------------------------------
experimentName = "R"+str(r)+"_Angle"+str(Angle)+"_Speed"+str(vectorLength)
fileName = experimentName+str("/Frame")
if(not(os.path.exists(experimentName))):
    try:
        os.mkdir(experimentName)
    except OSError:
        print(OSError)
while running:
    try:  
        # Did the user click the window close button?
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
        # Fill the background with white
        screen.fill((255, 255, 255))
        # Draw a solid blue circle in the center
        if(((X+r+vectorTransform['x'])>screenWidth) or ((X-r+vectorTransform['x'])<0)):
            vectorTransform['x'] = -vectorTransform['x']
            
        if(((Y+r+vectorTransform['y'])>screenHeight) or ((Y-r+vectorTransform['y'])<0)):
            vectorTransform['y'] = -vectorTransform['y']
            
        X+=vectorTransform['x']
        Y+=vectorTransform['y']
        pygame.draw.circle(screen, (0, 0, 0), (X, Y), r)
        fName = fileName+"_"+str(count)+".jpg"
        pygame.image.save(screen, fName)
        time.sleep(0.05)
        count+=1
        # Flip the display
        pygame.display.flip()
    # Done! Time to quit.
    except KeyboardInterrupt:
        running = False
pygame.quit()
