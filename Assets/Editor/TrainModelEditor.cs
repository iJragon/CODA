using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using libsvm;
using SVMConsole;

[CustomEditor(typeof(TrainModelScript))]
public class TrainModelEditor : Editor {
    private Dictionary<int, string> _PredictionDictionary = new Dictionary<int, string>() {
  { 0, "A" }, { 1, "B" }, { 2, "C" }, { 3, "D" }, { 4, "E" }, { 5, "H" }, { 6, "L" }, { 7, "1" }, { 8, "2" },
  { 9, "3" }, { 10, "G" }, { 11, "H" }, { 12, "M" }, { 13, "N" }, { 14, "S" }, { 15, "T" }, { 16, "R" },
  { 17, "4" }, { 18, "W" }, { 19, "Y" }, { 20, "You"}, { 21, "Me" }
 };

    private C_SVC model;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Train Model")) {
            Train();
        }
        if (GUILayout.Button("Run Tests")) {
            Test();
        }
        if (GUILayout.Button("Speed Test")) {
            SpeedTest();
        }
    }

    private void Train() {
        // Get the path of the CSV file
        var csvPath = Application.dataPath + @"/Resources/keypoint.csv";
        // Extract each line of the CSV file
        var csvLines = File.ReadAllLines(csvPath);
        // Read each line, separate each comma-separated string into an array
        var csvRows = csvLines.Select(row => row.Split(',')).ToList();

        // Create an array of the first column (mapped to ASL symbols)
        double[] symbols = csvRows.Select(i => double.Parse(i[0])).ToArray();

        // Create a 2D array of all 42 landmark points for each index (y[i]) and skipping the first column
        double[][] landmarks = csvRows.Select(r => r.Skip(1).Select(c => double.Parse(c)).ToArray()).ToArray();

        var problemBuilder = new SymbolClassificationProblemBuilder();
        var problem = problemBuilder.CreateProblem(landmarks, symbols);

        const int C = 1;
        C_SVC model = new C_SVC(problem, KernelHelper.LinearKernel(), C);

        var accuracy = model.GetCrossValidationAccuracy(10);
        Debug.Log("Accuracy of the model is " + (accuracy * 100).ToString("F3") + "%.");
        model.Export(Application.streamingAssetsPath + @"/model.model");
        this.model = model;
        Debug.Log("The model is trained.\n");
    }

    private void Test() {
        if (!File.Exists(Application.streamingAssetsPath + @"/model.model")) {
            Debug.LogError("That model does not exist. Make sure to train first!");
        }
        C_SVC model = new C_SVC(Application.streamingAssetsPath + @"/model.model");
        this.model = model;
        // Get the path of the CSV test file
        var csvTestPath = Application.dataPath + @"/Resources/keypointTest.csv";
        // Extract each line of the CSV test file
        var csvTestLines = File.ReadAllLines(csvTestPath);
        // Read each line, separate each comma-separated string into an array
        var csvTestRows = csvTestLines.Select(row => row.Split(',')).ToList();
        // Create an array of the first column (mapped to ASL symbols)
        double[] testSymbols = csvTestRows.Select(i => double.Parse(i[0])).ToArray();
        // Create a 2D array of all 42 test landmark points for each test symbol and skipping the first column
        double[][] testAllLandmarks = csvTestRows.Select(r => r.Skip(1).Select(c => double.Parse(c)).ToArray()).ToArray();

        for (int i = 0; i < testSymbols.Length; i++) {
            var newLandmarks = SymbolClassificationProblemBuilder.CreateNodes(testAllLandmarks[i]);
            var predictedSymbol = model.Predict(newLandmarks);
            if ((int)predictedSymbol != (int)testSymbols[i]) {
                Debug.Log($"XXXXXXX : The prediction is {_PredictionDictionary[(int)predictedSymbol]}, Expected {_PredictionDictionary[(int)testSymbols[i]]}");
            } else {
                Debug.Log($"The prediction is {_PredictionDictionary[(int)predictedSymbol]}, Expected {_PredictionDictionary[(int)testSymbols[i]]}");
            }
        }
    }

    private void SpeedTest() {
        double[] test = new double[] { 0.356382979, -0.10106383, 0.638297872, -0.425531915, 0.675531915, -0.760638298, 0.558510638, -1, 0.5, -0.776595745, 0.446808511, -0.760638298, 0.356382979, -0.510638298, 0.292553191, -0.319148936, 0.276595745, -0.845744681, 0.228723404, -0.755319149, 0.175531915, -0.462765957, 0.143617021, -0.239361702, 0.069148936, -0.840425532, 0.021276596, -0.75, 0.015957447, -0.473404255, 0.026595745, -0.265957447, -0.127659574, -0.787234043, -0.14893617, -0.744680851, -0.138297872, -0.526595745, -0.117021277, -0.361702128 };

        var newLandmarks = SymbolClassificationProblemBuilder.CreateNodes(test);
        var predictedSymbol = model.Predict(newLandmarks);
        Debug.Log($"The prediction is {_PredictionDictionary[(int)predictedSymbol]}");
    }
}