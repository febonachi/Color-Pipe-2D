using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UISwipeHint : MonoBehaviour {
    private Animator animator;

    #region private
    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void OnEnable() {
        animator.SetBool("hint", true);
    }

    private void OnDisable() {
        animator.SetBool("hint", false);
    }
    #endregion

    #region public
    public void show() {
        gameObject.SetActive(true);
    }

    public void hide() {
        gameObject.SetActive(false);
    }
    #endregion
}
