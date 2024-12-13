#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FindingMiss : MonoBehaviour
{
    [MenuItem("Tools/Find Missing References")]
    public static void FindMissingReferences()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        foreach (var rootObject in rootObjects)
        {
            Component[] components = rootObject.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                if (component == null)
                {
                    Debug.LogError($"Missing Component in GameObject: {rootObject.name}", rootObject);
                    continue;
                }

                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty property = serializedObject.GetIterator();

                while (property.NextVisible(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (property.objectReferenceValue == null
                            && property.objectReferenceInstanceIDValue != 0)
                        {
                            Debug.LogError($"Missing Reference in: {component.gameObject.name}, Component: {component.GetType().Name}, Property: {property.name}", component.gameObject);
                        }
                    }
                }
            }
        }
    }
}
#endif