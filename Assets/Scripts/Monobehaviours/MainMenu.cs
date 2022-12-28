using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Button _startNewGameBtn;
    
    [SerializeField]
    private Button _continueGameBtn;

    [SerializeField]
    private Text _score;
    
    void Start()
    {
        var score = PlayerPrefs.HasKey(Constants.SCORE_KEY) ? PlayerPrefs.GetInt(Constants.SCORE_KEY) : 0;
        _score.text = "Score: " + score;
        
        _continueGameBtn.gameObject.SetActive(score > 0);
        
        _startNewGameBtn.onClick.AddListener(OnStartNewGameClicked);
        _continueGameBtn.onClick.AddListener(OnContinueGameClicked);
    }

    private void OnStartNewGameClicked()
    {
        PlayerPrefs.SetInt(Constants.SCORE_KEY, 0);
        SceneManager.LoadScene(Constants.ProceduralLevel);
    }
    
    private void OnContinueGameClicked()
    {
        SceneManager.LoadScene(Constants.ProceduralLevel);
    }
}
