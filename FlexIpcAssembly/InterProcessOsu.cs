using osu_common.Bancho.Objects;
using System;

namespace osu.Helpers
{
    /// <summary>
    /// IPC Model used by Stable
    /// </summary>
    public class InterProcessOsu : MarshalByRefObject
    {
        [Serializable]
        public class ClientData
        {
            public bool Buffering;
            public OsuModes Mode;
            public int SpectatingID;
            public int Score;
            public int AudioTime;
            public int ReplayTime;
            public bool SkipCalculations;
            public string BeatmapChecksum;
            public int BeatmapId;
            public int DimLevel;
            public int MenuTime;
            public bool AudioPlaying;

            public ReplayAction LLastAction;
            public int LNextScoreSync;
            public bool LReplayToEnd;
            public bool LPlayerLoaded;
            public bool LReplayMode;
            public bool LReplayStreaming;
            public int LReplayFrame;
        }

        public void HandleArguments(string args)
        {
        }

        public bool WakeUp() => true;

        public void Quit()
        {
        }

        public ClientData GetBulkClientData() => null;

        public int GetCurrentTime() => 2147483647;

        public int GetAvailableTime() => 2147483647;

        public void ToggleClientBuffer()
        {
        }

        public void ToggleSkipCalculations()
        {
        }

        public void SetMenuTime(int time)
        {
        }

        public void SetSpectate(int userId)
        {
        }

        public void SetBeatmap(string checksum)
        {
        }

        public void PlayAudio()
        {
        }

        public int GetCurrentScore() => 0;

        public void SetUserDim(int dim)
        {
        }

        public OsuModes GetCurrentMode() => OsuModes.Unknown;

        public int GetSpectatingId() => -1;

        public void ChangeMode(OsuModes mode)
        {
        }
    }
}