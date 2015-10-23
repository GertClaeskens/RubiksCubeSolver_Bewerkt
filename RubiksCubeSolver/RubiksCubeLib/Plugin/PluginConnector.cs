using System.Collections.Generic;
using System.IO;

namespace RubiksCubeLib
{
    using System.Linq;

    /// <summary>
  /// Represents a connection for loading plugins from dll files
  /// </summary>
  /// <typeparam name="T">A pluginable type</typeparam>
  internal class PluginConnector<T> where T: IPluginable
  {
    /// <summary>
    /// Loads plugins from directory
    /// </summary>
    /// <param name="dirName">Full path of the directory</param>
    /// <returns>A collection of the detected compatible plugins</returns>
    public List<T> LoadPlugins(string dirName)
    {
      var plugins = new List<T>();

      // Get dlls in plugin directory
      foreach (var file in Directory.GetFiles(dirName).Select(fileOn => new FileInfo(fileOn)).Where(file => file.Extension.Equals(".dll")))
      {
          // Add plugin to list
          plugins.AddRange(this.GetPluginsFromDll(file.FullName));
      }
      return plugins;
    }

    /// <summary>
    /// Loads plugins from a dll library
    /// </summary>
    /// <param name="fileName">Full path of dll library</param>
    /// <returns>A collection of the detected compatible plugins</returns>
    public List<T> GetPluginsFromDll(string fileName)
    {
      var plugins = new List<T>();
      var a = System.Reflection.Assembly.LoadFile(fileName);
      var types = a.GetTypes();
      foreach (var t in types)
      {
        try
        {
          var x = a.CreateInstance(t.FullName);
          plugins.Add((T)x);
        }
        catch
        {
            // ignored
        }
      }
      return plugins;
    }
  }
}
