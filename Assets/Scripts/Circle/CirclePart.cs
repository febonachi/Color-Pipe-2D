using Utils;
using System.Linq;
using UnityEngine;
using System.Collections;

using static UnityEngine.Random;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EdgeCollider2D))]
public class CirclePart : MonoBehaviour {
    #region editor
    public Transform center = default;
    [SerializeField] private float force = .2f;
    [SerializeField] private SpriteRenderer spriteRenderer = default;
    [SerializeField] private Sprite defaultSprite = default;
    [SerializeField] private Sprite gradientSprite = default;
    [SerializeField] private Sprite[] spikeSprites = default;
    [SerializeField] private Sprite[] brokenSprites = default;
    [SerializeField] private SpriteRenderer gradientBackground = default;
    [SerializeField] private DestructiveEntity brokenCirclePart = default;
    [SerializeField] private ParticleSystem hidePs = default;
    #endregion

    public enum CirclePartState { Default, Spike, Reload};

    public Color color { get; private set; }
    [HideInInspector] public CirclePartState state = CirclePartState.Default;

    private Circle circle;
    private Animator animator;
    private EdgeCollider2D ec2d;
    private Coroutine reloadCoroutine = null;

    private int collisionCount = 0;
    private int maxCollisionCount = 1;
    private bool collisionDelay = false;

    #region private
    private void Awake() {
        animator = GetComponent<Animator>();
        ec2d = GetComponent<EdgeCollider2D>();
    }

    private void Start() {
        gradientBackground.sprite = gradientSprite;
    }

    private void show() {
        ec2d.enabled = true;
        spriteRenderer.enabled = true;
        gradientBackground.enabled = true;
    }

    private void hide() {
        ec2d.enabled = false;
        spriteRenderer.enabled = false;
        gradientBackground.enabled = false;
    }

    private void setColor(Color colorToSet) {
        color = colorToSet;

        ParticleSystem.MainModule mainModule = hidePs.main;
        mainModule.startColor = color;

        gradientBackground.color = color;

        spriteRenderer.color = Color.white;
        reloadCoroutine = StartCoroutine(Utility._colorOverTime(spriteRenderer, colorToSet, 1f));
    }

    private IEnumerator _reload(float time) {
        CirclePartState tmpState = state;
        state = CirclePartState.Reload;

        StopCoroutine(reloadCoroutine);

        hide();
        if (time == 0f) hidePs.Play();
        else {
            circle.gameController.audioController.brokenPlatform();
            DestructiveEntity entity = Instantiate(brokenCirclePart, transform.position, Quaternion.Euler(0f, 0f, Range(0f, 360f)));
            entity.brokeDown(spriteRenderer.color);
        }

        yield return new WaitUntil(() => collisionDelay == false);

        if (time != 0f) yield return new WaitForSeconds(Range(time / 2f, time));

        state = tmpState;

        show();

        collisionCount = 0;
        setColor(circle.randomColor());
        if (state == CirclePartState.Spike) {
            spriteRenderer.sprite = spikeSprites[Range(0, spikeSprites.Length)];
        } else {
            state = CirclePartState.Default;
            spriteRenderer.sprite = defaultSprite;
        }
    }

    private IEnumerator _bounce(float force) {
        collisionDelay = true;

        int smooth = 4;
        force /= smooth;
        for (int i = 0; i < smooth; i++) {
            transform.Translate(Vector3.right * force);
            yield return null;
        }
        for (int i = 0; i < smooth; i++) {
            transform.Translate(Vector3.left * force);
            yield return null;
        }

        collisionDelay = false;
    }
    #endregion

    #region public
    public void bounce() {
        if (state != CirclePartState.Spike) StartCoroutine(_bounce(force));
    }

    public void reload(float time) => reloadCoroutine = StartCoroutine(_reload(time));

    public void playerInteraction(bool reflection = false) {
        if(collisionDelay) return;

        bounce();
        circle.gameController.audioController.jump();

        int nextCollisionCount = collisionCount + 1;
        if (nextCollisionCount != maxCollisionCount) animator.SetTrigger("gradient");

        if (!reflection && state == CirclePartState.Default) {
            collisionCount = nextCollisionCount;
            if (collisionCount != maxCollisionCount) spriteRenderer.sprite = brokenSprites.ElementAt(Range(0, brokenSprites.Length));
            else reload(Range(circle.partsReloadTime / 2f, circle.partsReloadTime * 2f));
        }
    }

    public void setupCircle(Circle circle, CirclePartState state = CirclePartState.Default) {
        this.state = state;
        this.circle = circle;

        if (state == CirclePartState.Spike) spriteRenderer.sprite = spikeSprites[Range(0, spikeSprites.Length)];
        else spriteRenderer.sprite = defaultSprite;

        setColor(circle.randomColor());
        transform.SetParent(circle.transform);
        maxCollisionCount = circle.doubleCollisionCount ? 2 : 1;
    }

    public IEnumerator _gameOver() {
        StopCoroutine(reloadCoroutine);

        animator.SetTrigger("hide");
        hidePs.Play();

        yield return StartCoroutine(Utility._rotateOverTime(transform, Range(-5f, 5f), .5f));
    }
    #endregion
}
