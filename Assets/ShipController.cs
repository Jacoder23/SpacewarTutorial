using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipController : MonoBehaviour
{
    public enum AI
    {
        Aggressive,
        Passive,
        Mixed
    }

    Rigidbody2D rb;
    Vector2 movement;
    public float speed;
    public float rotationSpeed;
    float direction;
    float gravitationalSpeed = 0.25f;
    public bool WASD;
    GameObject bullet;
    GameObject scoreText;
    public bool computerControlled;
    public AI computerStrategy;
    RaycastHit2D hit;
    GameObject humanPlayer;
    bool canFire = true;
    bool touchingWall;
    float dotProduct;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bullet = transform.GetChild(0).gameObject;
        scoreText = GameObject.Find("Canvas/Score");

        if (computerControlled && transform.name == "Player1Ship")
        {
            humanPlayer = GameObject.Find("Player2Ship");
        }
        else if (computerControlled && transform.name == "Player2Ship")
        {
            humanPlayer = GameObject.Find("Player1Ship");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (computerControlled)
        {
            switch (computerStrategy)
            {
                case AI.Aggressive:
                    // AI's goals are to:
                    // Avoid the center of the screen
                    // Go closer to the player
                    // Shoot at the player
                    dotProduct = Vector2.Dot((humanPlayer.transform.position - transform.position).normalized, transform.up.normalized);
                    if (dotProduct > 0.9f)
                    {
                        direction = 0;
                        if ((humanPlayer.transform.position - transform.position).y > 0.25f)
                        {
                            if (canFire)
                            {
                                StartCoroutine(FireAIBullet(0.1f));
                            }
                        }
                    }
                    else
                    {
                        direction = 1;
                    }
                    if (touchingWall)
                    {
                        StartCoroutine(ReverseManuever(1f));
                    }
                    else if (Vector2.Distance(humanPlayer.transform.position, transform.position) > 1f)
                    {
                        movement = new Vector2(0, 1);
                    }
                    break;
                case AI.Passive:
                    // AI's goals are to:
                    // Head towards the player until a limit and then...
                    // Go farther from the player
                    // Shoot at the player
                    dotProduct = Vector2.Dot((humanPlayer.transform.position - transform.position).normalized, transform.up.normalized);
                    if (dotProduct > 0.9f)
                    {
                        direction = 0;
                        if ((humanPlayer.transform.position - transform.position).y > 0.25f)
                        {
                            if (canFire)
                            {
                                StartCoroutine(FireAIBullet(0.2f));
                            }
                        }
                    }
                    else
                    {
                        direction = 1;
                    }
                    if (touchingWall)
                    {
                        StartCoroutine(ReverseManuever(0.5f));
                    }
                    else if (Vector2.Distance(humanPlayer.transform.position, transform.position) < 13f)
                    {
                        movement = new Vector2(0, -1);
                        if (canFire)
                        {
                            StartCoroutine(FireAIBullet(0.2f));
                        }
                    }
                    else
                    {
                        movement = new Vector2(0, 1);
                    }
                    break;
                case AI.Mixed:
                    // AI's goals are to:
                    // Switch between Passive and Aggressive every few seconds
                    break;
            }
        }
        else
        {
            if (WASD)
            {
                movement = new Vector2(0, Input.GetAxisRaw("Vertical(WASD)"));

                if (Input.GetKey(KeyCode.A))
                {
                    direction = 1;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    direction = -1;
                }
                else
                {
                    direction = 0;
                }

                if (Input.GetKeyDown(KeyCode.S))
                {
                    FireBullet();
                }
            }
            else
            {
                movement = new Vector2(0, Input.GetAxisRaw("Vertical(Arrow Keys)"));

                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    direction = 1;
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    direction = -1;
                }
                else
                {
                    direction = 0;
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    FireBullet();
                }
            }
        }
    }

    void FixedUpdate()
    {
        MoveShip(movement);
        rb.AddForce((Vector2.zero - (Vector2)transform.position) * gravitationalSpeed);
    }

    void MoveShip(Vector2 vel)
    {
        rb.AddRelativeForce(vel * speed);
        rb.AddTorque(direction * rotationSpeed);
    }

    void FireBullet()
    {
        GameObject temp = Instantiate(bullet, transform.position, transform.rotation);
        temp.GetComponent<Rigidbody2D>().isKinematic = false;
        temp.GetComponent<BulletController>().enabled = true;
    }

    IEnumerator FireAIBullet(float time)
    {
        canFire = false;
        movement = new Vector2(0, -1);
        GameObject temp = Instantiate(bullet, transform.position, transform.rotation);
        temp.GetComponent<Rigidbody2D>().isKinematic = false;
        temp.GetComponent<BulletController>().enabled = true;
        yield return new WaitForSeconds(time/4);
        movement = new Vector2(0, 0);
        yield return new WaitForSeconds((time/4)*3);
        canFire = true;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.name == "Star" && this.name == "Player1Ship")
        {
            scoreText.GetComponent<ScoreTracker>().player2Score += 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (col.gameObject.name == "Star" && this.name == "Player2Ship")
        {
            scoreText.GetComponent<ScoreTracker>().player1Score += 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (col.gameObject.CompareTag("Wall"))
        {
            touchingWall = true;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.name == "Player2Bullet(Clone)" && this.name == "Player1Ship")
        {
            scoreText.GetComponent<ScoreTracker>().player2Score += 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (col.gameObject.name == "Player1Bullet(Clone)" && this.name == "Player2Ship")
        {
            scoreText.GetComponent<ScoreTracker>().player1Score += 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    IEnumerator ReverseManuever(float time)
    {
        direction = -1;
        yield return new WaitForSeconds(time);
        touchingWall = false;
    }
}

/*if ((humanPlayer.transform.position - transform.position).x > 0.25)
{
    // Human is to the right
    if ((humanPlayer.transform.position - transform.position).y > 0.25)
    {
        // Human is above
    }
    else if ((humanPlayer.transform.position - transform.position).y < -0.25)
    {
        // Human is below
    }
    else
    {
        // Human is perfectly to the right
    }
}
else if ((humanPlayer.transform.position - transform.position).x < -0.25)
{
    // Human is to the left
    if ((humanPlayer.transform.position - transform.position).y > 0.25)
    {
        // Human is above
    }
    else if ((humanPlayer.transform.position - transform.position).y < -0.25)
    {
        // Human is below
    }
    else
    {
        // Human is perfectly to the left
    }
}
else
{
    // Human neither right nor left
    if ((humanPlayer.transform.position - transform.position).y > 0.25)
    {
        // Human is above
    }
    else if ((humanPlayer.transform.position - transform.position).y < -0.25)
    {
        // Human is below
    }
    else
    {
        // Facing human
        FireBullet();
        direction = 0;
    }
}*/