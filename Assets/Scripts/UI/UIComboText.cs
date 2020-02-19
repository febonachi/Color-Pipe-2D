using TMPro;
using UnityEngine;

public class UIComboText : MonoBehaviour {
    #region editor
    [SerializeField] private TextMeshProUGUI xText = default;
    #endregion

    #region public
    public void setText(string text) => xText.text = text;
    #endregion
}
