using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private Movement playerMovement;
    [SerializeField] private Slider staminaSlider;

    private void Start()
    {
        staminaSlider.minValue = 0f;
        staminaSlider.maxValue = 1f;
    }

    private void Update()
    {
        if (playerMovement == null || staminaSlider == null)
            return;

        staminaSlider.value =
            playerMovement.CurrentStamina /
            playerMovement.MaxStamina;
    }
}