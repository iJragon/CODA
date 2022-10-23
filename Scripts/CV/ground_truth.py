from operator import le
from re import L
from turtle import width
import cv2
import numpy as np
import time
from pathlib import Path

ground_truths = {}
path = "./realistic.jpg"
# format (row, height, col, width) based on realistic.png
ground_truths["A"] = (50,150,70,110)
ground_truths["B"] = (7,190,260,110)
ground_truths["C"] = (36,156,460,90)
ground_truths["D"] = (9,183,654,76)
ground_truths["E"] = (46,146,841,80)
ground_truths["F"] = (133,174,168,95)
ground_truths["G"] = (137,132,363,84)
ground_truths["H"] = (152,131,545,95)
ground_truths["I"] = (108,162,737,85)
ground_truths["J"] = (258,188,85,85)
ground_truths["K"] = (232,188,267,81)
ground_truths["L"] = (237,183,434,118)
ground_truths["M"] = (274,146,644,77)
ground_truths["N"] = (279,141,836,74)
ground_truths["O"] = (404,140,183,73)
ground_truths["P"] = (390,133,346,85)
ground_truths["Q"] = (416,104,524,115)
ground_truths["R"] = (345,185,740,75) 
ground_truths["S"] = (500,140,85,75)
ground_truths["T"] = (500,140,256,81)
ground_truths["U"] = (454,186,436,69)
ground_truths["V"] = (452,188,653,67)
ground_truths["W"] = (456,184,832,83)
ground_truths["X"] = (616,159,158,78)
ground_truths["Y"] = (626,149,341,122)
ground_truths["Z"] = (616,159,532,95)

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

for letter in ground_truths.keys():
    print(letter)
    img = GetLetterSubImage(resize, ground_truths.get(letter))
    path = "./out/ground-truth/" + str(letter) + ".png"
    print(path)
    cv2.imwrite(path, img)

'''
while(1):
    cv2.imshow("reference", resize)
    cv2.imshow("A", )  
    time.sleep(0.25)

    # Press Q on keyboard to stop recording
    if cv2.waitKey(1) & 0xFF == ord('q'):
      break

print(ground_truths_static)
'''
