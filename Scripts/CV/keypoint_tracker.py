import cv2
import mediapipe as mp
import time
import numpy as np
import copy
import itertools
from util.keyboard import keyboardEmulate
from util.camera import Mode
from util.camera import get_capture_device
from single_annotation_test import gt
# initializations
m = Mode.WEBCAM_INT
cap = get_capture_device(m)
path = "./realistic.jpg"

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
        # print(results.multi_hand_lanAAAAAdmarks)
        for handLms in results.multi_hand_landmarks:
            lmlive = calc_landmark_list(img, handLms)
            for id, lm in enumerate(handLms.landmark):
                '''
                    optional: draw circles for all the landmark points
                '''
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

    nplmlive = np.array(lmlive)
    sh = nplmlive.shape
    lmlivepre = pre_process_landmark(lmlive)
    nplmlivepre = np.array(lmlivepre).reshape((sh))

    # closest scalar distance
    closest_dist = float('inf')
    most_probable_k = None
    
    # examine all the gts we have in the dict and check which one is the closest
    for k in gt().keys():
        diff = gt()[k] - nplmlivepre
        curr_dist = np.sum(np.linalg.norm(diff, axis=0))
        if (curr_dist < closest_dist):
            closest_dist = curr_dist
            most_probable_k = k

    output = ""
    if(curr_dist > 7 or np.isnan(curr_dist)):
        print("please move into frame!")
    else:
        output = most_probable_k
        print(str(most_probable_k))
        keyboardEmulate(str(most_probable_k))

    cv2.putText(img, str(output), (10, 70), cv2.FONT_HERSHEY_PLAIN, 3,
                (255, 0, 255), 3)
 
    cv2.imshow("Image", img)

    # Press Q on keyboard to stop recording
    if cv2.waitKey(1) & 0xFF == ord('q'):
      break

# When everything done, release the video capture and video write objects
if (m != Mode.PICTURE):
    cap.release()
