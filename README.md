# HackGT 9 2022!
## Revised Nov 2022

## VENV
*note: ensure that `$PYTHONPATH` is not set. check by running `echo $PYTHONPATH`, otherwise trackdown which rc file is setting `$PYTHONPATH`*
1. create a local venv named `env`: `python3 -m venv env` it will automatically be gitignored
2. activate `env` in local terminal session: `source env/bin/activate`
3. verify activation: `which python3` should return `path/to/folder/env/bin/python3` indicating the virtual env is activated
4. install the required packages `pip install -r requirements.txt`

## Directory Structure
`Assets/`: Unity game assets

`Packages/`: Unity game packages

`ProjectSettings/`: Unity ProjectSettings

`Scripts/`: Python Hand Tracking Backend Directories
    
` ├── CV/`: Computer Vision Code for Hand Tracking

` │   ├── res/`: Resources

` │    │   ├── annotations/`: annotated ground truth images

` │    │   ├── docs/`: documentation and reference images

` │    │   └── ground-truth/`: raw ground truth images

` │   ├── util/`: Utilities

` │    │   ├── camera.py`: webcam capture setup

` │    │   └── ground_truth.py`: raw ground truth images

` │    │   └── keyboard.py`: keyboard emulator helper

` │   └── keypoint_tracker.py`: main entry point

` └── env/`: Python Virtual Environment

## Commands

From `Scripts/CV`

- `python3 util/ground_truth.py`: generate cropped images each ground truth (only once)
- `python3 keyboard.py`: pyinput keyboard emulation helper function
- `python3 util/camera.py`: health-check for the camera streaming devices
- `ptyhon3 keypoint_tracker.py`: run main program


