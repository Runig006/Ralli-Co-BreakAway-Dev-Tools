using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;



public class SnapshotHolder : MonoBehaviour
{
    [SerializeField] private GameObjectSnapshot snapshot;
    public void CaptureSnapshot()
    {
        this.snapshot = this.CaptureRecursive(this.gameObject);
        Debug.Log("Status saved");
    }

    [ContextMenu("Restart")]
    public async Task RestoreSnapshot(bool skipRoot = false)
    {
        if (this.snapshot == null)
        {
            Debug.LogWarning("Not saved Snapshot");
            return;
        }
        await this.ApplyRecursive(this.gameObject, this.snapshot, skipRoot, 0);
        Debug.Log("Status Restore");
    }

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
        Component[] comps = gameObject.GetComponents<Component>();
        foreach (Component comp in comps)
        {
            ComponentSnapshot snap = this.ComponentCapture(comp, Array.IndexOf(gameObject.GetComponents(comp.GetType()), comp));
            if (snap != null)
            {
                objectSnapshot.components.Add(snap);
            }
        }


        //Run the childers
        foreach (Transform child in gameObject.transform)
        {
            objectSnapshot.children.Add(this.CaptureRecursive(child.gameObject));
        }

        return objectSnapshot;
    }

    private async Task<int> ApplyRecursive(GameObject gameObject, GameObjectSnapshot objectSnapshot, bool skipRoot, int count)
    {
        if (!skipRoot)
        {
            gameObject.SetActive(objectSnapshot.isActive);
        }

        foreach (ComponentSnapshot compSnap in objectSnapshot.components)
        {
            Type type = Type.GetType(compSnap.typeName);
            if (type == null)
            {
                continue;
            }

            Component[] comps = gameObject.GetComponents(type); //Get only the compents of a tpye
            if (compSnap.componentIndex >= comps.Length)
            {
                continue;
            }

            Component comp = comps[compSnap.componentIndex];
            if (comp == null)
            {
                continue;
            }

            this.ComponentRestore(comp, compSnap);
            
            count++;
        }

        if (count >= 30)
        {
            await Task.Yield();
            count = 0;
        }


        for (int i = 0; i < objectSnapshot.children.Count; i++)
        {
            if (i >= gameObject.transform.childCount) break;

            count = await this.ApplyRecursive(gameObject.transform.GetChild(i).gameObject, objectSnapshot.children[i], false, count);
        }

        return count;
    }



    private ComponentSnapshot ComponentCapture(Component comp, int compIndex)
    {
        ComponentSnapshot componentSnapshot = new ComponentSnapshot();
        componentSnapshot.typeName = comp.GetType().AssemblyQualifiedName;
        componentSnapshot.componentIndex = compIndex;
        componentSnapshot.wasEnabled = comp is Behaviour componentBehaviour ? componentBehaviour.enabled : true;
        switch (comp)
        {
            case Level:
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
                return;

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
                animator.enabled = componentSnapshot.wasEnabled;
                return;

            default:
                try
                {
                    JsonUtility.FromJsonOverwrite(componentSnapshot.jsonData, comp);
                    if (comp is Behaviour componentBehaviour)
                    {
                        componentBehaviour.enabled = componentSnapshot.wasEnabled;
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
