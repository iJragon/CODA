HackGT 9 2022!

## VENV
*note: ensure that `$PYTHONPATH` is not set. check by running `echo $PYTHONPATH`, otherwise trackdown which rc file is setting `$PYTHONPATH`*
1. create a local venv named `env`: `python3 -m venv env` it will automatically be gitignored
2. activate `env` in local terminal session: `source env/bin/activate`
3. verify activation: `which python3` should return `path/to/folder/env/bin/python3` indicating the virtual env is activated
4. install the required packages `pip install -r requirements.txt`