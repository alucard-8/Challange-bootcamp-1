using UnityEngine;

namespace Soulbreak.Meta
{
    // Observer: posts every finished run's score to the global leaderboard. Knows nothing about UI.
    public class LeaderboardSubmitter : MonoBehaviour
    {
        [SerializeField] private MetaRecorder recorder;

        private void OnEnable() => recorder.OnRunRecorded += HandleRun;
        private void OnDisable() => recorder.OnRunRecorded -= HandleRun;

        private void Start() => _ = LeaderboardService.EnsureConnected();   // sign in early so the post is quick

        private void HandleRun(RunResult result) => _ = LeaderboardService.SubmitScore(result.Score);
    }
}
