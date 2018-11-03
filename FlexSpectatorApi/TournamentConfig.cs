using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FlexSpectatorApi
{
    /// <summary>
    /// This class can read, store and save values from an osu! formated tournament.cfg file
    /// </summary>
    public class TournamentConfig
    {
        private readonly string file;
        private readonly Dictionary<string, string> entries = new Dictionary<string, string>();

        /// <summary>
        /// Constructs a new <see cref="TournamentConfig"/> class by reading the given <code>file</code>
        /// </summary>
        /// <param name="file">the path to the tournament.cfg</param>
        public TournamentConfig(string file)
        {
            this.file = file;
            Reload();
        }

        /// <summary>
        /// Read and store data from the tournament.cfg
        /// </summary>
        public void Reload()
        {
            FlexLogger.Log("Reloading configs");
            entries.Clear();

            if(File.Exists(file))
            {
                foreach (string l in File.ReadAllLines(file))
                {
                    string[] split = l.Split('=');
                    entries.Add(split[0].Trim(), split[1].Trim());
                }
            }

            FlexLogger.Log($"Loaded {entries.Count} entries!");
        }

        /// <summary>
        /// Write data to the tournament.cfg
        /// </summary>
        /// <returns>the running <see cref="Task"/></returns>
        public async Task SaveAsync()
        {
            FlexLogger.Log("Saving configs");
            List<string> lines = new List<string>();

            foreach (var e in entries)
                lines.Add(e.Key + " = " + e.Value);

            await Task.Run(() => File.WriteAllLines(file, lines)).ConfigureAwait(false);
        }

        /// <summary>
        /// Store the given value to the given key in memory
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, object value)
        {
            entries.Remove(key);
            entries.Add(key, value.ToString());
        }

        /// <summary>
        /// Retrieve the value of the given key
        /// </summary>
        /// <param name="key">The config key</param>
        /// <returns>The value assiociated with the given key</returns>
        /// <exception cref="KeyNotFoundException">Throws if the key is not set yet</exception>
        public string Get(string key) => entries[key];

        /// <summary>
        /// Retrieve the value of the given key, returns <code>def</code> if key does not exists
        /// </summary>
        /// <param name="key">the config key, used to indentified the searched config</param>
        /// <param name="def">The default value, returned if no value has been set</param>
        /// <returns></returns>
        public string GetOrDef(string key, object def) => entries.ContainsKey(key) ? entries[key] : def?.ToString();
    }
}