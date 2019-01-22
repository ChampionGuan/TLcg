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
        public enum ELauncher
        {
            None = 0,
            Initialize,
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
    }
}