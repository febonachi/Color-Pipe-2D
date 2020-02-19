using UnityEngine;
using EZCameraShake;
using System.Collections;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour {
    #region singleton
    public static GameController instance;
    #endregion

    #region editor
    public UIController ui = default;
    public ColorScheme colorScheme = default;
    public CameraShaker cameraShaker = default;
    public AudioController audioController = default;
    [SerializeField] private GameObject winPs = default;

    [SerializeField] private int levelToLoad = 0;
    #endregion

    public enum GameState { Loading, Game, Win, Lose};

    [HideInInspector] public Score score;
    [HideInInspector] public GameState gameState = GameState.Loading;

    private Circle circle;

    #region private
    private void Awake() {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(this);

        ui.setContinueButtonListener(continueGame);
        SceneManager.sceneLoaded += onSceneLoaded;

        if (levelToLoad == 0) PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level", 1));
        else PlayerPrefs.SetInt("level", levelToLoad);

        if (PlayerPrefs.GetInt("level") <= 1) {
            PlayerPrefs.SetInt("sound", 1);
            PlayerPrefs.SetString("theme", "dark");
        }

        SceneManager.LoadScene("game");
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "game") {
            gameState = GameState.Loading;

            score = new Score(PlayerPrefs.GetInt("level"));

            ui.reset();

            circle = FindObjectOfType<Circle>();
            circle.playerDied += onPlayerDied;
            circle.activated += onCircleActivated;
            circle.initialized += onCircleInitialized;
            circle.interaction += onCircleInteraction;
        }
    }

    private void onPlayerDied() => StartCoroutine(_gameOver(GameState.Lose));

    private void onCircleActivated() {
        gameState = GameState.Game;

        ui.showProgressBar();
    }

    private void onCircleInitialized() {
        ui.onGameInitialized();
    }

    private void onCircleInteraction(int count, bool isCombo, bool brokeDownPart) {
        score.addScore(count, isCombo);

        ui.updateScore(count, isCombo, brokeDownPart);

        if (isCombo) cameraShaker.ShakeOnce(4f, 3f, .5f, 2f);

        if (score.isMaxScore) StartCoroutine(_gameOver(GameState.Win));
    }

    private void reloadGameScene() {
        circle.playerDied -= onPlayerDied;
        circle.activated -= onCircleActivated;
        circle.initialized -= onCircleInitialized;
        circle.interaction -= onCircleInteraction;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void nextLevel() {
        PlayerPrefs.SetInt("loses", 0);
        PlayerPrefs.SetInt("level", score.level + 1);

        reloadGameScene();
    }
    private void restartLevel() {
        PlayerPrefs.SetInt("loses", PlayerPrefs.GetInt("loses") + 1);

        reloadGameScene();
    }
    #endregion

    #region public
    public IEnumerator _gameOver(GameState state) {
        gameState = state;

        if (state == GameState.Lose) {
            cameraShaker.ShakeOnce(2f, 2f, .1f, 1f);
        }

        yield return StartCoroutine(circle._gameOver(gameState));

        audioController.play("endGame");

        if (state == GameState.Win) {
            Instantiate(winPs);
        }

        ui.gameOver(state);
    }

    public void continueGame() {
        switch (gameState) {
            case GameState.Lose: {
                restartLevel();
                break;
            }
            case GameState.Win: {
                nextLevel();
                break;
            }
            default: break;
        }
    }
    #endregion
}
