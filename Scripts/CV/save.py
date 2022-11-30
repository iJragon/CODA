import numpy as np

myArray = np.array([2,4,6,8,10])
np.save('ground_truth.npy',myArray, allow_pickle=True)