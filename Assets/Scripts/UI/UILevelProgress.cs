using Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;

[RequireComponent(typeof(Animator))]
public class UILevelProgress : MonoBehaviour {
    #region editor
    [SerializeField] private Image progressLineBackground = default;
    [SerializeField] private Image progressLine = default;
    [SerializeField] private Image leftImage = default;
    [SerializeField] private Image rightImage = default;
    [SerializeField] private TextMeshProUGUI leftText = default;
    [SerializeField] private TextMeshProUGUI rightText = default;
    [SerializeField] private TextMeshProUGUI scoreText = default;
    #endregion

    public float fillAmount { get; private set; }

    private Animator animator;

    private Coroutine fillAmountCoroutine = null;

    #region private
    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private async void randomScaleAnimation() {
        animator.SetInteger("scale", Random.Range(1, 3 + 1));
        await Task.Delay(500);
        animator.SetInteger("scale", 0);
    }

    private IEnumerator _addProgress() {
        while(progressLine.fillAmount < fillAmount) {
            progressLine.fillAmount += .01f;
            yield return null;
        }
    }
    #endregion

    #region public
    public void addProgress(float amount) {
        amount = amount.map(0f, 100f, 0f, 1f);

        if(fillAmountCoroutine != null) StopCoroutine(fillAmountCoroutine);

        fillAmount += amount;
        fillAmountCoroutine = StartCoroutine(_addProgress());
        randomScaleAnimation();

        if (progressLine.fillAmount >= 1f) rightImage.color = progressLine.color;
    }

    public void setProgress(Score score) {
        float value = score.total.map(0f, 100f, 0f, 1f);

        scoreText.text = ((int)score.current).ToString();

        if (fillAmountCoroutine != null) StopCoroutine(fillAmountCoroutine);

        fillAmount = value;
        fillAmountCoroutine = StartCoroutine(_addProgress());
        randomScaleAnimation();

        if (progressLine.fillAmount >= 1f) rightImage.color = progressLine.color;
    }

    public void resetProgress(int level = 0) {
        fillAmount = 0f;
        progressLine.fillAmount = 0f;

        leftImage.color = progressLine.color;
        rightImage.color = progressLineBackground.color;

        leftText.text = level.ToString();
        rightText.text = (level + 1).ToString();
    }

    public void show() {
        scoreText.text = "0";
        animator.SetTrigger("show");
    }

    public void hide() {
        animator.SetTrigger("hide");
    }
    #endregion
}
