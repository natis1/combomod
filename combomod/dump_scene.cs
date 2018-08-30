using System.Collections;
using ModCommon;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace combomod
{
    public class dump_scene : MonoBehaviour
    {
        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= switchScenes;
        }

        private void Start()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += switchScenes;
        }

        private IEnumerator dumpSceneAfterTime(Scene sceneToDump, float time = 0.2f)
        {
            yield return new WaitForSeconds(time);
            sceneToDump.PrintHierarchy(-1, null, null, "gng/" + sceneToDump.name);
        }

        private void switchScenes(Scene from, Scene to)
        {
            combo_mod.log("from: " + from.name + " to: " + to.name);
            StartCoroutine(dumpSceneAfterTime(to));
        }
    }
}