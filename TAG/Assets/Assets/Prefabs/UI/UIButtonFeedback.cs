using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] MMF_Player onHoverFeedbackPlayer;
    [SerializeField] MMF_Player onExitFeedbackPlayer;
    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverEnter();
    }

    public void OnPointerExit(PointerEventData eventData) 
    {
        HoverExit();
    }
    
    void HoverEnter()
    {
        onHoverFeedbackPlayer.PlayFeedbacks();
    }

    void HoverExit()
    {
        onExitFeedbackPlayer.PlayFeedbacks();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
