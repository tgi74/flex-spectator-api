using osu;
using osu.Helpers;
using System;
using System.Threading;
using static osu.Helpers.InterProcessOsu;

namespace FlexSpectatorApi
{
    public class OsuSpectatorConnection : IDisposable
    {
        private Thread worker;
        private readonly string channel;

        public int ClientId { get; } = -1;
        public InterProcessOsu Ipc { get; private set; } = new InterProcessOsu();
        public ClientData Data { get; private set; } = new ClientData();

        public bool IsConnected { get; private set; }
        public bool IsAlive { get; private set; } = true;
        public string CommonName => ClientId == -1 ? channel : ClientId.ToString();

        #region IPC
        public int MenuTime { get => Data.MenuTime; set => Ipc.SetMenuTime(value); }
        public int SpectatingId { get => Data.SpectatingID; set => Ipc.SetSpectate(value); }
        public int DimLevel { get => Data.DimLevel; set => Ipc.SetUserDim(value); }
        public OsuModes Mode { get => Data.Mode; set => Ipc.ChangeMode(value); }
        public string Beatmap { get => Data.BeatmapChecksum; set => Ipc.SetBeatmap(value); }
        public bool AudioPlaying => Data.AudioPlaying;
        public void WakeUp() => Ipc.WakeUp();

        public bool Buffering
        {
            get => Data.Buffering;
            set
            {
                if (Buffering != value)
                    Ipc.ToggleClientBuffer();
            }
        }

        public bool SkipCalculations
        {
            get => Data.SkipCalculations;
            set
            {
                if (SkipCalculations != value)
                    Ipc.ToggleSkipCalculations();
            }
        }

        public void PlayAudio()
        {
            if (!AudioPlaying)
                Ipc.PlayAudio();
        }

        #endregion

        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;

        public OsuSpectatorConnection() : this("osu!")
        {
        }

        public OsuSpectatorConnection(int spectatorId) : this($"osu!-spectator-{spectatorId}")
        {
            ClientId = spectatorId;
        }

        public OsuSpectatorConnection(string channel)
        {
            this.channel = channel;
        }

        /// <summary>
        /// Start this <see cref="OsuSpectatorConnection"/>'s worker
        /// </summary>
        public void Start()
        {
            worker?.Abort();
            worker = new Thread(KeepAlive)
            {
                Name = channel
            };
            worker.Start();
        }

        /// <summary>
        /// Sends a Quit message, then disconnects the spectator
        /// </summary>
        public void Quit()
        {
            if (!IsAlive)
                return;

            IsAlive = false;
            try
            {
                Ipc.Quit();
            }

            // The client already left
            catch { }

            OnDisconnect?.Invoke(this, null);
            worker?.Abort();
        }

        /// <summary>
        /// Try to connect to the spectator, using the given IPC channel
        /// </summary>
        /// <returns>true if this connection was successful, false otherwise</returns>
        public bool Connect()
        {
            try
            {
                var ipc = (InterProcessOsu)Activator.GetObject(typeof(InterProcessOsu), $"ipc://{channel}/loader");
                var bulk = ipc.GetBulkClientData();
                if (bulk != null)
                {
                    Data = bulk;
                    Ipc = ipc;

                    OnConnect?.Invoke(this, null);
                    return true;
                }

                // Client is not ready
                return false;
            }
            catch
            {
                // Client is not responding
                return false;
            }
        }

        #region Worker
        private void KeepAlive()
        {
            while (IsAlive)
            {
                byte errors = 0;
                Thread.Sleep(10);

                do
                {
                    try
                    {
                        Work();
                        errors = 0;
                    }
                    catch
                    {
                        errors++;
                    }
                }
                // We allow a threshold of a few consecutive errors (as IPC can be unstable when under high load)
                while (errors != 0 && errors <= 10);

                if (errors != 0 && IsConnected)
                {
                    Log("Client has errored more than expected, quitting");
                    Quit();
                }
            }
        }

        private void Work()
        {
            if (!IsConnected)
            {
                if (Connect())
                {
                    IsConnected = true;
                    Log("Now connected!");
                }
                return;
            }

            Data = Ipc.GetBulkClientData();
        }
        #endregion

        /// <summary>
        /// Helper function: Call <see cref="FlexLogger.Log(object)"/> with a prefix
        /// </summary>
        /// <param name="o">The <see cref="object"/> to be logged</param>
        private void Log(object o)
        {
            FlexLogger.Log($"[#{CommonName}] {o}");
        }

        #region IDisposable Support

        private bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    Quit();
                }

                Ipc = null;

                isDisposed = true;
            }
        }

        ~OsuSpectatorConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}