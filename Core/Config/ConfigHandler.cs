namespace Plus.Core.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ConfigHandler
    {
        private readonly Dictionary<string, string> _configData = new Dictionary<string, string>();

        public ConfigHandler(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("Unable to locate configuration file at '" + filePath + "'.");
            }

            using (var stream = new StreamReader(filePath))
            {
                string line;

                while ((line = stream.ReadLine()) != null)
                {
                    if (line.Length < 1 || line.StartsWith("#"))
                    {
                        continue;
                    }

                    var delimiterIndex = line.IndexOf('=');

                    if (delimiterIndex == -1)
                    {
                        continue;
                    }

                    var key = line.Substring(0, delimiterIndex);
                    var val = line.Substring(delimiterIndex + 1);

                    _configData.Add(key, val);
                }
            }
        }

        public string this[string key] => _configData[key];
    }
}