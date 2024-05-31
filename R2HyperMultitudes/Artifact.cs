using System;
using System.IO;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using R2API;
using R2API.Utils;
using R2HyperMultitudes.MathParser;
using RoR2;
using UnityEngine;

namespace R2HyperMultitudes
{
    public static class Artifact
    {
        public class ModStageContext : IContext
        {
            public double Stage { get; set; }

            public double ResolveVariable(string name)
            {
                if (name.ToLower() == "stage")
                    return Stage;
                throw new InvalidDataException($"Unknown variable: {name}");
            }
        }

        public static ArtifactDef HyperMultitudes;
        public static Sprite OnSprite;
        public static Sprite OffSprite;

        public static Node MultitudesExpression;
        public static double MultitudesMultiplier { get; private set; }

        public static readonly ModStageContext StageContext = new ModStageContext();

        private static int _stageIndex;
        public static int StageIndex
        {
            get => _stageIndex;
            set
            {
                _stageIndex = value;
                StageContext.Stage = _stageIndex;
                MultitudesMultiplier = Math.Max(MultitudesExpression.Eval(StageContext), 1);
                Log.Info("HyperMultitudes Multiplier = " + MultitudesMultiplier);
            }
        }

        private delegate int RunInstanceReturnInt(Run self);
        private static RunInstanceReturnInt _origLivingPlayerCount;
        private static RunInstanceReturnInt _origParticipatingPlayerCount;

        public static void Init()
        {
            LanguageAPI.Add("ARTIFACT_HYPERMULTITUDES_NAME", "Artifact of Hyper Multitudes");
            LanguageAPI.Add("ARTIFACT_HYPERMULTITUDES_DESC", "Increasing multitudes power per stage");
            HyperMultitudes = ScriptableObject.CreateInstance<ArtifactDef>();
            HyperMultitudes.cachedName = "ARTIFACT_HYPERMULTITUDES";
            HyperMultitudes.nameToken = "ARTIFACT_HYPERMULTITUDES_NAME";
            HyperMultitudes.descriptionToken = "ARTIFACT_HYPERMULTITUDES_DESC";
            HyperMultitudes.smallIconSelectedSprite = OnSprite;
            HyperMultitudes.smallIconDeselectedSprite = OffSprite;
            ContentAddition.AddArtifactDef(HyperMultitudes);

            On.RoR2.Run.AdvanceStage += (orig, self, nextScene) =>
            {
                orig(self, nextScene);
                if (RunArtifactManager.instance.IsArtifactEnabled(HyperMultitudes))
                {
                    if (nextScene.sceneType == SceneType.Stage)
                        StageIndex += 1;
                }
            };
            Run.onRunStartGlobal += run =>
            {
                Log.Info("Resetting HyperMultitudes Multiplier");
                StageIndex = 1;
            };
            var getLivingPlayerHook = new Hook(typeof(Run).GetMethodCached("get_livingPlayerCount"), typeof(Artifact).GetMethodCached(nameof(GetLivingPlayerCountHook)));
            _origLivingPlayerCount = getLivingPlayerHook.GenerateTrampoline<RunInstanceReturnInt>();
            var getParticipatingPlayerHook = new Hook(typeof(Run).GetMethodCached("get_participatingPlayerCount"), typeof(Artifact).GetMethodCached(nameof(GetParticipatingPlayerCountHook)));
            _origParticipatingPlayerCount = getParticipatingPlayerHook.GenerateTrampoline<RunInstanceReturnInt>();

            IL.RoR2.AllPlayersTrigger.FixedUpdate += il =>
            {
                var c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt<Run>("get_livingPlayerCount")))
                {
                    c.EmitDelegate<Func<int, int>>(livingPlayerCount => _origLivingPlayerCountValue);
                }
            };
            IL.RoR2.MultiBodyTrigger.FixedUpdate += il =>
            {
                var c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt<Run>("get_livingPlayerCount")))
                {
                    c.EmitDelegate<Func<int, int>>(livingPlayerCount => _origLivingPlayerCountValue);
                }
            };
        }

        private static int _origLivingPlayerCountValue;
        
        private static int GetLivingPlayerCountHook(Run self)
        {
            _origLivingPlayerCountValue = _origLivingPlayerCount(self);
            return (int)(_origLivingPlayerCountValue * MultitudesMultiplier);
        }
        private static int GetParticipatingPlayerCountHook(Run self) => (int)(_origParticipatingPlayerCount(self) * MultitudesMultiplier);
    }
}
