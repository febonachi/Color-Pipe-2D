using Utils;
using TMPro;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class UIPerfectSystem : MonoBehaviour {
    #region editor
    [SerializeField] private UIComboText comboTextPrefab = default;
    [SerializeField] private TextMeshProUGUI plusOneTextPrefab = default;
    [SerializeField] private TextMeshProUGUI perfectTextPrefab = default;
    #endregion

    private GameController gameController;

    private int needToAdd = 0;
    private string[] perfectWords = new string[] { "Perfect!", "Nice!", "Alright!", "Amazing!", "Cool!", "Master!" };

    #region private
    private void Awake() {
        gameController = GameController.instance;
    }

    private IEnumerator _moveAndHide(TextMeshProUGUI target, Vector2 speed, float time = 1f) {
        RectTransform rect = target.GetComponent<RectTransform>();
        Vector2 randomPosition = rect.localPosition + new Vector3(Random.Range(-100, 100), 0, 0);
        rect.localPosition = randomPosition;
        float elapsed = 0f;
        float step = Time.deltaTime / time;
        while (elapsed < 1f) {
            if(elapsed > .5f) target.color = Color.Lerp(target.color, Utility.transparent, step * 2f);
            rect.localPosition = new Vector2(rect.localPosition.x + speed.x, rect.localPosition.y + speed.y);
            elapsed += step;
            yield return null;
        }
        target.color = Utility.transparent;
        Destroy(target.gameObject);
    }

    private void instantiatePlusOneText(string text) {
        TextMeshProUGUI plusOneText = Instantiate(plusOneTextPrefab, transform);
        plusOneText.text = text;
        StartCoroutine(_moveAndHide(plusOneText, new Vector2(Random.Range(-2f, 2f), 4f)));
    }

    private void instantiatePerfectText(string text) {
        TextMeshProUGUI perfectText = Instantiate(perfectTextPrefab, transform);
        perfectText.text = text;
        Destroy(perfectText.gameObject, 2.5f);
    }

    private void instantiateComboText(string text) {
        UIComboText perfectText = Instantiate(comboTextPrefab, transform);
        perfectText.setText(text);
        Destroy(perfectText.gameObject, 2f);
    }
    #endregion

    #region public
    public async void plusOne(int count, bool isCombo) {
        if (isCombo) {
            needToAdd += (int)gameController.score.comboCost;
            if(Random.Range(0, 100) < 70) instantiatePerfectText(perfectWords.First());
            else instantiatePerfectText(perfectWords[Random.Range(1, perfectWords.Length)]);
        } else {
            needToAdd = count;
            if (count % 2 == 0) instantiateComboText($"x{count}");
        }

        while (needToAdd > 0) {
            int valueToAdd = Random.Range(1, needToAdd / 2);
            instantiatePlusOneText($"+{valueToAdd}");
            needToAdd -= valueToAdd;
            await Task.Delay(100);
        }
    }
    #endregion
}
