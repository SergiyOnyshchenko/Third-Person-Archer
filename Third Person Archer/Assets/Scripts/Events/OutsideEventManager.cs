using System;
using System.Collections.Generic;
using UnityEngine;

public static class OutsideEventManager
{
    [Serializable]
    public struct KeyValue
    {
        public string key;
        public object value;
    }

    public struct KeyValueSetup
    {
        public KeyValue[] keyValues;

        public KeyValueSetup(params KeyValue[] keyValues)
        {
            this.keyValues = keyValues;
        }

        public void AddKeyValue(KeyValue keyValue)
        {
            if (this.keyValues == null)
            {
                this.keyValues = new KeyValue[0];
            }

            KeyValue[] newKeyValues = new KeyValue[this.keyValues.Length + 1];
            for (int i = 0; i < this.keyValues.Length; i++)
            {
                newKeyValues[i] = this.keyValues[i];
            }

            newKeyValues[newKeyValues.Length - 1] = keyValue;
            this.keyValues = newKeyValues;
        }

        public void ReplaceKey(string oldKey, string newKey)
        {
            if (this.keyValues == null || this.keyValues.Length == 0) return;

            for (int i = 0; i < keyValues.Length; i++)
            {
                if (keyValues[i].key == oldKey)
                {
                    keyValues[i].key = newKey;
                    return;
                }
            }
        }

        public void ReplaceValue(string key, string newValue)
        {
            if (this.keyValues == null || this.keyValues.Length == 0) return;

            for (int i = 0; i < keyValues.Length; i++)
            {
                if (keyValues[i].key == key)
                {
                    keyValues[i].value = newValue;
                    return;
                }
            }
        }

        public void RemoveKeyValue(string key)
        {
            if (this.keyValues == null || this.keyValues.Length == 0) return;

            KeyValue[] newKeyValues = new KeyValue[this.keyValues.Length - 1];
            for (int i = 0; i < this.keyValues.Length; i++)
            {
                if (this.keyValues[i].key != key)
                {
                    newKeyValues[i] = this.keyValues[i];
                }
            }

            this.keyValues = newKeyValues;
        }
    }

    #region Cases

    public enum LevelType
    {
        fps,
        tps
    }

    [Serializable]
    public struct LevelStartParameters
    {
        public int LevelNumber;
        public int LevelLoop;
        public LevelType LevelType;
    }

    [Serializable]
    public struct LevelFinishParameters
    {
        public int LevelNumber;
        public int LevelLoop;
        public LevelType LevelType;
    }

    public static void SendLevelStarted(LevelStartParameters parameters)
    {
        Dictionary<string, object> parameterList = CreateParameters();

        AddParameter(parameterList, "level_number", parameters.LevelNumber);
        AddParameter(parameterList, "level_loop", parameters.LevelLoop);
        AddParameter(parameterList, "level_type", parameters.LevelType);

        SendEvent("level_start", parameterList);
    }

    public static void SendLevelFinished(LevelFinishParameters parameters)
    {
        Dictionary<string, object> parameterList = CreateParameters();

        AddParameter(parameterList, "level_number", parameters.LevelNumber);
        AddParameter(parameterList, "level_loop", parameters.LevelLoop);
        AddParameter(parameterList, "level_type", parameters.LevelType);

        SendEvent("level_finish", parameterList);
    }

    #endregion

    private static Dictionary<string, object> CreateParameters()
    {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();

        return dictionary;
    }

    private static void AddParameter(Dictionary<string, object> parameters, string key,
        object value)
    {
        parameters.Add(key, value);
    }

    public static void SendEvent(string eventName, Dictionary<string, object> parameters, bool useEventBuffer = false)
    {
        if (String.IsNullOrWhiteSpace(eventName)) return;

        List<Firebase.Analytics.Parameter> firebaseParameters = new List<Firebase.Analytics.Parameter>();
        if (parameters != null)
        {
            foreach (KeyValuePair<string, object> parameterKeyValue in parameters)
            {
                if (parameterKeyValue.Value is string stringValue)
                {
                    Debug.Log($"Add Firebase Parameter: STR {parameterKeyValue.Key}/{stringValue}");
                    firebaseParameters.Add(new Firebase.Analytics.Parameter(parameterKeyValue.Key, stringValue));
                }
                else if (parameterKeyValue.Value is int intValue)
                {
                    Debug.Log($"Add Firebase Parameter: INT {parameterKeyValue.Key}/{intValue}");
                    firebaseParameters.Add(new Firebase.Analytics.Parameter(parameterKeyValue.Key, intValue));
                }
                else if (parameterKeyValue.Value is long longValue)
                {
                    Debug.Log($"Add Firebase Parameter: LNG {parameterKeyValue.Key}/{longValue}");
                    firebaseParameters.Add(new Firebase.Analytics.Parameter(parameterKeyValue.Key, longValue));
                }
                else if (parameterKeyValue.Value is float floatValue)
                {
                    Debug.Log($"Add Firebase Parameter: FLT {parameterKeyValue.Key}/{floatValue}");
                    firebaseParameters.Add(new Firebase.Analytics.Parameter(parameterKeyValue.Key, floatValue));
                }
                else if (parameterKeyValue.Value is double doubleValue)
                {
                    Debug.Log($"Add Firebase Parameter: DLB {parameterKeyValue.Key}/{doubleValue}");
                    firebaseParameters.Add(new Firebase.Analytics.Parameter(parameterKeyValue.Key, doubleValue));
                }
            }
        }

        if (parameters != null)
        {
            Debug.Log($"Send Firebase Event: {eventName}");
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, firebaseParameters.ToArray());
        }
        else
        {
            Debug.Log($"Send Firebase Event: {eventName}");
            Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);
        }
    }

    private const bool IsDebug = true;

    public static void Log(string eventName, params KeyValue[] parameters)
    {
        if (IsDebug)
        {
            string parametersString = "";
            if (parameters != null)
                for (int i = 0; i < parameters.Length; i++)
                {
                    parametersString += $"({parameters[i].key}/{parameters[i].value})" + " ";
                }

            Debug.Log($"> CustomAnalytics <b>{eventName}</b>: <color=#48f>{parametersString}</color>");
        }
    }

    public static void Log(string eventName, KeyValueSetup keyValueSetup)
    {
        if (IsDebug)
        {
            KeyValue[] parameters = keyValueSetup.keyValues;
            string parametersString = "";
            for (int i = 0; i < parameters.Length; i++)
            {
                parametersString += $"({parameters[i].key}: {parameters[i].value})" + " ";
            }

            Debug.Log($"> CustomAnalytics <b>{eventName}</b>: <color=#48f>{parametersString}</color>");
        }
    }

    public static void SendNameEvent(string eventName)
    {
        Log(eventName, null);

        SendEvent(eventName, null);
    }

    public static void SendNameParamsEvent(string name, params KeyValue[] parameter)
    {
        string eventName = name;

        Log(name, parameter);

        Dictionary<string, object> parameters = CreateParameters();

        for (int i = 0; i < parameter.Length; i++)
        {
            AddParameter(parameters, parameter[i].key, parameter[i].value);
        }

        SendEvent(eventName, parameters);
    }

    public static void SendNameParamsEvent(string name, KeyValueSetup keyValueSetup, bool isLogsEnabled = false,
        bool isUseBuffer = false)
    {
        string eventName = name;

        if (isLogsEnabled)
        {
            Log(name, keyValueSetup);
        }

        KeyValue[] parameterList = keyValueSetup.keyValues;

        Dictionary<string, object> parameters = CreateParameters();

        for (int i = 0; i < parameterList.Length; i++)
        {
            AddParameter(parameters, parameterList[i].key, parameterList[i].value);
        }

        SendEvent(eventName, parameters, isUseBuffer);
    }
}