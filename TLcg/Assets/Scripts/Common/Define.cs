namespace LCG
{
    public static class Define
    {
        public interface IMonoBase
        {
            void CustomUpdate();
            void CustomFixedUpdate();
            void CustomDestroy();
            void CustomAppFocus(bool focus);
        }
        public enum EBootup
        {
            None = 0,
            Game,
            Launcher,
            Check,
            Repair,
            DeepRepair
        }
        public enum EMode
        {
            None = 0,
            Launcher,
            Game
        }

        public static string ReporterPath = "Prefabs/Misc/Reporter";
    }
}