using System;
using System.Collections;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
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
        
        private int numHits = 0;
        private int currentLevel = 0;
        private double comboMeter = 0.0;
        
        private GameObject voidKnight;

        private GameObject canvasObj;
        private GameObject comboBar;

        private GameObject blueComboBar;
        private GameObject radiantText;

        private bool barActive;
        
        
        private Image comboBarPicture;
        private Image blueComboPicture;
        private Text radiantTextBox;

        private const double decayRateBase = 0.1;

        private const double FOUR_STAR_SUCCESS_RATE = 0.93;
        private const double FIVE_STAR_SUCCESS_RATE = 0.97;

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
            On.HealthManager.TakeDamage -= hitEnemy;
            GameManager.instance.OnFinishedEnteringScene -= restartMeter;
            ModHooks.Instance.TakeDamageHook -= dmgResetCombo;
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

        private int dmgResetCombo(ref int hazardtype, int damage)
        {
            if (damage <= 0) return damage;
            
            numHits = 0;
            comboMeter = 0.0;
            if (inGodmasterBattle)
                numFails++;
                
            updateComboBars();

            return damage;
        }

        private void restartMeter()
        {
            barActive = true;
        }

        private void hitEnemy(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitinstance)
        {
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
            if (comboBarPicture != null)
            {
                comboMeter = 1.0;
                numHits++;

                if (inGodmasterBattle)
                {
                    godmasterTotalHits++;
                }

                updateComboBars();
            }
        }

        private void updateComboBars()
        {
            blueComboPicture.fillAmount = (float) getLevelProgress(numHits);
            currentLevel = getPlayerLevel(numHits);
            
            radiantTextBox.text = currentLevel < 10 ? COMBO_STRINGS[currentLevel - 1] : COMBO_STRINGS[4];
        }

        private void Update()
        {
            if (!barActive)
                return;
            
            if (comboBarPicture != null && comboMeter > 0)
            {
                comboBarPicture.fillAmount = (float) comboMeter;
                comboMeter -= (decayRateBase * Time.deltaTime);
            } else if (comboBarPicture != null && numHits > 0)
            {
                comboMeter = 1.0;
                comboBarPicture.fillAmount = (float) comboMeter;
                if (inGodmasterBattle)
                {
                    numFails++;
                }

                numHits = ((getPlayerLevel(currentLevel) - 1) * globals.fileSettings.comboIncrementHits);
                if (numHits < 0)
                    numHits = 0;
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
                    new Vector2(0.9f, 0.89f), new Vector2(0.9f, 0.89f)));
            
            blueComboBar = CanvasUtil.CreateImagePanel(canvasObj,
                Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f)),
                new CanvasUtil.RectData(new Vector2(300f, 20f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.9f, 0.85f), new Vector2(0.9f, 0.85f)) );

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
            updateComboBars();
        }

        private void switchScenes(Scene from, Scene to)
        {
            barActive = false;
            combo_mod.log("from: " + from.name + " to: " + to.name);
            
            if (from.name == "GG_Boss_Door_Entrance")
            {
                combo_mod.log("found godmaster scene!");

                if (!inGodmasterBattle)
                {
                    setupGodMaster(to.name);
                }
            } else if (inGodmasterBattle && to.name == "GG_Atrium")
            {
                combo_mod.log("Left godmaster scene!");
                endGodMaster(from.name);
            }

            if (globals.fileSettings.onlyEnableInGodmaster && !inGodmasterBattle) return;

            StartCoroutine(loadComboBars());
            
            //new GameObject("comboBarDisplay", typeof(MeshFilter), typeof(MeshRenderer));

            /*
            MeshFilter mf = comboBar.GetComponent<MeshFilter>();
            MeshRenderer mr = comboBar.GetComponent<MeshRenderer>();

            mf.mesh = new Mesh
            {
                name = "comboBarMesh",
                vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, 0.015f),
                    new Vector3(0.5f, -0.5f, 0.015f),
                    new Vector3(0.5f, 0.5f, 0.015f),
                    new Vector3(-0.5f, 0.5f, 0.015f)
                },
                uv = new[]
                {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1)
                },
                triangles = new[]
                {
                    2, 1, 0,
                    3, 2, 0
                }
            };

            mf.mesh.RecalculateNormals();
            
            mr.material.shader = Shader.Find("Particles/Additive");
            mr.material.mainTexture = Texture2D.whiteTexture;
            mr.material.color = Color.white;

            comboBar.transform.parent = canvasObj.transform;
            comboBar.transform.localPosition = Vector3.zero;

            Text memeTestingText = comboBar.GetOrAddComponent<Text>();
            memeTestingText.fontSize = 100;
            memeTestingText.text = "meme testing";
            
            RectTransform rt = comboBar.GetOrAddComponent<RectTransform>();
            rt.localPosition = Vector3.zero;
            rt.sizeDelta = new Vector2(400, 100);
            */
            canvasObj.SetActive(true);
            comboBar.SetActive(true);
            
            combo_mod.log("Created combo bar successfully!");
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

        private void endGodMaster(string sceneName)
        {
            globals.gm_challenge_results results = new globals.gm_challenge_results();
            results.numBind = (globals.numbindings) numBindings;
            
            
            double successRate = (godmasterTotalHits - numFails) / (double) godmasterTotalHits;

            if (numFails == 0)
            {
                results.bestClear = globals.bestclear.FullCombo;
            } else if (successRate >= FIVE_STAR_SUCCESS_RATE)
            {
                results.bestClear = globals.bestclear.FiveStar;
            } else if (successRate >= FOUR_STAR_SUCCESS_RATE)
            {
                results.bestClear = globals.bestclear.FourStar;
            }
            else
            {
                results.bestClear = globals.bestclear.ThreeStar;
            }
            
            
            inGodmasterBattle = false;
            numHits = 0;
            currentLevel = 0;
            comboMeter = 0.0;
            updateComboBars();
            
            
            
            
            cheat_detect.saveResults();
        }

        private void setupGodMaster(string sceneName)
        {
            switch (sceneName)
            {
                case "GG_Vengefly":
                    godmasterLevel = 0;
                    break;
                default:
                    currentLevel = 0;
                    break;
            }
            
            inGodmasterBattle = true;
            numHits = 0;
            comboMeter = 0.0;
            updateComboBars();
            currentLevel = 0;
            godmasterTotalHits = 0;
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