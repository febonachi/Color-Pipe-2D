using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestructiveEntity : MonoBehaviour {
    #region editor
    [Header("All colliders in children must be NOT enabled")]

    [SerializeField] private Vector2 minForce = Vector2.zero;
    [SerializeField] private Vector2 maxForce = Vector2.zero;
    [SerializeField] private bool randomize = true;

    [Header("Remove Settings")]
    [SerializeField] private float removeOverTime = float.PositiveInfinity;
    #endregion

    private Collider2D[] children;

    #region private
    private IEnumerator _removeOverTime() {
        yield return new WaitForSeconds(.25f);

        foreach (Collider2D c2d in children) c2d.enabled = false;

        float elapsed = 0f;
        float step = Time.deltaTime / removeOverTime;

        List<SpriteRenderer> sprites = children.Select(c2d => c2d.GetComponent<SpriteRenderer>()).ToList();
        while(elapsed < 1f) {
            sprites.ForEach(sprite => sprite.color = Color.Lerp(sprite.color, Utils.Utility.transparent, Time.deltaTime / 4f));
            elapsed += step;
            yield return null;
        }

        Destroy(gameObject);
    }
    #endregion

    #region public
    public void brokeDown(Color color) {
        children = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D c2d in children) {
            Rigidbody2D rb2d = c2d.GetComponent<Rigidbody2D>();
            SpriteRenderer sr = c2d.GetComponent<SpriteRenderer>();

            sr.color = color;

            Vector2 force = maxForce;
            if (randomize) force = new Vector2(Random.Range(minForce.x, maxForce.x), Random.Range(minForce.y, maxForce.y));
            rb2d.mass = Random.Range(.5f, 1f);
            rb2d.AddForceAtPosition(force, Vector2.zero, ForceMode2D.Impulse);

            if (removeOverTime != float.PositiveInfinity) StartCoroutine(_removeOverTime());
        }
    }
    #endregion
}
