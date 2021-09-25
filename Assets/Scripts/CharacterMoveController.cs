using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMoveController : MonoBehaviour
{
    [Header("Movement")]
    public float moveAccel;
    public float maxSpeed;

    [Header("Jump")]
    public float jumpAccel;

    [Header("Ground Raycast")]
    public float groundRaycastDistance;
    public LayerMask groundLayerMask;
    
    [Header("Scoring")]
    public ScoreController score;
    public float scoringRatio;
    private float lastPositionX;

    [Header("GameOver")]
    public GameObject gameOverScreen;
    public float fallPositionY;

    [Header("Camera")]
    public CameraMoveController gameCamera;

    private bool isJumping;
    private bool isOnGround;

    private Rigidbody2D rbd2;
    private Animator anim;
    private CharacterSoundController sound;

    private void Start()
    {
        rbd2 = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sound = GetComponent<CharacterSoundController>();
    }

    private void Update()
    {
        //Input jump
        if(Input.GetMouseButtonDown(0)){
            if(isOnGround){
                isJumping = true;
                sound.PlayJump();
            }
        }

        //Jump Animation
        anim.SetBool("isOnGround", isOnGround);

        //Calculate Score
        int distancePassed = Mathf.FloorToInt(transform.position.x - lastPositionX);
        int scoreIncrement = Mathf.FloorToInt(distancePassed / scoringRatio);
        if(scoreIncrement > 0){
            score.IncreaseCurrentScore(scoreIncrement);
            lastPositionX += distancePassed;
        }

        //Game Over
        if(transform.position.y < fallPositionY){
            GameOver();
        }
    }

    private void FixedUpdate() {
        //Checking if on ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayerMask);
        if(hit){
            if(!isOnGround && rbd2.velocity.y <= 0){
                isOnGround = true;
            }
        } else {
            isOnGround = false;
        }

        //Movement
        Vector2 velocityVector = rbd2.velocity;
        velocityVector.x = Mathf.Clamp(velocityVector.x + moveAccel * Time.deltaTime, 0.0f, maxSpeed);
        
        //Jumping
        if(isJumping){
            velocityVector.y += jumpAccel;
            isJumping = false;
        }

        rbd2.velocity = velocityVector;
    }

    private void GameOver(){
        score.FinishScoring();
        gameCamera.enabled = false;
        gameOverScreen.SetActive(true);
        this.enabled = false;
    }

    private void OnDrawGizmos() {
        Debug.DrawLine(transform.position, transform.position + Vector3.down * groundRaycastDistance, Color.white);
    }
}
