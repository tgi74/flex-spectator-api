using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FlexSpectatorApi
{
    public class OsuSpectatorManager : IDisposable
    {
        private readonly string path;
        private string OsuPath => Path.Combine(path, "osu!.exe");
        private string ConfigPath => Path.Combine(path, "tournament.cfg");

        private readonly ConcurrentDictionary<int, OsuSpectatorConnection> clients = new ConcurrentDictionary<int, OsuSpectatorConnection>();
        private readonly TournamentConfig config;

        public OsuSpectatorManager(string path = "%localappdata%/osu!/")
        {
            this.path = path;
            config = new TournamentConfig(ConfigPath);
        }

        public int TeamSize
        {
            set => config.Set("TeamSize", value);
            get => int.Parse(config.GetOrDef("TeamSize", 1));
        }

        public int ClientCount
        {
            set => config.Set("FLEX_ClientCount", value);
            get => int.Parse(config.GetOrDef("FLEX_ClientCount", 2));
        }

        public OsuSpectatorConnection GetClient(int client) => clients.FirstOrDefault(c => c.Value.ClientId == client).Value;
        public List<OsuSpectatorConnection> GetClients => new List<OsuSpectatorConnection>(clients.Values);

        public event EventHandler OnConnectionJoin;
        public event EventHandler OnConnectionQuit;

        private void UpdatePresence(object o, EventArgs e = null)
        {
            var c = (OsuSpectatorConnection)o;
            if (!c.IsAlive)
            {
                clients.TryRemove(c.ClientId, out _);
                OnConnectionQuit?.Invoke(c, null);
                return;
            }

            OnConnectionJoin?.Invoke(c, null);
        }

        public async Task RestartAllAsync()
        {
            await QuitAllAsync().ConfigureAwait(false);
            await config.SaveAsync().ConfigureAwait(false);
            await StartMissingAsync().ConfigureAwait(false);
        }

        public async Task QuitAllAsync()
        {
            foreach (int client in new List<int>(clients.Keys))
                await QuitAsync(client).ConfigureAwait(false);
        }

        public async Task StartMissingAsync()
        {
            for (int i = 0; i < ClientCount; i++)
            {
                if (GetClient(i) == null)
                    await StartAsync(i).ConfigureAwait(false);
            }
        }

        public async Task StartAsync(int client)
        {
            Log($"Starting client #{client}");
            clients.TryAdd(client, new OsuSpectatorConnection(client));
            // seems to be osu!.exe -spectateclient {id} {teamsize}
            await Task.Run(() => Process.Start(OsuPath, $"-spectateclient {client} {TeamSize}")).ConfigureAwait(false);
            clients[client].OnConnect += UpdatePresence;
            clients[client].OnDisconnect += UpdatePresence;
            clients[client].Start();
        }

        public async Task RestartAsync(int client)
        {
            await QuitAsync(client).ConfigureAwait(false);
            await StartAsync(client).ConfigureAwait(false);
        }

        public async Task QuitAsync(int client)
        {
            var c = GetClient(client);
            if (c == null)
            {
                Log($"Client #{client} does not exists, ignoring quit");
                return;
            }
            Log($"Disconnecting client #{client}");
            await Task.Run(action: c.Quit).ConfigureAwait(false);
            clients.TryRemove(client, out _);
            c.Dispose();
        }

        public void Log(string s)
        {
            FlexLogger.Log($"[Manager] {s}");
        }

        #region IDisposable Support

        private bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    foreach (var c in new List<OsuSpectatorConnection>(clients.Values))
                        c.Dispose();
                    clients.Clear();
                }

                isDisposed = true;
            }
        }

        ~OsuSpectatorManager()
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