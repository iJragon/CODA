using UnityEngine;

public class CSVReader : MonoBehaviour {
    public TextAsset textAssetData;

    [System.Serializable]
    public class SymbolData {
        public float timeStamp;
        public string sign;
    }

    [System.Serializable]
    public class SymbolDataList {
        public SymbolData[] symbols;
    }

    public SymbolDataList mySymbolList = new SymbolDataList();

    private void Awake() {
        ReadCSV();
    }

    private void ReadCSV() {
        string[] data = textAssetData.text.Split(new string[] { ",", "\n" }, System.StringSplitOptions.None);

        int tableSize = data.Length / 2 - 1;
        mySymbolList.symbols = new SymbolData[tableSize];

        for (int i = 0; i < tableSize; i++) {
            mySymbolList.symbols[i] = new SymbolData();
            mySymbolList.symbols[i].timeStamp = float.Parse(data[2 * (i + 1)]);
            mySymbolList.symbols[i].sign = data[2 * (i + 1) + 1];
        }
    }
}