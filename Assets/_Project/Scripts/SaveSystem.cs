using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Dictionary<string, bool> clickedItems = new Dictionary<string, bool>();
}

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    private static SaveData currentSave;

    public static void SetItemClicked(string itemId, bool value)
    {
        Load(); // make sure data is loaded
        currentSave.clickedItems[itemId] = value;
        Save();
    }

    public static bool GetItemClicked(string itemId)
    {
        Load();
        return currentSave.clickedItems.TryGetValue(itemId, out bool value) && value;
    }

    private static void Save()
    {
        string json = JsonUtility.ToJson(currentSave, prettyPrint: true);
        File.WriteAllText(SavePath, json);
    }

    private static void Load()
    {
        if (currentSave != null) return; // already loaded, skip

        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            currentSave = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            currentSave = new SaveData(); // fresh save
        }
    }
}