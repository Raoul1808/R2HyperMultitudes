using System.IO;
using System.Reflection;
using BepInEx;
using R2HyperMultitudes.MathParser;
using UnityEngine;

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
            var expression = Config.Bind(
                "HyperMultitudes",
                "MultiplierExpression",
                "2 * stage",
                "A mathematical expression which is calculated on every stage to determine the new multitudes multiplier to apply. Supports additions (+), subtractions (-), multiplications (*), divisions (/), parentheses and exponents (^)"
            );
            var parsedExpression = new ExpressionParser(expression.Value);
            Artifact.MultitudesExpression = parsedExpression.Parse();
            Artifact.MultitudesExpression.Eval(Artifact.StageContext);

            var backupMag = LoadTextureFromEmbeddedResource("Backup_Magazine.png");
            var backup = LoadTextureFromEmbeddedResource("The_Back-up.png");

            Artifact.OnSprite = Sprite.Create(backupMag, RectFromTex(backupMag), Vector2.zero);
            Artifact.OffSprite = Sprite.Create(backup, RectFromTex(backup), Vector2.zero);
            
            Log.Init(Logger);
            Log.Info($"Hello from {Name}!");
            Artifact.Init();
        }
    }
}
