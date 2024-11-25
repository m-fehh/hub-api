using Hub.Infrastructure.Localization;
using System.Collections;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

namespace Hub.Infrastructure.Resources
{
    public class ResourceWrapper : IResourceWrapper
    {
        private Dictionary<string, ResourceSet> _resourceSets = new Dictionary<string, ResourceSet>();
        private readonly Assembly resourceAssembly;
        private readonly string resourceNamespace;

        public ResourceWrapper(Assembly resourceAssembly, string resourceNamespace)
        {
            this.resourceAssembly = resourceAssembly;
            this.resourceNamespace = resourceNamespace;

            try
            {
                Load("pt-BR");
                Load("en-US");
            }
            catch { }
        }

        private void Load(string lang)
        {
            if (string.IsNullOrEmpty(lang) || _resourceSets.ContainsKey(lang))
            {
                return;
            }

            lock (new object())
            {
                if (_resourceSets.ContainsKey(lang))
                {
                    return;
                }

                try
                {
                    var currentAssemblyName = resourceAssembly.GetName().Name;

                    var path = Path.GetDirectoryName(resourceAssembly.GetName().CodeBase).Substring(6);

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        path = string.Concat("/", path);
                    }

                    var asm = Assembly.LoadFrom(Path.Combine(path, lang, $"{currentAssemblyName}.resources.dll"));

                    var resourceName = $"{currentAssemblyName}.{resourceNamespace}.{lang}.resources";

                    _resourceSets.Add(lang, new ResourceSet(asm.GetManifestResourceStream(resourceName)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }


        public string GetByValue(string value)
        {
            string language = Thread.CurrentThread.CurrentCulture.Name.ToLower();

            var resource = _resourceSets[language]
             .OfType<DictionaryEntry>()
             .FirstOrDefault(e => e.Value.ToString() == value);

            if (resource.Key == null) return value;

            return resource.Key.ToString();
        }

        public string GetString(string key)
        {
            try
            {
                string language = Thread.CurrentThread.CurrentCulture.Name.ToLower();

                return GetString(key, language);
            }
            catch
            {
                return key;
            }
        }

        public string GetString(string key, string language)
        {
            string value = null;

            try
            {
                if (_resourceSets.ContainsKey(language))
                {
                    value = _resourceSets[language].GetString(key);
                }
            }
            catch { }

            return value ?? key;
        }
    }
}
