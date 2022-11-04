using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private const string SCORE_KEY = "Score";
    
    [SerializeField]
    private Button _startNewGame;

    [SerializeField]
    private Text _score;
    
    void Start()
    {
        int score = PlayerPrefs.HasKey(SCORE_KEY) ? PlayerPrefs.GetInt(SCORE_KEY) : 0;
        _score.text = "Score: " + score;
        _startNewGame.onClick.AddListener(OnStartNewGameClicked);
    }

    private void OnStartNewGameClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
