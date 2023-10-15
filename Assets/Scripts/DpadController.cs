using UnityEngine;

public class DpadController : MonoBehaviour
{    
    public DpadDirection direction;

    [SerializeField]
    GameObject leftButton;

    [SerializeField]
    GameObject rightButton;

    [SerializeField]
    GameObject upButton;

    [SerializeField]
    GameObject downButton;

    [SerializeField]
    GameObject centerButton;

    void Start()
    {
        direction = DpadDirection.None;
    }

    public void OnLeftButtonPressed()
    {
        direction = DpadDirection.Left;
    }

    public void OnLeftButtonReleased()
    {
        direction = DpadDirection.None;
    }

    public void OnRightButtonPressed()
    {
        direction = DpadDirection.Right;
    }

    public void OnRightButtonReleased()
    {
        direction = DpadDirection.None;
    }

    public void OnUpButtonPressed()
    {
        direction = DpadDirection.Up;
    }

    public void OnUpButtonReleased()
    {
        direction = DpadDirection.None;
    }

    public void OnDownButtonPressed()
    {
        direction = DpadDirection.Down;
    }

    public void OnDownButtonReleased()
    {
        direction = DpadDirection.None;
    }

    public void Show()
    {
        leftButton.SetActive(true);
        rightButton.SetActive(true);
        upButton.SetActive(true);
        downButton.SetActive(true);
        centerButton.SetActive(true);
    }

    public void Hide()
    {
        leftButton.SetActive(false);
        rightButton.SetActive(false);
        upButton.SetActive(false);
        downButton.SetActive(false);
        centerButton.SetActive(false);
    }
}

public enum DpadDirection
{
    Left,
    Right,
    Up,
    Down,
    None
}
