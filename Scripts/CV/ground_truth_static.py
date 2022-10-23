from turtle import width
import cv2
import numpy as np
import time

ground_truths_static = {}
path = "./realistic.jpg"
# format (row, height, col, width) based on realistic.png
ground_truths_static["A"] = (50,150,70,110)
ground_truths_static["B"] = (7,190,260,110)
ground_truths_static["C"] = (36,156,460,90)
ground_truths_static["D"] = (9,183,654,76)
ground_truths_static["E"] = (46,146,841,80)
ground_truths_static["F"] = (133,174,168,95)
ground_truths_static["G"] = (137,132,363,84)
ground_truths_static["H"] = (152,131,545,95)
ground_truths_static["I"] = (108,162,737,85)
ground_truths_static["J"] = (258,188,85,85)
ground_truths_static["K"] = (232,188,267,81)
ground_truths_static["L"] = (237,183,434,118)
ground_truths_static["M"] = (274,146,644,77)
ground_truths_static["N"] = (279,141,836,74)
ground_truths_static["O"] = (404,140,183,73)
ground_truths_static["P"] = (390,133,346,85)
ground_truths_static["Q"] = (416,104,524,115)
ground_truths_static["R"] = (345,185,740,75) 
ground_truths_static["S"] = (500,140,85,75)
ground_truths_static["T"] = (500,140,256,81)
ground_truths_static["U"] = (454,186,436,69)
ground_truths_static["V"] = (452,188,653,67)
ground_truths_static["W"] = (456,184,832,83)
ground_truths_static["X"] = (616,159,158,78)
ground_truths_static["Y"] = (626,149,341,122)
ground_truths_static["Z"] = (616,159,532,95)

def ResizeWithAspectRatio(image, width=None, height=None, inter=cv2.INTER_AREA):
    '''
        resizes an input image with one dimension specified and the other dimension locked in aspect ratio
        returns resized image and resized ratio
    '''
    dim = None
    (h, w) = image.shape[:2]

    if width is None and height is None:
        return image
    if width is None:
        r = height / float(h)
        dim = (int(w * r), height)
    else:
        r = width / float(w)
        dim = (width, int(h * r))

    return cv2.resize(image, dim, interpolation=inter), r

# take in a tuple from the ground truth dict
def GetLetterSubImage(src, dim):
    '''
        src: input image
        dim: tuple representing dimensions to extract from the src img
    '''
    row = dim[0]
    height = dim[1]
    col = dim[2]
    width = dim[3]
    return src[row:row+height,col:col+width]

im_raw = cv2.imread(path)
resize, resize_ratio = ResizeWithAspectRatio(im_raw, width=1000)

while(1):
    cv2.imshow("reference", resize)
    cv2.imshow("A", GetLetterSubImage(resize, ground_truths_static.get("A")))  
    time.sleep(0.25)

    # Press Q on keyboard to stop recording
    if cv2.waitKey(1) & 0xFF == ord('q'):
      break

print(ground_truths_static)