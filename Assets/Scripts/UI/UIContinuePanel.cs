using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class UIContinuePanel : MonoBehaviour {
    #region editor
    [SerializeField] private Image background = default;
    [SerializeField] private Button continueButton = default;
    [SerializeField] private TextMeshProUGUI goodJobText = default;
    [SerializeField] private TextMeshProUGUI progressText = default;
    [SerializeField] private TextMeshProUGUI continueText = default;
    #endregion

    #region private
    private IEnumerator _setProgress(float progress) {
        int percent = 0;
        while (percent++ <= progress) {
            progressText.text = $"{percent}% completed";
            yield return null;
        }
    }
    #endregion

    #region public
    public void setContinueButtonListener(UnityAction action) {
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(action);
    }

    public void showProgress(Score score) {
        gameObject.SetActive(true);
        background.gameObject.SetActive(true);
        progressText.gameObject.SetActive(true);
        if (score.total < 100f) {
            continueText.text = "Click to restart!";
            goodJobText.text = "Try again!";
            StartCoroutine(_setProgress(score.total));
        } else {
            continueText.text = "Click to continue!";
            progressText.text = $"Level {score.level} completed!";
            goodJobText.text = "Good Job!";
        }
    }

    public void showContinueButton() {
        gameObject.SetActive(true);
        background.gameObject.SetActive(true);
        continueButton.interactable = true;
        continueButton.gameObject.SetActive(true);
        continueText.gameObject.SetActive(true);
    }

    public void setBackgroundColor(Color color) {
        Color alphaColor = color;
        alphaColor.a = .5f;
        background.color = alphaColor;
    }

    public void hide() {
        gameObject.SetActive(false);
        background.gameObject.SetActive(false);
        continueButton.interactable = false;
        progressText.gameObject.SetActive(false);
        continueText.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
    }

    //public void show() {
    //    gameObject.SetActive(true);
    //    background.gameObject.SetActive(true);
    //    continueButton.interactable = true;
    //    progressText.gameObject.SetActive(true);
    //    continueButton.gameObject.SetActive(true);
    //}
    #endregion
}
