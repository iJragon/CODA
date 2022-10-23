'''
    collection of vision utilities
'''
import cv2

def quit_capture():
    # Press Q on keyboard to stop recording
    return cv2.waitKey(1) & 0xFF == ord('q')