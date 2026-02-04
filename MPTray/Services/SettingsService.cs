using MPTray.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace MPTray.Services
{
    public static class SettingsService
    {
        private static readonly string _filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MPTray", "settings.json");

        public static PlayerSettings Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                    return new PlayerSettings();
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<PlayerSettings>(json) ?? new PlayerSettings();
            }
            catch
            {
                return new PlayerSettings();
            }
        }

        public static void Save(PlayerSettings settings)
        {
            try
            {
                string directory = Path.GetDirectoryName(_filePath);
                if (!Directory.Exists(directory)) 
                    Directory.CreateDirectory(directory);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
    }
}
