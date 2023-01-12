using System.Collections.Generic;
using System.Linq;
using libsvm;

namespace SVMConsole {
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
}