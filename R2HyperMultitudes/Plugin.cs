using BepInEx;

namespace R2HyperMultitudes
{
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = Author + "." + Name;
        public const string Author = "Raoul1808";
        public const string Name = "R2HyperMultitudes";
        public const string Version = "0.1.0";
        
        private void Awake()
        {
            Log.Init(Logger);
            Log.Info($"Hello from {Name}!");
        }
    }
}
