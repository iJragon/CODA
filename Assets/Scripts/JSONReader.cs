using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONReader : MonoBehaviour {
    public TextAsset textJSON;

    [System.Serializable]
    public class SymbolAsset {
        public float time;
        public string symbol;
    }

    [System.Serializable]
    public class SymbolList {
        public SymbolAsset[] symbol; 
    }

    public SymbolList mySymbolList = new SymbolList();

    private void Start() {
        mySymbolList = JsonUtility.FromJson<SymbolList>(textJSON.text);
    }
}