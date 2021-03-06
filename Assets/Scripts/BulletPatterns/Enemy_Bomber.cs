﻿using UnityEngine;
using UnityExtensions.MathfExtensions;
using System.Collections;
using MonsterLove.StateMachine;

public class Enemy_Bomber : BulletPattern {

    private SoundManager sound;

    private enum State { Anticipation, Seeking, PreExploding, Exploding } //Bomber's current state
    private StateMachine<State> state;

    private float speed = 2.5f; //Bomber's speed
    private float anticipationSpeed; //Speed during anticipation state

    private float engagementDistance = 5f; //Distance to be under to trigger explosion
    private float preExplosionTimer = 5f; //Timer before pre-explosion starts by default
    private float explosionTimer = 2f; //Time before bomber explodes
    private float anticipationTimer = 0.5f; //Time before anticipation state ends

    private float preExplosionStartTime; //Time when preExplosion begins

    public GameObject bomberBullets; //Bullets to be spawned

    void Awake()
    {
        sound = Object.FindObjectOfType<SoundManager>();

        state = StateMachine<State>.Initialize(this);
        state.ChangeState(State.Anticipation);
    }

    void Start()
    {
        if (Player)
        {
            float angleToPlayer = MathfExtensions.AngleFromTo(this.gameObject, Player);
            transform.rotation = Quaternion.AngleAxis(angleToPlayer, Vector3.forward);
            transform.Rotate(0, 0, transform.rotation.z + Random.Range(-5, 5));
        }
    }

    void Anticipation_Enter()
    {
        anticipationSpeed = speed;
    }

    void Anticipation_FixedUpdate()
    {
        //Slow down once object enters the game boundaries
        if ((transform.position.x > -9.4f && transform.position.x < 9.4f) && (transform.position.y > -7.3f && transform.position.y < 7.3f))
        {
            if (anticipationSpeed <= 0)
            {
                anticipationSpeed = 0;
                if (anticipationTimer < 0)
                {
                    state.ChangeState(State.Seeking);
                }
                else
                {
                    anticipationTimer -= GameManager.slowMotionMultiplier * Time.deltaTime;
                }
            }
            else
            {
                anticipationSpeed -= 0.05f * GameManager.slowMotionMultiplier;
            }
        }

        //Move forwards
        transform.Translate(Vector2.up * anticipationSpeed * GameManager.slowMotionMultiplier * Time.deltaTime);
    }

    //Bomber is seeking the player
    void Seeking_FixedUpdate()
    {
        //Move towards the player
        transform.Translate(Vector2.up * speed * GameManager.slowMotionMultiplier * Time.deltaTime);

        //Check if it is time to move onto next state
        if(Player)
        {
            float angleToPlayer = MathfExtensions.AngleFromTo(this.gameObject, Player);
            transform.rotation = Quaternion.AngleAxis(angleToPlayer, Vector3.forward);

            //Check if ready to shoot
            if (preExplosionTimer <= 0 || Vector2.Distance(transform.position, Player.transform.position) < engagementDistance)
            {
                state.ChangeState(State.PreExploding);
            }

            preExplosionTimer -= (GameManager.slowMotionMultiplier * Time.deltaTime);
        }
        else
        {
            state.ChangeState(State.PreExploding);
        }
    }

    //Bomber is about to explode
    void PreExploding_Enter()
    {
        //Set pre-explosion start time
        preExplosionStartTime = Time.time;
    }

    void PreExploding_FixedUpdate()
    {
        //Slow down
        if (speed > 0)
            speed -= 2 * GameManager.slowMotionMultiplier * Time.deltaTime;
        else
            speed = 0;

        transform.Translate(Vector2.up * speed * GameManager.slowMotionMultiplier * Time.deltaTime);

        //Change scale
        this.gameObject.GetComponent<Transform>().localScale = (Vector3.one * Mathf.Lerp(1, 1.2f, (Time.time - preExplosionStartTime)));

        //Check if it is time to die
        if(explosionTimer < 0)
        {
            //Die
            state.ChangeState(State.Exploding);
        }
        explosionTimer -= (GameManager.slowMotionMultiplier * Time.deltaTime);
    }

    //Bomber explodes
    void Exploding_Enter()
    {
        //Play sound
        sound.Play("sfx_BomberExplosion", SoundManager.SoundChannel.SFX, 0.3f, false, 0.3f);

        //Screen shake
        Camera.main.GetComponent<MainCameraManager>().ScreenShake(0.4f);

        //Spawn bullets
        GameObject g = Instantiate(bomberBullets, transform.position, Quaternion.identity) as GameObject;

        //Die
        Destroy(g, 10);
        Destroy(this.gameObject);
    }
}
