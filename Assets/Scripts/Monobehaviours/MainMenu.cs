using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private Button _startNewGame;
    
    void Start()
    {
        _startNewGame.onClick.AddListener(OnStartNewGameClicked);
    }

    private void OnStartNewGameClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
