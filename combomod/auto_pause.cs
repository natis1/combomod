using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using ModCommon.Util;
using UnityEngine;

namespace combomod
{
    public class auto_pause
    {
        public struct gm_fsm_pause_data
        {
            public string sceneName;
            public string gameObjectName;
            public string fsmName;
            public string fsmStateName;

            public FsmVar timeToRestartCD;

            public CallMethod outputCallMethod;

            public gm_fsm_pause_data(string scene, string go, string fsm, string fsmState, int restartType = 0,
                float timeToRestart = 0f)
            {
                sceneName = scene;
                gameObjectName = go;
                fsmName = fsm;
                fsmStateName = fsmState;
                timeToRestartCD = new FsmVar(typeof(float)) {floatValue = timeToRestart};
                

                if (restartType == 1)
                {
                    outputCallMethod = new CallMethod
                    {
                        behaviour = GameManager.instance.gameObject.GetComponent<combos>(),
                        methodName = "pauseCombosForTime",
                        parameters = new FsmVar[1] {new FsmVar(timeToRestartCD)},
                        everyFrame = false
                    };
                }
                else if (restartType == 0)
                {
                    outputCallMethod = new CallMethod
                    {
                        behaviour = GameManager.instance.gameObject.GetComponent<combos>(),
                        methodName = "pauseCombos",
                        parameters = new FsmVar[0],
                        everyFrame = false
                    };
                }
                else
                {
                    outputCallMethod = new CallMethod
                    {
                        behaviour = GameManager.instance.gameObject.GetComponent<combos>(),
                        methodName = "pauseCombosUntilNextScene",
                        parameters = new FsmVar[0],
                        everyFrame = false
                    };
                }
            }
        }

        public struct gm_scene_speed_data
        {
            public string sceneName;
            public float speedMultiplier;

            public gm_scene_speed_data(string sceneName, float speedMult)
            {
                this.sceneName = sceneName;
                this.speedMultiplier = speedMult;
            }
        }

        public struct gm_death_speed_data
        {
            public string sceneName;
            public string gameObjName;

            public gm_death_speed_data(string sceneName, string go)
            {
                this.sceneName = sceneName;
                this.gameObjName = go;
            }
        }
        

        public readonly gm_fsm_pause_data[] godmasterPauseData;
        public readonly gm_scene_speed_data[] godmasterSceneData;
        public readonly gm_death_speed_data[] godmasterDeathPauses;

        public List<HealthManager> godmasterTrackedHms = new List<HealthManager>();

        public auto_pause()
        {
            godmasterSceneData = new[]
            {
                new gm_scene_speed_data("GG_Ghost_Hu", 0f),
                new gm_scene_speed_data("GG_Uumuu_V", 0f),
                new gm_scene_speed_data("GG_Uumuu", 0f),
                new gm_scene_speed_data("GG_Nailmasters", 0f),
                new gm_scene_speed_data("GG_Radiance", 0f),
                new gm_scene_speed_data("GG_Soul_Master", 0.4f),
                new gm_scene_speed_data("GG_Soul_Tyrant", 0.4f),
                new gm_scene_speed_data("GG_Ghost_Markoth_V", 0.5f),
                new gm_scene_speed_data("GG_Ghost_Markoth", 0.5f),
                new gm_scene_speed_data("GG_Dung_Defender", 0.5f),
                new gm_scene_speed_data("GG_White_Defender", 0.5f),
                new gm_scene_speed_data("GG_Ghost_No_Eyes_V", 0.6f),
                new gm_scene_speed_data("GG_Ghost_No_Eyes", 0.6f),
                new gm_scene_speed_data("GG_Grey_Prince_Zote", 0.6f),
                new gm_scene_speed_data("GG_Vengefly_V", 0.7f),
                new gm_scene_speed_data("GG_Vengefly", 0.7f)
            };
            
            godmasterPauseData = new[]
            {
                new gm_fsm_pause_data("GG_False_Knight", "Battle Scene.False Knight New", "FalseyControl", "Death Open"),
                new gm_fsm_pause_data("GG_False_Knight", "Battle Scene.False Knight New", "FalseyControl", "Stun Start", 2),
                new gm_fsm_pause_data("GG_Failed_Champion", "False Knight Dream", "FalseyControl", "Stun Start"),
                new gm_fsm_pause_data("GG_Failed_Champion", "False Knight Dream", "FalseyControl", "Death Open", 2),
                new gm_fsm_pause_data("GG_Flukemarm", "Fluke Mother", "Fluke Mother", "Delay Roar"),
                new gm_fsm_pause_data("GG_Soul_Master", "Mage Lord", "Mage Lord", "GG Pause"),
                new gm_fsm_pause_data("GG_Soul_Tyrant", "Dream Mage Lord", "Mage Lord", "GG Enter"),
                new gm_fsm_pause_data("GG_Grimm", "Grimm Scene.Grimm Boss", "Control", "Balloon Tele In", 1, 9f),
                new gm_fsm_pause_data("GG_Grimm", "Grimm Scene.Grimm Boss", "Control", "Spike Antic", 1, 2f),
                new gm_fsm_pause_data("GG_Grimm_Nightmare", "Grimm Control.Nightmare Grimm Boss", "Control", "Balloon Tele In", 1, 9f),
                new gm_fsm_pause_data("GG_Grimm_Nightmare", "Grimm Control.Nightmare Grimm Boss", "Control", "Spike Attack", 1, 2f),
                new gm_fsm_pause_data("GG_Nosk_Hornet", "Battle Scene.Hornet Nosk", "Hornet Nosk", "Acid Roar Antic", 1, 2f),
                new gm_fsm_pause_data("GG_Nosk_Hornet", "Battle Scene.Hornet Nosk", "Hornet Nosk", "Roof Antic"),
                new gm_fsm_pause_data("GG_Sly", "Battle Scene.Sly Boss", "Control", "Death Stun"),
                new gm_fsm_pause_data("GG_Grey_Prince_Zote", "Grey Prince", "Control", "GG Pause")
            };

            godmasterDeathPauses = new[]
            {
                new gm_death_speed_data("GG_God_Tamer", "Entry Object.Lobster")
            };


        }

        private static GameObject parseGameObjectName(string name)
        {
            string[] gameObjs = name.Split('.');
            GameObject enemy = GameObject.Find(gameObjs[0]);

            if (enemy == null)
            {
                combo_mod.log("Unable to find game object of name " + gameObjs[0]);
                combo_mod.log("Please report this as a bug!");
                return enemy;
            }

            for (int i = 1; i < gameObjs.Length; i++)
            {
                enemy = enemy.FindGameObjectInChildren(gameObjs[i]);
                if (enemy != null) continue;
                combo_mod.log("Unable to find sub obj " + gameObjs[i] + " from " + name);
                combo_mod.log("Please report this as a bug!");
            }
            return enemy;
        }

        public void generateDeathPausesForScene(string sceneName)
        {
            godmasterTrackedHms = new List<HealthManager>();
            
            for (int i = 0; i < godmasterDeathPauses.Length; i++)
            {
                if (sceneName != godmasterDeathPauses[i].sceneName)
                    continue;
                
                godmasterTrackedHms.Add(parseGameObjectName(godmasterDeathPauses[i].gameObjName).GetEnemyHealthManager());
            }
        }

        public float getCurrentComboSpeed(string sceneName)
        {
            float speed = 1.0f;
            for (int i = 0; i < godmasterSceneData.Length; i++)
            {
                if (sceneName != godmasterSceneData[i].sceneName)
                    continue;
                speed = godmasterSceneData[i].speedMultiplier;
            }
            return speed;
        }


        public void addCallMethodToEnemiesInScene(string sceneName)
        {
            foreach (gm_fsm_pause_data p in godmasterPauseData)
            {
                if (p.sceneName != sceneName)
                    continue;
                GameObject go = parseGameObjectName(p.gameObjectName);
                if (go == null)
                    continue;

                PlayMakerFSM pFSM = go.LocateMyFSM(p.fsmName);
                if (pFSM == null)
                {
                    combo_mod.log("Invalid FSM on " + p.gameObjectName + " of name " + p.fsmName);
                    continue;
                }
                if (pFSM.GetState(p.fsmStateName) == null)
                {
                    combo_mod.log("Invalid FSM State on " + p.gameObjectName + " of name " + p.fsmName + " of state name " + p.fsmStateName);
                }
                
                pFSM.InsertAction(p.fsmStateName, p.outputCallMethod, 0);
                combo_mod.log("Inserted action into [" + p.gameObjectName + "][" + p.fsmName + "][" + p.fsmStateName + "]");
            }
        }




    }
}