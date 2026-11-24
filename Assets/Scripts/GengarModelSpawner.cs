using UnityEngine;

public sealed class GengarModelSpawner : MonoBehaviour
{
    [SerializeField] private GameObject modelPrefab;
    [SerializeField] private Material modelMaterial;

    private void Awake()
    {
        if (modelPrefab == null)
        {
            Debug.LogError("No hay un modelo asociado a GengarContent.", this);
            enabled = false;
            return;
        }
        if (modelMaterial == null)
        {
            Debug.LogError("No hay un material asociado a GengarContent.", this);
            enabled = false;
            return;
        }

        GameObject model = Instantiate(modelPrefab, transform, false);
        model.name = "GengarModel";

        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            Debug.LogError("El modelo de Gengar no contiene renderers.", this);
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].sharedMaterials;
            if (materials.Length == 0)
                materials = new[] { modelMaterial };

            for (int j = 0; j < materials.Length; j++)
                materials[j] = modelMaterial;
            renderers[i].sharedMaterials = materials;

            if (i > 0)
                bounds.Encapsulate(renderers[i].bounds);
        }

        float largestDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (largestDimension <= Mathf.Epsilon)
        {
            Debug.LogError("No se pudo determinar el tamaño del modelo de Gengar.", this);
            return;
        }

        model.transform.localScale = Vector3.one * (0.1f / largestDimension);
        model.transform.localPosition = new Vector3(0.3925f, 0.002f, 0.6432f);
    }
}
