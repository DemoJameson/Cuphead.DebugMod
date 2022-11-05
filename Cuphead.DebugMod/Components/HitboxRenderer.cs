using System;
using System.Collections;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Components;

public class HitboxRenderer : MonoBehaviour {
    public static class Colors {
        public static Color Secret = ColorUtils.HexToColor("6aff00");

        public static Color Undefined = ColorUtils.HexToColor("818181");

        public static Color UndefinedSolid = ColorUtils.HexToColor("636363");

        public static Color Player = ColorUtils.HexToColor("f6ed0d");

        public static Color PlayerAttack = ColorUtils.HexToColor("ff6700");

        public static Color Hurt = Color.red;

        public static Color HurtSolid = ColorUtils.HexToColor("a10000");

        public static Color Damageable = ColorUtils.HexToColor("7800ae");

        public static Color Parry = ColorUtils.HexToColor("ff36e1");

        public static Color ParryEnemy = ColorUtils.HexToColor("d609c8");

        public static Color PlatformSolid = ColorUtils.HexToColor("0c2acf");

        public static Color PlatformSemiSolid = ColorUtils.HexToColor("0ca5cf");

        public static Color Other = ColorUtils.HexToColor("cff1ff");

        public static Color Petrify = ColorUtils.HexToColor("00b30c");

        public static Color PlatformSolidWall = ColorUtils.HexToColor("0c2acf");

        public static Color PlatformSolidGround = ColorUtils.HexToColor("2848fc");

        public static Color PlatformSolidCeiling = ColorUtils.HexToColor("081d70");
    }

    public enum UpdateType {
        Parryable,
        Damageable
    }

    private CircleCollider2D[] circleColliders;

    private LineRenderer[] circleCollidersLines;

    private BoxCollider2D[] boxColliders;

    private LineRenderer[] boxCollidersLines;

    private PolygonCollider2D[] polygonColliders;

    private LineRenderer[] polygonCollidersLines;

    private EdgeCollider2D[] edgeColliders;

    private LineRenderer[] edgeCollidersLines;

    private bool active;

    private bool colorUpdate;

    private bool lineUpdate;

    private float hitboxTransparency;

    private bool updateLinesOnce;

    private AbstractLevelEntity compAbstractLevelEntity;

    private AbstractParryEffect compAbstractParryEffect;

    private AbstractProjectile compAbstractProjectile;

    private ParrySwitch compParrySwitch;

    private AbstractCollidableObject compAbstractCollidableObject;

    private DamageReceiver compDamageReceiver;

    private DamageReceiverChild compDamageReceiverChild;

    private CollisionChild compCollisionChild;

    public static bool show;

    private static AbstractCupheadGameCamera camera;

    private UpdateType updateType;

    public static LineRenderer blackScreen;

    public static bool obscureScreen;

    private static readonly float MAX_UPDATE_DISTANCE;

    private static SpriteLayer renderLayer;

    public bool isPlatformElement;

    private bool updateOnEnable;

    private bool updateColorOnce;

    public static bool hidePlatforms;

    private bool nonHurting;

    private void Start() {
#if v1_0
        renderLayer = SpriteLayer.Top;
#else
        renderLayer = Map.Current != null ? SpriteLayer.ForegroundEffects : SpriteLayer.Top;
#endif

        active = true;
        hitboxTransparency = 1f;
        if (Level.Current != null && Level.Current.CurrentLevel == Levels.ShmupTutorial && gameObject.layer == 5) {
            gameObject.layer = 0;
        }

        lineUpdate = true;
        colorUpdate = false;
        if (gameObject.layer == 18 || gameObject.layer == 19 || gameObject.layer == 20) {
            lineUpdate = false;
        }

        for (int i = 0; i < 1; i++) {
            if (gameObject.tag == "PlayerProjectile") {
                break;
            }

            if (!CanChangeColor()) {
                continue;
            }

            FindComponents();
            if (compAbstractLevelEntity != null || compAbstractParryEffect != null || compAbstractProjectile != null ||
                compParrySwitch != null) {
                updateType = UpdateType.Parryable;
            } else if (compDamageReceiver != null || compDamageReceiverChild != null) {
                updateType = UpdateType.Damageable;
                if (compDamageReceiverChild != null &&
                    compDamageReceiverChild.Receiver.gameObject.GetComponent<TrainLevelEngineBoss>() != null) {
                    nonHurting = true;
                }
            }

            colorUpdate = true;
            if ((bool) gameObject.GetComponent<FlyingBirdLevelBirdFeather>() ||
                (bool) gameObject.GetComponent<RobotLevelGemProjectile>()) {
                lineUpdate = false;
                colorUpdate = false;
                updateColorOnce = true;
                updateOnEnable = true;
            }
        }

        SetupBoxColliders();
        SetupCircleColliders();
        SetupPolygonColliders();
        SetupEdgeColliders();
        if (!lineUpdate) {
            StartCoroutine(waitUntilUpdate_cr(0.035f));
        }
    }

    private void LateUpdate() {
        if (!show) {
            SetEnabled(enabled: false);
            return;
        }

        if (hidePlatforms && isPlatformElement) {
            SetEnabled(enabled: false);
            return;
        }

        if (camera != null) {
            active = Vector2.Distance(transform.position, camera.transform.position) < MAX_UPDATE_DISTANCE;
        }

        if (active && CupheadTime.GlobalSpeed != 0f) {
            if (updateLinesOnce) {
                lineUpdate = true;
            }

            if (updateColorOnce) {
                colorUpdate = true;
            }

            UpdateBoxColliders();
            UpdateCircleColliders();
            UpdatePolygonColliders();
            UpdateEdgeColliders();
            if (updateLinesOnce) {
                lineUpdate = false;
                updateLinesOnce = false;
            }

            if (updateColorOnce) {
                colorUpdate = false;
                updateColorOnce = false;
            }
        }
    }

    private void SetupCircleColliders() {
        CircleCollider2D[] components = gameObject.GetComponents<CircleCollider2D>();
        if (circleColliders == null) {
            circleColliders = new CircleCollider2D[components.Length];
            circleCollidersLines = new LineRenderer[components.Length];
        } else {
            CircleCollider2D[] array = circleColliders;
            LineRenderer[] array2 = circleCollidersLines;
            circleColliders = new CircleCollider2D[components.Length];
            circleCollidersLines = new LineRenderer[components.Length];
            for (int i = 0; i < array.Length; i++) {
                circleColliders[i] = array[i];
                circleCollidersLines[i] = array2[i];
            }
        }

        for (int j = 0; j < components.Length; j++) {
            if (!(circleColliders[j] != null)) {
                circleColliders[j] = components[j];
                circleCollidersLines[j] =
                    ((gameObject.GetComponents<Collider2D>().Length == 1 && components.Length == 1)
                        ? gameObject.AddComponent<LineRenderer>()
                        : new GameObject().AddComponent<LineRenderer>());
                circleCollidersLines[j].gameObject.transform
                    .SetParent(gameObject.transform, worldPositionStays: false);
                float radius = circleColliders[j].radius;
                Vector2 offset = circleColliders[j].offset;
                Vector3[] array3 = new Vector3[(int) radius / 3 + 5 * ((!(GameObject.Find("Map") != null)) ? 1 : 6)];
                for (int k = 1; k < array3.Length + 1; k++) {
                    float f = (float) Math.PI / 180f * (float) j;
                    array3[k - 1] = new Vector3(offset.x + Mathf.Sin(f) * radius, offset.y + Mathf.Cos(f) * radius, 0f);
                }

                circleCollidersLines[j].material = new Material(Shader.Find("Sprites/Default"));
                circleCollidersLines[j].positionCount = array3.Length;
                circleCollidersLines[j].SetPositions(array3);
                Color color = GetColor(circleColliders[j]);
                color.a = hitboxTransparency;
                circleCollidersLines[j].startColor = color;
                circleCollidersLines[j].endColor = color;
                circleCollidersLines[j].startWidth = ((Map.Current != null) ? 0.05f : 5f);
                circleCollidersLines[j].endWidth = ((Map.Current != null) ? 0.05f : 5f);
                circleCollidersLines[j].sortingLayerName = renderLayer.ToString();
                circleCollidersLines[j].sortingOrder = 20000;
                circleCollidersLines[j].material.renderQueue = 5000;
                circleCollidersLines[j].useWorldSpace = false;
                circleCollidersLines[j].loop = true;
                circleCollidersLines[j].enabled = circleColliders[j].enabled && show;
            }
        }
    }

    private void SetupBoxColliders() {
        BoxCollider2D[] components = gameObject.GetComponents<BoxCollider2D>();
        if (boxColliders == null) {
            boxColliders = new BoxCollider2D[components.Length];
            boxCollidersLines = new LineRenderer[components.Length];
        } else {
            BoxCollider2D[] array = boxColliders;
            LineRenderer[] array2 = boxCollidersLines;
            boxColliders = new BoxCollider2D[components.Length];
            boxCollidersLines = new LineRenderer[components.Length];
            for (int i = 0; i < array.Length; i++) {
                boxColliders[i] = array[i];
                boxCollidersLines[i] = array2[i];
            }
        }

        for (int j = 0; j < components.Length; j++) {
            if (!(boxColliders[j] != null)) {
                boxColliders[j] = components[j];
                boxCollidersLines[j] =
                    ((gameObject.GetComponents<Collider2D>().Length == 1 && components.Length == 1)
                        ? gameObject.AddComponent<LineRenderer>()
                        : new GameObject().AddComponent<LineRenderer>());
                boxCollidersLines[j].gameObject.transform
                    .SetParent(gameObject.transform, worldPositionStays: false);
                Vector2 size = boxColliders[j].size;
                Vector2 offset = boxColliders[j].offset;
                size *= 0.5f;
                Vector3[] array3 = new Vector3[4] {
                    new Vector3(0f - size.x + offset.x, 0f - size.y + offset.y, 1000f),
                    new Vector3(0f - size.x + offset.x, size.y + offset.y, 1000f),
                    new Vector3(size.x + offset.x, size.y + offset.y, 1000f),
                    new Vector3(size.x + offset.x, 0f - size.y + offset.y, 1000f)
                };
                boxCollidersLines[j].material = new Material(Shader.Find("Sprites/Default"));
                boxCollidersLines[j].positionCount = array3.Length;
                boxCollidersLines[j].SetPositions(array3);
                Color color = GetColor(boxColliders[j]);
                color.a = hitboxTransparency;
                boxCollidersLines[j].startColor = color;
                boxCollidersLines[j].endColor = color;
                boxCollidersLines[j].startWidth = ((Map.Current != null) ? 0.05f : 5f);
                boxCollidersLines[j].endWidth = ((Map.Current != null) ? 0.05f : 5f);
                boxCollidersLines[j].sortingLayerName = renderLayer.ToString();
                boxCollidersLines[j].sortingOrder = 20000;
                boxCollidersLines[j].material.renderQueue = 5000;
                boxCollidersLines[j].useWorldSpace = false;
                boxCollidersLines[j].loop = true;
                boxCollidersLines[j].enabled = boxColliders[j].enabled && show;
            }
        }
    }

    private void SetupPolygonColliders() {
        PolygonCollider2D[] components = gameObject.GetComponents<PolygonCollider2D>();
        if (polygonColliders == null) {
            polygonColliders = new PolygonCollider2D[components.Length];
            polygonCollidersLines = new LineRenderer[components.Length];
        } else {
            PolygonCollider2D[] array = polygonColliders;
            LineRenderer[] array2 = polygonCollidersLines;
            polygonColliders = new PolygonCollider2D[components.Length];
            polygonCollidersLines = new LineRenderer[components.Length];
            for (int i = 0; i < array.Length; i++) {
                polygonColliders[i] = array[i];
                polygonCollidersLines[i] = array2[i];
            }
        }

        for (int j = 0; j < components.Length; j++) {
            if (!(polygonColliders[j] != null)) {
                polygonColliders[j] = components[j];
                polygonCollidersLines[j] =
                    ((gameObject.GetComponents<Collider2D>().Length == 1 && components.Length == 1)
                        ? gameObject.AddComponent<LineRenderer>()
                        : new GameObject().AddComponent<LineRenderer>());
                polygonCollidersLines[j].gameObject.transform
                    .SetParent(gameObject.transform, worldPositionStays: false);
                Vector3[] array3 = new Vector3[polygonColliders[j].points.Length];
                _ = polygonColliders[j].offset;
                for (int k = 0; k < array3.Length; k++) {
                    array3[k] = polygonColliders[j].points[k] + polygonColliders[j].offset;
                }

                polygonCollidersLines[j].material = new Material(Shader.Find("Sprites/Default"));
                polygonCollidersLines[j].positionCount = array3.Length;
                polygonCollidersLines[j].SetPositions(array3);
                Color color = GetColor(polygonColliders[j]);
                color.a = hitboxTransparency;
                polygonCollidersLines[j].startColor = color;
                polygonCollidersLines[j].endColor = color;
                polygonCollidersLines[j].startWidth = ((Map.Current != null) ? 0.05f : 5f);
                polygonCollidersLines[j].endWidth = ((Map.Current != null) ? 0.05f : 5f);
                polygonCollidersLines[j].sortingLayerName = renderLayer.ToString();
                polygonCollidersLines[j].sortingOrder = 20000;
                polygonCollidersLines[j].material.renderQueue = 5000;
                polygonCollidersLines[j].useWorldSpace = false;
                polygonCollidersLines[j].loop = true;
                polygonCollidersLines[j].enabled = polygonColliders[j].enabled && show;
            }
        }
    }

    private void SetupEdgeColliders() {
        EdgeCollider2D[] components = gameObject.GetComponents<EdgeCollider2D>();
        if (edgeColliders == null) {
            edgeColliders = new EdgeCollider2D[components.Length];
            edgeCollidersLines = new LineRenderer[components.Length];
        } else {
            EdgeCollider2D[] array = edgeColliders;
            LineRenderer[] array2 = edgeCollidersLines;
            edgeColliders = new EdgeCollider2D[components.Length];
            edgeCollidersLines = new LineRenderer[components.Length];
            for (int i = 0; i < array.Length; i++) {
                edgeColliders[i] = array[i];
                edgeCollidersLines[i] = array2[i];
            }
        }

        for (int j = 0; j < components.Length; j++) {
            if (!(edgeColliders[j] != null)) {
                edgeColliders[j] = components[j];
                edgeCollidersLines[j] =
                    ((gameObject.GetComponents<Collider2D>().Length == 1 && components.Length == 1)
                        ? gameObject.AddComponent<LineRenderer>()
                        : new GameObject().AddComponent<LineRenderer>());
                edgeCollidersLines[j].gameObject.transform
                    .SetParent(gameObject.transform, worldPositionStays: false);
                Vector3[] array3 = new Vector3[edgeColliders[j].points.Length];
                _ = edgeColliders[j].offset;
                for (int k = 0; k < array3.Length; k++) {
                    array3[k] = edgeColliders[j].points[k] + edgeColliders[j].offset;
                }

                edgeCollidersLines[j].material = new Material(Shader.Find("Sprites/Default"));
                edgeCollidersLines[j].positionCount = array3.Length;
                edgeCollidersLines[j].SetPositions(array3);
                Color color = GetColor(edgeColliders[j]);
                color.a = hitboxTransparency;
                edgeCollidersLines[j].startColor = color;
                edgeCollidersLines[j].endColor = color;
                edgeCollidersLines[j].startWidth = ((Map.Current != null) ? 0.05f : 5f);
                edgeCollidersLines[j].endWidth = ((Map.Current != null) ? 0.05f : 5f);
                edgeCollidersLines[j].sortingLayerName = renderLayer.ToString();
                edgeCollidersLines[j].sortingOrder = 20000;
                edgeCollidersLines[j].material.renderQueue = 5000;
                edgeCollidersLines[j].useWorldSpace = false;
                edgeCollidersLines[j].loop = false;
                edgeCollidersLines[j].enabled = edgeColliders[j].enabled && show;
            }
        }
    }

    private void UpdateBoxColliders() {
        for (int i = 0; i < boxColliders.Length; i++) {
            boxCollidersLines[i].enabled = boxColliders[i].enabled;
            if (boxCollidersLines[i].enabled) {
                Color color = boxCollidersLines[i].startColor;
                if (colorUpdate) {
                    color = UpdateColor(boxColliders[i]);
                }

                color.a = hitboxTransparency;
                if (compParrySwitch != null && !compParrySwitch.enabled) {
                    color.a = 0f;
                }

                boxCollidersLines[i].startColor = color;
                boxCollidersLines[i].endColor = color;
                if (lineUpdate) {
                    Vector3[] positions = new Vector3[4] {
                        new Vector3((0f - boxColliders[i].size.x) * 0.5f + boxColliders[i].offset.x,
                            (0f - boxColliders[i].size.y) * 0.5f + boxColliders[i].offset.y, 1000f),
                        new Vector3((0f - boxColliders[i].size.x) * 0.5f + boxColliders[i].offset.x,
                            boxColliders[i].size.y * 0.5f + boxColliders[i].offset.y, 1000f),
                        new Vector3(boxColliders[i].size.x * 0.5f + boxColliders[i].offset.x,
                            boxColliders[i].size.y * 0.5f + boxColliders[i].offset.y, 1000f),
                        new Vector3(boxColliders[i].size.x * 0.5f + boxColliders[i].offset.x,
                            (0f - boxColliders[i].size.y) * 0.5f + boxColliders[i].offset.y, 1000f)
                    };
                    boxCollidersLines[i].SetPositions(positions);
                }

                if (UnityEngine.Random.Range(0, 1) == 2) {
                    boxCollidersLines[i].positionCount = (CupheadLevelCamera.Current.ContainsPoint(
                        boxColliders[i].bounds.center,
                        new Vector2(boxColliders[i].bounds.center.x + boxColliders[i].bounds.extents.x,
                            boxColliders[i].bounds.center.y + boxColliders[i].bounds.extents.y))
                        ? 4
                        : 0);
                }
            }
        }
    }

    private void UpdateCircleColliders() {
        for (int i = 0; i < circleColliders.Length; i++) {
            circleCollidersLines[i].enabled = circleColliders[i].enabled;
            if (!circleCollidersLines[i].enabled) {
                continue;
            }

            Color color = circleCollidersLines[i].startColor;
            if (colorUpdate) {
                color = UpdateColor(circleColliders[i]);
            }

            color.a = hitboxTransparency;
            if (compParrySwitch != null && !compParrySwitch.enabled) {
                color.a = 0f;
            }

            circleCollidersLines[i].startColor = color;
            circleCollidersLines[i].endColor = color;
            if (lineUpdate) {
                Vector3[] array =
                    new Vector3[(int) circleColliders[i].radius /
                                ((Time.deltaTime > 1f / (float) Application.targetFrameRate * 1.1f) ? 6 : 3) +
                                5 * ((!(Map.Current != null)) ? 1 : 6)];
                for (int j = 1; j < array.Length + 1; j++) {
                    float f = (float) Math.PI / 180f * ((float) j * (360f / (float) array.Length));
                    array[j - 1] = new Vector3(circleColliders[i].offset.x + Mathf.Sin(f) * circleColliders[i].radius,
                        circleColliders[i].offset.y + Mathf.Cos(f) * circleColliders[i].radius, 0f);
                }

                circleCollidersLines[i].positionCount = array.Length;
                circleCollidersLines[i].SetPositions(array);
            }
        }
    }

    private void UpdatePolygonColliders() {
        for (int i = 0; i < polygonColliders.Length; i++) {
            polygonCollidersLines[i].enabled = polygonColliders[i].enabled;
            if (!polygonCollidersLines[i].enabled) {
                continue;
            }

            Color color = polygonCollidersLines[i].startColor;
            if (colorUpdate) {
                color = UpdateColor(polygonColliders[i]);
            }

            color.a = hitboxTransparency;
            polygonCollidersLines[i].startColor = color;
            polygonCollidersLines[i].endColor = color;
            if (lineUpdate) {
                Vector3[] array = new Vector3[polygonColliders[i].points.Length];
                for (int j = 0; j < array.Length; j++) {
                    array[j] = polygonColliders[i].points[j] + polygonColliders[i].offset;
                }

                polygonCollidersLines[i].SetPositions(array);
            }
        }
    }

    private void UpdateEdgeColliders() {
        for (int i = 0; i < edgeColliders.Length; i++) {
            edgeCollidersLines[i].enabled = edgeColliders[i].enabled;
            if (!edgeCollidersLines[i].enabled) {
                continue;
            }

            Color color = edgeCollidersLines[i].startColor;
            if (colorUpdate) {
                color = UpdateColor(edgeColliders[i]);
            }

            color.a = hitboxTransparency;
            edgeCollidersLines[i].startColor = color;
            edgeCollidersLines[i].endColor = color;
            if (lineUpdate) {
                Vector3[] array = new Vector3[edgeColliders[i].points.Length];
                for (int j = 0; j < array.Length; j++) {
                    array[j] = edgeColliders[i].points[j] + edgeColliders[i].offset;
                }

                edgeCollidersLines[i].SetPositions(array);
            }
        }
    }

    private void OnDestroy() {
        if (!(Level.Current != null) || Level.Current.CurrentLevel != 0) {
            return;
        }

        Collider2D[] array = FindObjectsOfType<Collider2D>();
        foreach (Collider2D collider2D in array) {
            if (!collider2D.gameObject.GetComponent<HitboxRenderer>()) {
                collider2D.gameObject.AddComponent<HitboxRenderer>();
            }
        }
    }

    private Color UpdateColor(Collider2D collider) {
        if (updateType == UpdateType.Parryable) {
            return UpdateColorParry(collider);
        }

        return UpdateColorDamageable(collider);
    }

    private void FindComponents() {
        try {
            compAbstractProjectile = gameObject.GetComponent<AbstractProjectile>();
        } catch (Exception) { }

        try {
            compAbstractCollidableObject = gameObject.GetComponent<AbstractCollidableObject>();
        } catch (Exception) { }

        try {
            compAbstractLevelEntity = gameObject.GetComponent<AbstractLevelEntity>();
        } catch (Exception) { }

        try {
            compAbstractParryEffect = gameObject.GetComponent<AbstractParryEffect>();
        } catch (Exception) { }

        try {
            compDamageReceiver = gameObject.GetComponent<DamageReceiver>();
        } catch (Exception) { }

        try {
            compDamageReceiverChild = gameObject.GetComponent<DamageReceiverChild>();
        } catch (Exception) { }

        try {
            compParrySwitch = gameObject.GetComponent<ParrySwitch>();
        } catch (Exception) { }

        try {
            compCollisionChild = gameObject.GetComponent<CollisionChild>();
        } catch (Exception) { }
    }

    private bool CanChangeColor() {
        if (!(gameObject.tag == "EnemyProjectile") && !(gameObject.GetComponent<ParrySwitch>() != null) &&
            !(gameObject.GetComponent<DamageReceiver>() != null) &&
            !(gameObject.GetComponent<DamageReceiverChild>() != null) &&
            !(gameObject.GetComponentInParent<TutorialLevelParryNext>() != null)) {
            return gameObject.GetComponent<TutorialShmupLevelParryNext>() != null;
        }

        return true;
    }

    private bool HitboxOther(Collider2D collider) {
        if ((!(collider.attachedRigidbody != null) || collider.attachedRigidbody.simulated) &&
            !collider.gameObject.GetComponent<MausoleumLevelUrn>() &&
            !collider.gameObject.GetComponent<HarbourPlatformingLevelOctoProjectile>() &&
            !collider.gameObject.GetComponent<FunhousePlatformingLevelMovingFloor>() &&
            !collider.gameObject.GetComponent<MountainPlatformingLevelScalePart>() &&
            !collider.gameObject.GetComponent<MountainPlatformingLevelFanBlow>() &&
            !collider.gameObject.GetComponent<CircusPlatformingLevelDunkPlatform>() &&
            !collider.gameObject.GetComponent<PlatformingLevelJumpTrigger>() &&
            !collider.gameObject.GetComponent<FlyingMermaidLevelSplashManager>() &&
            !collider.gameObject.GetComponent<FlyingMermaidLevelEelSegment>() &&
            !collider.gameObject.GetComponent<FlyingMermaidLevelSplashManager>() &&
            !collider.gameObject.GetComponent<TreePlatformingLevelMosquito>() &&
            !collider.gameObject.GetComponent<CircusPlatformingLevelTrampoline>() &&
            !collider.gameObject.GetComponent<DevilLevelHole>() &&
            !collider.gameObject.GetComponent<BeeLevelSecurityGuardBomb>()) {
            return collider.gameObject.GetComponent<FlyingBlimpLevelGeminiShoot>();
        }

        return true;
    }

    private bool HitboxPlayerAttack(Collider2D collider) {
        if (!(collider.gameObject.GetComponentInParent<AbstractPlaneSuper>() != null) &&
            !collider.gameObject.GetComponentInParent<AbstractPlayerSuper>()) {
            if ((collider.gameObject.GetComponent<LevelPlayerParryEffect>() != null ||
                 collider.gameObject.GetComponent<PlanePlayerParryEffect>() != null) &&
                collider.gameObject.GetComponentInParent<AbstractPlayerController>().stats.Loadout.charm ==
                Charm.charm_parry_attack) {
                if (collider.gameObject.GetComponentInParent<AbstractPlayerController>().GetComponent<IParryAttack>()
                    .AttackParryUsed) {
                    return Level.Current.playerMode == PlayerMode.Plane;
                }

                return true;
            }

            return false;
        }

        return true;
    }

    private bool HitboxPlayer() {
        if (!gameObject.GetComponent<PlayerInput>()) {
            if ((bool) gameObject.GetComponent<DamageReceiver>()) {
                return gameObject.GetComponent<DamageReceiver>().type == DamageReceiver.Type.Player;
            }

            return false;
        }

        return true;
    }

    private bool HitboxParryElement() {
        if ((!gameObject.GetComponent<AbstractLevelEntity>() ||
             !gameObject.GetComponent<AbstractLevelEntity>().canParry) &&
            !gameObject.GetComponent<AbstractParryEffect>() &&
            (!gameObject.GetComponent<AbstractProjectile>() ||
             !gameObject.GetComponent<AbstractProjectile>().CanParry)) {
            return gameObject.GetComponent<ParrySwitch>();
        }

        return true;
    }

    private bool HitboxParryEnemy() {
        if (gameObject.tag == "Enemy" || gameObject.GetComponent<Collider2D>().attachedRigidbody != null ||
            (transform.parent != null && transform.parent.gameObject.tag == "Enemy") ||
            ((bool) gameObject.GetComponent<AbstractCollidableObject>() &&
             gameObject.GetComponent<AbstractCollidableObject>().allowCollisionPlayer &&
             (gameObject.tag == "EnemyProjectile" || gameObject.tag == "Enemy"))) {
            return !gameObject.GetComponent<AbstractParryEffect>();
        }

        return false;
    }

    private bool HitboxDamageableEnemy(Collider2D collider) {
        if ((bool) gameObject.GetComponent<DamageReceiver>() && collider.attachedRigidbody != null &&
            gameObject.GetComponent<DamageReceiver>().type == DamageReceiver.Type.Enemy) {
            return gameObject.GetComponent<DamageReceiver>().enabled;
        }

        return false;
    }

    private bool HitboxDamageableEnemyChildComponent(Collider2D collider) {
        if (collider.attachedRigidbody != null && (bool) collider.gameObject.GetComponent<DamageReceiverChild>() &&
            collider.gameObject.GetComponent<DamageReceiverChild>().enabled &&
            collider.gameObject.GetComponent<DamageReceiverChild>().Receiver.enabled) {
            return collider.gameObject.GetComponent<DamageReceiverChild>().Receiver.type == DamageReceiver.Type.Enemy;
        }

        return false;
    }

    private bool HitboxPlatformElement() {
        if (!gameObject.GetComponent<LevelPlatform>() &&
            !gameObject.GetComponent<PlatformingLevelEditorPlatform>() && !(gameObject.tag == "Wall") &&
            !(gameObject.tag == "Ground") && !(gameObject.tag == "Ceiling") && gameObject.layer != 18 &&
            gameObject.layer != 19) {
            return gameObject.layer == 20;
        }

        return true;
    }

    private bool HitboxBulletBlocker(Collider2D collider) {
        if (gameObject.tag != "EnemyProjectile" && collider.attachedRigidbody != null &&
            ((collider.attachedRigidbody.gameObject.tag != "Wall" &&
              collider.attachedRigidbody.gameObject.tag != "Ceiling" &&
              collider.attachedRigidbody.gameObject.tag != "Ground" &&
              collider.attachedRigidbody.gameObject.tag != "Enemy" &&
              collider.attachedRigidbody.gameObject.tag != "EnemyProjectile" &&
              collider.attachedRigidbody.gameObject.tag != "Player" &&
              collider.attachedRigidbody.gameObject.tag != "PlayerProjectile" &&
              collider.attachedRigidbody.gameObject.layer == 0) ||
             (collider.attachedRigidbody.gameObject.tag != "EnemyProjectile" &&
              collider.attachedRigidbody.gameObject.layer == 12) ||
             (collider.attachedRigidbody.gameObject.tag == "Enemy" &&
              collider.attachedRigidbody.gameObject.layer != 13) ||
             (collider.attachedRigidbody.gameObject.transform.parent != null &&
              collider.attachedRigidbody.gameObject.transform.parent.gameObject.tag == "Enemy" &&
              collider.attachedRigidbody.gameObject.transform.parent.gameObject.layer != 13)) &&
            collider.gameObject.layer != 13) {
            if (Level.Current.playerMode == PlayerMode.Plane) {
                if (Level.Current.playerMode == PlayerMode.Plane) {
                    if (!collider.isTrigger &&
                        collider.attachedRigidbody.constraints != RigidbodyConstraints2D.FreezeRotation) {
                        return collider.attachedRigidbody.constraints == RigidbodyConstraints2D.FreezeAll;
                    }

                    return true;
                }

                return false;
            }

            return true;
        }

        return false;
    }

    private bool HitboxOtherBulletBlocker(Collider2D collider) {
        if (collider.attachedRigidbody != null &&
            ((collider.gameObject.tag != "Wall" && collider.gameObject.tag != "Ceiling" &&
              collider.gameObject.tag != "Ground" && collider.gameObject.tag != "Enemy" &&
              collider.gameObject.tag != "EnemyProjectile" && collider.gameObject.tag != "Player" &&
              collider.gameObject.tag != "PlayerProjectile" && gameObject.layer == 0) ||
             (collider.gameObject.tag != "EnemyProjectile" && collider.gameObject.layer == 12) ||
             (collider.gameObject.tag == "Enemy" && collider.gameObject.layer != 13) ||
             (collider.gameObject.transform.parent != null &&
              collider.gameObject.transform.parent.gameObject.tag == "Enemy" &&
              collider.gameObject.transform.parent.gameObject.layer != 13))) {
            if (Level.Current.playerMode == PlayerMode.Plane) {
                if (Level.Current.playerMode == PlayerMode.Plane) {
                    if (!collider.isTrigger &&
                        collider.attachedRigidbody.constraints != RigidbodyConstraints2D.FreezeRotation) {
                        return collider.attachedRigidbody.constraints == RigidbodyConstraints2D.FreezeAll;
                    }

                    return true;
                }

                return false;
            }

            return true;
        }

        return false;
    }

    private bool HitboxPlayerHurting() {
        if (!(tag == "EnemyProjectile") &&
            (!gameObject.GetComponent<CollisionChild>() ||
             !(gameObject.GetComponent<CollisionChild>().collisionParent != null) ||
             !gameObject.GetComponent<CollisionChild>().collisionParent.allowCollisionPlayer) &&
            (!gameObject.GetComponent<AbstractCollidableObject>() ||
             !gameObject.GetComponent<AbstractCollidableObject>().allowCollisionPlayer)) {
            if ((bool) gameObject.GetComponent<AbstractProjectile>()) {
                return gameObject.GetComponent<AbstractProjectile>().DamagesType.Player;
            }

            return false;
        }

        return true;
    }

    private bool HitboxMapElementOther() {
        if (!(gameObject.GetComponent<MapLayerChanger>() != null) &&
            !(gameObject.GetComponent<MapZoomOut>() != null) &&
            !(gameObject.GetComponent<ChangeLightingZone>() != null) &&
            (!(transform.parent != null) || !(gameObject.GetComponentInParent<MapNPCTurtle>() != null) ||
             !(gameObject.GetComponent<MapNPCTurtle>() == null))) {
            return gameObject.GetComponent<MapSpritePlaySound>() != null;
        }

        return true;
    }

    private bool HitboxMapElement() {
        if (gameObject.name != "Colliders" &&
            (!(transform.parent != null) ||
             !(transform.GetComponentInParent<MapDialogueInteraction>() != null) &&
             transform.parent.gameObject.name != "Colliders") &&
            !(gameObject.GetComponent<MapDialogueInteraction>() != null) &&
            !(gameObject.GetComponent<MapLevelLoader>() != null) &&
            !(gameObject.GetComponent<MapFlagpole>() != null) &&
            !(gameObject.GetComponent<MapShopLoader>() != null) &&
            !(gameObject.GetComponent<MapDiceGateSceneLoader>() != null) &&
            !(gameObject.GetComponent<MapTutorialLoader>() != null) &&
            (!(transform.parent != null) ||
             !(gameObject.GetComponentInParent<MapLevelDependentObstacle>() != null)) &&
            !(gameObject.GetComponent<MapSceneLoader>() != null)) {
            return gameObject.GetComponent<MapLevelMausoleumEntity>() != null;
        }

        return true;
    }

    private Color UpdateColorParry(Collider2D collider) {
        if (compAbstractLevelEntity != null && compAbstractLevelEntity.canParry || compAbstractParryEffect != null ||
            compAbstractProjectile != null && compAbstractProjectile.CanParry ||
#if v1_0
            compParrySwitch != null) {
#else
            compParrySwitch != null && compParrySwitch.IsParryable) {
#endif
            if ((gameObject.tag == "Enemy" || collider.attachedRigidbody != null ||
                 (compAbstractCollidableObject != null && compAbstractCollidableObject.allowCollisionPlayer &&
                  (gameObject.tag == "EnemyProjectile" || gameObject.tag == "Enemy"))) &&
                compAbstractParryEffect == null) {
                return Colors.ParryEnemy;
            }

            return Colors.Parry;
        }

        if (compDamageReceiver != null || compDamageReceiverChild != null) {
            return UpdateColorDamageable(collider);
        }

        if (HitboxOtherBulletBlocker(collider) && collider.gameObject.layer != 12) {
            if (nonHurting) {
                return Colors.UndefinedSolid;
            }

            return Colors.HurtSolid;
        }

        if (tag == "EnemyProjectile" ||
            (compCollisionChild != null && compCollisionChild.collisionParent != null &&
             compCollisionChild.collisionParent.allowCollisionPlayer) || (compAbstractCollidableObject != null &&
                                                                          compAbstractCollidableObject
                                                                              .allowCollisionPlayer)) {
            return Colors.Hurt;
        }

        if (collider.gameObject.layer == 13) {
            return Colors.Hurt;
        }

        return Colors.Parry;
    }

    private Color UpdateColorDamageable(Collider2D collider) {
        if ((compAbstractLevelEntity != null && compAbstractLevelEntity.canParry) || compAbstractParryEffect != null ||
            (compAbstractProjectile != null && compAbstractProjectile.CanParry) || compParrySwitch != null) {
            updateType = UpdateType.Parryable;
            return UpdateColorParry(collider);
        }

        if (compDamageReceiver != null && collider.attachedRigidbody != null &&
            compDamageReceiver.type == DamageReceiver.Type.Enemy && compDamageReceiver.enabled) {
            return Colors.Damageable;
        }

        if (collider.attachedRigidbody != null && compDamageReceiverChild != null && compDamageReceiverChild.enabled &&
            compDamageReceiverChild.Receiver.enabled &&
            compDamageReceiverChild.Receiver.type == DamageReceiver.Type.Enemy) {
            return Colors.Damageable;
        }

        if (collider.attachedRigidbody != null) {
            if (nonHurting) {
                return Colors.UndefinedSolid;
            }

            return Colors.HurtSolid;
        }

        if (nonHurting) {
            return Colors.Undefined;
        }

        return Colors.Hurt;
    }

    private IEnumerator waitUntilUpdate_cr(float time) {
        yield return CupheadTime.WaitForSeconds(this, time);
        updateLinesOnce = true;
    }

    static HitboxRenderer() {
        MAX_UPDATE_DISTANCE = 1300f;
        renderLayer = SpriteLayer.Top;
        show = true;
    }

    private Color GetColor(Collider2D collider) {
        if (collider.gameObject.tag == "PlayerProjectile" && collider.transform.parent == null) {
            return Colors.PlayerAttack;
        }

        if (collider.gameObject.GetComponent<LevelPlayerWeaponFiringHitbox>() != null) {
            lineUpdate = false;
            return Colors.PlayerAttack;
        }

        if (HitboxPlayer()) {
            colorUpdate = false;
            return Colors.Player;
        }

        if (Map.Current != null) {
            lineUpdate = false;
            if (gameObject.GetComponent<MapSecretAchievementUnlocker>() != null ||
                gameObject.GetComponent<MapCoin>() != null) {
                return Colors.Secret;
            }

            if (HitboxMapElementOther()) {
                hitboxTransparency = 0.375f;
                return Colors.Other;
            }

            return Colors.PlatformSolid;
        }

        if (HitboxPlayerAttack(collider)) {
            return Colors.PlayerAttack;
        }

        if ((bool) collider.gameObject.GetComponent<FlyingMermaidLevelLaser>()) {
            colorUpdate = false;
            return Colors.Petrify;
        }

        if (HitboxOther(collider)) {
            hitboxTransparency =
                ((collider.attachedRigidbody != null && collider.gameObject.layer != 11) ? 0.75f : 0.375f);
            colorUpdate = false;
            return Colors.Other;
        }

        if (HitboxParryElement()) {
            updateType = UpdateType.Parryable;
            if (gameObject.GetComponent<ParrySwitch>() != null &&
                !gameObject.GetComponent<ParrySwitch>().enabled) {
                return Color.clear;
            }

            if (HitboxParryEnemy()) {
                return Colors.ParryEnemy;
            }

            return Colors.Parry;
        }

        if (collider.gameObject.GetComponentInParent<HarbourPlatformingLevelOctopus>() != null &&
            collider.gameObject.GetComponent<CollisionChild>() != null) {
            colorUpdate = false;
            lineUpdate = false;
            return Colors.Other;
        }

        if (HitboxDamageableEnemy(collider)) {
            updateType = UpdateType.Damageable;
            return Colors.Damageable;
        }

        if (HitboxDamageableEnemyChildComponent(collider)) {
            updateType = UpdateType.Damageable;
            return Colors.Damageable;
        }

        if (HitboxPlatformElement()) {
            isPlatformElement = true;
            lineUpdate = false;
            colorUpdate = false;
            hitboxTransparency = ((collider.attachedRigidbody != null) ? 1f : 0.65f);
            if (collider.gameObject.GetComponent<BasicDamageDealingObject>() != null) {
                return Colors.HurtSolid;
            }

            if (Level.Current.playerMode == PlayerMode.Plane && collider.gameObject.layer != 19 &&
                collider.gameObject.layer != 20) {
                hitboxTransparency = 0.375f;
                return Colors.Other;
            }

            if ((bool) gameObject.GetComponent<LevelPlatform>() &&
                gameObject.GetComponent<LevelPlatform>().canFallThrough) {
                return Colors.PlatformSemiSolid;
            }

            return collider.gameObject.layer switch {
                18 => Colors.PlatformSolidWall,
                19 => Colors.PlatformSolidCeiling,
                20 => Colors.PlatformSolidGround,
                _ => Colors.PlatformSolid,
            };
        }

        if (HitboxBulletBlocker(collider)) {
            return Colors.HurtSolid;
        }

        if (HitboxPlayerHurting()) {
            if (collider.gameObject.layer == 13) {
                return Colors.Hurt;
            }

            if (HitboxOtherBulletBlocker(collider)) {
                return Colors.HurtSolid;
            }

            return Colors.Hurt;
        }

        if (collider.attachedRigidbody != null) {
            return Colors.UndefinedSolid;
        }

        return Colors.Undefined;
    }

    public static void Setup() {
#if v1_0
        renderLayer = SpriteLayer.Top;
#else
        renderLayer = Map.Current != null ? SpriteLayer.ForegroundEffects : SpriteLayer.Top;
#endif
        Collider2D[] array = FindObjectsOfType<Collider2D>();
        foreach (Collider2D collider2D in array) {
            if (collider2D.gameObject.GetComponent<HitboxRenderer>() == null) {
                collider2D.gameObject.AddComponent<HitboxRenderer>();
            }
        }

        if (camera == null) {
            camera = FindObjectOfType<AbstractCupheadGameCamera>();
        }

        if (blackScreen == null && ShopScene.Current == null && (Level.Current != null || Map.Current != null)) {
            blackScreen = new GameObject().AddComponent<LineRenderer>();
            blackScreen.gameObject.transform.SetParent(camera.gameObject.transform, worldPositionStays: false);
            float num = 2000f / ((Map.Current != null) ? 100f : 1f);
            Vector3[] array2 = new Vector3[2] {
                new Vector3(0f - num, 0f, 0f),
                new Vector3(num, 0f, 0f)
            };
            blackScreen.material = new Material(Shader.Find("Sprites/Default"));
            blackScreen.positionCount = array2.Length;
            blackScreen.SetPositions(array2);
            Color black = Color.black;
            black.a = 0.75f;
            blackScreen.startColor = black;
            blackScreen.endColor = black;
            blackScreen.startWidth = num;
            blackScreen.endWidth = num;
            blackScreen.sortingLayerName = renderLayer.ToString();
            blackScreen.sortingOrder = 10000;
            blackScreen.material.renderQueue = 5000;
            blackScreen.useWorldSpace = false;
            blackScreen.loop = false;
            blackScreen.enabled = obscureScreen && show;
        }
    }

    private void OnEnable() {
        if (updateOnEnable) {
            updateColorOnce = true;
            updateLinesOnce = true;
        }
    }

    public void SetEnabled(bool enabled) {
        LineRenderer[] array = boxCollidersLines;
        for (int i = 0; i < array.Length; i++) {
            array[i].enabled = enabled;
        }

        array = circleCollidersLines;
        for (int j = 0; j < array.Length; j++) {
            array[j].enabled = enabled;
        }

        array = edgeCollidersLines;
        for (int k = 0; k < array.Length; k++) {
            array[k].enabled = enabled;
        }

        array = polygonCollidersLines;
        for (int l = 0; l < array.Length; l++) {
            array[l].enabled = enabled;
        }

        this.enabled = enabled;
    }
}