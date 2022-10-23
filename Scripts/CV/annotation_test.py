
import cv2
import mediapipe as mp
import time

path = "./out/ground-truth/B.png"
img = cv2.imread(path)

# init media pipe
mpHands = mp.solutions.hands
hands = mpHands.Hands()
mpDraw = mp.solutions.drawing_utils
 
pTime = 0
cTime = 0

imgRGB = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
results = hands.process(imgRGB)

cap = cv2.VideoCapture(2)

if results.multi_hand_landmarks:
    print(results.multi_hand_landmarks)
    for handLms in results.multi_hand_landmarks:
        print(type(handLms))
        for id, lm in enumerate(handLms.landmark):
            print("lm shape", id, lm) # lm has three dimensions which we can use to determine pose

            h, w, c = img.shape
            cx, cy = int(lm.x * w), int(lm.y * h)
            cv2.circle(img, (cx, cy), 8, (40, 100, 255), cv2.FILLED)

        mpDraw.draw_landmarks(img, handLms, mpHands.HAND_CONNECTIONS)

cv2.imwrite("./out/annotations/B.png", img)