using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace combomod
{
    public class auto_pause
    {
        public struct gmFSMPauseData
        {
            public string sceneName;
            public string gameObjectName;
            public string fsmName;
            public string fsmStateName;

            public bool restartAfterTime;
            public FsmFloat timeToRestartCD;

            public CallMethod outputCallMethod;

            gmFSMPauseData(string scene, string go, string fsm, string fsmState, bool restartAfterTime = false,
                float timeToRestart = 0f)
            {
                sceneName = scene;
                gameObjectName = go;
                fsmName = fsm;
                fsmStateName = fsmState;
                this.restartAfterTime = restartAfterTime;
                timeToRestartCD = new FsmFloat {Value = timeToRestart};
                

                if (restartAfterTime)
                {
                    outputCallMethod = new CallMethod
                    {
                        behaviour = GameManager.instance.gameObject.GetComponent<combos>(),
                        methodName = "pauseCombosForTime",
                        parameters = new FsmVar[1] {new FsmVar(timeToRestartCD)},
                        everyFrame = false
                    };
                }
                else
                {
                    outputCallMethod = new CallMethod
                    {
                        behaviour = GameManager.instance.gameObject.GetComponent<combos>(),
                        methodName = "pauseCombos",
                        parameters = new FsmVar[0],
                        everyFrame = false
                    };
                }
            }
        }

        public readonly gmFSMPauseData[] godmasterPauseData;

        public auto_pause()
        {
            
            
            
        }


        public void addCallMethodToEnemiesInScene(string sceneName)
        {
            
            
            
        }




    }
}