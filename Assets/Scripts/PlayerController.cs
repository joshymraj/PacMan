using System;

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const string ENEMY_TAG = "Enemy";
    const string POWERUP_TAG = "PowerUp";

    public Action<PowerUpController> OnPowerUpCollected;
    public Action<EnemyController> OnHitEnemy;

    public bool HasPowerUp;

    [SerializeField]
    DpadDirection direction = DpadDirection.None;

    [SerializeField]
    float speed = 2;
   
    [SerializeField]
    Transform pacmanMoveRefPos;

    bool _isMoving;
    Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(_isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, pacmanMoveRefPos.position, speed * Time.deltaTime);
        }
    }

    public void Spawn(Vector2 position)
    {
        transform.position = position;
        pacmanMoveRefPos.position = transform.position;
    }

    public void Look(DpadDirection moveDirection)
    {
        switch (moveDirection)
        {
            case DpadDirection.Left:
                direction = DpadDirection.Left;
                transform.localScale = -Vector3.one;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;

            case DpadDirection.Right:
                direction = DpadDirection.Right;
                transform.localScale = Vector3.one;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;

            case DpadDirection.Up:
                direction = DpadDirection.Up;
                transform.localScale = Vector3.one;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                transform.Rotate(0, 0, 90);
                break;

            case DpadDirection.Down:
                direction = DpadDirection.Down;
                transform.localScale = Vector3.one;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                transform.Rotate(0, 0, 270);
                break;
        }        
    }

    public void Move(DpadDirection moveDirection)
    {
        Vector3 directionVector = Vector3.zero;

        switch(moveDirection)
        {
            case DpadDirection.Left:
                if (direction != DpadDirection.Left)
                {
                    direction = DpadDirection.Left;
                    transform.localScale = Vector3.one * -1;
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                directionVector = Vector3.left;
                break;

            case DpadDirection.Right:
                if (direction != DpadDirection.Right)
                {
                    direction = DpadDirection.Right;
                    transform.localScale = Vector3.one;
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                directionVector = Vector3.right;
                break;

            case DpadDirection.Up:
                if (direction != DpadDirection.Up)
                {
                    transform.localScale = Vector3.one;
                    direction = DpadDirection.Up;
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    transform.Rotate(0, 0, 90);
                }
                directionVector = Vector3.up;
                break;

            case DpadDirection.Down:
                if (direction != DpadDirection.Down)
                {
                    transform.localScale = Vector3.one;
                    direction = DpadDirection.Down;
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    transform.Rotate(0, 0, 270);
                }
                directionVector = Vector3.down;
                break;
        }

        if (Vector3.Distance(transform.position, pacmanMoveRefPos.position) <= 0.05f)
        {
            pacmanMoveRefPos.position += directionVector;
            _isMoving = true;
        }
    }

    public bool IsTurningBack(DpadDirection dPadDirection)
    {
        switch(dPadDirection)
        {
            case DpadDirection.Left:
                return direction == DpadDirection.Right;

            case DpadDirection.Right:
                return direction == DpadDirection.Left;

            case DpadDirection.Up:
                return direction == DpadDirection.Down;

            case DpadDirection.Down:
                return direction == DpadDirection.Up;
        }

        return false;
    }

    public void StopPlayer()
    {
        _isMoving = false;
        _animator.speed = 0;
    }

    public void StartPlayer()
    {
        _isMoving = true;
        _animator.speed = 1;
    }

    public void Init()
    {
        Look(DpadDirection.Right);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(string.Compare(collision.gameObject.tag, ENEMY_TAG) == 0)
        {
            OnHitEnemy?.Invoke(collision.gameObject.GetComponent<EnemyController>());
        }
        else if (string.Compare(collision.gameObject.tag, POWERUP_TAG) == 0)
        {
            OnPowerUpCollected?.Invoke(collision.gameObject.GetComponent<PowerUpController>());
        }
    }
}
