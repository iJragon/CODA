import cv2
import mediapipe as mp
import time
import copy
import itertools
import numpy as np

'''
    This file contains the helper functions used to annotate the ground truth images
    These outputs are used to determine which expression best matches with the hand being shown
'''

target = "3.png"
path = "./out/ground-truth/" + target
img = cv2.imread(path)

# init mediapipe
mpHands = mp.solutions.hands
hands = mpHands.Hands()
mpDraw = mp.solutions.drawing_utils
imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
results = hands.process(imgRGB)

'''
    Useful Computational Functions
'''

def calc_landmark_list(image, landmarks):
    image_width, image_height = image.shape[1], image.shape[0]

    landmark_point = []

    # Keypoint
    for _, landmark in enumerate(landmarks.landmark):
        landmark_x = min(int(landmark.x * image_width), image_width - 1)
        landmark_y = min(int(landmark.y * image_height), image_height - 1)
        # landmark_z = landmark.z

        landmark_point.append([landmark_x, landmark_y])

    return landmark_point

def pre_process_landmark(landmark_list):
    temp_landmark_list = copy.deepcopy(landmark_list)

    # Convert to relative coordinates
    base_x, base_y = 0, 0
    for index, landmark_point in enumerate(temp_landmark_list):
        if index == 0:
            base_x, base_y = landmark_point[0], landmark_point[1]

        temp_landmark_list[index][0] = temp_landmark_list[index][0] - base_x
        temp_landmark_list[index][1] = temp_landmark_list[index][1] - base_y

    # Convert to a one-dimensional list
    temp_landmark_list = list(
        itertools.chain.from_iterable(temp_landmark_list))

    # Normalization
    max_value = max(list(map(abs, temp_landmark_list)))

    def normalize_(n):
        return n / max_value

    temp_landmark_list = list(map(normalize_, temp_landmark_list))

    return temp_landmark_list


'''
     The following code will help annotate the ground-truth image
'''
cap = cv2.VideoCapture(2)

if results.multi_hand_landmarks:
    # print(results.multi_hand_landmarks)
    for handLms in results.multi_hand_landmarks:
        landmark_list = calc_landmark_list(img, handLms)
        for id, lm in enumerate(handLms.landmark):
            h, w, c = img.shape
            cx, cy = int(lm.x * w), int(lm.y * h)
            # annotate the 21 key points on the static image
            cv2.circle(img, (cx, cy), 8, (40, 100, 255), cv2.FILLED)

        mpDraw.draw_landmarks(img, handLms, mpHands.HAND_CONNECTIONS)

# Determine and Normalize the Characteristic Array for 
numpy_landmark_list = np.array(landmark_list)
normalized_landmark_list = pre_process_landmark(landmark_list)
result = np.array(normalized_landmark_list).reshape((numpy_landmark_list.shape))

# print the result to capture the annotation's array, then generate a lookup function
print(result)
print(result.shape)

write_path = "./out/annotations/" + target
cv2.imwrite(write_path, img)

'''
    Annotated Ground Truths for Comparison in keypoint_tracker.py
    These were generated, individually, using the code above
    Each one will return a ground-truth (gt) array for the corresponding symbol / expression
'''

def gt_A():
    return np.array(
    [[ 0.,0.],
    [ 0.34782609 ,-0.10144928],
    [ 0.62318841 ,-0.39130435],
    [ 0.65217391 ,-0.72463768],
    [ 0.66666667 ,-1.        ],
    [ 0.44927536, -0.73913043],
    [ 0.43478261 ,-0.89855072],
    [ 0.36231884 ,-0.65217391],
    [ 0.31884058, -0.43478261],
    [ 0.23188406, -0.75362319],
    [ 0.2173913 , -0.89855072],
    [ 0.17391304, -0.57971014],
    [ 0.1884058 , -0.36231884],
    [ 0.02898551, -0.73913043],
    [ 0.01449275 ,-0.85507246],
    [ 0.        , -0.53623188],
    [ 0.02898551, -0.33333333],
    [-0.1884058 , -0.68115942],
    [-0.17391304, -0.7826087 ],
    [-0.15942029 ,-0.53623188],
    [-0.10144928 ,-0.37681159]])

def gt_B():
    return np.array([[ 0.,  0.],
 [ 0.14503817, -0.07633588],
 [ 0.22900763, -0.22137405],
 [ 0.21374046, -0.3740458 ],
 [ 0.15267176, -0.50381679],
 [ 0.17557252 ,-0.47328244],
 [ 0.18320611, -0.67938931],
 [ 0.17557252 ,-0.81679389],
 [ 0.16793893 ,-0.93129771],
 [ 0.0610687 , -0.48091603],
 [ 0.07633588, -0.72519084],
 [ 0.06870229 ,-0.87022901],
 [ 0.06870229 ,-1.        ],
 [-0.03816794 ,-0.45801527],
 [-0.03053435, -0.67938931],
 [-0.03053435 ,-0.81679389],
 [-0.02290076, -0.93129771],
 [-0.13740458 ,-0.41221374],
 [-0.12977099, -0.57251908],
 [-0.1221374 , -0.6870229 ],
 [-0.11450382 ,-0.78625954]])

def gt_C():
    return np.array(
        [[ 0.,0.],
        [0.2195122,   0.02439024],
        [0.46341463, -0.12195122],
        [0.59756098,-0.26829268],
        [0.62195122 ,-0.43902439],
        [0.42682927, -0.54878049],
        [0.47560976, -0.79268293],
        [0.5,-0.92682927],
        [ 0.48780488 ,-0.96341463],
        [ 0.2804878 , -0.62195122],
        [ 0.32926829, -0.90243902],
        [0.35365854 ,-1.        ],
        [ 0.34146341 ,-0.95121951],
        [ 0.12195122 ,-0.64634146],
        [ 0.17073171, -0.8902439 ],
        [ 0.19512195, -0.98780488],
        [ 0.19512195 ,-0.93902439],
        [-0.04878049, -0.6097561 ],
        [ 0.   ,      -0.81707317],
        [ 0.03658537 ,-0.93902439],
        [ 0.08536585, -0.95121951]])

def gt_1():
    return np.array([[ 0.,0.],
 [-0.14705882, -0.15294118],
 [-0.17647059, -0.35882353],
 [-0.10588235,-0.52941176],
 [-0.01176471, -0.60588235],
 [-0.11176471, -0.42352941],
 [-0.10588235, -0.71176471],
 [-0.08823529, -0.87058824],
 [-0.06470588 ,-1.        ],
 [ 0.02941176, -0.41764706],
 [ 0.02352941, -0.64705882],
 [-0.01764706, -0.59411765],
 [-0.05294118, -0.51176471],
 [ 0.14705882, -0.38823529],
 [ 0.09411765, -0.52941176],
 [ 0.05882353, -0.47647059],
 [ 0.04117647, -0.41176471],
 [ 0.24117647, -0.34705882],
 [ 0.15882353, -0.44705882],
 [ 0.12352941, -0.39411765],
 [ 0.11764706,-0.34117647]])

def gt_2():
    return np.array([[ 0.,          0.        ],
    [-0.1299435 , -0.18079096],
    [-0.11299435, -0.37853107],
    [ 0.00564972, -0.50282486],
    [ 0.10734463 ,-0.55367232],
    [-0.06779661, -0.45762712],
    [-0.08474576, -0.72316384],
    [-0.09039548 ,-0.87570621],
    [-0.09039548 ,-1.        ],
    [ 0.06779661 ,-0.43502825],
    [ 0.11864407 ,-0.71186441],
    [ 0.14689266 ,-0.8700565 ],
    [ 0.17514124 ,-0.98870056],
    [ 0.18079096 ,-0.38983051],
    [ 0.17514124 ,-0.57062147],
    [ 0.10734463 ,-0.53672316],
    [ 0.07344633, -0.46892655],
    [ 0.26553672, -0.33333333],
    [ 0.20338983 ,-0.46327684],
    [ 0.14124294 ,-0.42937853],
    [ 0.12429379 ,-0.37288136]])

def gt_3():
    return np.array([[ 0.,0.],
 [-0.15428571, -0.16      ],
 [-0.25714286, -0.33714286],
 [-0.37714286 ,-0.48      ],
 [-0.47428571 ,-0.55428571],
 [-0.05714286 ,-0.46285714],
 [-0.06285714 ,-0.72571429],
 [-0.06285714, -0.88      ],
 [-0.05714286, -1.        ],
 [ 0.07428571 ,-0.44571429],
 [ 0.14285714 ,-0.72      ],
 [ 0.17142857 ,-0.88      ],
 [ 0.2       , -0.98857143],
 [ 0.18857143 ,-0.39428571],
 [ 0.2       , -0.59428571],
 [ 0.14285714 ,-0.54285714],
 [ 0.10285714 ,-0.45714286],
 [ 0.28   ,    -0.32571429],
 [ 0.24     ,  -0.44      ],
 [ 0.17714286 ,-0.38857143],
 [ 0.15428571 ,-0.32571429]])