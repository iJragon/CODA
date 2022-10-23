import cv2
import numpy as np

'''
  Use this as a health-check for the camera streaming devices
'''

cap = cv2.VideoCapture(2)
# Check if camera opened successfully
if (cap.isOpened() == False): 
  print("Unable to read camera feed")
# Default resolutions of the frame are obtained.The default resolutions are system dependent.
# We convert the resolutions from float to integer.
frame_width = int(cap.get(3))
frame_height = int(cap.get(4))
 
while(True):
  ret, frame = cap.read()
  if ret == True: 
    # Display the resulting frame    
    cv2.imshow('frame',frame)
 
    # Press Q on keyboard to stop recording
    if cv2.waitKey(1) & 0xFF == ord('q'):
      break
  # Break the loop
  else:
    break 
# When everything done, release the video capture and video write objects
cap.release() 
# Closes all the frames
cv2.destroyAllWindows()