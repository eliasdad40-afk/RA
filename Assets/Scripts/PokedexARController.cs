using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Mini Pokédex AR: reconoce cada carta, crea el Prefab de su Pokémon como hijo
/// de la carta y abre la ficha del Pokémon detectado con el botón "Ver ficha".
/// Se agrega como componente al objeto XR Origin.
/// </summary>
public class PokedexARController : MonoBehaviour
{
    // Una entrada por carta: nombre en la biblioteca -> Prefab -> ficha.
    [Serializable]
    public class PokemonEntry
    {
        public string targetName;        // IDÉNTICO al Name en BibliotecaTargets (ej. TargetPikachu)
        public string displayName;       // Texto para mostrar (ej. Pikachu)
        public GameObject contentPrefab; // Prefab del Pokémon (ej. PikachuContent)
        public GameObject ficha;         // Panel de la ficha en el Canvas (ej. FichaPikachu)
    }

    [Header("AR")]
    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private List<PokemonEntry> pokemons = new List<PokemonEntry>();

    [Header("Interfaz")]
    [SerializeField] private Button verFichaButton;
    [SerializeField] private TMP_Text verFichaLabel;
    [SerializeField] private TMP_Text mensajeTexto;

    // Una carta reconocida -> su contenido 3D y su entrada (sin duplicados).
    private readonly Dictionary<TrackableId, GameObject> spawnedContent =
        new Dictionary<TrackableId, GameObject>();
    private readonly Dictionary<TrackableId, PokemonEntry> entryById =
        new Dictionary<TrackableId, PokemonEntry>();

    // Última carta reconocida: define qué ficha abre "Ver ficha".
    private PokemonEntry currentEntry;
    private TrackableId currentId;
    private bool hasCurrent;

    private void OnEnable()
    {
        if (imageManager != null)
            imageManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    private void OnDisable()
    {
        if (imageManager != null)
            imageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    private void Start()
    {
        // Al iniciar: botón oculto y fichas cerradas.
        RefreshUI();
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (ARTrackedImage image in eventArgs.added)
            CreateContent(image);

        foreach (ARTrackedImage image in eventArgs.updated)
            UpdateContent(image);

        foreach (var removed in eventArgs.removed)
            RemoveContent(removed.Key);

        RefreshUI();
    }

    // ---------- Contenido 3D ----------

    private PokemonEntry FindEntry(string imageName)
    {
        foreach (PokemonEntry entry in pokemons)
            if (entry.targetName == imageName)
                return entry;
        return null;
    }

    private void CreateContent(ARTrackedImage image)
    {
        TrackableId id = image.trackableId;
        if (spawnedContent.ContainsKey(id)) return;

        PokemonEntry entry = FindEntry(image.referenceImage.name);
        if (entry == null || entry.contentPrefab == null)
        {
            Debug.LogWarning("Sin Pokémon asociado a la imagen: " + image.referenceImage.name);
            return;
        }

        // El Prefab nace como hijo de la carta, en su centro.
        GameObject content = Instantiate(entry.contentPrefab, image.transform, false);
        content.transform.localPosition = Vector3.zero;
        content.transform.localRotation = Quaternion.identity;

        spawnedContent.Add(id, content);
        entryById.Add(id, entry);
        content.SetActive(image.trackingState == TrackingState.Tracking);
    }

    private void UpdateContent(ARTrackedImage image)
    {
        if (!spawnedContent.TryGetValue(image.trackableId, out GameObject content))
        {
            CreateContent(image);
            return;
        }
        // Solo se ve mientras ARCore sigue la carta con Tracking.
        content.SetActive(image.trackingState == TrackingState.Tracking);
    }

    private void RemoveContent(TrackableId id)
    {
        if (spawnedContent.TryGetValue(id, out GameObject content))
        {
            Destroy(content);
            spawnedContent.Remove(id);
            entryById.Remove(id);
        }
    }

    // ---------- Interfaz ----------

    private void RefreshUI()
    {
        // Busca la carta que se está siguiendo (prefiere la última reconocida).
        ARTrackedImage best = null;
        if (imageManager != null)
        {
            foreach (ARTrackedImage img in imageManager.trackables)
            {
                if (img.trackingState != TrackingState.Tracking) continue;
                if (!entryById.ContainsKey(img.trackableId)) continue;

                if (hasCurrent && img.trackableId == currentId) { best = img; break; }
                if (best == null) best = img;
            }
        }

        if (best == null)
        {
            // Ninguna carta a la vista.
            hasCurrent = false;
            currentEntry = null;
            CerrarFicha();
            if (verFichaButton != null) verFichaButton.gameObject.SetActive(false);
            if (mensajeTexto != null) mensajeTexto.text = "Apunta la cámara a una carta";
            return;
        }

        PokemonEntry detected = entryById[best.trackableId];
        if (currentEntry != detected) CerrarFicha(); // otra carta: se cierra la ficha anterior

        currentEntry = detected;
        currentId = best.trackableId;
        hasCurrent = true;

        if (verFichaButton != null) verFichaButton.gameObject.SetActive(true);
        if (verFichaLabel != null) verFichaLabel.text = "Ver ficha: " + detected.displayName;
        if (mensajeTexto != null) mensajeTexto.text = "Pokémon detectado: " + detected.displayName;
    }

    // Se asigna al OnClick del botón "Ver ficha".
    public void VerFicha()
    {
        if (currentEntry != null && currentEntry.ficha != null)
            currentEntry.ficha.SetActive(true);
    }

    // Se asigna al OnClick de los botones "Cerrar".
    public void CerrarFicha()
    {
        foreach (PokemonEntry entry in pokemons)
            if (entry.ficha != null)
                entry.ficha.SetActive(false);
    }
}
