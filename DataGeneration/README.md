# **Python Image generation script for Video Learning Experiment**

# **Idea:**
- Create Training Videos for the Video Learning test

# **Usage:**
- Open the python script in Python IDLE and run it
- Go to the directory in window's command prompt, type *py circle.py*

# **Current state:**
- 3 scripts to create videos of a bouncing circle/triangle/square
- in these script the color/size/startPosition/speed/startingAngle can be customized.
- The color are in RGB
- For the Size, R is used in circle.py as radius, in rectangle.py as half width and half height, in triangle as the radius of outer circle through 3 points.
The Python scripts used OpenCV, can be later ported to C#  

# **Experiment NOTE:**
- The Test now runs primarily with binarized image using lumiance value, for speed. So when creating further videos, it is recommended to use black white frame by setting all the RGB value to 0 on lines:  
```python
#rectangle.py
cv2.fillPoly(frame, pts= [triag_Point], color =(0, 0, 0)) #Line55
#circle.py
cv2.circle(frame, (X, Y), r, (0, 0, 0), -1) #Line45
#triangle.py
cv2.fillPoly(frame, pts= [triag_Point], color =(0, 0, 0)) #Line54
```
- Running with Colored video took a great amount of time (days) to run.
- Renaming the video before the learning is recommended e.g. vd1.