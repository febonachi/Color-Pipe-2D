using System;
using UnityEngine;

using static UnityEngine.Random;

public class ColorScheme : MonoBehaviour {
    [Serializable] public class SchemeColorGroup {
        public Color[] partsColor = default;

        public Color randomColor() => partsColor[Range(0, partsColor.Length)];
    }

    #region editor
    [SerializeField] private int loadScheme = -1; 
    [SerializeField] private SchemeColorGroup[] groups = default;
    #endregion

    #region public
    public SchemeColorGroup randomGroup() {
        if (loadScheme != -1) return groups[loadScheme];
        return groups[Range(0, groups.Length)];
    }
    #endregion
}
