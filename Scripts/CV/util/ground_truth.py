import cv2
import numpy as np

'''
    ground truth slicing from reference image
    only needs to be run once
'''

ground_truths_letters = {}
ground_truths_numbers = {}
path_letters = "./res/docs/realistic.jpg"
path_numbers = "./res/docs/numbers.jpg"

# format (row, height, col, width) based on realistic.png
ground_truths_letters["A"] = (50,150,70,110)
ground_truths_letters["B"] = (7,190,260,110)
ground_truths_letters["C"] = (36,156,460,90)
ground_truths_letters["D"] = (9,183,654,76)
ground_truths_letters["E"] = (46,146,841,80)
ground_truths_letters["F"] = (133,174,168,95)
ground_truths_letters["G"] = (137,132,363,84)
ground_truths_letters["H"] = (152,131,545,95)
ground_truths_letters["I"] = (108,162,737,85)
ground_truths_letters["J"] = (258,188,85,85)
ground_truths_letters["K"] = (232,188,267,81)
ground_truths_letters["L"] = (237,183,434,118)
ground_truths_letters["M"] = (274,146,644,77)
ground_truths_letters["N"] = (279,141,836,74)
ground_truths_letters["O"] = (404,140,183,73)
ground_truths_letters["P"] = (390,133,346,85)
ground_truths_letters["Q"] = (416,104,524,115)
ground_truths_letters["R"] = (345,185,740,75) 
ground_truths_letters["S"] = (500,140,85,75)
ground_truths_letters["T"] = (500,140,256,81)
ground_truths_letters["U"] = (454,186,436,69)
ground_truths_letters["V"] = (452,188,653,67)
ground_truths_letters["W"] = (456,184,832,83)
ground_truths_letters["X"] = (616,159,158,78)
ground_truths_letters["Y"] = (626,149,341,122)
ground_truths_letters["Z"] = (616,159,532,95)

ground_truths_numbers["1"] = (9,210,45,113)
ground_truths_numbers["2"] = (15,217,224,116)
ground_truths_numbers["3"] = (20,214,375,536-375)


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

im_raw_letters = cv2.imread(path_letters)
im_raw_numbers = cv2.imread(path_numbers)
letters_resize, resize_ratio = ResizeWithAspectRatio(im_raw_letters, width=1000)
numbers_resize, resize_ratio = ResizeWithAspectRatio(im_raw_numbers, width=1000)

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

'''
    slice and capture the pictures for each image
'''
for letter in ground_truths_letters.keys():
    print(letter)
    img = GetLetterSubImage(letters_resize, ground_truths_letters.get(letter))
    path = "./res/ground-truth/" + str(letter) + ".png"
    cv2.imwrite(path, img)

for num in ground_truths_numbers.keys():
    print(num)
    img = GetLetterSubImage(numbers_resize, ground_truths_numbers.get(num))
    path = "./res/ground-truth/" + str(num) + ".png"
    cv2.imwrite(path, img)


