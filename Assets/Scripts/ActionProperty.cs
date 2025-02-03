using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActionProperty : MonoBehaviour
{
    public InputActionReference InputAction;
    public GameObject MainMenu;
    public GameObject subMenu;
    public GameObject Player;
    public GameObject locomotionSystem;
    
    // Distance in front of the player at which the menu will appear.
    public float menuDistance = 2.0f;
    // Vertical offset to raise the menu (e.g., to the player's eye level).
    public float verticalOffset = 1.5f;
    // Rotation offset applied after LookRotation (in degrees). For example, if the UI appears flipped,
    // set Y to 180.
    public Vector3 rotationOffset = new Vector3(0f, 180f, 0f);

    void Update()
    {
        if (InputAction.action.WasPressedThisFrame())
        {
            Debug.Log("Pressed!");
            if(!subMenu.activeSelf)
            {
                // Toggle the locomotion system and the main menu.
                locomotionSystem.SetActive(!locomotionSystem.activeSelf);
                MainMenu.SetActive(!MainMenu.activeSelf);

                // Calculate the base position above the player's position.
                Vector3 basePosition = Player.transform.position + Vector3.up * verticalOffset;
                // Position the menu in front of the player.
                Vector3 menuPosition = basePosition + Player.transform.forward * menuDistance;

                // Set the MainMenu position.
                MainMenu.transform.position = menuPosition;
                // Compute the rotation so that the menu faces the player.
                Quaternion lookRotation = Quaternion.LookRotation(Player.transform.position + Vector3.up * verticalOffset - menuPosition);
                // Apply an additional rotation offset.
                MainMenu.transform.rotation = lookRotation * Quaternion.Euler(rotationOffset);
                subMenu.transform.position = menuPosition;
                subMenu.transform.rotation = lookRotation * Quaternion.Euler(rotationOffset);
            }
            else{
                subMenu.SetActive(false);
                MainMenu.SetActive(false);
                locomotionSystem.SetActive(true);
            }
        }
    }
}
