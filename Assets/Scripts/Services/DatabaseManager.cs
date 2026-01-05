using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Authentication;   // AuthenticationService
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;             // UnityServices
using UnityEngine;
using System.Linq;

/*
 * This class manages loading and saving key-value pairs in the CloudSaveService.
 */
public class DatabaseManager
{
    // Sample code from https://docs.unity.com/ugs/manual/cloud-save/manual/tutorials/unity-sdk

    public static async Task<Dictionary<string, string>> SaveData(params (string key, object value)[] kwargs)
    {
        // Idea from  here: https://stackoverflow.com/a/77002085/827927
        Dictionary<string, object> playerData = kwargs.ToDictionary(x => x.key, x => x.value);
        Debug.Log($"DatabaseManager: Saving data to Cloud Save");
        Debug.Log($"DatabaseManager: Player ID: {AuthenticationService.Instance.PlayerId}");
        var result = await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);
        Debug.Log($"DatabaseManager: Saved data {string.Join(',', playerData)}. result={string.Join(',', result)}");
        return result;
    }


    public static async Task<Dictionary<string, Item>> LoadData(params string[] args)
    {
        Debug.Log($"DatabaseManager: Loading data from Cloud Save");
        Debug.Log($"DatabaseManager: Player ID: {AuthenticationService.Instance.PlayerId}");
        Debug.Log($"DatabaseManager: Loading keys: {string.Join(',', args)}");
        HashSet<string> keys = new HashSet<string>(args);
        Dictionary<string, Item> playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
        Debug.Log($"DatabaseManager: Loaded player data: {string.Join(',', playerData)}");
        return playerData;
    }



}

