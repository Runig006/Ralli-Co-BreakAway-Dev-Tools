using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformEditor : RDRSNodeWithFrequency
{
    public enum TargetProperty
    {
        Position,
        Rotation,
        Scale
    }

    public enum SpaceMode
    {
        Local,
        Global
    }

    [SerializeField] private RDRSNode valueReader;
    [SerializeField] private RDRSNode[] transformsReaders;

    [SerializeField] private TargetProperty property = TargetProperty.Position;
    [SerializeField] private SpaceMode space = SpaceMode.Local;
    [SerializeField] private Vector3 baseValue = Vector3.zero;
    [SerializeField] private Vector3 axisMask = new Vector3(0, 1, 0);
    [SerializeField] private bool additive = true;
    [SerializeField][Tooltip("Applies Time.deltaTime to input when using Additive mode, making movement FPS-independent.")] private bool useTimeDeltaTime = true;

    public override void Execute()
    {
        object valueRaw = this.valueReader?.GetValue();
        Transform[] targets = this.GetTargets();
        if (targets == null || targets.Length == 0)
        {
            return;
        }

        switch (this.property)
        {
            case TargetProperty.Rotation:
                this.ExecuteRotation(valueRaw, targets);
                break;

            case TargetProperty.Position:
            case TargetProperty.Scale:
                this.ExecuteVector3(valueRaw, targets);
                break;
        }
    }

    private void ExecuteVector3(object valueRaw, Transform[] targets)
    {
        Vector3 input = this.ValueToVector3(valueRaw);
        Vector3 value = this.additive ? input : baseValue + input;

        if (this.useTimeDeltaTime)
        {
            value *= Time.deltaTime;
        }

        foreach (Transform t in targets)
        {
            if (t == null)
            {
                continue;
            }

            switch (this.property)
            {
                case TargetProperty.Position:
                    if (space == SpaceMode.Local)
                    {
                        t.localPosition = this.ApplyVector3(t.localPosition, value, axisMask, additive);
                    }
                    else
                    {
                        t.position = this.ApplyVector3(t.position, value, axisMask, additive);
                    }
                    break;

                case TargetProperty.Scale:
                    t.localScale = this.ApplyVector3(t.localScale, value, axisMask, additive);
                    break;
            }
        }
    }

    private void ExecuteRotation(object valueRaw, Transform[] targets)
    {
        Vector3 valueVector3 = Vector3.zero;
        Quaternion valueQuaternion;
        Vector3 currentRotationEuler = Vector3.zero;
        Quaternion currentRotation;

        foreach (Transform t in targets)
        {
            if (t == null)
            {
                continue;
            }
            currentRotationEuler = (this.space == SpaceMode.Local) ? t.transform.localRotation.eulerAngles : t.transform.rotation.eulerAngles;
            currentRotation = (this.space == SpaceMode.Local) ? t.transform.localRotation : t.transform.rotation;

            if (valueRaw is Quaternion q)
            {
                valueQuaternion = q;
                valueVector3 = q.eulerAngles;
            }
            else
            {
                valueVector3 = this.ValueToVector3(valueRaw);
                valueVector3 = this.ApplyVector3(this.baseValue, valueVector3, this.axisMask, this.additive);
                valueQuaternion = Quaternion.Euler(valueVector3);
            }

            if (this.additive)
            {
                if (this.useTimeDeltaTime)
                {
                    valueVector3 *= Time.deltaTime;
                }

                Quaternion rotation = (this.space == SpaceMode.Local) ? t.localRotation : t.rotation;

                if (axisMask.x != 0)
                {
                    rotation *= Quaternion.AngleAxis(valueVector3.x, Vector3.right);
                }
                if (axisMask.y != 0)
                {
                    rotation *= Quaternion.AngleAxis(valueVector3.y, Vector3.up);
                }
                if (axisMask.z != 0)
                {
                    rotation *= Quaternion.AngleAxis(valueVector3.z, Vector3.forward);
                }

                if (this.space == SpaceMode.Local)
                {
                    t.localRotation = rotation;
                }
                else
                {
                    t.rotation = rotation;
                }
            }
            else
            {
                if (this.space == SpaceMode.Local)
                {
                    t.localRotation = valueQuaternion;
                }
                else
                {
                    t.rotation = valueQuaternion;
                }
            }
        }
    }

    private Transform[] GetTargets()
    {
        List<Transform> collected = new();

        foreach (RDRSNode reader in this.transformsReaders)
        {
            if (reader == null)
            {
                continue;
            }


            object result = reader.GetValue();

            if (result is Transform t)
            {
                collected.Add(t);
            }
            else if (result is Transform[] arr)
            {
                collected.AddRange(arr);
            }
            else if (result is GameObject go)
            {
                collected.Add(go.transform);
            }
            else if (result is GameObject[] objArray)
            {
                foreach (GameObject obj in objArray)
                {
                    collected.Add(obj.transform);
                }
            }
        }

        return collected.ToArray();
    }

    private Vector3 ValueToVector3(object valueRaw)
    {
        Vector3 inputValue = Vector3.zero;
        if (valueRaw != null)
        {
            if (valueRaw is float f)
            {
                inputValue = new Vector3(f, f, f);
            }
            else if (valueRaw is Vector3 v3)
            {
                inputValue = v3;
            }
            else if (valueRaw is Vector2 v2)
            {
                inputValue = new Vector3(v2.x, v2.y, 0f);
            }
            else
            {
                Debug.LogWarning($"[TransformEditor] type not valid: {valueRaw.GetType().Name}");
            }
        }
        return inputValue;
    }

    private Vector3 ApplyVector3(Vector3 original, Vector3 added, Vector3 mask, bool additive)
    {
        Vector3 result = original;
        if (this.additive)
        {
            result.x += mask.x != 0 ? added.x * mask.x : 0f;
            result.y += mask.y != 0 ? added.y * mask.y : 0f;
            result.z += mask.z != 0 ? added.z * mask.z : 0f;
        }
        else
        {
            result.x = mask.x != 0 ? added.x * mask.x : original.x;
            result.y = mask.y != 0 ? added.y * mask.y : original.y;
            result.z = mask.z != 0 ? added.z * mask.z : original.z;
        }
        return result;
    }
}
