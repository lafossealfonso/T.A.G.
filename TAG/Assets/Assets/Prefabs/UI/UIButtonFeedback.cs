using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
{

    [SerializeField] MMF_Player onHoverFeedbackPlayer;
    [SerializeField] MMF_Player onExitFeedbackPlayer;
    [SerializeField] LevelDisplay levelDisplayScript;
    [SerializeField] int thisIndexInt = 0;
    [SerializeField] bool displaylevel = false;
    public void OnPointerEnter(PointerEventData eventData)
    {
        HoverEnter();
    }

    public void OnPointerExit(PointerEventData eventData) 
    {
        HoverExit();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        HoverExit();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        HoverExit();
    }

    public void OnSelect(BaseEventData eventData)
    {
        HoverEnter();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        HoverExit();
    }
    
    public void HoverEnter()
    {
        onHoverFeedbackPlayer.PlayFeedbacks();
        if(displaylevel) levelDisplayScript.SetIndexTo(thisIndexInt);
    }

    public void HoverExit()
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
