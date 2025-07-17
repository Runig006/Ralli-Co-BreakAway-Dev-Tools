using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GameObjectSnapshot {
    public string name;
    public bool isActive;
    public List<ComponentSnapshot> components = new();
    public List<GameObjectSnapshot> children = new();
}

[Serializable]
public class ComponentSnapshot {
    public string typeName;
    public string jsonData;
    public bool? wasEnabled;
    public string extra; //A Json
}

[Serializable]
public class TransformExtra
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}


[Serializable]
public class AnimatorExtra {
    public int stateHash;
    public float normalizedTime;
}