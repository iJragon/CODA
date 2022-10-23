import cv2
import mediapipe as mp
from enum import Enum
import time
import numpy as np
import copy
import itertools
from keyboardEmulator import keyboardEmulate
from annotation_test import gt_A, gt_B, gt_C, gt_L, gt_1, gt_2, gt_3

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
    cap = cv2.VideoCapture(2)
    # cap = cvCreateCameraCapture( -1 )
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

    diff = gt_A() - nplmlivepre
    diff2 = gt_1() - nplmlivepre

    norm = np.linalg.norm(diff, axis=0)
    norm2 = np.linalg.norm(diff2, axis=0)
    total_dist1 = np.sum(norm)
    total_dist2 = np.sum(norm2)

    print("d1", total_dist1)
    print("d2", total_dist2)

    if(total_dist1 > 3 or total_dist2 > 3 or np.isnan(total_dist1) or np.isnan(total_dist2)):
        print("please move into frame!")
    elif(total_dist1 < total_dist2):
        print("A")
        keyboardEmulate('A')
        # time.sleep(0.5)
    else:
        print("1")
        keyboardEmulate('1')
        # time.sleep(0.5)

    # Press Q on keyboard to stop recording
    if cv2.waitKey(1) & 0xFF == ord('q'):
      break

# When everything done, release the video capture and video write objects
if (m != Mode.PICTURE):
    cap.release()
