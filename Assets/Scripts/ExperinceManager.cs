using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public enum ExperienceMode
{
    Free,
    Guided
}

/// <summary>
/// Manages mode selection (free mode vs guided mode) for the VR experience.
/// </summary>
public class ModeSelectionManager : MonoBehaviour
{
    public static ModeSelectionManager Instance;

    [Header("XR Rig (Player)")]
    [Tooltip("Reference to the XR Rig or XR Origin that represents the player.")]
    public GameObject xrRig;

    [Header("Mode Start Positions")]
    [Tooltip("Transform to place the player when Free Mode is selected.")]
    public Transform freeModeStartTransform;
    [Tooltip("Transform to place the player when Guided Mode is selected.")]
    public Transform guidedModeStartTransform;

    [Header("Teleportation Setup")]
    [Tooltip("Optional: Teleportation Areas that allow free teleportation.")]
    public TeleportationArea[] teleportationAreas;
    [Tooltip("Optional: Teleportation Anchors that are allowed in Guided Mode.")]
    public TeleportationAnchor[] teleportationAnchors;

    [Header("Scene Management")]
    [Tooltip("Name of the main experience scene to load (if using separate scenes).")]
    public string mainExperienceSceneName = "MainExperience";

    // Store the selected mode for later use by other systems if needed.
    public ExperienceMode selectedMode { get; private set; }

    private void Awake()
    {
        // Singleton setup.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optionally persist if using scene loading.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region UI Button Callbacks

    /// <summary>
    /// Called when the Free Mode button is pressed.
    /// </summary>
    public void SelectFreeMode()
    {
        selectedMode = ExperienceMode.Free;
        Debug.Log("Free Mode Selected");
        // Optionally, load the main experience scene if it is separate from the menu.
        // SceneManager.LoadScene(mainExperienceSceneName);
        // If the scene is already loaded, simply move the XR Rig.
        MovePlayerToStartPosition();
        SetupTeleportation();
    }

    /// <summary>
    /// Called when the Guided Mode button is pressed.
    /// </summary>
    public void SelectGuidedMode()
    {
        selectedMode = ExperienceMode.Guided;
        Debug.Log("Guided Mode Selected");
        // Optionally, load the main experience scene if it is separate from the menu.
        // SceneManager.LoadScene(mainExperienceSceneName);
        // If the scene is already loaded, simply move the XR Rig.
        MovePlayerToStartPosition();
        SetupTeleportation();
    }

    #endregion

    /// <summary>
    /// Moves the XR Rig to the appropriate start transform based on the selected mode.
    /// </summary>
    private void MovePlayerToStartPosition()
    {
        if (xrRig == null)
        {
            Debug.LogError("XR Rig reference is missing!");
            return;
        }

        Transform targetTransform = null;
        if (selectedMode == ExperienceMode.Free)
        {
            targetTransform = freeModeStartTransform;
        }
        else if (selectedMode == ExperienceMode.Guided)
        {
            targetTransform = guidedModeStartTransform;
        }

        if (targetTransform != null)
        {
            // Set position and rotation. Depending on your setup, you might only adjust position.
            xrRig.transform.position = targetTransform.position;
            xrRig.transform.rotation = targetTransform.rotation;
        }
        else
        {
            Debug.LogWarning("Target start transform is not assigned for the selected mode!");
        }
    }

    /// <summary>
    /// Configures teleportation based on the selected mode.
    /// </summary>
    private void SetupTeleportation()
    {
        // If you are using the XR Interaction Toolkit, you can enable/disable Teleportation Areas and Anchors.
        // For Free Mode, enable Teleportation Areas and (optionally) disable Anchors.
        // For Guided Mode, disable Teleportation Areas (preventing free teleportation) and enable only Teleportation Anchors.

        if (selectedMode == ExperienceMode.Free)
        {
            // Enable all teleportation areas.
            if (teleportationAreas != null)
            {
                foreach (var area in teleportationAreas)
                {
                    area.gameObject.SetActive(true);
                }
            }

            // Optionally disable guided teleport anchors.
            if (teleportationAnchors != null)
            {
                foreach (var anchor in teleportationAnchors)
                {
                    anchor.gameObject.SetActive(false);
                }
            }
        }
        else if (selectedMode == ExperienceMode.Guided)
        {
            // Disable free teleportation areas.
            if (teleportationAreas != null)
            {
                foreach (var area in teleportationAreas)
                {
                    area.gameObject.SetActive(false);
                }
            }

            // Enable only the preset teleportation anchors.
            if (teleportationAnchors != null)
            {
                foreach (var anchor in teleportationAnchors)
                {
                    anchor.gameObject.SetActive(true);
                }
            }
        }
    }

    public void ExitExperience()
    {
        #if UNITY_STANDALONE
            Application.Quit();
        #endif
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
