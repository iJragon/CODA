
import cv2
import mediapipe as mp
from enum import Enum
import time

# different analysis modes
class Mode(Enum):
  WEBCAM_INT = 0
  WEBCAM_EX = 1
  RECORDING = 2
  PICTURE = 3

# initializations
m = Mode.PICTURE
cap = None
path = "./realistic.jpg"

# Create a VideoCapture object
if (m==Mode.WEBCAM_INT):
    cap = cv2.VideoCapture(0)
elif (m==Mode.WEBCAM_EX):
    cap = cv2.VideoCapture(2)
elif (m==Mode.RECORDING):
    cap = cv2.VideoCapture(path)
else:
    cap = cv2.imread(path)


# init media pipe
mpHands = mp.solutions.hands
hands = mpHands.Hands()
mpDraw = mp.solutions.drawing_utils
 
pTime = 0
cTime = 0

iteration = 0
 
while True:
    if (m == Mode.PICTURE):
        img = cap
    elif(m == Mode.PICTURE_COMPLETE):
        time.sleep(4)
        continue # loop without re-checking points since it as stabilized
    else:
        success, img = cap.read()

    imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    results = hands.process(imgRGB)
    # print(results.multi_hand_landmarks)
 
    if results.multi_hand_landmarks:
        for handLms in results.multi_hand_landmarks:
            for id, lm in enumerate(handLms.landmark):
                print("lm shape",id, lm) # lm has three dimensions which we can use to determine pose

                h, w, c = img.shape
                cx, cy = int(lm.x * w), int(lm.y * h)
                print(id, cx, cy)
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

    # Press Q on keyboard to stop recording
    if cv2.waitKey(1) & 0xFF == ord('q'):
      break

# When everything done, release the video capture and video write objects
if (m != Mode.PICTURE or m != Mode.PICTURE_COMPLETE):
    cap.release()
