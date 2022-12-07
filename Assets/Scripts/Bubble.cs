using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bubble : MonoBehaviour
{
    public float Speed = 2;
    public GameObject PrefabExplosion;

    public bool Attached;
    
    bool _isMoving;
    Vector3 _direction;
    public BubbleColor Color;

    float _randomTimerAnimation;


    private void Awake()
    {
        _randomTimerAnimation = Random.Range(0, 15.0f);

    }


    public void Move(Vector2 direction)
    {
        GetComponent<Collider2D>().enabled = true;
        _direction = direction;
        _isMoving = true;
    }

    // Update is called once per frame
    void Update()
    {
        _randomTimerAnimation -= Time.deltaTime;

        if (_randomTimerAnimation <=0 )
        {
            GetComponent<UnityCommon.Graphics.SpriteAnimationSimple>().Play();
            _randomTimerAnimation = Random.Range(0, 15.0f);
        }

        if (_isMoving == false)
            return;

        transform.position += Speed * Time.deltaTime * _direction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isMoving == false)
            return;

        BounceComponent bounce = collision.GetComponent<BounceComponent>();
        if (bounce != null)
        {
            _direction.x = -_direction.x;
        }
        else
        {
            _isMoving = false;
            MainGame.Instance.FixBubble(this);
        }
    }

    public void DestroyBubble()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        GetComponent<Rigidbody2D>().AddForce(Random.insideUnitCircle , ForceMode2D.Impulse);
        GetComponent<SpriteRenderer>().DOFade(0, 0.8f);
        GameObject.Destroy(gameObject,0.8f);
        GameObject.Instantiate(PrefabExplosion, transform.position, Quaternion.identity);
    }

}
