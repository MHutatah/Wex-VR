using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class UIManager : MonoBehaviour
{
    [Header("Component References")]
    [Tooltip("Reference to the Character Controller that controls the player's height.")]
    public CharacterController characterController;

    [Tooltip("Reference to the Tunneling Vignette Controller that manages the vignette effect.")]
    public GameObject tunnelingVignetteController;

    public void UpdateCharacterHeight(float newHeight)
    {
        if (characterController != null)
        {
            characterController.height = newHeight;
            // Optionally, update the character's center so that the controller remains centered.
            Vector3 center = characterController.center;
            center.y = newHeight / 2f;
            characterController.center = center;
        }
        else
        {
            Debug.LogWarning("UIManager: CharacterController reference is not assigned.");
        }
    }

    public void UpdateVignetteAperture(float newAperture)
    {
        if (tunnelingVignetteController != null)
        {
            // Call the newly created method on the vignette controller.
            tunnelingVignetteController.GetComponent<TunnelingVignetteController>().SetApertureSize(newAperture);
        }
        else
        {
            Debug.LogWarning("UIManager: TunnelingVignetteController reference is not assigned.");
        }
    }
}
