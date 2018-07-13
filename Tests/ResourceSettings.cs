using System.Configuration;

namespace Tests
{
    public class ResourceSettings
    {
        private ResourceSettings() { }

        private static readonly ResourceSettings _instance = new ResourceSettings();
        public static ResourceSettings Instance
        { get { return _instance; } }
        /// <summary>
        /// Get connectionstring to database
        /// </summary>
        /// <returns></returns>
        public string GetDBConnString()
        {
            return ConfigurationManager.ConnectionStrings["DB"].ConnectionString;
        }
    }
}
