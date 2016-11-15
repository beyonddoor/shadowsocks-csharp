using Shadowsocks.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace Shadowsocks.Model
{
    public class ConfigBase<T> where T:class
    {
        public static T Load(string file)
        {
            try
            {
                string configContent = File.ReadAllText(file);
                T config = SimpleJson.SimpleJson.DeserializeObject<T>(configContent, new JsonSerializerStrategy());
                return config;
            }
            catch (Exception e)
            {
                if (!(e is FileNotFoundException))
                {
                    Console.WriteLine(e);
                }
                return default(T);
            }
        }

        public static void Save(T config, string file)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(File.Open(file, FileMode.Create)))
                {
                    string jsonString = SimpleJson.SimpleJson.SerializeObject(config);
                    sw.Write(jsonString);
                    sw.Flush();
                }
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e);
            }
        }

        private static void Assert(bool condition)
        {
            if (!condition)
            {
                throw new Exception(I18N.GetString("assertion failure"));
            }
        }

        private class JsonSerializerStrategy : SimpleJson.PocoJsonSerializerStrategy
        {
            // convert string to int
            public override object DeserializeObject(object value, Type type)
            {
                if (type == typeof(Int32) && value.GetType() == typeof(string))
                {
                    return Int32.Parse(value.ToString());
                }
                return base.DeserializeObject(value, type);
            }
        }
    }

    [Serializable]
    public class Configuration : ConfigBase<Configuration>
    {
        public List<Server> configs;
        public int index;
        public bool global;
        public bool enabled;
        public bool shareOverLan;
        public bool isDefault;
        public int localPort;
        public string pacUrl;
        public bool useOnlinePac;
        public bool killPolipoAtFirst;

        private static string CONFIG_FILE = "gui-config.json";

        public Server GetCurrentServer()
        {
            if (index >= 0 && index < configs.Count)
            {
                return configs[index];
            }
            else
            {
                return GetDefaultServer();
            }
        }

        public static void CheckServer(Server server)
        {
            CheckPort(server.server_port);
            CheckPassword(server.password);
            CheckServer(server.server);
        }

        public static Configuration Load()
        {
            var config = Load(CONFIG_FILE);
            if (config == null)
            {
                config = new Configuration
                {
                    index = 0,
                    isDefault = true,
                    localPort = 1080,
                    configs = new List<Server>()
                    {
                        GetDefaultServer()
                    }
                };
            }
            else if (config.localPort == 0)
            {
                config.localPort = 1080;
            }
            return config;
        }

        public static void Save(Configuration config)
        {
            if (config.index >= config.configs.Count)
            {
                config.index = config.configs.Count - 1;
            }
            if (config.index < 0)
            {
                config.index = 0;
            }
            config.isDefault = false;
            Save(config, CONFIG_FILE);
        }

        public static Server GetDefaultServer()
        {
            return new Server();
        }

        private static void Assert(bool condition)
        {
            if (!condition)
            {
                throw new Exception(I18N.GetString("assertion failure"));
            }
        }

        public static void CheckPort(int port)
        {
            if (port <= 0 || port > 65535)
            {
                throw new ArgumentException(I18N.GetString("Port out of range"));
            }
        }

        private static void CheckPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException(I18N.GetString("Password can not be blank"));
            }
        }

        private static void CheckServer(string server)
        {
            if (string.IsNullOrEmpty(server))
            {
                throw new ArgumentException(I18N.GetString("Server IP can not be blank"));
            }
        }
    }

    /// <summary>
    /// var settings = ConfigurationManager.AppSettings;
    /// </summary>
    [Serializable]
    class TestConfig : ConfigBase<TestConfig>
    {
        public bool killPolipo = false;
        public bool standalonePolipo = true;
        public const string File = "test.conf";
    }
}
