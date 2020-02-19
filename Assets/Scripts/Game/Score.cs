using Utils;
using UnityEngine;

public class Score {
    public int level;

    public float comboCost => required * .15f;
    public bool isMaxScore => current >= required;
    public float total => current.map(0f, required, 0f, 100f);

    public float current { get; private set; }
    public float required { get; private set; }

    #region public
    public Score(int level) {
        this.level = level;

        current = 0f;
        required = Mathf.Clamp(100f + (level * 5f), 100f, 500f);

        //required = 4; // dbg
    }

    public void addScore(float count, bool combo) {
        float amount = 0f;

        if (combo) amount = comboCost;
        else amount = count * 2;

        //amount = 1; // dbg

        current += amount;
    }
    #endregion
}
