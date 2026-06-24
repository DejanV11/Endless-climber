using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    // --- INSTANCES ---        
    private CameraController cameraController;      

    [Header("UI ELEMENTS")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject highScoresPanel;
    [SerializeField] private GameObject newHighScorePanel;    
    [SerializeField] private GameObject highScoresObjectsParent;
    [SerializeField] private Button highScoresButton;    
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_InputField playerNameInput;
    private TextMeshProUGUI[] ranksText;
    private TextMeshProUGUI[] highScoresText;
    private TextMeshProUGUI[] playerNamesText;
    private GameObject[] highScoresParentObjects;
    private GameObject panel;    
    private Canvas canvas;

    // --- HIGHSCORES INFO ---
    private HighScoreData Data;        
    private string jsonContent;
    private const string k_HighScoresKey = "HighScores";      
    public static int[] s_Rank;
    public static int[] s_HighScores;
    public static string[] s_PlayerNames;
    public static int s_NewHighScoreID;
    public static bool s_IsHighScoreChanged = false;

    
    // --- UNITY METHODS ---
    void Awake()
    {
        canvas = GetComponent<Canvas>();                           

        // Create arrays for HighScores data.
        Data = new HighScoreData();     
        highScoresParentObjects = new GameObject[highScoresObjectsParent.transform.childCount];
        ranksText = new TextMeshProUGUI[highScoresParentObjects.Length];
        highScoresText = new TextMeshProUGUI[highScoresParentObjects.Length];
        playerNamesText = new TextMeshProUGUI[highScoresParentObjects.Length];

        for (int i = 0; i < highScoresParentObjects.Length; i++)
        {
            highScoresParentObjects[i] = highScoresObjectsParent.transform.GetChild(i).gameObject;
            ranksText[i] = highScoresParentObjects[i].transform.Find("Rank").GetComponent<TextMeshProUGUI>();
            highScoresText[i] = highScoresParentObjects[i].transform.Find("Score").GetComponent<TextMeshProUGUI>();
            playerNamesText[i] = highScoresParentObjects[i].transform.Find("Name").GetComponent<TextMeshProUGUI>();            
        }                                        

        if (!PlayerPrefs.HasKey(k_HighScoresKey)) // Create HighScores list if it doesn't exist.
        {
            int points = 100;
            s_Rank = new int[ranksText.Length];
            s_HighScores = new int[highScoresText.Length];
            s_PlayerNames = new string[playerNamesText.Length];

            for (int i = 0; i < s_HighScores.Length; i++)
            {
                s_Rank[i] = i + 1;
                s_HighScores[i] = points;
                s_PlayerNames[i] = "DEV";
                points -= 10;
            }

            SaveHighScores();            
        }                
        else if (s_IsHighScoreChanged) // Otherwise, update HighScores list if there is a new HighScore.
        {
            SaveHighScores();            
            OpenPanel();
        }

        LoadHighScores();                                
        DisplayHighScores();
    }

    void Start()
    {
        cameraController = CameraController.Instance;        

        canvas.worldCamera = cameraController.MainCamera;
    }

    
    // --- MY METHODS ---
    // Start game.
    public void StartGame()
    {                
        SceneManager.LoadScene(1);
    }

    // Open the panel that the user wants to see.
    public void OpenPanel()
    { 
        if (!s_IsHighScoreChanged)
        {
            GameObject selectedButton = EventSystem.current.currentSelectedGameObject;            

            if (selectedButton == highScoresButton.gameObject)
            {
                panel = highScoresPanel;
            }            
        }
        else
        {
            panel = newHighScorePanel;
            s_IsHighScoreChanged = false;            
        }               
        
        mainMenuPanel.SetActive(false);
        panel.SetActive(true);        

        if (panel == newHighScorePanel)
        {
            playerNameInput.Select();
            submitButton.gameObject.SetActive(false);
        }        
    }

    // Close the panel that's currently displayed on the screen.
    public void ClosePanel()
    { 
        if (panel != null)
        {
            panel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }                             
    }

    // Display highscores information.
    private void DisplayHighScores()
    {
        for (int i = 0; i < s_HighScores.Length; i++)
        {
            ranksText[i].text = s_Rank[i].ToString();
            highScoresText[i].text = s_HighScores[i].ToString("D6");                        
            playerNamesText[i].text = s_PlayerNames[i];
        }
    }    

    // Display name that contains only uppercase letters, characterLimit is defined in the inspector.
    public void DisplayPlayerName()
    {         
        if (playerNameInput.text.Length > 0)
        {
            bool isLetter = char.IsLetter(playerNameInput.text[playerNameInput.text.Length - 1]);

            if (!isLetter)
            {            
                playerNameInput.text = playerNameInput.text.Remove(playerNameInput.text.Length - 1);
            }
            else
            {
                playerNameInput.text = playerNameInput.text.ToUpper();
            }            

            if (playerNameInput.text.Length == playerNameInput.characterLimit)
            {
                submitButton.gameObject.SetActive(true);
            }
            else
            {
                submitButton.gameObject.SetActive(false);
            }                  
        }              
    }

    // Save player's name and display highscore leaderboard.
    public void SubmitPlayerName()
    {        
        s_PlayerNames[s_NewHighScoreID] = playerNameInput.text;                
        playerNamesText[s_NewHighScoreID].text = s_PlayerNames[s_NewHighScoreID];                        
        SaveHighScores();        
        ClosePanel();
        panel = highScoresPanel;                
        OpenPanel();        
    }
    
    // Exit game.
    public void ExitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // --- JSON ---
    [System.Serializable]
    public class HighScoreData
    {
        public int[] jsonRank;
        public int[] jsonHighScores;
        public string[] jsonPlayerNames;
    }

    // Save highscores to PlayerPrefs.
    private void SaveHighScores()
    {
        Data.jsonRank = s_Rank;
        Data.jsonHighScores = s_HighScores;
        Data.jsonPlayerNames = s_PlayerNames;
        jsonContent = JsonUtility.ToJson(Data);
        PlayerPrefs.SetString(k_HighScoresKey, jsonContent);
        PlayerPrefs.Save();
    }

    // Load highscores from PlayerPrefs.
    private void LoadHighScores()
    {
        jsonContent = PlayerPrefs.GetString(k_HighScoresKey);
        Data = JsonUtility.FromJson<HighScoreData>(jsonContent);
        s_Rank = Data.jsonRank;
        s_HighScores = Data.jsonHighScores;
        s_PlayerNames = Data.jsonPlayerNames;
    }
}
