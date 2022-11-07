using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class LyricGenerator : MonoBehaviour {
    public static LyricGenerator instance; 

    /* The symbol prefab that we are displaying to the screen */
    [SerializeField] private GameObject symbolPrefab;
    /* The symbol object that is instantiated */
    private GameObject symbolInstant;
    private Symbol symbol;
    /* The four possible spawn locations */
    [SerializeField] private Transform[] spawnLocations;
    private int spawnIndex;
    private bool isForwards;
    private const float endpointY = -4.2f;

    /* The error message prefab to display onto the screen */
    [SerializeField] private GameObject errorMessage;

    /* The images of each letter or number in sign language */
    [SerializeField] private Sprite[] alphabet;
    /* A mapping from character to the corresponding image that goes with it */
    private Dictionary<char, Sprite> charToSprite;

    /* A mapping from characters to the order in which they appear on the screen in y-ascending order */
    private Dictionary<char, Queue<GameObject>> letterToOrder;

    /* Timings for when the symbols actually show up on the screen */
    private float currTimer;
    private CSVReader reader;
    private int currSymbolIndex;
    private float nextSymbolArrival;
    private char nextSymbol;

    /* Mapping accuracy text */
    [SerializeField] private TextMeshProUGUI textfield;
    private int totalCorrect;
    private int symbolsTerminated; 

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
        nextSymbolArrival = reader.mySymbolList.symbols[currSymbolIndex].timeStamp - SongManager.instance.currentSong.offset;
        nextSymbol = char.ToLower(reader.mySymbolList.symbols[currSymbolIndex].sign[0]);

        /* Reset accuracy to 100% */
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
                    nextSymbolArrival = reader.mySymbolList.symbols[currSymbolIndex].timeStamp - SongManager.instance.currentSong.offset;
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
        } else {
            errorMessage.SetActive(true);
            errorMessage.GetComponent<Animator>().SetTrigger("SwipeIn");
            AudioManager.instance.Play("ErrorSound");
            DestroyImmediate(popLetter, true);
        }

        textfield.text = ((int)(((float)totalCorrect / symbolsTerminated) * 100)).ToString() + "%";
            
        if (letterToOrder[letter].Count <= 0)
            letterToOrder.Remove(letter);
    }
}