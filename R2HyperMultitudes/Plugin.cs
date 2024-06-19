using System;
using System.Globalization;
using BepInEx;
using BepInEx.Configuration;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using R2API.Utils;
using R2HyperMultitudes.MathParser;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace R2HyperMultitudes
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(Guid, Name, Version)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    public class Plugin : BaseUnityPlugin
    {
        #region Plugin Info
        public const string Guid = Author + "." + Name;
        public const string Author = "Raoul1808";
        public const string Name = "R2HyperMultitudes";
        public const string Version = "0.1.0";
        #endregion

        #region Config
        private static ConfigEntry<string> _scalingExpression;
        private static ConfigEntry<bool> _hypermultitudesEnabled;
        #endregion

        #region Variables
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
                string hmLog = "HyperMultitudes Multiplier = " + MultitudesMultiplier;
                Debug.Log(hmLog);
                Log.Info(hmLog);
            }
        }
        #endregion

        private void Awake()
        {
            Log.Init(Logger);

            _hypermultitudesEnabled = Config.Bind(
                "HyperMultitudes",
                "Enabled",
                true,
                "Whether HyperMultitudes should be enabled or not"
            );
            _scalingExpression = Config.Bind(
                "HyperMultitudes",
                "MultiplierExpression",
                "2 * stage",
                "A mathematical expression which is calculated on every stage to determine the new multitudes multiplier to apply. Supports additions (+), subtractions (-), multiplications (*), divisions (/), parentheses and exponents (^)"
            );

            Log.Info("Loading expression: " + _scalingExpression.Value);
            var parsedExpression = new ExpressionParser(_scalingExpression.Value);
            MultitudesExpression = parsedExpression.Parse();
            MultitudesExpression.Eval(StageContext);
            Log.Info("Testing expression");
            for (int i = 1; i < 10; i++)
            {
                StageIndex = i;
            }
            Log.Info("Test ended");
            StageIndex = 1;

            On.RoR2.Run.AdvanceStage += (orig, self, nextScene) =>
            {
                orig(self, nextScene);
                if (_hypermultitudesEnabled.Value)
                {
                    if (nextScene.sceneType == SceneType.Stage)
                    {
                        StageIndex += 1;
                        SendChatScaling();
                    }
                }
            };
            Run.onRunStartGlobal += run =>
            {
                Log.Info("Resetting HyperMultitudes Multiplier");
                StageIndex = 1;
            };
            var getLivingPlayerHook = new Hook(typeof(Run).GetMethodCached("get_livingPlayerCount"), typeof(Plugin).GetMethodCached(nameof(GetLivingPlayerCountHook)));
            _origLivingPlayerCount = getLivingPlayerHook.GenerateTrampoline<RunInstanceReturnInt>();
            var getParticipatingPlayerHook = new Hook(typeof(Run).GetMethodCached("get_participatingPlayerCount"), typeof(Plugin).GetMethodCached(nameof(GetParticipatingPlayerCountHook)));
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

        private static void SendChatScaling()
        {
            if (!NetworkServer.active)
                return;

            Chat.SendBroadcastChat(
                new Chat.SimpleChatMessage()
                {
                    baseToken = "HyperMultitudes Multiplier now at: ",
                    paramTokens = new[] { MultitudesMultiplier.ToString(CultureInfo.InvariantCulture) }
                }
            );
        }

        private static void SendChatExpression()
        {
            if (!NetworkServer.active)
                return;

            Chat.SendBroadcastChat(
                new Chat.SimpleChatMessage()
                {
                    baseToken = "HyperMultitudes Expression set to: {}",
                    paramTokens = new[] { _scalingExpression.Value },
                }
            );
        }
        
        #region Hooks
        private delegate int RunInstanceReturnInt(Run self);
        private static RunInstanceReturnInt _origLivingPlayerCount;
        private static RunInstanceReturnInt _origParticipatingPlayerCount;

        private static int _origLivingPlayerCountValue;
        private static int _origParticipatingPlayerCountValue;
        
        private static int GetLivingPlayerCountHook(Run self)
        {
            _origLivingPlayerCountValue = _origLivingPlayerCount(self);
            return (int)(_origLivingPlayerCountValue * MultitudesMultiplier);
        }

        private static int GetParticipatingPlayerCountHook(Run self)
        {
            _origParticipatingPlayerCountValue = _origParticipatingPlayerCount(self);
            return (int)(_origParticipatingPlayerCountValue * MultitudesMultiplier);
        }
        #endregion

        #region Console Commands
        [ConCommand(commandName = "mod_hm_enable", flags = ConVarFlags.SenderMustBeServer, helpText = "Enable HyperMultitudes")]
        private static void CCEnable(ConCommandArgs args)
        {
            if (args.Count != 0)
            {
                Debug.LogError("Invalid arguments. Did you mean mod_hm_set_expression or mod_hm_test_expression?");
                return;
            }

            if (_hypermultitudesEnabled.Value)
            {
                Debug.LogWarning("HyperMultitudes is already enabled.");
                return;
            }
            _hypermultitudesEnabled.Value = true;
            Debug.Log("HyperMultitudes enabled. Good luck");
        }

        [ConCommand(commandName = "mod_hm_disable", flags = ConVarFlags.SenderMustBeServer, helpText = "Disable HyperMultitudes")]
        private static void CCDisable(ConCommandArgs args)
        {
            if (args.Count != 0)
            {
                Debug.LogError("Invalid arguments. Did you mean mod_hm_set_expression or mod_hm_test_expression?");
                return;
            }

            if (!_hypermultitudesEnabled.Value)
            {
                Debug.LogWarning("HyperMultitudes is already disabled.");
                return;
            }
            _hypermultitudesEnabled.Value = false;
            Debug.Log("HyperMultitudes disabled.");
        }

        [ConCommand(commandName = "mod_hm_set_expression", flags = ConVarFlags.SenderMustBeServer, helpText = "Set HyperMultitudes scaling equation")]
        private static void CCSetExpression(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);
            try
            {
                string exp = args[0];
                var expressionNode = new ExpressionParser(exp).Parse();
                expressionNode.Eval(new ModStageContext { Stage = 1 });
                MultitudesExpression = expressionNode;
                _scalingExpression.Value = exp;
                Debug.Log("New HyperMultitudes expression set to: " + exp);
                SendChatExpression();
            }
            catch (Exception e)
            {
                Debug.LogError("Invalid expression given. Make sure the expression is put in quotes! (e.g: \"2 * stage\")");
                Debug.LogError("Check the logs for (potentially) more details");
                Log.Error($"Caught exception when setting new expression: {e}");
            }
        }

        [ConCommand(commandName = "mod_hm_get_expression", flags = 0, helpText = "Get current HyperMultitudes expression")]
        private static void CCGetExpression(ConCommandArgs args)
        {
            Debug.LogWarning(args.Count == 0
                ? "Current Expression: " + _scalingExpression.Value
                : "Invalid arguments. Did you mean mod_hm_set_expression or mod_hm_test_expression?");
        }

        [ConCommand(commandName = "mod_hm_test_expression", flags = ConVarFlags.SenderMustBeServer, helpText = "Test current HyperMultitudes expression with stage index")]
        private static void CCTestExpression(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);
            if (double.TryParse(args[0], out var value))
            {
                try
                {
                    var expressionNode = new ExpressionParser(_scalingExpression.Value).Parse();
                    double result = expressionNode.Eval(new ModStageContext { Stage = value });
                    Debug.Log($"Result = {result}");
                }
                catch (Exception e)
                {
                    Log.Error($"Caught exception when testing existing expression: {e}");
                }
            }
            else
            {
                Debug.LogError("Invalid Argument. Correct usage is `mod_hm_test_expression <number>`");
            }
        }
        #endregion
    }
}
