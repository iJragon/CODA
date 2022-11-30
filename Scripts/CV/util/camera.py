import cv2
import argparse
from enum import Enum

class Mode(Enum):
  WEBCAM_INT = 0    # internal 
  WEBCAM_EX = 1     # external
  RECORDING = 2     # pre-recorded video
  PICTURE = 3       # single frame image

def get_capture_device(m: Mode, path="", index=0):
  cap = None

  # Create a VideoCapture object based on mode
  if (m==Mode.WEBCAM_INT):
      cap = cv2.VideoCapture(0)
  elif (m==Mode.WEBCAM_EX):
      cap = cv2.VideoCapture(index)
      if (cap.isOpened() == False):
          print("external camera failed, defaulting to built-in")
          cap = cv2.VideoCapture(0)
  elif (m==Mode.RECORDING):
      cap = cv2.VideoCapture(path)
  else:
      cap = cv2.imread(path)

  return cap

'''
  Use this as a health-check for the camera streaming devices
'''
if __name__ == "__main__":
  # test keyboard emulation, as a stand alone file
  parser = argparse.ArgumentParser()
  parser.add_argument('--source', help='camera source', default=0, type=int)
  args = parser.parse_args()

  print("Opening camera at index", args.source)

  cap = cv2.VideoCapture(args.source)
  # Check if camera opened successfully
  if (cap.isOpened() == False): 
    print("Unable to access camera feed, please try another index")

  # Default resolutions of the frame are obtained.The default resolutions are system dependent.
  
  # We convert the resolutions from float to integer.
  frame_width = int(cap.get(3))
  frame_height = int(cap.get(4))
  
  # loop camera stream until user presses q
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