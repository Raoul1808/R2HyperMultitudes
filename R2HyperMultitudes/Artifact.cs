using System;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace R2HyperMultitudes
{
    public static class Artifact
    {
        public static ArtifactDef HyperMultitudes;
        public static Sprite OnSprite;
        public static Sprite OffSprite;
        public static int StartMultiplier = 1;
        public static int StepMultiplier = 1;
        public static bool Exponential = false;

        private static int _multitudesMultiplier;
        public static int MultitudesMultiplier => RunArtifactManager.instance.IsArtifactEnabled(HyperMultitudes) ? Exponential ? (int)Math.Pow(2, _multitudesMultiplier - 1) : _multitudesMultiplier : 1;

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
                    Log.Info("Increasing HyperMultitudes Multiplier");
                    _multitudesMultiplier += StepMultiplier;
                }
            };
            Run.onRunStartGlobal += run =>
            {
                Log.Info("Resetting HyperMultitudes Multiplier");
                _multitudesMultiplier = StartMultiplier;
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
                    c.EmitDelegate<Func<int, int>>(livingPlayerCount => livingPlayerCount / MultitudesMultiplier);
                }
            };
            IL.RoR2.MultiBodyTrigger.FixedUpdate += il =>
            {
                var c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After, i => i.MatchCallOrCallvirt<Run>("get_livingPlayerCount")))
                {
                    c.EmitDelegate<Func<int, int>>(livingPlayerCount => livingPlayerCount / MultitudesMultiplier);
                }
            };
        }

        private static int GetLivingPlayerCountHook(Run self) => _origLivingPlayerCount(self) * MultitudesMultiplier;
        private static int GetParticipatingPlayerCountHook(Run self) => _origParticipatingPlayerCount(self) * MultitudesMultiplier;
    }
}
