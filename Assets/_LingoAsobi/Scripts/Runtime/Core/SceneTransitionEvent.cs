namespace Scripts.Runtime.Core
{
    /// <summary>
    /// シーン遷移イベント
    /// </summary>
    public class SceneTransitionEvent : IEvent
    {
        public string SceneName { get; set; }
        public bool IsAdditive { get; set; }
        public float Progress { get; set; }
        public TransitionPhase Phase { get; set; }
        
        public enum TransitionPhase
        {
            Started,
            Loading,
            Completed,
            Failed
        }
        
        public SceneTransitionEvent(string sceneName, TransitionPhase phase = TransitionPhase.Started)
        {
            SceneName = sceneName;
            Phase = phase;
        }
    }
}