using System;

using UnityEngine;

public class MenuAnimationController : MonoBehaviour
{
    const string INKY_MOVERIGHT_TRIGGER = "MoveRight";
    const string INKY_MOVELEFT_TRIGGER = "MoveLeft";

    [SerializeField]
    GameObject pacMan;

    [SerializeField]
    GameObject inky;

    [SerializeField]
    Transform rightRefPoint;

    [SerializeField]
    Transform leftRefPoint;

    [SerializeField]
    float pacManSpeedOnInkyChasing;

    [SerializeField]
    float pacManSpeedOnChasingInky;

    [SerializeField]
    float inkySpeedOnPacmanChasing;

    [SerializeField]
    float inkySpeedOnChasingPacman;

    public Action OnCharacterAnimationComplete;

    Vector2 characterDirection;

    bool _isAnimating;

    Animator _inkyAnimator;

    void Start()
    {
        characterDirection = Vector2.right;
        _inkyAnimator = inky.GetComponent<Animator>();
    }

    void Update()
    {
        if(!_isAnimating)
        {
            return;
        }

        if(characterDirection == Vector2.right)
        {
            if (Vector3.Distance(pacMan.transform.position, rightRefPoint.position) < 0.05f)
            {
                characterDirection = Vector2.left;
                pacMan.transform.localScale = -Vector3.one * 5;
            }
            else
            {
                pacMan.transform.position = Vector3.MoveTowards(pacMan.transform.position, rightRefPoint.position, pacManSpeedOnInkyChasing * Time.deltaTime);

                _inkyAnimator.SetTrigger(INKY_MOVERIGHT_TRIGGER);
                inky.transform.position = Vector3.MoveTowards(inky.transform.position, rightRefPoint.position, inkySpeedOnChasingPacman * Time.deltaTime);
            }
        }
        else
        {
            if (Vector3.Distance(pacMan.transform.position, leftRefPoint.position) < 0.05f)
            {
                _isAnimating = false;
                OnCharacterAnimationComplete?.Invoke();
            }
            else
            {
                pacMan.transform.position = Vector3.MoveTowards(pacMan.transform.position, leftRefPoint.position, pacManSpeedOnChasingInky * Time.deltaTime);

                _inkyAnimator.SetTrigger(INKY_MOVELEFT_TRIGGER);
                inky.transform.position =Vector3.MoveTowards(inky.transform.position, leftRefPoint.position, inkySpeedOnPacmanChasing * Time.deltaTime);
            }
        }
    }

    public void PlayCharacterAnimation()
    {
        pacMan.SetActive(true);
        inky.SetActive(true);

        pacMan.transform.position = leftRefPoint.position;
        inky.transform.position = leftRefPoint.position;
        pacMan.transform.localScale = Vector3.one;        
        _isAnimating = true;
    }
}
