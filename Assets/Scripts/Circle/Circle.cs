using Utils;
using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using static UnityEngine.Random;

[RequireComponent(typeof(CircleCollider2D))]
public class Circle : MonoBehaviour {
    #region editor
    [Header("Prefabs")]
    [SerializeField] private Player playerPrefab = default;
    [SerializeField] private CirclePart x4Prefab = default;
    [SerializeField] private CirclePart x8Prefab = default;
    [SerializeField] private CirclePart x16Prefab = default;

    [Header("Circle parts settings")]
    public bool doubleCollisionCount = true;

    [Header("Player settings")]
    [SerializeField] private bool playerDebug = true;
    [Range(1, 6)] public int playerForce = 1;
    [Range(1, 1000)] public int playerComboCount = 3;
    [Range(0, 100)] [SerializeField] private int playerReflectionCount = 10;

    [Header("Circle settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [Range(0, 100)] [SerializeField] private float fillAmount = 100f;
    #endregion

    public bool isActivated { get; private set; }
    public float partsReloadTime { get; private set; }
    [HideInInspector] public GameController gameController;
    [HideInInspector] public ColorScheme.SchemeColorGroup colorGroup;

    public event Action activated;
    public event Action playerDied;
    public event Action initialized;
    public event Action<int, bool, bool> interaction;

    private enum CirclePartType : int {x4 = 4, x8 = 2, x16 = 1};
    private const float deltaPartCost = 11.25f;

    private Player player;
    private CircleCollider2D c2d;
    private List<CirclePart> parts = new List<CirclePart>();

    #region private
    private void Awake() {
        c2d = GetComponent<CircleCollider2D>();

        gameController = GameController.instance;
    }

    private void Start() {
        isActivated = false;
        colorGroup = gameController.colorScheme.randomGroup();

        preparePlayer();
        prepareCircleParts();

        initialized?.Invoke();
    }

    private void FixedUpdate() {
        if (player.isDead) return;
#if UNITY_EDITOR
        if (Input.GetMouseButton(0)) {
            float horizontal = Input.GetAxis("Mouse X") * rotationSpeed;
            if(Mathf.Abs(horizontal) >= .5f && !isActivated) activate();
            transform.Rotate(Vector3.forward, horizontal);
        }
#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Moved) {
                float horizontal = (touch.deltaPosition.x / 30f) * rotationSpeed; // magic numbers ftw
                if(Mathf.Abs(horizontal) >= .5f && !isActivated) activate();
                transform.Rotate(Vector3.forward, horizontal);
            }
        }
#endif
    }

    private void activate() {
        isActivated = true;
        player.gameObject.SetActive(true);

        activated?.Invoke();
    }

    private void prepareCircleParts() {
        parts.ForEach(part => Destroy(part.gameObject));
        parts.Clear();

        float angle = 0;

        List<CirclePartType> partTypes = randomPartTypes();

        #region Spikes Difficulty
        int level = gameController.score.level;
        int spikesCount = 0;
        switch (level) {
            case int lvl when level <= 3: {
                spikesCount = 0;
                break;
            }
            case int lvl when level > 3 && level < 25: {
                spikesCount = Range(1, 3 + 1);
                break;
            }
            case int lvl when level >= 25: {
                spikesCount = Range(2, 4 + 1);
                break;
            }
            case int lvl when level >= 50: {
                spikesCount = Range(3, 5 + 1);
                break;
            }
            default: break;
        }

        int count = partTypes.Count(partType => isTruePartType(partType));
        spikesCount = Mathf.Clamp(spikesCount, 0, (int)(count / 2f) - 1);
        if (count <= 5) spikesCount = 1;

        if (count >= 7) partsReloadTime = 2.25f;
        else partsReloadTime = 1.5f;
        #endregion

        int spikesSpawned = 0;

        for (int i = 0; i < partTypes.Count; i++) {
            CirclePartType partType = partTypes.ElementAt(i);

            if(i > 0) {
                CirclePartType previousPartType = partTypes.ElementAt(i - 1);

                int partTypeValue = truePartValue(partType);
                int previousPartTypeValue = truePartValue(previousPartType);
                // get true values of angle cost
                int difference = Mathf.Abs(partTypeValue - previousPartTypeValue);
                float differenceAngle = (difference * deltaPartCost);
                if(partTypeValue < previousPartTypeValue) differenceAngle = -differenceAngle;
                else if(partTypeValue == previousPartTypeValue) differenceAngle = 0f;
                angle += differenceAngle;
            }

            float radius = c2d.radius;
            float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector3 point = new Vector3(x, y, 0);

            CirclePart partPrefab = circlePartPrefab(partType);
            if(partPrefab != null) {
                CirclePart part = Instantiate(partPrefab, point, Quaternion.AngleAxis(angle, Vector3.forward));
                part.transform.position = part.transform.position + transform.position;

                bool spawnSpike = false;
                if(spikesSpawned < spikesCount) {
                    float chance = (i + 1) * (100f / partTypes.Count);
                    if (Range(0f, 100f) <= chance) {
                        spikesSpawned++;
                        spawnSpike = true;
                    }
                }

                part.setupCircle(this, spawnSpike ? CirclePart.CirclePartState.Spike : CirclePart.CirclePartState.Default);
                parts.Add(part);
            }

            angle += circlePartCost(partType);
        }
    }

    private List<CirclePartType> randomPartTypes() {
        List<CirclePartType> partTypes = new List<CirclePartType>();

        CirclePartType[] getCirclePartTypeArray() {
            CirclePartType[][] validPartTypes = new CirclePartType[][]{
                new CirclePartType[] { CirclePartType.x8, CirclePartType.x8 },
                new CirclePartType[] { CirclePartType.x4 },
                new CirclePartType[] { CirclePartType.x16, CirclePartType.x8, CirclePartType.x16 },
                new CirclePartType[] { CirclePartType.x8, CirclePartType.x16, CirclePartType.x16 },
                new CirclePartType[] { CirclePartType.x16, CirclePartType.x16, CirclePartType.x8 },
                new CirclePartType[] { CirclePartType.x16, CirclePartType.x16, CirclePartType.x16, CirclePartType.x16 }
            };

            //// circle part depends on game level
            //float chanceDelta = 100 / validPartTypes.Length;
            //int index = Range(0, validPartTypes.Length);
            //for (int i = 0; i < validPartTypes.Length; i++) {
            //    float chance = chanceDelta;
            //    int difference = index - i;
            //    int differenceAbs = Mathf.Abs(difference);
            //    if (difference > 0) chance = differenceAbs * chanceDelta;
            //    else if (difference < 0) {
            //        chance /= differenceAbs;

            //        // add some delta to give more chance
            //        float difficultyChanceDelta = gameController.score.level / 50f;
            //        chance += Mathf.Clamp(difficultyChanceDelta, 0f, 15f); // (750+ level will give +16%)
            //    }
                
            //    int randomChance = Range(0, 100);
            //    if (randomChance <= chance) index = i;
            //}

            return validPartTypes.ElementAt(Range(0, validPartTypes.Length));
        }

        for(int i = 0; i < 4; i++) {
            CirclePartType[] validType = getCirclePartTypeArray();
            foreach(CirclePartType partType in validType) partTypes.Add(partType);
        }

        if (fillAmount == 100f && gameController.score.level > 0 && (gameController.score.level % 2) == 0) fillAmount = Range(70f, 95f);

        percentageCircleFill(ref partTypes, fillAmount);

        return partTypes;
    }

    private void percentageCircleFill(ref List<CirclePartType> lst, float percentage) {
        float emptyAmount = 360f - percentage.map(0f, 100f, 0f, 360f);
        while (emptyAmount >= deltaPartCost) {
            IEnumerable<CirclePartType> partsToRemove = lst.Where(part => isTruePartType(part) && circlePartCost(part) <= emptyAmount);
            int partsToRemoveCount = partsToRemove.Count();
            if (partsToRemoveCount == 0) break;

            CirclePartType partToRemove = partsToRemove.ElementAt(Range(0, partsToRemoveCount));
            int partToRemoveIndex = lst.IndexOf(partToRemove);

            lst.Remove(partToRemove);
            // +100 emits fake circle part (get angle but dont create)
            lst.Insert(partToRemoveIndex, partToRemove + 100);

            emptyAmount -= circlePartCost(partToRemove);
        }
    }

    private int truePartValue(CirclePartType type) => (int)type % 100;

    private bool isTruePartType(CirclePartType type) => (int)type < 100;

    private float circlePartCost(CirclePartType type) => (truePartValue(type) * deltaPartCost) * 2f;

    private CirclePart circlePartPrefab(CirclePartType type) {
        if(type == CirclePartType.x4) return x4Prefab;
        else if(type == CirclePartType.x8) return x8Prefab;
        else if(type == CirclePartType.x16) return x16Prefab;
        return null;
    }

    private void preparePlayer() {
        if (!playerDebug) {
            #region player difficulty
            int minForce = 1;
            int maxForce = 6;

            int level = gameController.score.level;

            switch (level) {
                case int lvl when level < 3: {
                    playerForce = minForce;
                    playerComboCount = 4;
                    break;
                }
                case int lvl when level >= 3 && level < 10: {
                    playerForce = 2;
                    playerComboCount = Range(4, 6 + 1);
                    break;
                }
                case int lvl when level >= 10 && level < 20: {
                    playerForce = 3;
                    playerComboCount = Range(6, 8 + 1);
                    break;
                }
                case int lvl when level >= 20 && level < 40: {
                    playerForce = 4;
                    playerComboCount = Range(6, 10 + 1);
                    break;
                }
                case int lvl when level >= 40 && level < 50: {
                    playerForce = 5;
                    playerComboCount = Range(6, 10 + 1);
                    break;
                }
                case int lvl when level >= 50: {
                    playerForce = maxForce;
                    playerComboCount = Range(8, 12 + 1);
                    break;
                }
                default: break;
            }

            int losesInRow = PlayerPrefs.GetInt("loses");
            if (losesInRow >= 3 && losesInRow < 5) playerForce -= 1; // 5 loses in a row
            else if (losesInRow >= 5 && losesInRow < 8) playerForce -= 2;
            else if (losesInRow >= 8) playerForce -= 3;

            playerForce = Mathf.Clamp(playerForce, minForce, maxForce); // min/max value
            playerReflectionCount = 8;
        }
        #endregion

        player = Instantiate(playerPrefab);

        player.setCircle(this);
        player.died += onPlayerDied;
        player.interactedWithCircle += onPlayerInteraction;
        player.reflectionStarted += onPlayerStartReflection;

        player.gameObject.SetActive(false);
    }

    private void onPlayerDied() {
        playerDied?.Invoke();
    }

    private void onPlayerInteraction (int comboCount, bool combo, bool reloadState){
        interaction?.Invoke(comboCount, combo, reloadState);
    }

    private void onPlayerStartReflection() {
        parts.ForEach(part => part.reload(0f));
    }

    //private void OnDrawGizmos() {
    //    foreach(CirclePart part in parts) {
    //        Gizmos.DrawLine(transform.position, part.transform.position);
    //    }
    //}
    #endregion

    #region public
    public Color randomColor() => colorGroup.randomColor();

    public List<Vector3> getReflectionPoints(bool all = false) {
        List<Vector3> reflectPoints = new List<Vector3>();
        CirclePart[] circleParts = parts.ToArray();

        reflectPoints.Add(transform.position);
        if (all == false) {
            while (reflectPoints.Count < playerReflectionCount) {
                IEnumerable<CirclePart> tmp = circleParts.Where(part => part.center.position != reflectPoints.Last());
                int rndIndex = Range(0, tmp.Count());
                reflectPoints.Add(tmp.ElementAt(rndIndex).center.position);
            }
        }else {
            System.Random rnd = new System.Random();
            foreach (CirclePart cp in circleParts.Shuffle(rnd)) reflectPoints.Add(cp.center.position);
        }

        return reflectPoints;
    }

    public IEnumerator _gameOver(GameController.GameState state) {
        player.died -= onPlayerDied;
        player.interactedWithCircle -= onPlayerInteraction;
        player.reflectionStarted -= onPlayerStartReflection;

        if (state == GameController.GameState.Win) {
            yield return StartCoroutine(player._gameOver());
            foreach(CirclePart circlePart in parts.Shuffle(new System.Random())) {
                StartCoroutine(circlePart._gameOver());
                yield return new WaitForSeconds(.05f);
            }
        }
    }    
    #endregion
}
