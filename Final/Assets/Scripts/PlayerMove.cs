using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public float maxSpeed;
    public float jumpPower;
    public EnemyMove enemy;
    public CapsuleCollider2D capsuleCollider;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;

    public AudioClip audioJump;
    public AudioClip audioSpringJump;
    public AudioClip audioCoin;
    public AudioClip audioDamaged;
    public AudioClip audioKick;

    public int jumpCount;
    bool doubleJump;

    AudioSource audioSource;
    JumpBox jumpbox;

    void Awake()
    {
        
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        this.audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        
        // Jump
        if (Input.GetButton("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
        }

        // Stop Speed
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }

        // Direction Sprite
        if (Input.GetButton("Horizontal"))
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        // Animation
        if (Mathf.Abs(rigid.velocity.x) < 0.3)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);

    }

    void FixedUpdate()
    {
        // MoveControll
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h * 2, ForceMode2D.Impulse);
        // Max Speed
        if (rigid.velocity.x > maxSpeed)
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        if (rigid.velocity.x < maxSpeed * (-1))
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);

        //Landing Platform
        if (rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.5f)
                    anim.SetBool("isJumping", false);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            
            //Attack
            if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y){
                OnAttack(collision.transform);
            }
        else
            OnDamaged(collision.transform.position);
            PlaySound("DAMAGED");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.tag == "Item")
        {

            // Point
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");
            bool isHeal = collision.gameObject.name.Contains("Heal");
            if (isBronze)
            {
                gameManager.stagePoint += 50;
                PlaySound("COIN");
            }
            else if (isSilver)
            {
                gameManager.stagePoint += 100;
                PlaySound("COIN");
            }
            else if (isGold)
            {
                gameManager.stagePoint += 300;
                PlaySound("COIN");
            } 
            else if (isHeal)
            {
                Debug.Log("힐이 되었습니다");
                gameManager.HealthUp();
            }
            // Deactive item
            collision.gameObject.SetActive(false);
            


        }
        else if (collision.gameObject.tag == "Finish")
        {
            // Next Stage
            gameManager.NextStage();
        }
        
        else if (collision.gameObject.name.Contains("jumpbox"))
        {

            JumpBox jumpbox = collision.gameObject.GetComponent<JumpBox>();

            switch (jumpbox.type)
            {
                case "UP":
                    
                    rigid.AddForce(Vector2.up * jumpbox.value, ForceMode2D.Impulse);
                    anim.SetBool("isJumping", true);
                    PlaySound("SpringJump");
                    break;

                case "doubleJump":
                    
                    if (Input.GetButtonDown("Jump") && (!anim.GetBool("isJumping") || (anim.GetBool("isJumping") && doubleJump)))
                    {
                        
                        jumpCount++;
                        doubleJump = true;
                        anim.SetBool("isJumping", true);
                        anim.SetTrigger("Jump");

                        if (jumpCount == 2)
                        {
                            doubleJump = false;
                            jumpCount = 0;
                           
                        }
                    }

                    break;
            }

        

        }
        
    }



    void OnAttack(Transform enemy)
    {
        // Point
        gameManager.stagePoint += 100;
        
        // Reaction Force
        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
        // Enemy Die
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
        PlaySound("KICK");
    }

void OnDamaged(Vector2 targetPos)
     {
         // Health Down
         gameManager.HealthDown();
         gameObject.layer = 11;
         spriteRenderer.color = new Color(1, 1, 1, 0.4f);
         int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
         rigid.AddForce(new Vector2(dirc,1)*7, ForceMode2D.Impulse);
         
         anim.SetTrigger("doDamaged");
         Invoke("OffDamaged",3);
         
     }

     void OffDamaged()
     {
         gameObject.layer = 10;
         spriteRenderer.color = new Color(1, 1, 1, 1);
     }

     public void OnDie()
     {
         // Sprite Alpha
         spriteRenderer.color = new Color(1, 1, 1, 0.4f);
         // Sprite Flip Y
         spriteRenderer.flipY = true;
         // Collider Disable
         capsuleCollider.enabled = false;
         // Die Effect Jump
         rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
     }

     public void VelocityZero()
     {
         rigid.velocity = Vector2.zero;
     }

    void PlaySound(String action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "SpringJump":
                audioSource.clip = audioSpringJump;
                break;
            case "COIN":
                audioSource.clip = audioCoin;
                break;
            case "KICK":
                audioSource.clip = audioKick;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
        }
        audioSource.Play();
    }
}
