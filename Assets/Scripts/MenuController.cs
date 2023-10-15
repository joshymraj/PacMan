using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    MenuAnimationController menuAnimationController;

    [SerializeField]
    GameObject startButton;

    void Start()
    {
        menuAnimationController.OnCharacterAnimationComplete = HandleCharacterAnimationComplete;
        menuAnimationController.PlayCharacterAnimation();
    }

    void HandleCharacterAnimationComplete()
    {
        startButton.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}
