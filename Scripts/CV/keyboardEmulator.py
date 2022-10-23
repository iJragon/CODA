from pynput.keyboard import Key, Controller

def keyboardEmulate(charactor):
    keyboard = Controller()
    keyboard.press(charactor)
    keyboard.release(charactor)