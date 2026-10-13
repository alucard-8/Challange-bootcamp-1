using UnityEngine;

namespace Soulbreak.Meta
{
    public class LeaderboardSubmitter : MonoBehaviour
    {
        [SerializeField] private MetaRecorder recorder;

        private void OnEnable() => recorder.OnRunRecorded += HandleRun;
        private void OnDisable() => recorder.OnRunRecorded -= HandleRun;

        private void Start() => _ = LeaderboardService.EnsureConnected();

        private void HandleRun(RunResult result) => _ = LeaderboardService.SubmitScore(result.Score);
    }
}
