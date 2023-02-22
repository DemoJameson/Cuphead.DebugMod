using BepInEx.CupheadDebugMod.Config;
using UnityEngine;

namespace BepInEx.CupheadDebugMod.Components;

public class DebugInfo : PluginComponent {
    private static Level CurrentLevel => Level.Current;

    private Texture2D backgroundTexture;
    private GUIStyle textureStyle;
    private bool guiShowAll;
    private int guiPanelState;
    private float pdPercentage;
    private int pdDeaths1;
    private int pdDeaths2;
    private int pdCoins;
    public Vector3 guiScale;
    public float levelRealTime;
    public bool previousFrameWon;

    private void Awake() {
        GUISetMatrix();
        Init();
        InitHotkeys();
        HookHelper.ActiveSceneChanged(RefPercentage);
    }

    private void GUISetMatrix() {
        guiScale.x = Screen.width / 1920f;
        guiScale.y = Screen.height / 1080f;
        guiScale.z = 1f;
    }
    
    private void Init() {
        guiShowAll = false;
        guiPanelState = 2;
        pdPercentage = 0f;
        pdDeaths1 = 0;
        pdDeaths2 = 0;
        pdCoins = 0;

        if (backgroundTexture == null) {
            backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, Color.white);
            backgroundTexture.Apply();
        }

        if (textureStyle == null) {
            textureStyle = new GUIStyle();
            textureStyle.normal.background = backgroundTexture;
        }
    }

    private void InitHotkeys() {
        Settings.OnKeyUpdate += () => {
            if (Settings.ToggleAllPanels.IsDownEx()) {
                guiShowAll = !guiShowAll;
            }

            if (Settings.ToggleBetweenPanels.IsDownEx()) {
                GUISwapPanels();
            }
        };
    }

    private void DrawRect(Rect position, Color color) {
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUI.Box(position, GUIContent.none, textureStyle);
        GUI.backgroundColor = backgroundColor;
    }

    private void OnGUI() {
        GUI.depth = 1;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, guiScale);
        GUISetDefColors();
        if (CurrentLevel != null && CurrentLevel.type is Level.Type.Battle or Level.Type.Platforming) {
            if (!guiShowAll) {
                GUISetDefElements();
                GUILayout.BeginArea(new Rect(10f, 52f, 300f, 600f));
                GUILayout.BeginVertical("box");
                GUILayout.Label("Cuphead Debug Mod, RNG fork ALPHA 3");
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndArea();
                return;
            }

            if (guiPanelState == 0) {
                GUISetDefElements();
                GUI.skin.label.fontSize = 20;
                try {
                    GUI.Label(new Rect(10f, 10f, 300f, 30f), "Press " + Settings.ToggleBetweenPanels.Value.ToString() + " to toggle elements");
                }
                // idk if this is actually necessary
                catch {
                    GUI.Label(new Rect(10f, 10f, 300f, 30f), "Press ? to toggle elements");
                }

            }

            if (guiPanelState > 0 && CurrentLevel.type == Level.Type.Battle && CurrentLevel.timeline.health > 0f) {
                GUISetDefElements();
                float num = 200f;
                float num2 = 40f;
                float num3 = CurrentLevel.timeline.health - CurrentLevel.timeline.damage;
                float num4 = num * (num3 / CurrentLevel.timeline.health);
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                GUI.Label(new Rect(10f, 10f, 100f, 40f), "<size=30><b>BOSS:</b></size>");
                DrawRect(new Rect(109f, 9f, num + 2f, num2 + 2f), Color.gray);
                DrawRect(new Rect(110f, 10f, num, num2), Color.black);
                if (CurrentLevel.timeline.damage < CurrentLevel.timeline.health) {
                    DrawRect(new Rect(110f, 10f, num4, num2), Color.red);
                }

                for (int num5 = CurrentLevel.timeline.events.Count - 1; num5 >= 0; num5--) {
                    Color color = Color.black;
                    float num6 = num * CurrentLevel.timeline.events[num5].percentage;
                    if (num4 < num6) {
                        color = Color.gray;
                    }

                    DrawRect(new Rect(110f + (num6 - 2f), 10f, 2f, num2), color);
                }

                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.skin.label.fontSize = 28;
                GUI.Label(new Rect(110f, 10f, num, num2), num3.ToString("F2") + "/" + CurrentLevel.timeline.health);
                GUISetDefElements();
            }

            // TODO: need to come up with a better way of not leaving an empty space on run n guns, this is going to mess with people's settings
            if (guiPanelState == 1 && CurrentLevel.type != Level.Type.Battle) {
                guiPanelState++;
            }


            if (guiPanelState == 2) {
                GUISetDefElements();
                GUILayout.BeginArea(new Rect(10f, 52f, 300f, 600f));
                GUILayout.BeginVertical("box");
                GUILayout.Label("[LEVEL DATA]");
                GUILayout.Label("RTA: " + levelRealTime.ToString("F2") + "s");
                GUILayout.Label("IGT: " + CurrentLevel.LevelTime.ToString("F2") + "s");
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndArea();
            }

            if (guiPanelState > 2) {
                GUISetDefElements();
                GUILayout.BeginArea(new Rect(10f, 52f, 300f, 600f));
                GUILayout.BeginVertical("box");
                GUILayout.Label("[LEVEL DATA]");
                GUILayout.Label("Time: " + CurrentLevel.LevelTime.ToString("F2") + "s");
                GUILayout.Label("Goal: " + Level.ScoringData.goalTime + "s");
                GUILayout.Label("Parries: " + Level.ScoringData.numParries);
                GUILayout.Label("Supers: " + Level.ScoringData.superMeterUsed);
                GUILayout.Label("Damage: " + Level.ScoringData.numTimesHit);
                GUILayout.Label(
                    "CurrRank: " + Level.PreviousGrade.ToString().Replace("Minus", "-").Replace("Plus", "+"));
                if (CurrentLevel.type == Level.Type.Platforming) {
                    GUILayout.Label("Pacifist?: " + Level.ScoringData.pacifistRun);
                }

                GUILayout.Label("Difficulty: " + Level.ScoringData.difficulty);
                GUILayout.Label("DmgMultiplier: " + PlayerManager.DamageMultiplier);
                GUILayout.Label("Player Count: " + PlayerManager.Count);
                GUILayout.Label("Current scene: " + CurrentSceneName);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndArea();
                GUISetDefElements();
            }
        }

        if (IsMapScene(CurrentSceneName)) {
            GUISetDefElements();
            GUILayout.BeginArea(new Rect(10f, 52f, 300f, 620f));
            GUILayout.BeginVertical("box");
            GUILayout.Label("[GLOBAL DATA]");
            GUILayout.Label("Deaths P1: " + pdDeaths1);
            if (PlayerManager.Multiplayer) {
                GUILayout.Label("Deaths P2: " + pdDeaths2);
            }

            GUILayout.Label("Multiplayer?: " + PlayerManager.Multiplayer);
            GUILayout.Label("Coins count: " + pdCoins);
            GUILayout.Label("Percentage: " + pdPercentage + "%");
            GUILayout.Label("Savegame slot: " + PlayerData.CurrentSaveFileIndex);
            GUILayout.Label("Vsync mode: " + QualitySettings.vSyncCount);
            GUILayout.Label("Current scene: " + CurrentSceneName);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
            GUISetDefElements();
        }
    }

    private void Update() {
        if (!previousFrameWon) {
            levelRealTime += Time.deltaTime;
        }

        previousFrameWon = Level.Won;

        if (CurrentLevel?.LevelTime == 0f) {
            levelRealTime = 0f;
        }
    }

    private void GUISetDefElements() {
        GUI.skin.label.fontSize = 24;
        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
    }

    private void GUISetDefColors() {
        GUI.backgroundColor = Color.white;
        GUI.contentColor = Color.white;
        GUI.color = Color.white;
    }

    private void GUISwapPanels() {
        if (guiPanelState + 1 < 4) {
            guiPanelState++;
        } else {
            guiPanelState = 0;
        }
    }

    private bool IsMapScene(string sceneName) {
        return sceneName.StartsWith("scene_map_");
    }

    private void RefPercentage() {
        pdPercentage = PlayerData.Data.GetCompletionPercentage();
        pdDeaths1 = PlayerData.Data.DeathCount(PlayerId.PlayerOne);
        pdDeaths2 = PlayerData.Data.DeathCount(PlayerId.PlayerTwo);
        pdCoins = PlayerData.Data.coinManager.NumCoinsCollected();
    }
}