using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using GlobalEnums;
using HutongGames.PlayMaker;
using ModCommon;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace combomod
{
    public class combos : MonoBehaviour
    {
        public bool currentlyRunning = false;

        private bool inGodmasterBattle = false;
        private int godmasterLevel = 0;
        private int numBindings = 0;
        private int numFails = 0;
        private int godmasterTotalHits = 0;

        private int godmasterBestCombo = 0;
        private int numHits = 0;
        private int currentLevel = 0;
        private double comboMeter = 0.0;

        private float godmasterSceneSpeedMult = 1.0f;

        private auto_pause autoPauseModule = new auto_pause();
        
        private GameObject voidKnight;
        private GameObject knightRadiantEffect;

        private GameObject canvasObj;
        private GameObject comboBar;

        private GameObject blueComboBar;
        private GameObject radiantText;
        private GameObject hitText;

        private GameObject winLossCanvas;
        private GameObject winLossText;
        private GameObject winLossBlanker;

        private List<GameObject> sceneEnemies;

        private bool barActive;

        private Image comboBarPicture;
        private Image blueComboPicture;
        private Text radiantTextBox;
        private Text totalHitNumber;
        private ParticleSystem radKnightSystem;
        private ParticleSystemRenderer radKnightRenderer;

        private const double decayRateBase = 0.15;

        private const double FOUR_STAR_SUCCESS_RATE = 0.88;
        private const double FIVE_STAR_SUCCESS_RATE = 0.95;

        private readonly Texture2D perfectWhiteCircle;

        private combos()
        {
            perfectWhiteCircle = proc_gen.generateWhiteCircle();
            perfectWhiteCircle.Apply();
        }

        public void pauseCombos()
        {
            combo_mod.log("Paused combos because FSM said to");
            barActive = false;
        }

        public void pauseCombosUntilNextScene()
        {
            godmasterSceneSpeedMult = 0f;
        }
        
        public void pauseCombosForTime(FsmVar timeToPause)
        {
            combo_mod.log("Paused combos for time because FSM said to. Time is " + timeToPause.floatValue);
            StartCoroutine(pauseCombosInternal(timeToPause.floatValue));
        }

        private IEnumerator pauseCombosInternal(float time)
        {
            barActive = false;
            yield return new WaitForSeconds(time);
            barActive = true;
        }

        private static readonly string[] COMBO_STRINGS = new[]
        {
            "1x",
            "Good 2x",
            "Attuned 3x",
            "Ascended 4x",
            "Radiant 10x"
        };

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= switchScenes;
            GameManager.instance.OnFinishedEnteringScene -= restartMeter;
            ModHooks.Instance.TakeDamageHook -= dmgResetCombo;

            try
            {
                On.HealthManager.TakeDamage -= hitEnemy;
            }
            catch (KeyNotFoundException k)
            {
                
            }
        }

        private void Start()
        {
            combo_mod.log("Starting combo thingy");
            
            StartCoroutine(getVoidKnight());
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += switchScenes;

            On.HealthManager.TakeDamage += hitEnemy;
            GameManager.instance.OnFinishedEnteringScene += restartMeter;
            if (globals.fileSettings.comboAffectsPlayerDamage)
            {
                ModHooks.Instance.TakeDamageHook += dmgResetCombo;
            }
        }

        private static bool canTakeDamage(int hazardType)
        {
            return (HeroController.instance.damageMode != DamageMode.NO_DAMAGE &&
                    HeroController.instance.transitionState == HeroTransitionState.WAITING_TO_TRANSITION &&
                    (!HeroController.instance.cState.invulnerable && !HeroController.instance.cState.recoiling) &&
                    (!HeroController.instance.playerData.isInvincible && !HeroController.instance.cState.dead &&
                     (!HeroController.instance.cState.hazardDeath && !BossSceneController.IsTransitioning)) &&
                    (HeroController.instance.damageMode != DamageMode.HAZARD_ONLY || hazardType != 1) &&
                    (!HeroController.instance.cState.shadowDashing || hazardType != 1) &&
                    ((double) HeroController.instance.parryInvulnTimer <= 0.0 || hazardType != 1) &&
                    (!HeroController.instance.playerData.equippedCharm_5 ||
                     HeroController.instance.playerData.blockerHits <= 0 ||
                     (hazardType != 1 || !HeroController.instance.cState.focusing)));
        }

        private int dmgResetCombo(ref int hazardType, int damage)
        {
            if (!canTakeDamage(hazardType) || damage <= 0) return damage;

            if ( (globals.fileSettings.onlyEnableInGodmaster && !inGodmasterBattle) || 
                 !globals.fileSettings.comboLossOnHit) return damage;
             
            if (inGodmasterBattle)
            {
                numFails++;
            }

            if (getPlayerLevel(numHits) != 10)
            {
                numHits = ((getPlayerLevel(numHits) - 2) * globals.fileSettings.comboIncrementHits);
            }
            else
            {
                numHits = 3 * globals.fileSettings.comboIncrementHits;
            }

            if (numHits <= 0)
            {
                numHits = 0;
                comboMeter = 0.0;
            }
            comboBarPicture.fillAmount = (float) comboMeter;

                
            updateComboBars();

            return damage;
        }

        private void restartMeter()
        {
            if (!inGodmasterBattle)
                barActive = true;
        }

        private void hitEnemy(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitinstance)
        {
            if (globals.fileSettings.onlyEnableInGodmaster && !inGodmasterBattle)
            {
                orig(self, hitinstance);
                return;
            }
            
            barActive = true;
            
            if (globals.fileSettings.comboAffectsPlayerDamage)
            {
                hitinstance.DamageDealt =
                    (int) Math.Round((hitinstance.DamageDealt * globals.fileSettings.damageModifier * currentLevel));

                if (hitinstance.DamageDealt < 1)
                {
                    hitinstance.DamageDealt = 1;
                }
            }
            
            orig(self, hitinstance);

            if (autoPauseModule.godmasterTrackedHms.Contains(self) && (self.isDead || self.hp <= 0))
            {
                autoPauseModule.godmasterTrackedHms.Remove(self);

                if (autoPauseModule.godmasterTrackedHms.Count == 0)
                {
                    barActive = false;
                }
            }
            
            bool hasObject = sceneEnemies.Contains(self.gameObject);
            if ( (self.isDead || self.hp <= 0) && hasObject)
            {
                sceneEnemies.Remove(self.gameObject);
            } else if (!hasObject && (!self.isDead && self.hp > 0))
            {
                sceneEnemies.Add(self.gameObject);
            }
            
            if (comboBarPicture != null)
            {
                comboMeter = 1.0;
                comboBarPicture.fillAmount = (float) comboMeter;
                numHits++;

                if (inGodmasterBattle)
                {
                    godmasterTotalHits++;

                    if (godmasterBestCombo < numHits)
                        godmasterBestCombo = numHits;
                }

                updateComboBars();
            }
        }

        private void updateComboBars()
        {
            blueComboPicture.fillAmount = (float) getLevelProgress(numHits);
            currentLevel = getPlayerLevel(numHits);
            
            radiantTextBox.text = currentLevel < 10 ? COMBO_STRINGS[currentLevel - 1] : COMBO_STRINGS[4];

            if (numHits > 0)
            {
                totalHitNumber.text = numHits + "";
            }
            else
            {
                totalHitNumber.text = "";
            }

            ParticleSystem.MainModule partMain = radKnightSystem.main;
            switch (currentLevel)
            {
                case 10:
                    partMain.startColor = Color.white;
                    break;
                case 4:
                    partMain.startColor = new Color(1f, 0.8f, 0.2f, 0.8f);
                    break;
                case 3:
                    partMain.startColor = new Color(1f, 0.8f, 0.2f, 0.4f);
                    break;
                case 2:
                    partMain.startColor = new Color(0.9f, 0f, 0f, 0.3f);
                    break;
                default:
                    partMain.startColor = new Color(0f, 0f, 0f, 0f);
                    break;
            }            
        }

        private void Update()
        {
            if (!barActive || sceneEnemies.Count == 0)
                return;
            
            if (comboBarPicture != null && comboMeter > 0)
            {
                comboBarPicture.fillAmount = (float) comboMeter;
                comboMeter -= (decayRateBase * Time.deltaTime * globals.fileSettings.comboDrainRate * godmasterSceneSpeedMult);
            } else if (comboBarPicture != null && numHits > 0)
            {
                comboMeter = 1.0;
                if (inGodmasterBattle)
                {
                    numFails++;
                }

                if (getPlayerLevel(numHits) != 10)
                {
                    numHits = ((getPlayerLevel(numHits) - 2) * globals.fileSettings.comboIncrementHits);
                }
                else
                {
                    numHits = 3 * globals.fileSettings.comboIncrementHits;
                }

                if (numHits <= 0)
                {
                    numHits = 0;
                    comboMeter = 0.0;
                }
                comboBarPicture.fillAmount = (float) comboMeter;
                updateComboBars();
            } else if (comboBarPicture != null)
            {
                comboBarPicture.fillAmount = 0f;
            }

        }

        private IEnumerator getVoidKnight()
        {
            while (voidKnight == null)
            {
                yield return null;
                
                if (HeroController.instance == null)
                    continue;
                if (HeroController.instance.spellControl == null)
                    continue;
                
                voidKnight = HeroController.instance.spellControl.gameObject;
                if (knightRadiantEffect != null)
                {
                    knightRadiantEffect.transform.parent = voidKnight.transform;
                    knightRadiantEffect.transform.localPosition = Vector3.zero;
                }
                
                combo_mod.log("Found the knight!");
                yield break;
            }
        }
        
        private IEnumerator loadComboBars()
        {
            yield return new WaitForFinishedEnteringScene();
            
            if (comboBar != null)
                yield break;
            
            canvasObj = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920f, 1080f));
            comboBar = CanvasUtil.CreateImagePanel(canvasObj,
                Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f)),
                new CanvasUtil.RectData(new Vector2(300f, 100f), new Vector2(0.9f, 0.9f),
                    new Vector2(0.917f, 0.89f), new Vector2(0.917f, 0.89f)));
            
            blueComboBar = CanvasUtil.CreateImagePanel(canvasObj,
                Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f)),
                new CanvasUtil.RectData(new Vector2(301f, 20f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.917f, 0.85f), new Vector2(0.917f, 0.85f)) );

            comboBarPicture = comboBar.GetComponent<Image>();

            comboBarPicture.preserveAspect = false;
            comboBarPicture.type = Image.Type.Filled;
            comboBarPicture.fillMethod = Image.FillMethod.Horizontal;
            comboBarPicture.fillAmount = (float) comboMeter;

            blueComboPicture = blueComboBar.GetComponent<Image>();

            blueComboPicture.preserveAspect = false;
            blueComboPicture.type = Image.Type.Filled;
            blueComboPicture.fillMethod = Image.FillMethod.Horizontal;
            blueComboPicture.color = Color.blue;

            
            radiantText = CanvasUtil.CreateTextPanel(canvasObj, "", 42,
                TextAnchor.UpperRight, new CanvasUtil.RectData(new Vector2(400f, 150f), 
                    new Vector2(0.5f, 0.5f), new Vector2(0.89f, 0.92f), new Vector2(0.89f, 0.92f)));
            radiantTextBox = radiantText.GetComponent<Text>();
            
            hitText = CanvasUtil.CreateTextPanel(canvasObj, "", 42,
                TextAnchor.LowerRight, new CanvasUtil.RectData(new Vector2(400f, 150f), 
                    new Vector2(0.5f, 0.5f), new Vector2(0.89f, 0.854f), new Vector2(0.89f, 0.854f)));
            totalHitNumber = hitText.GetComponent<Text>();

            if (knightRadiantEffect == null)
            {
                knightRadiantEffect = new GameObject("knightRadiantParticles",
                    typeof(ParticleSystem));
                radKnightSystem = knightRadiantEffect.GetComponent<ParticleSystem>();
                radKnightRenderer = knightRadiantEffect.GetComponent<ParticleSystemRenderer>();
                
                ParticleSystem.MainModule partMain = radKnightSystem.main;
                partMain.loop = true;
                radKnightSystem.useAutoRandomSeed = true;
                partMain.gravityModifier = 0f;
                //partMain.startColor = new 
                //    ParticleSystem.MinMaxGradient(new Color(1f, 0f, 0f), new Color(1f, 1f, 0.3f));
                //partMain.startColor = new 
                //    ParticleSystem.MinMaxGradient(new Color(1f, 1f, 0.3f));
                partMain.startSize = new ParticleSystem.MinMaxCurve(0.25f);
                partMain.startLifetime = new ParticleSystem.MinMaxCurve(0.3f);
                partMain.maxParticles = 300;
                partMain.startSpeed = new ParticleSystem.MinMaxCurve(3f, 15f);
                
            
                ParticleSystem.EmissionModule partEmission = radKnightSystem.emission;
                partEmission.enabled = true;
                partEmission.rateOverTime = new ParticleSystem.MinMaxCurve(50f);

                ParticleSystem.SizeOverLifetimeModule soe = radKnightSystem.sizeOverLifetime;
                AnimationCurve curve = new AnimationCurve();
                curve.AddKey(0.0f, 1f);
                curve.AddKey(1f, 0f);
                soe.size = new ParticleSystem.MinMaxCurve(1f, curve);

                soe.enabled = true;
                
                radKnightRenderer.material.shader = Shader.Find("Particles/Additive");
                radKnightRenderer.material.mainTexture = perfectWhiteCircle;
                radKnightRenderer.renderMode = ParticleSystemRenderMode.Billboard;
                
                
                if (voidKnight != null)
                {
                    knightRadiantEffect.transform.parent = voidKnight.transform;
                    knightRadiantEffect.transform.localPosition = Vector3.zero;
                }
            }
            //canvasObj.SetActive(true);
            //comboBar.SetActive(true);
            //blueComboBar.SetActive(true);
            
            updateComboBars();
        }

        private void switchScenes(Scene from, Scene to)
        {
            sceneEnemies = new List<GameObject>();
            
            foreach (GameObject go in to.GetRootGameObjects())
            {
                foreach (HealthManager enemy in go.GetComponentsInChildren<HealthManager>())
                {
                    sceneEnemies.Add(enemy.gameObject);
                }
            }
            
            barActive = false;
            
            if (from.name == "GG_Boss_Door_Entrance")
            {
                combo_mod.log("found godmaster scene!");

                if (!inGodmasterBattle)
                {
                    setupGodMaster(to.name);
                }
            } else if (inGodmasterBattle && to.name == "GG_Atrium")
            {
                combo_mod.log("Left godmaster scene because you died!");
                endGodMaster(from.name, true);
            } else if (inGodmasterBattle && (to.name == "GG_End_Sequence" || to.name == "End_Credits"))
            {
                combo_mod.log("Left godmaster scene because you won!!!");
                endGodMaster(from.name, false);
            }
            StartCoroutine(gmDelayedBossAdd(to.name));
            godmasterSceneSpeedMult = autoPauseModule.getCurrentComboSpeed(to.name);

            if (globals.fileSettings.onlyEnableInGodmaster && !inGodmasterBattle) return;

            if (!to.name.Contains("Cinematic"))
            {
                StartCoroutine(loadComboBars());
            }

            
            combo_mod.log("Created combo bar successfully!");
        }

        private IEnumerator gmDelayedBossAdd(string toName)
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            autoPauseModule.addCallMethodToEnemiesInScene(toName);
            autoPauseModule.generateDeathPausesForScene(toName);
        }

        private static int getPlayerLevel(int currentCombo)
        {
            int level = currentCombo / globals.fileSettings.comboIncrementHits;

            if (level < 3) return (level + 1);
            return (currentCombo - (globals.fileSettings.comboIncrementHits * 3) -
                    globals.fileSettings.tenXcomboIncrementHits) >= 0 ? 10 : 4;
        }

        private static double getLevelProgress(int currentCombo)
        {
            int level = currentCombo / globals.fileSettings.comboIncrementHits;

            if (level >= 3)
            {
                if ((currentCombo - (globals.fileSettings.comboIncrementHits * 3) -
                     globals.fileSettings.tenXcomboIncrementHits) >= 0)
                {
                    return 1.0;
                }

                return ((currentCombo - (globals.fileSettings.comboIncrementHits * 3)) /
                        (double) globals.fileSettings.tenXcomboIncrementHits);

            }

            return ((currentCombo % globals.fileSettings.comboIncrementHits) /
                    (double) globals.fileSettings.comboIncrementHits);
        }

        private void endGodMaster(string sceneName, bool died)
        {

            if (!died)
            {
                combo_mod.log("Congrats on winning!");
                
                globals.gm_challenge_results results = new globals.gm_challenge_results();
                results.numBind = (globals.numbindings) numBindings;
                double successRate = (godmasterTotalHits - numFails) / (double) godmasterTotalHits;

                if (numFails == 0)
                {
                    results.bestClear = globals.bestclear.FullCombo;
                }
                else if (successRate >= FIVE_STAR_SUCCESS_RATE)
                {
                    results.bestClear = globals.bestclear.FiveStar;
                }
                else if (successRate >= FOUR_STAR_SUCCESS_RATE)
                {
                    results.bestClear = globals.bestclear.FourStar;
                }
                else
                {
                    results.bestClear = globals.bestclear.ThreeStar;
                }

                if (results.numBind > globals.ALL_RESULTS[godmasterLevel].numBind)
                {
                    combo_mod.log("Found better score, saving!");
                    globals.ALL_RESULTS[godmasterLevel] = results;
                    cheat_detect.saveResults();
                } else if (results.numBind == globals.ALL_RESULTS[godmasterLevel].numBind && results.bestClear > globals.ALL_RESULTS[godmasterLevel].bestClear)
                {
                    combo_mod.log("Found better score, saving!");
                    globals.ALL_RESULTS[godmasterLevel] = results;
                    cheat_detect.saveResults();
                }
                else
                {
                    combo_mod.log("Found worse score than before, not saving.");
                }
                
                StartCoroutine(showWinScreen());
            }
            else
            {
                combo_mod.log("Sorry for your loss. :(");
                StartCoroutine(showFailureScreen());
            }

            inGodmasterBattle = false;
            godmasterSceneSpeedMult = 1.0f;
            numHits = 0;
            currentLevel = 1;
            godmasterLevel = 0;
            comboMeter = 0.0;
            updateComboBars();
            
            
            
            
            cheat_detect.saveResults();
        }

        private IEnumerator showWinScreen()
        {
            yield return new WaitForFinishedEnteringScene();
            yield return new WaitForSeconds(0.4f);

            winLossCanvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920f, 1080f));
            
            winLossText = CanvasUtil.CreateTextPanel(winLossCanvas, "", 42,
                TextAnchor.MiddleCenter, new CanvasUtil.RectData(new Vector2(400f, 150f), 
                    new Vector2(0.5f, 0.5f), new Vector2(0.7f, 0.2f), new Vector2(0.7f, 0.2f)));
            Text t = winLossText.GetComponent<Text>();

            t.text = "";
            switch (globals.ALL_RESULTS[godmasterLevel].bestClear)
            {
                case globals.bestclear.FiveStar:
                    t.text += "Outstanding!\n★★★★★";
                    break;
                case globals.bestclear.None:
                    t.text += "invalid";
                    break;
                case globals.bestclear.ThreeStar:
                    t.text += "Great!\n★★★";
                    break;
                case globals.bestclear.FourStar:
                    t.text += "Awesome!\n★★★★";
                    break;
                case globals.bestclear.FullCombo:
                    t.text += "Perfect!\n★★★★★ FC";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            t.text += "\nBindings: " + globals.ALL_RESULTS[godmasterLevel].numBind +
                      "\nCombo: " + godmasterBestCombo + "\nTotal hits: " + godmasterTotalHits;
            
        }
        

        private IEnumerator showFailureScreen()
        {
            yield return new WaitForFinishedEnteringScene();
            yield return new WaitForSeconds(0.4f);
            
            winLossCanvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920f, 1080f));
            winLossBlanker = CanvasUtil.CreateImagePanel(winLossCanvas,
                Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f)),
                new CanvasUtil.RectData(new Vector2(4000f, 2000f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 0f), new Vector2(1f, 1)));

            Image img = winLossBlanker.GetComponent<Image>();
            
            winLossText = CanvasUtil.CreateTextPanel(winLossCanvas, "", 42,
                TextAnchor.MiddleCenter, new CanvasUtil.RectData(new Vector2(400f, 150f), 
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f)));
            Text t = winLossText.GetComponent<Text>();

            t.text = "Failed!\nBest combo: " + godmasterBestCombo + "\nTotal hits: " + godmasterTotalHits;
            Color c = img.color;
            c.g = 0f;
            c.b = 0f;
            c.r = 0f;
            c.a = 1f;
            img.color = c;
            yield return new WaitForSeconds(3.5f);
            const float fadeTime = 4f;

            
            
            for (float time = 0f; time < fadeTime; time += Time.deltaTime)
            {
                c.a = ((fadeTime - time) / fadeTime);
                img.color = c;
                yield return null;
            }
            
            Destroy(winLossBlanker);
            Destroy(winLossText);
            Destroy(winLossCanvas);
        }
        

        private void setupGodMaster(string sceneName)
        {
            switch (sceneName)
            {
                case "GG_Vengefly":
                    godmasterLevel = 0;
                    break;
                case "GG_Ghost_Xero":
                    godmasterLevel = 1;
                    break;
                case "GG_Hive_Knight":
                    godmasterLevel = 2;
                    break;
                case "GG_Crystal_Guardian_2":
                    godmasterLevel = 3;
                    break;
                case "GG_Vengefly_V":
                    godmasterLevel = 4;
                    break;
                default:
                    godmasterLevel = 0;
                    break;
            }
            
            inGodmasterBattle = true;
            numHits = 0;
            comboMeter = 0.0;
            updateComboBars();
            currentLevel = 0;
            godmasterTotalHits = 0;
            godmasterBestCombo = 0;
            numFails = 0;
            numBindings = 0;

            if (BossSequenceController.BoundCharms)
                numBindings++;
            if (BossSequenceController.BoundNail)
                numBindings++;
            if (BossSequenceController.BoundShell)
                numBindings++;
            if (BossSequenceController.BoundSoul)
                numBindings++;
        }
        
    }
}