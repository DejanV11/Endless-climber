using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameProgressController : Singleton<GameProgressController>
{
    // --- COROUTINES ---
    private Coroutine coroutineTimer;

    [Header("UI ELEMENTS")]
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI secondsText;
    [SerializeField] private GameObject gameOverScreen;

    // --- GAME PROGRESS VARIABLES ---    
    private int points;
    private int timer;
    private int originalPoints;    
    private int originalTimer;
    private const int k_PointValue = 10;
    public bool IsTimerActive { get; private set; }


    // --- UNITY METHODS ---
    protected override void Awake()
    {
        base.Awake();
        
        points = int.Parse(pointsText.text);
        timer = int.Parse(secondsText.text);         
        originalPoints = points;
        originalTimer = timer;        
    }

    void OnEnable()
    {        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    

    // --- MY METHODS ---   
    // Deactivate GameProgress gameObject whenever a MainMenu scene is loaded.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {        
        if (scene.buildIndex == 0)
        {            
            gameObject.SetActive(false);
        }
    }

    // Start DecreaseTimer coroutine.
    public void StartTimer()
    {
        IsTimerActive = true;
        secondsText.gameObject.SetActive(true);
        coroutineTimer = StartCoroutine(DecreaseTimer());        
    }

    // Decrease timer by 1 point every second.
    private IEnumerator DecreaseTimer()
    {                
        while (timer > 0)
        {
            yield return new WaitForSecondsRealtime(1f);
            timer--;
            secondsText.text = timer.ToString();
        }

        StopTimer();
    }

    // Stop timer.
    private void StopTimer()
    {
        if (coroutineTimer != null)
        {
            StopCoroutine(coroutineTimer);
            coroutineTimer = null;
            secondsText.gameObject.SetActive(false);
            IsTimerActive = false;
        }        
    }

    // Reset GameProgress variables to their original values.
    public void ResetGameProgress()
    {                
        points = originalPoints;
        timer = originalTimer;
        pointsText.text = points.ToString("D6");
        secondsText.text = timer.ToString();
    }

    // Display gameOverScreen when player dies.
    public IEnumerator DisplayGameOverScreen()
    {         
        float pauseDuration = 2f;

        gameOverScreen.SetActive(true);
        yield return StartCoroutine(PauseGameForSeconds(pauseDuration));
        gameOverScreen.SetActive(false);

        CheckAndAddHighScore();
    }

    // Pause game and its audio for a set duration of time.
    private IEnumerator PauseGameForSeconds(float pauseDuration)
    {        
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        AudioListener.pause = true;                
        yield return new WaitForSecondsRealtime(pauseDuration);
        AudioListener.pause = false;
        
        Time.timeScale = originalTimeScale;
    }

    // Update score.
    public void UpdateScore()
    {
        points += k_PointValue;
        pointsText.text = points.ToString("D6");
    }

    // Add score to the highscores list if it's high enough.
    private void CheckAndAddHighScore()
    {
        for (int i = 0; i < MainMenuController.s_HighScores.Length; i++)
        {
            if (points > MainMenuController.s_HighScores[i]) // If it is highscore, add it to the list.
            {                    
                for (int j = MainMenuController.s_HighScores.Length - 1; j > i; j--) // Update information of highscores that are lower than the new highscore.
                {                    
                    MainMenuController.s_HighScores[j] = MainMenuController.s_HighScores[j-1];
                    MainMenuController.s_PlayerNames[j] = MainMenuController.s_PlayerNames[j-1];
                }
                                
                MainMenuController.s_HighScores[i] = points;
                MainMenuController.s_IsHighScoreChanged = true;
                MainMenuController.s_NewHighScoreID = i;
                break;
            }
            else // Otherwise, do nothing.
            {
                MainMenuController.s_IsHighScoreChanged = false; 
            }                           
        }

        SceneManager.LoadScene(0);                                  
    }  
}
