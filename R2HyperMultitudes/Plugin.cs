using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using R2HyperMultitudes.MathParser;
using RoR2;
using UnityEngine;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace R2HyperMultitudes
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = Author + "." + Name;
        public const string Author = "Raoul1808";
        public const string Name = "R2HyperMultitudes";
        public const string Version = "0.1.0";

        private static ConfigEntry<string> _scalingExpression;

        private Texture2D LoadTextureFromEmbeddedResource(string name)
        {
            byte[] backupMagImageData;
            using (Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("R2HyperMultitudes." + name))
            {
                using (MemoryStream mem = new MemoryStream())
                {
                    imageStream.CopyTo(mem);
                    backupMagImageData = mem.ToArray();
                }
            }
            var tex = new Texture2D(1, 1);
            tex.LoadImage(backupMagImageData);
            return tex;
        }

        private Rect RectFromTex(Texture2D tex) => new Rect(0, 0, tex.width, tex.height);
        
        private void Awake()
        {
            Log.Init(Logger);

            _scalingExpression = Config.Bind(
                "HyperMultitudes",
                "MultiplierExpression",
                "2 * stage",
                "A mathematical expression which is calculated on every stage to determine the new multitudes multiplier to apply. Supports additions (+), subtractions (-), multiplications (*), divisions (/), parentheses and exponents (^)"
            );
            Log.Info("Loading expression: " + _scalingExpression.Value);
            var parsedExpression = new ExpressionParser(_scalingExpression.Value);
            Artifact.MultitudesExpression = parsedExpression.Parse();
            Artifact.MultitudesExpression.Eval(Artifact.StageContext);
            Log.Info("Testing expression");
            for (int i = 1; i < 10; i++)
            {
                Artifact.StageIndex = i;
            }
            Log.Info("Test ended");
            Artifact.StageIndex = 1;

            var backupMag = LoadTextureFromEmbeddedResource("Backup_Magazine.png");
            var backup = LoadTextureFromEmbeddedResource("The_Back-up.png");

            Artifact.OnSprite = Sprite.Create(backupMag, RectFromTex(backupMag), Vector2.zero);
            Artifact.OffSprite = Sprite.Create(backup, RectFromTex(backup), Vector2.zero);
            
            Artifact.Init();
        }

        [ConCommand(commandName = "hm_set_expression", flags = ConVarFlags.SenderMustBeServer, helpText = "Set HyperMultitudes scaling equation")]
        private static void CCSetExpression(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);
            try
            {
                string exp = args[0];
                var expressionNode = new ExpressionParser(exp).Parse();
                expressionNode.Eval(new ModStageContext { Stage = 1 });
                Artifact.MultitudesExpression = expressionNode;
                _scalingExpression.Value = exp;
                Debug.Log("New HyperMultitudes expression set to: " + exp);
            }
            catch (Exception e)
            {
                Debug.LogError("Invalid expression given. Make sure the expression is put in quotes! (e.g: \"2 * stage\")");
                Debug.LogError("Check the logs for (potentially) more details");
                Log.Error($"Caught exception when setting new expression: {e}");
            }
        }

        [ConCommand(commandName = "hm_get_expression", flags = 0, helpText = "Get current HyperMultitudes expression")]
        private static void CCGetExpression(ConCommandArgs args)
        {
            Debug.Log(args.Count == 0
                ? "Current Expression: " + _scalingExpression.Value
                : "Invalid arguments. Did you mean hm_set_expression or hm_test_expression?");
        }

        [ConCommand(commandName = "hm_test_expression", flags = ConVarFlags.SenderMustBeServer, helpText = "Test current HyperMultitudes expression with stage index")]
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
                Debug.Log("Invalid Argument. Correct usage is `hm_test_expression <number>`");
            }
        }
    }
}
