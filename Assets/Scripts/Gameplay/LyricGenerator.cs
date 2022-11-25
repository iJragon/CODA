using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LyricGenerator : MonoBehaviour {
    public static LyricGenerator instance; 

    [SerializeField] private GameObject symbolPrefab;           // Symbol prefab that we are displaying to the 
    private GameObject symbolInstant;                           // Symbol object that is instantiated
    private Symbol symbol;                                      
    [SerializeField] private Transform[] spawnLocations;        // Possible spawn locations
    private int spawnIndex;                                     // Particular spawn index for some symbol
    private bool isForwards;                                    // Whether we're spawning them in forward or backward direction
    private const float endpointY = -4.2f;                      // Exact level where player can knock out the symbol

    [SerializeField] private GameObject errorMessage;           // Error message prefab to display onto the screen
    [SerializeField] private Sprite[] alphabet;                 // Images of each letter or number in sign language
    private Dictionary<char, Sprite> charToSprite;              // Map character to the corresponding image that goes with it

    private Dictionary<char, Queue<GameObject>> letterToOrder;  // Map characters to order appearing on screen in y-ascending order

    private float currTimer;                                    // Timings for when the symbols actually show up on the screen
    private CSVReader reader;                                   // Read in the data for the current song 
    private int currSymbolIndex;                                // Current index of the symbol into CSV of current song 
    private float nextSymbolArrival;                            // When the next symbol should arrive (timed to hit the endpointY)
    private char nextSymbol;                                    // Next letter/word we have to hit

    [SerializeField] private TextMeshProUGUI textfield;         // Field where score gets written into
    private int totalCorrect;                                   // How many symbols player knocked out on the beat
    private int symbolsTerminated;                              // How many total symbols either got knocked out on beat or despawned

    private void Awake() {
        if (instance == null)
            instance = this;
    }

    private void Start() {
        letterToOrder = new Dictionary<char, Queue<GameObject>>();
        charToSprite = new Dictionary<char, Sprite>();
        reader = gameObject.GetComponent<CSVReader>();

        /* Map the alphanumeric to its corresponding image */
        for (int i = 0; i < alphabet.Length; i++) {
            if (i <= 25)
                charToSprite.Add((char)('a' + i), alphabet[i]);
            else if (i <= 30)
                charToSprite.Add((char)('1' + (i - 26)), alphabet[i]);
            else {
                if (i == 31)
                    charToSprite.Add('6', alphabet[i]);
                else if (i == 32)
                    charToSprite.Add('7', alphabet[i]);
            }
        }

        /* Start with the first symbol */
        currTimer = 0f;
        currSymbolIndex = 0;
        spawnIndex = currSymbolIndex;
        isForwards = true;
        nextSymbolArrival = reader.mySymbolList.symbols[currSymbolIndex].timeStamp - SongManager.instance.songs[SongManager.instance.currentSongIdx].offset;
        nextSymbol = char.ToLower(reader.mySymbolList.symbols[currSymbolIndex].sign[0]);

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
                    nextSymbol = char.ToLower(reader.mySymbolList.symbols[currSymbolIndex].sign[0]);
                }
            }
        }

        /* If player hits a key and that key is on the screen, then remove that symbol off the screen */
        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKeyDown(vKey)) {
                char letter = char.ToLower((char)vKey);
                if (letterToOrder.ContainsKey(letter))
                    RemoveLyric(letter);
            }
        }
    }

    /// <summary>
    /// Generates a new alphanumeric character with the respective character and sprite features and displays it onto screen
    /// </summary>
    /// <param name="letter"></param> character that we are displaying to the screen
    private void GenerateLyric(char letter) {
        /* Spawn the symbol at the top of the screen */ 
        symbolInstant = Instantiate(symbolPrefab, spawnLocations[spawnIndex].position, Quaternion.identity);
        /* Load the symbol with the corresponding character and sprite features */
        symbol = symbolInstant.GetComponent<Symbol>();
        if (symbol == null)
            return;
        symbol.SetLetter(letter);
        symbol.SetSprite(charToSprite[letter]);
        /* Symbol gets added to dictionary for what's currently on screen, according to its letter */
        if (!letterToOrder.ContainsKey(letter)) {
            Queue<GameObject> occurrences = new Queue<GameObject>();
            occurrences.Enqueue(symbolInstant);
            letterToOrder.Add(letter, occurrences);
        } else
            letterToOrder[letter].Enqueue(symbolInstant);
    }

    /// <summary>
    /// Removes the lowest symbol on the screen
    /// </summary>
    /// <param name="letter"></param> character that we are removing from the screen
    public void RemoveLyric(char letter) {
        symbolsTerminated++;
        GameObject popLetter = letterToOrder[letter].Dequeue();
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
            
        if (letterToOrder[letter].Count <= 0)
            letterToOrder.Remove(letter);
    }

    /// <summary>
    /// Called when the song has changed and we need to remove all of the symbols currently on the screen and reset fields
    /// </summary>
    public void ResetStats() {
        Start();

        foreach (char letter in letterToOrder.Keys) {
            while (letterToOrder[letter].Count > 0) {
                GameObject popLetter = letterToOrder[letter].Dequeue();
                DestroyImmediate(popLetter, true);
            }
            letterToOrder.Remove(letter);
        }
    }
}