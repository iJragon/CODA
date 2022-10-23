from keyboardEmulator import keyboardEmulate
import time

while (True):
    time.sleep(1)
    # move cursor to desired place of charactor output within 3 seconds
    keyboardEmulate('a') # you will see 'x' is outputted