using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LyricGenerator : MonoBehaviour {
    public static LyricGenerator instance; 
    private bool isClearing;                                        // If we're clearing, don't generate any more symbols

    [SerializeField] private GameObject symbolPrefab;               // Symbol prefab that we are displaying to the 
    private GameObject symbolInstant;                               // Symbol object that is instantiated
    private Symbol symbol;                                          
    [SerializeField] private Transform[] spawnLocations;            // Possible spawn locations
    private int spawnIndex;                                         // Particular spawn index for some symbol
    private bool isForwards;                                        // Whether we're spawning them in forward or backward direction
    private const float endpointY = -4.2f;                          // Exact level where player can knock out the symbol

    [SerializeField] private GameObject errorMessage;               // Error message prefab to display onto the screen
    public Queue<GameObject> errorMessages;                         // Pool of error messages to allow reusability
    private int currSortingOrder;                                   // Ensure each newly-spawning error message is layered on top of previous
    [SerializeField] private GameObject glowEffect;                 // Glow effect that runs every time player is correct

    private Queue<GameObject> signScreenOrder;                      // All symbols appearing on the screen in y-ascending order
    private Dictionary<string, int> signToMissedCount;              // Map signs to the number of times player missed

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

    public struct signToAccuracy {                                  // Key-Value pair for sign to number of times they were missed
        public string sign;
        public int amtMissed;
    };

    private signToAccuracy[] topWorstSignsMissed;                   // Array of top worst signs (missed the most times)


    private void Awake() {
        if (instance == null)
            instance = this;

        /* Initialize each <key, value> of the dictionary depending on our fake dictionary on the inspector */
        signToSprites = new Dictionary<string, Sprite>();
        foreach (var s2s in ssDict)
            signToSprites[s2s.sign] = s2s.sprite;

        signScreenOrder = new Queue<GameObject>();
        signToMissedCount = new Dictionary<string, int>();
        reader = gameObject.GetComponent<CSVReader>();
        errorMessages = new Queue<GameObject>();
        topWorstSignsMissed = new signToAccuracy[3] {
            new signToAccuracy() {sign="", amtMissed=0},
            new signToAccuracy() {sign="", amtMissed=0},
            new signToAccuracy() {sign="", amtMissed=0}
        };
    }

    private void Start() {

    }

    private void OnEnable() {
        isClearing = false;
        glowEffect.SetActive(true);
        /* Start with the first symbol */
        currTimer = 0f;
        currSymbolIndex = 0;
        spawnIndex = currSymbolIndex;
        isForwards = true;
        nextSymbolArrival = reader.mySymbolList.symbols[currSymbolIndex].timeStamp - SongManager.instance.songs[SongManager.instance.currentSongIdx].offset;
        nextSymbol = reader.mySymbolList.symbols[currSymbolIndex].sign.ToLower().Replace("\r", "");

        /* Reset accuracy to 100% */
        totalCorrect = 0;
        textfield.text = "0%";
        symbolsTerminated = 0;
        currSortingOrder = 0;
    }

    private void Update() {
        /* Don't do anything if we're in the middle of clearing */
        if (isClearing)
            return;

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
                    nextSymbol = reader.mySymbolList.symbols[currSymbolIndex].sign.ToLower().Replace("\r", "");
                }
            }
        }

        if (signScreenOrder.Count > 0) {
            GameObject firstSymbol = signScreenOrder.Peek();
            /* Check if bottom edge passes top of taskbar (valid), center passes top of taskbar (invalid) */
            float bottomEdge = firstSymbol.transform.position.y - (firstSymbol.GetComponent<Symbol>().GetHeight() / 2);
            float topEdge = firstSymbol.transform.position.y + (firstSymbol.GetComponent<Symbol>().GetHeight() / 2);
            if (bottomEdge <= endpointY && topEdge >= endpointY) {
                /* Using Google MediaPipe Plugin */
                RemoveLyric(HandDetection.symbol.ToLower());

                ///* Reset if player is about to sign a new word */
                //if (Input.GetKeyDown(KeyCode.Space))
                //    currSign = "";

                ///* Keep track of all the keys the player is hitting */
                //foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
                //    if (Input.GetKeyDown(vKey) && vKey != KeyCode.Space) {
                //        // If the player is currently holding spacebar, then append each letter to the currSign, denoting it's a word, not a letter
                //        // Otherwise, treat the key as a single alphanumeric character
                //        if (Input.GetKey(KeyCode.Space)) {
                //            currSign += vKey;
                //        } else {
                //            currSign = ((char)vKey).ToString().ToLower();
                //            RemoveLyric(currSign);
                //        }
                //    }
                //}

                ///* Once player releases space, the word is complete. Now check if the word is one of the ones appearing on the screen */
                //if (Input.GetKeyUp(KeyCode.Space)) {
                //    currSign = currSign.ToLower();
                //    RemoveLyric(currSign);
                //}
            }
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
        signScreenOrder.Enqueue(symbolInstant);
    }
    
    /// <summary>
    /// Removes the lowest symbol on the screen
    /// </summary>
    /// <param name="letter"></param> character that we are removing from the screen
    public void RemoveLyric(string sign) {
        /* Get the lowest symbol on the screen
         * If the corresponding sign on the symbol matches with what the player signed, then give score
         * If the player doesn't sign anything or is incorrect and the symbol despawns, then error
         */
        GameObject popLetter = signScreenOrder.Peek();
        string currSign = popLetter.GetComponent<Symbol>().GetSign();
        if (currSign.CompareTo(sign) == 0) {
            totalCorrect++;
            glowEffect.GetComponent<Animator>().SetTrigger("Glow");
            popLetter.GetComponent<Animator>().enabled = true;
            popLetter.GetComponent<Animator>().SetTrigger("Correct");
            popLetter.GetComponent<Symbol>().isDestroyed = true;
            signScreenOrder.Dequeue();
            symbolsTerminated++;
        }
        if (sign.Length == 0){
            // If there are error messages ready in our messages pool (idle), then reuse them
            // Otherwise, if all messages are currently busy and there's nothing reusable in our pool, then create a new one
            if (errorMessages.Count > 0) {
                GameObject currentMsg = errorMessages.Dequeue();
                currentMsg.GetComponent<Animator>().SetTrigger("SwipeIn");
                currentMsg.GetComponent<SpriteRenderer>().sortingOrder = ++currSortingOrder;
            } else
                Instantiate(errorMessage).GetComponent<SpriteRenderer>().sortingOrder = ++currSortingOrder;

            // Updating the map of the sign the player missed to the number of times it's already been missed
            if (!signToMissedCount.ContainsKey(currSign))
                signToMissedCount.Add(currSign, 1);
            else
                signToMissedCount[currSign]++;
            // Update the top 3 worst signs if needed (sign, amount of missed times)
            for (int i = 0; i < 3; i++) {
                // If the sign already exists on the top 3, then just update the amount of missed times
                if (currSign.CompareTo(topWorstSignsMissed[i].sign) == 0) {
                    topWorstSignsMissed[i].amtMissed++;
                    break;
                }
                // If the current sign's number exceeds one of the top 3, then shift everything down, then update
                if (signToMissedCount[currSign] > topWorstSignsMissed[i].amtMissed) {
                    for (int j = 2; j > i; j--) {
                        topWorstSignsMissed[j] = topWorstSignsMissed[j - 1];
                    }
                    topWorstSignsMissed[i].sign = currSign;
                    topWorstSignsMissed[i].amtMissed = signToMissedCount[currSign];
                    break;
                }
            }

            AudioManager.instance.Play("ErrorSound");
            popLetter.GetComponent<Symbol>().isDestroyed = true;
            signScreenOrder.Dequeue();
            DestroyImmediate(popLetter, true);
            symbolsTerminated++;
        }

        textfield.text = ((int)(((float)totalCorrect / symbolsTerminated) * 100)).ToString() + "%";
    }

    public signToAccuracy[] GetWorstSigns() {
        return topWorstSignsMissed;
    }

    /// <summary>
    /// Called when the song has changed and we need to remove all of the symbols currently on the screen and reset fields
    /// </summary>
    public void ClearScreen() {
        isClearing = true;
        while (signScreenOrder.Count > 0)
            DestroyImmediate(signScreenOrder.Dequeue(), true);
        glowEffect.SetActive(false);
    }
}