using System.Collections;
using System.Linq;
using System.Net.Mime;
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
        private int godmasterCombo = 0;
        private int numBindings = 0;
        private int numFails = 0;

        private int numHits = 0;
        private double comboMeter = 1.0;
        
        private GameObject voidKnight;

        private GameObject canvasObj;
        private GameObject comboBar;
        private Image comboBarPicture;

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= switchScenes;
            On.HealthManager.TakeDamage -= hitEnemy;

        }

        private void Start()
        {
            combo_mod.log("Starting combo thingy");
            StartCoroutine(getVoidKnight());
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += switchScenes;

            On.HealthManager.TakeDamage += hitEnemy;
        }

        private void hitEnemy(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitinstance)
        {
            comboMeter = 1.0;
            orig(self, hitinstance);
        }

        private void Update()
        {

            if (comboBarPicture != null && comboMeter > 0)
            {
                comboBarPicture.fillAmount = (float) comboMeter;
                comboMeter -= 0.001;
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

        private void switchScenes(Scene from, Scene to)
        {
            combo_mod.log("from: " + from.name + " to: " + to.name);
            
            if (globals.GM_BOSS_LEVELS.Any(s => s.Equals(to.name)))
            {
                combo_mod.log("found godmaster scene!");

                if (!inGodmasterBattle)
                {
                    setupGodmaster(to.name);
                }
            } else if (globals.GM_BOSS_LEVELS.Any(s => s.Equals(from.name)))
            {
                combo_mod.log("Left godmaster scene!");
                endGodmaster(from.name);
            }

            if (globals.fileSettings.onlyEnableInGodmaster && !inGodmasterBattle) return;
            
            
            canvasObj = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920f, 1080f));
            comboBar = CanvasUtil.CreateImagePanel(canvasObj,
                Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f)),
                new CanvasUtil.RectData(new Vector2(300f, 100f), new Vector2(0.9f, 0.9f)));

            comboBarPicture = comboBar.GetComponent<Image>();

            comboBarPicture.preserveAspect = false;
            comboBarPicture.type = Image.Type.Filled;
            comboBarPicture.fillMethod = Image.FillMethod.Horizontal;
            comboBarPicture.fillAmount = (float) comboMeter;
            
            
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

        private void endGodmaster(string sceneName)
        {
            inGodmasterBattle = false;
        }

        private void setupGodmaster(string sceneName)
        {
            inGodmasterBattle = true;
            godmasterCombo = 0;
            godmasterLevel = 0;
        }
        
    }
}