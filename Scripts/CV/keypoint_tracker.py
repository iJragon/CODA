import cv2
import mediapipe as mp
from enum import Enum
import time
import numpy as np
import copy
import itertools
from annotation_test import gt_A, gt_B, gt_C, gt_1, gt_2, gt_3

# different analysis modes
class Mode(Enum):
  WEBCAM_INT = 0
  WEBCAM_EX = 1
  RECORDING = 2
  PICTURE = 3

# initializations
m = Mode.WEBCAM_EX
cap = None
path = "./realistic.jpg"

# Create a VideoCapture object
if (m==Mode.WEBCAM_INT):
    cap = cv2.VideoCapture(0)
elif (m==Mode.WEBCAM_EX):
    cap = cv2.VideoCapture(-1)
    if (cap.isOpened() == False):
        print("external camera failed, defaulting to built-in")
        cap = cv2.VideoCapture(0)
elif (m==Mode.RECORDING):
    cap = cv2.VideoCapture(path)
else:
    cap = cv2.imread(path)

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

L_test = np.array([[ 0.,          0.        ],
[ 0.16814159, -0.07079646],
[ 0.32743363, -0.25663717],
[ 0.46902655, -0.38938053],
[ 0.59292035, -0.45132743],
[ 0.12389381, -0.50442478],
[ 0.13274336, -0.73451327],
[ 0.11504425, -0.87610619],
[ 0.09734513, -1.        ],
[-0.00884956,-0.48672566],
[ 0.03539823, -0.57522124],
[ 0.07079646, -0.38053097],
[ 0.07079646, -0.27433628],
[-0.13274336, -0.43362832],
[-0.05309735, -0.47787611],
[ 0.,         -0.30088496],
[ 0.,         -0.21238938],
[-0.23893805, -0.36283186],
[-0.15044248, -0.38938053],
[-0.07964602, -0.26548673],
[-0.07079646, -0.20353982]
])


# init media pipe
mpHands = mp.solutions.hands
hands = mpHands.Hands()
mpDraw = mp.solutions.drawing_utils
 
pTime = 0
cTime = 0

iteration = 0
lmlive = np.zeros((21,2))
 
while True:
    if (m == Mode.PICTURE):
        img = cap
    else:
        success, img = cap.read()

    imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    results = hands.process(imgRGB)
    # print(results.multi_hand_landmarks)
 
    if results.multi_hand_landmarks:
        # print(results.multi_hand_landmarks)
        for handLms in results.multi_hand_landmarks:
            lmlive = calc_landmark_list(img, handLms)
            # print(type(lmlive))
            for id, lm in enumerate(handLms.landmark):
                # print("lm shape",id, lm) # lm has three dimensions which we can use to determine pose

                h, w, c = img.shape
                cx, cy = int(lm.x * w), int(lm.y * h)
                # print(id, cx, cy)
                # if id == 4:
                cv2.circle(img, (cx, cy), 8, (40, 100, 255), cv2.FILLED)
 
            mpDraw.draw_landmarks(img, handLms, mpHands.HAND_CONNECTIONS)
 
    cTime = time.time()
    fps = 1 / (cTime - pTime)
    pTime = cTime
 
    cv2.putText(img, str(int(fps)), (10, 70), cv2.FONT_HERSHEY_PLAIN, 3,
                (255, 0, 255), 3)
 
    cv2.imshow("Image", img)
    if iteration == 0:
        cv2.imwrite("./out/annotations/single_test.png", img)
        iteration += 1

    nplmlive = np.array(lmlive)
    sh = nplmlive.shape
    lmlivepre = pre_process_landmark(lmlive)
    nplmlivepre = np.array(lmlivepre).reshape((sh))

    # print(nplmlivepre)

    # diff = L_test - nplmlivepre
    # diff = gt_C() - nplmlivepre
    diff = gt_3() - nplmlivepre
    diff2 = gt_B() - nplmlivepre

    norm = np.linalg.norm(diff, axis=0)
    norm2 = np.linalg.norm(diff2, axis=0)
    total_dist1 = np.sum(norm)
    total_dist2 = np.sum(norm2)

    if(total_dist1 < total_dist2):
        print("3")
    else:
        print("B")

    # print(norm.shape)
    # print(norm)
    # print("total_dist",total_dist)


    # Press Q on keyboard to stop recording
    if cv2.waitKey(1) & 0xFF == ord('q'):
      break

# When everything done, release the video capture and video write objects
if (m != Mode.PICTURE):
    cap.release()
