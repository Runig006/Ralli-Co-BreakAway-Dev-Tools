using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class SnapshotHolder : MonoBehaviour
{
    [HideInInspector] [SerializeField] private GameObjectSnapshot snapshot;

#if UNITY_EDITOR
    [ContextMenu("Save status")]
    public void CaptureSnapshot()
    {
        this.snapshot = this.CaptureRecursive(this.gameObject);
        EditorUtility.SetDirty(this);
        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        Debug.Log("Status saved");
    }

    [ContextMenu("Restart")]
    public void RestoreSnapshot()
    {
        if (this.snapshot == null)
        {
            Debug.LogWarning("Not saved Snapshot");
            return;
        }
        this.ApplyRecursive(this.gameObject, this.snapshot);
        Debug.Log("Status Restore");
    }
#endif

    private GameObjectSnapshot CaptureRecursive(GameObject gameObject)
    {
        //Create the Snapshot, with the position appart
        GameObjectSnapshot objectSnapshot = new GameObjectSnapshot
        {
            name = gameObject.name,
            isActive = gameObject.activeSelf,
            components = new List<ComponentSnapshot>(),
            children = new List<GameObjectSnapshot>()
        };

        //Save the components
        foreach (Component comp in gameObject.GetComponents<Component>())
        {
            ComponentSnapshot snap = ComponentCapture(comp);
            if (snap != null)
            {
                objectSnapshot.components.Add(snap);
            }
        }


        //Run the childers
        foreach (Transform child in gameObject.transform)
        {
            objectSnapshot.children.Add(CaptureRecursive(child.gameObject));
        }

        return objectSnapshot;
    }

    private void ApplyRecursive(GameObject gameObject, GameObjectSnapshot objectSnapshot)
    {
        //gameObject.name = objectSnapshot.name;
        gameObject.SetActive(objectSnapshot.isActive);

        foreach (ComponentSnapshot compSnap in objectSnapshot.components)
        {
            Type type = Type.GetType(compSnap.typeName);
            if (type == null)
            {
                continue;
            }

            Component comp = gameObject.GetComponent(type);
            if (comp == null)
            {
                continue;
            }

            this.ComponentRestore(comp, compSnap);
        }

        for (int i = 0; i < objectSnapshot.children.Count; i++)
        {
            if (i >= gameObject.transform.childCount)
            {
                break;
            }

            ApplyRecursive(gameObject.transform.GetChild(i).gameObject, objectSnapshot.children[i]);
        }
    }

    private ComponentSnapshot ComponentCapture(Component comp)
    {
        ComponentSnapshot componentSnapshot = new ComponentSnapshot();
        componentSnapshot.typeName = comp.GetType().AssemblyQualifiedName;
        componentSnapshot.wasEnabled = comp is Behaviour componentBehaviour ? componentBehaviour.enabled : (bool?)null;
        switch (comp)
        {
            case SnapshotHolder:
            case MeshFilter:
            case MeshRenderer:
            case SkinnedMeshRenderer:
            case BoxCollider:
            case SphereCollider:
            case CapsuleCollider:
            case MeshCollider:
            case Collider:
            case Rigidbody:
            case AudioSource:
            case Animation:
            case Camera:
            case Light:
            case Canvas:
            case CanvasRenderer:
                return null;


            case Transform transform:
                componentSnapshot.extra = JsonUtility.ToJson(new TransformExtra
                {
                    position = transform.localPosition,
                    rotation = transform.localRotation,
                    scale = transform.localScale
                });
                break;

            case Animator animator:
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                componentSnapshot.extra = JsonUtility.ToJson(new AnimatorExtra
                {
                    stateHash = state.fullPathHash,
                    normalizedTime = state.normalizedTime
                });
                break;

            default:
                try
                {
                    componentSnapshot.jsonData = JsonUtility.ToJson(comp);
                    break;
                }
                catch
                {
                    return null;
                }
        }
        return componentSnapshot;
    }

    private void ComponentRestore(Component comp, ComponentSnapshot componentSnapshot)
    {
        switch (comp)
        {
            case SnapshotHolder:
            case MeshFilter:
            case MeshRenderer:
            case SkinnedMeshRenderer:
            case BoxCollider:
            case SphereCollider:
            case CapsuleCollider:
            case MeshCollider:
            case Collider:
            case Rigidbody:
            case AudioSource:
            case Animation:
            case Camera:
            case Light:
            case Canvas:
            case CanvasRenderer:
                return; // ignorados, no se restauran

            case Transform transform:
                if (string.IsNullOrEmpty(componentSnapshot.extra) == false)
                {
                    TransformExtra data = JsonUtility.FromJson<TransformExtra>(componentSnapshot.extra);
                    transform.localPosition = data.position;
                    transform.localRotation = data.rotation;
                    transform.localScale = data.scale;
                }
                return;

            case Animator animator:
                JsonUtility.FromJsonOverwrite(componentSnapshot.jsonData, animator);
                if (string.IsNullOrEmpty(componentSnapshot.extra) == false)
                {
                    AnimatorExtra data = JsonUtility.FromJson<AnimatorExtra>(componentSnapshot.extra);
                    animator.Play(data.stateHash, 0, data.normalizedTime);
                }
                if (componentSnapshot.wasEnabled.HasValue)
                {
                    animator.enabled = componentSnapshot.wasEnabled.Value;
                }
                return;

            default:
                try
                {
                    JsonUtility.FromJsonOverwrite(componentSnapshot.jsonData, comp);
                    if (comp is Behaviour componentBehaviour && componentSnapshot.wasEnabled.HasValue)
                    {
                        componentBehaviour.enabled = componentSnapshot.wasEnabled.Value;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Fail to restore component '{comp.GetType().Name}' on '{comp.gameObject.name}': {e.Message}");
                }
                return;
        }
    }
}
