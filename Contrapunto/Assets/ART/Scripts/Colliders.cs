using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ColliderDebugEditorOnly : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f); // verde con algo de transparencia

        foreach (BoxCollider col in FindObjectsOfType<BoxCollider>())
        {
            if (col.enabled)
            {
                Gizmos.matrix = col.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(col.center, col.size);
            }
        }
    }
}
