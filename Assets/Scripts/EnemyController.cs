using System;

using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyController : MonoBehaviour
{
    const string MOVERIGHT_TRIGGER = "MoveRight";
    const string MOVELEFT_TRIGGER = "MoveLeft";
    const string MOVEUP_TRIGGER = "MoveUp";
    const string MOVEDOWN_TRIGGER = "MoveDown";

    public Tilemap tilemap;

    public float normalSpeed;

    public float powerUpSpeed;

    public float minimumOneWayTravel = 6;

    bool _isMoving;

    Animator animator;

    public Vector3 direction = Vector3.left;

    public int index;

    float speed;

    CharacterDirection moveDirection = CharacterDirection.Left;

    public Action<EnemyController> OnEnemyTrapped;

    float oneWaydistanceTravelled;

    void Start()
    {
        direction = Vector3.left;

        moveDirection = UnityEngine.Random.Range(0, 100) % 2 == 0 ? CharacterDirection.Left : CharacterDirection.Down;

        if(moveDirection == CharacterDirection.Down)
        {
            direction = Vector3.down;
        }

        animator = GetComponent<Animator>();

        animator.SetTrigger(MOVELEFT_TRIGGER);

        speed = normalSpeed;
    }

    void Update()
    {
        if (tilemap != null)
        {
            if (tilemap.HasTile(new Vector3Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), 0)))
            {
                OnEnemyTrapped?.Invoke(this);
            }
            else
            {
                switch(moveDirection)
                {
                    case CharacterDirection.Left:
                        if (tilemap.HasTile(new Vector3Int(Mathf.FloorToInt(transform.position.x) - 1, Mathf.FloorToInt(transform.position.y), 0)))
                        {
                            if (oneWaydistanceTravelled < minimumOneWayTravel)
                            {
                                OnEnemyTrapped?.Invoke(this);
                            }
                            else
                            {
                                direction = Vector3.right;
                                moveDirection = CharacterDirection.Right;
                                animator.SetTrigger(MOVERIGHT_TRIGGER);
                                oneWaydistanceTravelled = 0;
                            }
                        }
                        break;
                    case CharacterDirection.Right:
                        if (tilemap.HasTile(new Vector3Int(Mathf.FloorToInt(transform.position.x) + 1, Mathf.FloorToInt(transform.position.y), 0)))
                        {
                            if (oneWaydistanceTravelled < minimumOneWayTravel)
                            {
                                OnEnemyTrapped?.Invoke(this);
                            }
                            else
                            {
                                direction = Vector3.left;
                                moveDirection = CharacterDirection.Left;
                                animator.SetTrigger(MOVELEFT_TRIGGER);
                                oneWaydistanceTravelled = 0;
                            }
                        }
                        break;
                    case CharacterDirection.Down:
                        if (tilemap.HasTile(new Vector3Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y) - 1, 0)))
                        {
                            if (oneWaydistanceTravelled < minimumOneWayTravel)
                            {
                                OnEnemyTrapped?.Invoke(this);
                            }
                            else
                            {
                                direction = Vector3.up;
                                moveDirection = CharacterDirection.Up;
                                animator.SetTrigger(MOVEUP_TRIGGER);
                                oneWaydistanceTravelled = 0;
                            }
                        }
                        break;
                    case CharacterDirection.Up:
                        if (tilemap.HasTile(new Vector3Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y) + 1, 0)))
                        {
                            if (oneWaydistanceTravelled < minimumOneWayTravel)
                            {
                                OnEnemyTrapped?.Invoke(this);
                            }
                            else
                            {
                                direction = Vector3.down;
                                moveDirection = CharacterDirection.Down;
                                animator.SetTrigger(MOVEDOWN_TRIGGER);
                                oneWaydistanceTravelled = 0;
                            }
                        }
                        break;
                }                
            }
        }

        if (_isMoving)
        {
            oneWaydistanceTravelled += speed * Time.deltaTime;
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    public void Move()
    {
        _isMoving = true;
    }

    public void Stop()
    {
        _isMoving = false;
    }

    public void EnablePowerUp()
    {
        speed = powerUpSpeed;
    }

    public void DisablePowerUp()
    {
        speed = normalSpeed;
    }
}
