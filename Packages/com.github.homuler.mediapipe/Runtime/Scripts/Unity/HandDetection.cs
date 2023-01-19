using System.Collections.Generic;
using UnityEngine;
using libsvm;
using System.Linq;
using Mediapipe;
using System;

public class HandDetection : MonoBehaviour {
    public Dictionary<string, int> predictionMapToNums;

    public static string symbol;
    public static List<double> landmarks;

    private C_SVC model;

    [SerializeField] private List<string> signs;

    private void Start() {
        if (model == null) {
            try {
                model = new C_SVC(Application.streamingAssetsPath + @"/model.model");
            } catch {
                Debug.LogError("Model doesn't exist. Make sure to train first!");
            }
        }

        predictionMapToNums = signs.Select((value, index) =>
        new { value, index }).ToDictionary(pair => pair.value.ToLower(), pair => pair.index);
    }

    public void DetectSymbol(IList<NormalizedLandmarkList> _currentHandLandmarkLists) {
        // If there are hands on the screen
        if (_currentHandLandmarkLists != null) {
            // For each hand on the screen
            foreach (var hand in _currentHandLandmarkLists) {
                // List of 21 landmarks for current hand
                var handLandmarks = hand.Landmark.ToList();

                var landmarkList = CalcLandmarkList(handLandmarks);

                var preProcessedLandmarkList = PreprocessLandmark(landmarkList);
                landmarks = preProcessedLandmarkList;
                //Debug.Log(string.Join(",", from f in preProcessedLandmarkList select f));

                var newLandmarks = SymbolClassificationProblemBuilder.CreateNodes(preProcessedLandmarkList.ToArray());
                var predictedSymbol = model.Predict(newLandmarks);
                symbol = signs[(int)predictedSymbol];

                #region Mathematical NumPy method
                ////Closest scalar distance
                //double closestDist = double.MaxValue;
                //string mostProbableK = null;

                ////Examine all the gts we have in the dict and check which one is the closest
                //var groundTruths = GetGroundTruths();
                //var sh = np.array(landmarkList).shape;
                //var npPreprocessedLandmarkList = np.array(preProcessedLandmarkList).reshape(sh);
                //var currDist = double.MaxValue;

                //foreach (var k in groundTruths.Keys) {
                //  var diff = np.array(groundTruths[k]) - npPreprocessedLandmarkList;
                //  //var currDist = np.sum(np.linalg.norm(diff, axis=0));
                //  //if (currDist < closestDist) {
                //  //  closestDist = currDist;
                //  //  mostProbableK = k;
                //  //}
                //}

                //Debug.Log(mostProbableK);
                #endregion
            }
        }
    }

    private List<ValueTuple<int, int>> CalcLandmarkList(List<NormalizedLandmark> landmarks) {
        //var imageWidth = 640; var imageHeight = 480;
        var imageWidth = Screen.width; var imageHeight = Screen.height;

        var landmarkPoints = new List<ValueTuple<int, int>>();
        foreach (var landmark in landmarks) {
            landmarkPoints.Add(new ValueTuple<int, int>(
              Mathf.Min((int)(landmark.X * imageWidth), imageWidth - 1),
              Mathf.Min((int)(landmark.Y * imageHeight), imageHeight - 1))
              );
        }
        return landmarkPoints;
    }

    private List<double> PreprocessLandmark(List<ValueTuple<int, int>> landmarks) {
        // Make a deep copy of the calculated landmarks
        var preprocessedList = new List<ValueTuple<int, int>>(landmarks);

        // Iterate over each landmark (21) and their corresponding index in the list
        foreach (var element in preprocessedList.ToList().Select((value, index) => new { value, index })) {
            preprocessedList[element.index] = new ValueTuple<int, int>(
              preprocessedList[element.index].Item1 - landmarks[0].Item1,
              preprocessedList[element.index].Item2 - landmarks[0].Item2
              );
        }

        // Convert preprocessed list to 1D
        var oneDimensionalLandmarks = new List<double>();
        var maxValue = double.MinValue;
        foreach (var landmark in preprocessedList) {
            oneDimensionalLandmarks.Add(landmark.Item1);
            maxValue = Math.Max(maxValue, Mathf.Abs(landmark.Item1));
            oneDimensionalLandmarks.Add(landmark.Item2);
            maxValue = Math.Max(maxValue, Mathf.Abs(landmark.Item2));
        }

        // Normalization
        for (var i = 0; i < oneDimensionalLandmarks.Count; i++) {
            oneDimensionalLandmarks[i] /= maxValue;
        }

        return oneDimensionalLandmarks;
    }
}

public class SymbolClassificationProblemBuilder {
    public svm_problem CreateProblem(double[][] landmarks, double[] symbol) {
        return new svm_problem {
            y = symbol,
            x = landmarks.Select(symbolLandmarks => CreateNodes(symbolLandmarks)).ToArray(),
            l = symbol.Length
        };
    }

    public static svm_node[] CreateNodes(double[] landmarks) {
        var nodes = new List<svm_node>(landmarks.Length);

        for (int i = 0; i < landmarks.Length; i++) {
            nodes.Add(new svm_node {
                index = i + 1,
                value = landmarks[i]
            }); ;
        }

        return nodes.ToArray();
    }
}
