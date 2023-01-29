using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using libsvm;

[CustomEditor(typeof(TrainModelScript))]
public class TrainModelEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Train Model")) {
            Train();
        }
        if (GUILayout.Button("Run Tests")) {
            Test();
        }
    }

    private void Train() {
        // Get the path of the CSV file
        var csvPath = Application.dataPath + @"/Resources/keypointNew.csv";
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
        Debug.Log("The model is trained.\n");
    }

    private void Test() {
        if (!File.Exists(Application.streamingAssetsPath + @"/model.model")) {
            Debug.LogError("That model does not exist. Make sure to train first!");
        }
        C_SVC model = new C_SVC(Application.streamingAssetsPath + @"/model.model");
        // Get the path of the CSV test file
        var csvTestPath = Application.dataPath + @"/Resources/keypointNew.csv";
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
                Debug.Log($"XXXXXXX : The prediction is {((char)(int)predictedSymbol).ToString()}, Expected {((char)(int)testSymbols[i]).ToString()}");
            } else {
                Debug.Log($"The prediction is {((char)(int)predictedSymbol).ToString()}, Expected {((char)(int)testSymbols[i]).ToString()}");
            }
        }
    }
}