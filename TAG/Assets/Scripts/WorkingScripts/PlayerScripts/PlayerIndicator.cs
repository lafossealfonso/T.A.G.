using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerIndicator : MonoBehaviour
{
    private UICommunicator uiCommunicator;
    public TextMeshProUGUI textMeshProUGUI;
    public string downArrow;
    private PlayerInput playerInput;
    private bool indicatorActive;
    public GameObject ping;

    // Start is called before the first frame update
    void Start()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        uiCommunicator = GetComponentInParent<UICommunicator>();
        playerInput = GetComponentInParent<PlayerInput>();
        textMeshProUGUI.text = "Player " + (uiCommunicator.playerInt + 1) + "<br>" + downArrow;
        playerInput.actions["Indicate"].performed += StartIndicator;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DisableText()
    {
        textMeshProUGUI.enabled = false;
    }
    public IEnumerator EnableText()
    {
        if (!indicatorActive)
        {
            indicatorActive = true;
            ping.GetComponent<Animator>().Play("Ping");
            textMeshProUGUI.enabled = true;
            yield return new WaitForSeconds(2);
            textMeshProUGUI.enabled = false;
            indicatorActive = false;
        }

    }
    private void StartIndicator(InputAction.CallbackContext context)
    {
        StartCoroutine(EnableText());
    }
}
