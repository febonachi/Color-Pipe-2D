using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading.Tasks;

[RequireComponent(typeof(Animator))]
public class UIController : MonoBehaviour {
    #region editor
    [Header("Buttons Settings")]
    [SerializeField] private Button soundButton = default;
    [SerializeField] private Sprite soundButtonOnSprite = default;
    [SerializeField] private Sprite soundButtonOffSprite = default;

    [SerializeField] private Button colorSwitchButton = default;
    [SerializeField] private Image backgroundImage = default;
    [SerializeField] private Color[] lightThemeColors = default;
    [SerializeField] private Color[] darkThemeColors = default;
    
    [Header("UI Elements")]
    [SerializeField] private UISwipeHint swipeHint = default;
    [SerializeField] private UILevelProgress progress = default;
    [SerializeField] private UIContinuePanel continuePanel = default;
    [SerializeField] private UIPerfectSystem perfectSystem = default;
    #endregion

    private GameController gameController;
    private Animator animator;

    #region private
    private void Awake() {
        gameController = GameController.instance;
        animator = GetComponent<Animator>();

        soundButton.onClick.AddListener(onSoundButtonClick);
        colorSwitchButton.onClick.AddListener(onColorSwitchButtonClick);

        // sound settings
        bool savedSoundStatus = PlayerPrefs.GetInt("sound") == 1 ? true : false;
        gameController.audioController.on = savedSoundStatus;
        if (gameController.audioController.on) ((Image)soundButton.targetGraphic).sprite = soundButtonOnSprite;
        else ((Image)soundButton.targetGraphic).sprite = soundButtonOffSprite;

        //theme settings
        Color backgroundColor = PlayerPrefs.GetString("theme") == "dark" ? 
            darkThemeColors[Random.Range(0, darkThemeColors.Length)] : 
            lightThemeColors[Random.Range(0, lightThemeColors.Length)];
        backgroundImage.color = backgroundColor;
    }

    private void onSoundButtonClick() {
        bool savedSoundStatus = PlayerPrefs.GetInt("sound") == 1 ? true : false;
        gameController.audioController.on = !savedSoundStatus;
        if (gameController.audioController.on) ((Image)soundButton.targetGraphic).sprite = soundButtonOnSprite;
        else ((Image)soundButton.targetGraphic).sprite = soundButtonOffSprite;

        PlayerPrefs.SetInt("sound", gameController.audioController.on ? 1 : 0);
    }

    private void onColorSwitchButtonClick() {
        bool isDarkTheme = PlayerPrefs.GetString("theme") == "dark";

        colorSwitchButton.targetGraphic.rectTransform.Rotate(Vector3.forward, 180f);

        Color backgroundColor;
        if (isDarkTheme) {
            PlayerPrefs.SetString("theme", "light");
            backgroundColor = lightThemeColors[Random.Range(0, lightThemeColors.Length)];
        } else {
            PlayerPrefs.SetString("theme", "dark");
            backgroundColor = darkThemeColors[Random.Range(0, darkThemeColors.Length)];
        }
        backgroundImage.color = backgroundColor;
    }
    #endregion

    #region public
    public void setContinueButtonListener(UnityAction action) => continuePanel.setContinueButtonListener(action);

    public void reset() {
        swipeHint.hide();
        continuePanel.hide();
        progress.gameObject.SetActive(false);

        progress.resetProgress(gameController.score.level);

        if (gameController.gameState == GameController.GameState.Loading) {
            animator.SetTrigger("showSettings");
            swipeHint.show();
        }
    }

    public void onGameInitialized() => progress.show();

    public void updateScore(int count, bool isCombo, bool brokeDownPart) {
        progress.setProgress(gameController.score);

        perfectSystem.plusOne(count, isCombo);
    }

    public void showProgressBar() {
        progress.gameObject.SetActive(true);
        progress.show();
        swipeHint.hide();

        animator.SetTrigger("hideSettings");
    }

    public async void gameOver(GameController.GameState state) {
        progress.hide();

        await Task.Delay(500);

        continuePanel.showProgress(gameController.score);

        //switch (state) {
        //    case GameController.GameState.Lose: {
        //        break;
        //    }
        //    case GameController.GameState.Win: {
        //        break;
        //    }
        //    default: break;
        //}

        await Task.Delay(1000);

        continuePanel.showContinueButton();
    }
    #endregion
}
