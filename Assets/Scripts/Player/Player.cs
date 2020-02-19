using Utils;
using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using static UnityEngine.Random;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Player : MonoBehaviour {
    #region editor
    [SerializeField] private SpriteRenderer sprite = default;
    [SerializeField] private LayerMask circlePartsLayer = default;
    [SerializeField] private ParticleSystem diePs = default;
    [SerializeField] private ParticleSystem jumpPs = default;
    [SerializeField] private ParticleSystem comboPs = default;
    [SerializeField] private DestructiveEntity brokenPlayer = default;
    #endregion

    public bool isDead { get; private set; }
    public bool reflecting { get; private set; }

    public enum DieType { Default, Spike};

    public event Action died;
    public event Action reflectionStarted;
    public event Action<int, bool, bool> interactedWithCircle;

    private const float interactionDelay = .1f;

    private Circle circle;
    private Rigidbody2D rb2d;
    private Animator animator;
    private CircleCollider2D cc2d;
    

    private float force = 0f;
    private float radius = 0f;
    private int comboCount = 0;
    private float gravityScale = 1f;
    private float forceGravityScale = 1f;
    private float lastInteractionTime = 0f;

    #region private
    private void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        cc2d = GetComponent<CircleCollider2D>();
    }

    private void Start() {
        reflecting = false;
        rb2d.gravityScale = 0f;
        rb2d.velocity = Vector3.zero;

        radius = cc2d.radius * transform.localScale.x;
    }

    private void Update() {
        if (!isDead && circle.isActivated && !reflecting) {
            bool useForceGravity = rb2d.velocity.y < 0f;
            if (useForceGravity) rb2d.gravityScale = forceGravityScale;
            else rb2d.gravityScale = gravityScale;

            animator.SetFloat("velocity", rb2d.velocity.y);
        } else {
            rb2d.gravityScale = 0f;
            rb2d.velocity = Vector3.zero;
            animator.SetFloat("velocity", 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        bool canInteract = Mathf.Abs(lastInteractionTime - Time.time) > interactionDelay;
        if(canInteract && other.CompareTag("CirclePart")) {
            lastInteractionTime = Time.time;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, circlePartsLayer);
            if(colliders.Length > 0) {
                for(int i = 0; i < colliders.Length; i++) {
                    other = colliders.ElementAt(i);
                    CirclePart circlePart = other.gameObject.GetComponent<CirclePart>();
                    if(i == 0) {
                        circlePart.playerInteraction(reflecting);
                        circlePartInteraction(circlePart);
                    } else circlePart.bounce();
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "GameOverPlatform") die(DieType.Spike);
    }

    private float prepareGravityScale(int f, ref float forceRef) {
        float startForce = 8f;
        float startGravityScale = 1.6f;

        float gravityScaleDelta = 0f;
        forceRef = startForce + (f * 2f);
        for (int i = 1; i <= f; i++) gravityScaleDelta += i * .2f;
        gravityScale = startGravityScale + (gravityScaleDelta + (f * .2f));

        return gravityScale;
    }

    private IEnumerator _reflect(List<Vector3> points) {
        reflecting = true;
        reflectionStarted?.Invoke();

        ParticleSystem.MainModule main = comboPs.main;
        main.startColor = sprite.color;
        comboPs.Play();

        float tmpForce = force;
        force = 30f;
        
        points.Add(circle.transform.position);

        foreach(Vector3 point in points) {
            float distance = Vector3.Distance(transform.position, point);
            while(distance > .1f) {
                float distanceForceDelta = 1f;
                if(distance > 1.5f) distanceForceDelta = 1.5f;
                transform.position = Vector3.MoveTowards(transform.position, point, force * distanceForceDelta * Time.deltaTime);
                distance = Vector3.Distance(transform.position, point);
                yield return null;
            }
            yield return null;
        }

        comboPs.Stop();
        force = tmpForce;
        transform.position = points.Last();

        reflecting = false;
    }

    private void die(DieType type) {
        isDead = true;

        cc2d.enabled = false;
        sprite.gameObject.SetActive(false);

        if (type == DieType.Spike) {
            DestructiveEntity bp = Instantiate(brokenPlayer, transform.position, Quaternion.Euler(0f, 0f, Range(0f, 360f)));
            bp.brokeDown(sprite.color);

            ParticleSystem.MainModule main = diePs.main;
            main.startColor = sprite.color;
            diePs.Play();
        }

        died?.Invoke();
    }
    #endregion

    #region public
    public void circlePartInteraction(CirclePart part) {
        if (reflecting) return;

        if (part.state == CirclePart.CirclePartState.Default || part.state == CirclePart.CirclePartState.Reload) {
            if (sprite.color == part.color) comboCount++;
            else comboCount = 1;

            sprite.color = part.color;
            rb2d.velocity = Vector2.up * force;

            bool combo = comboCount >= circle.playerComboCount;

            if (combo) {
                comboCount = 0;
                StartCoroutine(_reflect(circle.getReflectionPoints()));
            }

            ParticleSystem.MainModule main = jumpPs.main;
            main.startColor = sprite.color;
            jumpPs.Play();
            
            interactedWithCircle?.Invoke(comboCount, combo, part.state == CirclePart.CirclePartState.Reload);
        } else if (part.state == CirclePart.CirclePartState.Spike) die(DieType.Spike);
    }

    public void setCircle(Circle circle) {
        this.circle = circle;

        gravityScale = prepareGravityScale(circle.playerForce, ref force);
        forceGravityScale = gravityScale * 1.5f;

        transform.position = new Vector3(0f, .9f, 0f);
    }

    public IEnumerator _gameOver() {
        yield return new WaitUntil(() => reflecting == false);
        gameObject.SetActive(false);
    }
    #endregion
}
