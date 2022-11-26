using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class LyricGenerator : MonoBehaviour {
    public static LyricGenerator instance; 

    [SerializeField] private GameObject symbolPrefab;               // Symbol prefab that we are displaying to the 
    private GameObject symbolInstant;                               // Symbol object that is instantiated
    private Symbol symbol;                                          
    [SerializeField] private Transform[] spawnLocations;            // Possible spawn locations
    private int spawnIndex;                                         // Particular spawn index for some symbol
    private bool isForwards;                                        // Whether we're spawning them in forward or backward direction
    private const float endpointY = -4.2f;                          // Exact level where player can knock out the symbol

    [SerializeField] private GameObject errorMessage;               // Error message prefab to display onto the screen

    private Dictionary<string, Queue<GameObject>> signScreenOrder;    // Map characters to order appearing on screen in y-ascending order

    private float currTimer;                                        // Timings for when the symbols actually show up on the screen
    private CSVReader reader;                                       // Read in the data for the current song 
    private int currSymbolIndex;                                    // Current index of the symbol into CSV of current song 
    private float nextSymbolArrival;                                // When the next symbol should arrive (timed to hit the endpointY)
    private string nextSymbol;                                      // Next letter/word we have to hit

    [SerializeField] private TextMeshProUGUI textfield;             // Field where score gets written into
    private int totalCorrect;                                       // How many symbols player knocked out on the beat
    private int symbolsTerminated;                                  // How many total symbols either got knocked out on beat or despawned

    private string currSign;                                        // Current sign player is doing - number, letter, or word
    private Dictionary<string, Sprite> signToSprites;               // Dictionary of signs (numbers, letters, words) to their respective sprites

    [Serializable]
    public struct signToSprite {                                    // A fake dictionary to show up on the inspector
        public string sign;
        public Sprite sprite;
    };
    public signToSprite[] ssDict;    

    private void Awake() {
        if (instance == null)
            instance = this;

        /* Initialize each <key, value> of the dictionary depending on our fake dictionary on the inspector */
        signToSprites = new Dictionary<string, Sprite>();
        foreach (var s2s in ssDict)
            signToSprites[s2s.sign] = s2s.sprite;
    }

    private void Start() {
        signScreenOrder = new Dictionary<string, Queue<GameObject>>();
        reader = gameObject.GetComponent<CSVReader>();

        /* Start with the first symbol */
        currTimer = 0f;
        currSymbolIndex = 0;
        spawnIndex = currSymbolIndex;
        isForwards = true;
        nextSymbolArrival = reader.mySymbolList.symbols[currSymbolIndex].timeStamp - SongManager.instance.songs[SongManager.instance.currentSongIdx].offset;
        nextSymbol = reader.mySymbolList.symbols[currSymbolIndex].sign.ToLower();
        nextSymbol = nextSymbol.Substring(0, nextSymbol.Length - 1);

        /* Reset accuracy to 100% */
        totalCorrect = 0;
        textfield.text = "0%";
        symbolsTerminated = 0;
    }

    private void Update() {
        /* Once timer (that runs from the beginning of game) passes the next symbol's arrival time, generate the lyric */
        /* After decoding and executing the lyric, proceed to fetching the next lyric */
        if (currSymbolIndex < reader.mySymbolList.symbols.Length) {
            currTimer += Time.deltaTime;
            if (currTimer >= nextSymbolArrival) {
                GenerateLyric(nextSymbol);
                currSymbolIndex++;
                /* If we're going forward, check if we've reached the right end
                 * If we're going backward, check if we've reached the left end
                 * If we haven't reached either end, keep iterating in the current direction
                 */
                if (isForwards) {
                    if (spawnIndex >= spawnLocations.Length - 1) {
                        spawnIndex--;
                        isForwards = false;
                    } else
                        spawnIndex++;
                } else {
                    if (spawnIndex <= 0) {
                        spawnIndex++;
                        isForwards = true;
                    } else
                        spawnIndex--;
                }
                if (currSymbolIndex < reader.mySymbolList.symbols.Length) {
                    nextSymbolArrival = reader.mySymbolList.symbols[currSymbolIndex].timeStamp - SongManager.instance.songs[SongManager.instance.currentSongIdx].offset;
                    nextSymbol = reader.mySymbolList.symbols[currSymbolIndex].sign.ToLower();
                    nextSymbol = nextSymbol.Substring(0, nextSymbol.Length - 1);
                }
            }
        }

        /* Reset if player is about to sign a new word */
        if (Input.GetKeyDown(KeyCode.Space))
            currSign = "";

        /* Keep track of all the keys the player is hitting */
        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKeyDown(vKey) && vKey != KeyCode.Space) {
                // If the player is currently holding spacebar, then append each letter to the currSign, denoting it's a word, not a letter
                // Otherwise, treat the key as a single alphanumeric character
                if (Input.GetKey(KeyCode.Space)) {
                    currSign += vKey;
                } else {
                    currSign = ((char) vKey).ToString().ToLower();
                    if (signScreenOrder.ContainsKey(currSign))
                        RemoveLyric(currSign);
                }
            }
        }

        /* Once player releases space, the word is complete. Now check if the word is one of the ones appearing on the screen */
        if (Input.GetKeyUp(KeyCode.Space)) {
            currSign = currSign.ToLower();
            if (signScreenOrder.ContainsKey(currSign))
                RemoveLyric(currSign);
        }
    }

    /// <summary>
    /// Generates a new alphanumeric character with the respective character and sprite features and displays it onto screen
    /// </summary>
    /// <param name="letter"></param> character that we are displaying to the screen
    private void GenerateLyric(string sign) {
        /* Spawn the symbol at the top of the screen */ 
        symbolInstant = Instantiate(symbolPrefab, spawnLocations[spawnIndex].position, Quaternion.identity);
        /* Load the symbol with the corresponding character and sprite features */
        symbol = symbolInstant.GetComponent<Symbol>();
        if (symbol == null)
            return;
        symbol.SetSign(sign);
        symbol.SetSprite(signToSprites[sign]);
        /* Symbol gets added to dictionary for what's currently on screen, according to its letter */
        if (!signScreenOrder.ContainsKey(sign)) {
            Queue<GameObject> occurrences = new Queue<GameObject>();
            occurrences.Enqueue(symbolInstant);
            signScreenOrder.Add(sign, occurrences);
        } else
            signScreenOrder[sign].Enqueue(symbolInstant);
    }

    /// <summary>
    /// Removes the lowest symbol on the screen
    /// </summary>
    /// <param name="letter"></param> character that we are removing from the screen
    public void RemoveLyric(string sign) {
        symbolsTerminated++;
        GameObject popLetter = signScreenOrder[sign].Dequeue();
        /* Check if bottom edge passes top of taskbar (valid), center passes top of taskbar (invalid) */
        float bottomEdge = popLetter.transform.position.y - (popLetter.GetComponent<Symbol>().GetHeight() / 2);
        float topEdge = popLetter.transform.position.y + (popLetter.GetComponent<Symbol>().GetHeight() / 2);
        if (bottomEdge <= endpointY && topEdge >= endpointY) {
            totalCorrect++;
            popLetter.GetComponent<Animator>().enabled = true;
            popLetter.GetComponent<Animator>().SetTrigger("Correct");
            popLetter.GetComponent<Symbol>().isDestroyed = true;
        } else {
            errorMessage.SetActive(true);
            errorMessage.GetComponent<Animator>().SetTrigger("SwipeIn");
            AudioManager.instance.Play("ErrorSound");
            popLetter.GetComponent<Symbol>().isDestroyed = true;
            DestroyImmediate(popLetter, true);
        }

        textfield.text = ((int)(((float)totalCorrect / symbolsTerminated) * 100)).ToString() + "%";
            
        if (signScreenOrder[sign].Count <= 0)
            signScreenOrder.Remove(sign);
    }

    /// <summary>
    /// Called when the song has changed and we need to remove all of the symbols currently on the screen and reset fields
    /// </summary>
    public void ResetStats() {
        Start();

        foreach (string sign in signScreenOrder.Keys) {
            while (signScreenOrder[sign].Count > 0) {
                GameObject popLetter = signScreenOrder[sign].Dequeue();
                DestroyImmediate(popLetter, true);
            }
            signScreenOrder.Remove(sign);
        }
    }
}