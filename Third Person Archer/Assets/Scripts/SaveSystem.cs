using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

//namespace Saves
//{
    [Serializable]
    public class Wrapper<T>
    {
        public T Value;
    }

    [Serializable]
    public class WrapperArr<T>
    {
        public T[] Items;
    }

    public static class SaveSystem
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static string GetPath(string fileName) => CreatePath(fileName);

        public static void SaveWithoutWrapper<T>(T value, string fileName)
        {
            /*#if !UNITY_EDITOR
                        Saves.UnitedSaves.AddOrReplaceSave(fileName, value, false);
                        return;
            #endif*/

            string json = JsonConvert.SerializeObject(value, Formatting.Indented, Settings);

            // Saves.UnitedSaves.AddOrReplaceSave(fileName, json);

            File.WriteAllText(CreatePath(fileName), json);
        }

        public static System.Object LoadWithoutWrapper<T>(string fileName, T defaultValue = default)
        {
            /*#if !UNITY_EDITOR
                        if (Saves.UnitedSaves.HasSaves(fileName))
                        {
                            return Saves.UnitedSaves.TryFetchSave(fileName, defaultValue);
                        }

                        return defaultValue;
            #endif*/

            if (File.Exists(CreatePath(fileName)))
            {
                string fileContents = File.ReadAllText(CreatePath(fileName));

                object json = JsonConvert.DeserializeObject(fileContents, defaultValue.GetType(), Settings);

                return json;
            }

            return defaultValue;
        }

        public static void Save<T>(T value, string fileName)
        {
            /*#if !UNITY_EDITOR
                        Saves.UnitedSaves.AddOrReplaceSave(fileName, value);
                        return;
            #endif*/

            string json =
                JsonConvert.SerializeObject(new Wrapper<T>() { Value = value }, Formatting.Indented, Settings);

            File.WriteAllText(CreatePath(fileName), json);
        }

        public static void SaveJpg(byte[] value, string fileName)
        {
            File.WriteAllBytes(Application.persistentDataPath + "/" + fileName + ".jpg", value);
        }

        public static void Save<T>(string fileName, T value)
        {
            /*#if !UNITY_EDITOR
                        Saves.UnitedSaves.AddOrReplaceSave(fileName, value);
                        return;
            #endif*/

            string json =
                JsonConvert.SerializeObject(new Wrapper<T>() { Value = value }, Formatting.Indented, Settings);

            File.WriteAllText(CreatePath(fileName), json);
        }

        public static T Load<T>(string fileName, T defaultValue = default)
        {
            /*#if !UNITY_EDITOR
                        if (Saves.UnitedSaves.TryFetchSave(fileName, out T value))
                        {
                            return value;
                        }

                        return defaultValue;
            #endif*/

            if (File.Exists(CreatePath(fileName)))
            {
                string fileContents = File.ReadAllText(CreatePath(fileName));
                Wrapper<T> json = JsonConvert.DeserializeObject<Wrapper<T>>(fileContents, Settings);
                return json.Value;
            }

            return defaultValue;
        }

        private static string CreatePath(string id)
        {
            return Application.persistentDataPath + "/" + id + ".json";
        }

        public static void Clear()
        {
#if UNITY_EDITOR
            string path = Application.persistentDataPath;
            if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    File.Delete(file);
                }

                foreach (string directory in Directory.GetDirectories(path))
                {
                    Clear(directory);
                    Directory.Delete(directory);
                }
            }
#endif
        }

        public static bool Exists(string fileName)
        {
            return File.Exists(CreatePath(fileName));
        }

        public static void Clear(string fileName)
        {
            if (File.Exists(CreatePath(fileName)))
            {
                File.Delete(CreatePath(fileName));
            }
        }
    }
//}