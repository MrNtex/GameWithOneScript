using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class TheScript : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Rigidbody2D rb;

    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float speedToTarget = .3f;

    public int health = 100;

    public int damage = 10;

    //LASER
    private float force = 0;

    private float requiredForce = .7f;

    private float startTimer = 0;

    private float laserDuration = 0.5f;
    [SerializeField]
    private float startWidth = 0.2f;

    [SerializeField]
    private AnimationCurve curve;

    public GameObject target;

    //Waves
    private const float WaveCooldown = 6;
    public GameObject prefab;

    public static int score = 0;
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody2D>();

        StartCoroutine(SpawnWaves());
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        rb.velocity = input * speed;
        if(target != null)
        {
            Vector2 dir = (target.transform.position - transform.position).normalized;

            rb.velocity += dir * speedToTarget;
        }
        

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;
        
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        if(lineRenderer.enabled && lineRenderer != null)
        {
            if(Time.time - startTimer > laserDuration)
            {
                lineRenderer.enabled = false;
            }
            float width = curve.Evaluate((Time.time-startTimer) /laserDuration);
            lineRenderer.startWidth = width* startWidth;
            lineRenderer.endWidth = width*startWidth;
        }
        else if (lineRenderer != null)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                force = 0;
                startTimer = Time.time;
            }
            if (Input.GetButton("Fire1"))
            {
                force = Time.time - startTimer;
            }
            if (Input.GetButtonUp("Fire1"))
            {
                if (force > requiredForce)
                {
                    lineRenderer.enabled = true;
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, transform.position + (Vector3)direction * 300);
                    startTimer = Time.time;
                    RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, direction, 300);
                    foreach(RaycastHit2D h in hit)
                    {
                        if (h.collider != null && h.collider.gameObject != gameObject)
                        {
                            TheScript script = h.collider.GetComponent<TheScript>();
                            script.health -= damage;
                            Debug.Log(script.health);
                            if (script.health <= 0)
                            {
                                score++;
                                Camera.main.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "Score: " + score;
                                script.Die();
                            }
                        }
                    }
                }
            }
        }
    }
    private IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(WaveCooldown);
        Vector2 spawnPosition = Random.insideUnitCircle.normalized * 9;
        if (!(spawnPosition.y > -5 && spawnPosition.y < 5 && spawnPosition.x > -10 && spawnPosition.x < 10))
        {
            StartCoroutine(SpawnWaves());
        }
        else
        {
            GameObject newObj = Instantiate(prefab, spawnPosition, Quaternion.identity);

            TheScript script = newObj.GetComponent<TheScript>();
            script.target = target ?? gameObject;
            float levelOfDifficulty = 0;
            script.damage = Random.Range(5, 20);
            levelOfDifficulty += script.damage;
            script.speed = Random.Range(0, 2);
            levelOfDifficulty += script.speed * 2;
            script.speedToTarget = Random.Range(0.2f, 0.5f);
            levelOfDifficulty += script.speedToTarget * 100;
            script.health = Random.Range(5, 36);
            levelOfDifficulty += script.health * 0.5f;
            script.prefab = prefab;

            newObj.GetComponent<SpriteRenderer>().color = new Color(levelOfDifficulty / 108, 0, 0);

        }


        StartCoroutine(SpawnWaves());
    }
    public void Die()
    {
        Destroy(gameObject);
    }
}
