from pynput.keyboard import Key, Controller
import argparse

def keyboardEmulate(to_type: str, hold_space=False) -> None:
    keyboard = Controller()

    # sanitize inputs to lowercase to be compatible with game inputs
    to_type = to_type.lower()

    # hold space to toggle string mode
    if (hold_space == True):
        with keyboard.pressed(Key.space):
            for c in to_type:
                keyboard.press(c)
                keyboard.release(c)
    # otherwise type a singular character
    else:
        keyboard.press(to_type[0])
        keyboard.release(to_type[0])

if __name__ == "__main__":
    # test keyboard emulation, as a stand alone file
    parser = argparse.ArgumentParser()
    parser.add_argument('--input', help='input to type', default='A')
    parser.add_argument('--space', help='boolean flag to enable space, default False', default=False, type=bool)
    args = parser.parse_args()

    keyboardEmulate(args.input, hold_space=args.space)