using System.Collections.Generic;
using BepInEx.CupheadDebugMod.Config;
using UnityEngine;
using static PlayerData.PlayerLoadouts;

namespace BepInEx.CupheadDebugMod.Components;

public class DebugInfo : PluginComponent {
    private static Level CurrentLevel => Level.Current;

    private Texture2D backgroundTexture;
    private GUIStyle textureStyle;
    private int guiPanelState;
    private float pdPercentage;
    private int pdDeaths1;
    private int pdDeaths2;
    private int pdCoins;
    private bool hasExtraIGTTickBeenHandledKingDice;
    public Vector3 guiScale;
    public static int winScreenFrameCounter;
    public static int onWinScreenButtonPressFrame;
    public static int onWinScreenStarFrame;
    public static int winScreenStarSkipFrameOffset;
    public static int[] winScreenStarSkipFrameList;
    public static int spriteSwapLevelFrameCounter;
    public static int spriteSwapOnSpriteSwapFrameCounter;
    public static int spriteSwapOnLobberEXFrameCounter;
    public static int spriteSwapQuadFrameOffset;
    public static float levelRealTime;
    public static float levelInGameTime;
    public static float levelRealTimeKingDice;
    public static float levelInGameTimeKingDice;
    public static bool isMinibossStartingKingDice;
    public static bool previousFrameWon;
    public List<string> planeLevelNames = new() {
        "scene_level_flying_blimp",
        "scene_level_flying_bird",
        "scene_level_flying_genie",
        "scene_level_robot",
        "scene_level_flying_mermaid",
        "scene_level_flying_cowboy",
        "scene_level_dice_palace_flying_horse",
        "scene_level_dice_palace_flying_memory"
    };

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

            if (guiPanelState == 0) {
                GUISetDefElements();
                GUILayout.BeginArea(new Rect(10f, 52f, 300f, 600f));
                GUILayout.BeginVertical("box");
                GUILayout.Label("Cuphead Debug Mod");
                GUILayout.Label("Mod Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndArea();

            }

            // add health bar if available
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

            // to avoid having an ampty GUI on run n guns, this increments the panel state to the next valid one
            // this means that if someone comes from a boss fight with guiPanelState == 1, then plays a run n gun, it will get auto-incremeneted to 2
            // which is a minor inconvenience, but i think it's a fine solution to avoid showing an empty space 
            if (guiPanelState == 1 && CurrentLevel.type != Level.Type.Battle) {
                guiPanelState++;
            }

            if (guiPanelState == 2) {
                GUISetDefElements();
                GUILayout.BeginArea(new Rect(10f, 52f, 300f, 600f));
                GUILayout.BeginVertical("box");
                GUILayout.Label("[LEVEL DATA]");
                if (Settings.RTATime.Value) {
                    GUILayout.Label("RTA: " + levelRealTime.ToString("F2") + "s");
                    if (CurrentSceneName.StartsWith("scene_level_dice_palace")) {
                        GUILayout.Label("RTA Total: " + levelRealTimeKingDice.ToString("F2") + "s");
                    }
                }
                if (Settings.IGTTime.Value) {
                    GUILayout.Label("IGT: " + levelInGameTime.ToString("F2") + "s");
                    if (CurrentSceneName.StartsWith("scene_level_dice_palace")) {
                        GUILayout.Label("IGT Total: " + levelInGameTimeKingDice.ToString("F2") + "s");
                    }
                }
                if (Settings.WeaponCooldowns.Value) {

                    string weaponInfo = $"";
                    if (planeLevelNames.Contains(CurrentSceneName)) {
#if v1_3
                        if (GameInfoHelper.isChalice) {
                            weaponInfo += $"Peashooter: {GameInfoHelper.weaponsTimer[Weapon.plane_chalice_weapon_3way]} Bomb: {GameInfoHelper.weaponsTimer[Weapon.plane_chalice_weapon_bomb]}";
                        }
                        else {
#endif
                            weaponInfo += $"Peashooter: {GameInfoHelper.weaponsTimer[Weapon.plane_weapon_peashot]} Bomb: {GameInfoHelper.weaponsTimer[Weapon.plane_weapon_bomb]}";
#if v1_3
                    }
#endif
                    } else {
                        PlayerLoadout loadOut = PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne);
                      
                        if (GameInfoHelper.weaponsTimer.TryGetValue(loadOut.primaryWeapon, out int frame1)) {
                            weaponInfo += $"{GameInfoHelper.GetWeaponName(loadOut.primaryWeapon)}: {frame1}";
                            if (loadOut.primaryWeapon == Weapon.level_weapon_charge) {
                                weaponInfo += $" {GameInfoHelper.chargeFrames}";
                            }
                        }

                        if (GameInfoHelper.weaponsTimer.TryGetValue(loadOut.secondaryWeapon, out int frame2)) {

                            weaponInfo += $" {GameInfoHelper.GetWeaponName(loadOut.secondaryWeapon)}: {frame2}";

                            if (loadOut.secondaryWeapon == Weapon.level_weapon_charge) {
                                weaponInfo += $" {GameInfoHelper.chargeFrames}";
                            }
                        }
                    }
                    GUILayout.Label(weaponInfo);
                }
                if (Settings.OnEXWeaponCooldown.Value) {
                    GUILayout.Label(GameInfoHelper.onEXWeaponName + ": " + GameInfoHelper.onEXWeaponCooldown.ToString());
                }

                if (Settings.Goal.Value) {
                    GUILayout.Label("Goal: " + Level.ScoringData.goalTime + "s");
                }
                if (Settings.Parries.Value) {
                    GUILayout.Label("Parries: " + Level.ScoringData.numParries);
                }
                if (Settings.Supers.Value) {
                    GUILayout.Label("Supers: " + Level.ScoringData.superMeterUsed);
                }
                if (Settings.Damage.Value) {
                    GUILayout.Label("Damage: " + Level.ScoringData.numTimesHit);
                }
                if (Settings.CurrentRank.Value) {
                    GUILayout.Label("CurrRank: " + Level.PreviousGrade.ToString().Replace("Minus", "-").Replace("Plus", "+"));
                    if (CurrentLevel.type == Level.Type.Platforming) {
                        GUILayout.Label("Pacifist?: " + Level.ScoringData.pacifistRun);
                    }
                }
                if (Settings.Difficulty.Value) {
                    GUILayout.Label("Difficulty: " + Level.ScoringData.difficulty);
                }
                if (Settings.DmgMultiplier.Value) {
                    GUILayout.Label("DmgMultiplier: " + PlayerManager.DamageMultiplier);
                }
                if (Settings.PlayerCount.Value) {
                    GUILayout.Label("Player Count: " + PlayerManager.Count);
                }
                if (Settings.CurrentScene.Value) {
                    GUILayout.Label("Current scene: " + CurrentSceneName);
                }
                if (Settings.QuadEXOffset.Value) {
                    if (CurrentSceneName.Equals("scene_level_slime") || CurrentSceneName.Equals("scene_level_mouse")) {
                        if (spriteSwapQuadFrameOffset < -7) {
                            GUILayout.Label("EARLY by " + (-6 - spriteSwapQuadFrameOffset).ToString() + " frames");
                        }
                        if (spriteSwapQuadFrameOffset == -7) {
                            GUILayout.Label("EARLY by " + (-6 - spriteSwapQuadFrameOffset).ToString() + " frame");
                        }
                        if (spriteSwapQuadFrameOffset > -7 && spriteSwapQuadFrameOffset < 0) {
                            GUILayout.Label("HIT on frame " + (spriteSwapQuadFrameOffset + 7).ToString() + " out of 6");
                        }
                        if (spriteSwapQuadFrameOffset == 0) {
                            GUILayout.Label("LATE by " + (1 + spriteSwapQuadFrameOffset).ToString() + " frame");
                        }
                        if (spriteSwapQuadFrameOffset > 0) {
                            GUILayout.Label("LATE by " + (1 + spriteSwapQuadFrameOffset).ToString() + " frames");
                        }
                    }
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndArea();
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

        if (IsWinScreen(CurrentSceneName)) {
            GUISetDefElements();
            GUILayout.BeginArea(new Rect(10f, 52f, 300f, 620f));
            GUILayout.BeginVertical("box");
            GUILayout.Label("[Star Skip Info]");
            GUILayout.Label("Star skip offset: " + winScreenStarSkipFrameOffset.ToString());
            GUILayout.Label("Star skip plink: " + (winScreenStarSkipFrameList[1] - winScreenStarSkipFrameList[0]).ToString());
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
            GUISetDefElements();
        }
    }

    private void Update() {
        if (!previousFrameWon) {
            if (CurrentSceneName.StartsWith("scene_level_dice_palace")) {
                if (!isMinibossStartingKingDice) {
                    levelRealTime += Time.deltaTime;
                }
            }
            else if (CurrentLevel != null) {
                levelRealTime += Time.deltaTime;
                levelInGameTime = CurrentLevel.LevelTime;
            }

        }
        else {
            isMinibossStartingKingDice = false;
            hasExtraIGTTickBeenHandledKingDice = false;
        }

        if (!Level.Won) {
            if (CurrentSceneName.StartsWith("scene_level_dice_palace")) {
                if (!isMinibossStartingKingDice) {
                    levelInGameTime = CurrentLevel.LevelTime;
                } else if (hasExtraIGTTickBeenHandledKingDice == false) {
                    levelInGameTime = CurrentLevel.LevelTime;
                    levelInGameTimeKingDice = Level.ScoringData.time;
                    hasExtraIGTTickBeenHandledKingDice = true;
                }
            }
        }

        if (CurrentSceneName.StartsWith("scene_level_dice_palace")) {
            if (!isMinibossStartingKingDice && !previousFrameWon) {
                levelRealTimeKingDice += Time.deltaTime;
            }
            if (!isMinibossStartingKingDice && !Level.Won) {
                levelInGameTimeKingDice = Level.ScoringData.time + CurrentLevel.LevelTime;
            }
        }

        if (IsWinScreen(CurrentSceneName)) {
            CupheadInput.AnyPlayerInput controllerInput = ((WinScreen)Object.FindObjectOfType(typeof(WinScreen))).input;


            winScreenFrameCounter++;
            if (controllerInput.GetButtonDown(CupheadButton.Accept)) {
                onWinScreenButtonPressFrame = winScreenFrameCounter;
                winScreenStarSkipFrameList[0] = winScreenStarSkipFrameList[1];
                winScreenStarSkipFrameList[1] = winScreenFrameCounter;
            }
            if (onWinScreenButtonPressFrame != 0 && onWinScreenStarFrame != 0) {
                winScreenStarSkipFrameOffset = onWinScreenButtonPressFrame - onWinScreenStarFrame;
            }
        }

        if (spriteSwapOnLobberEXFrameCounter != 0 && spriteSwapOnSpriteSwapFrameCounter != 0) {
            spriteSwapQuadFrameOffset = spriteSwapOnLobberEXFrameCounter - spriteSwapOnSpriteSwapFrameCounter;
        }
        spriteSwapLevelFrameCounter++;

        previousFrameWon = Level.Won;

        if (CurrentLevel?.LevelTime == 0f) {
            if (Level.ScoringData.time == 0f) {
                levelRealTimeKingDice = 0f;
                levelInGameTimeKingDice = 0f;
                spriteSwapLevelFrameCounter = 0;
                spriteSwapOnSpriteSwapFrameCounter = 0;
                spriteSwapOnLobberEXFrameCounter = 0;
                spriteSwapQuadFrameOffset = 0;
            }
            isMinibossStartingKingDice = false;
            hasExtraIGTTickBeenHandledKingDice = false;
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
        if (guiPanelState + 1 < 3) {
            guiPanelState++;
        } else {
            guiPanelState = 0;
        }
    }

    private bool IsMapScene(string sceneName) {
        return sceneName.StartsWith("scene_map_");
    }

    private bool IsWinScreen(string sceneName) {
        return sceneName == "scene_win";
    }

    private void RefPercentage() {
        pdPercentage = PlayerData.Data.GetCompletionPercentage();
        pdDeaths1 = PlayerData.Data.DeathCount(PlayerId.PlayerOne);
        pdDeaths2 = PlayerData.Data.DeathCount(PlayerId.PlayerTwo);
        pdCoins = PlayerData.Data.coinManager.NumCoinsCollected();
    }
}