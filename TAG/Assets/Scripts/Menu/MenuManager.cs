using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public MMF_Player transitionPlayer;
    [SerializeField] List<MMF_Player> feedbackPlayOnStart;

    public GameObject currentMenu;

    public bool maskOn = false;

    public RectTransform maskTransform;
    [Header("Mask Attributes")]
    public float maskOnPosX;
    public float maskOnWidth;
    public float maskOffPosX;
    public float maskOffWidth;

    private void Start()
    {
        foreach(MMF_Player player in feedbackPlayOnStart)
        {
            if (player != null)
            {
                player.PlayFeedbacks();
            }
        }

        if(currentMenu != null)
        {
            MenuPage page = currentMenu.GetComponent<MenuPage>();

            if (page != null && page.defaultButton != null)
            {
                EventSystem.current.SetSelectedGameObject(
                    page.defaultButton.gameObject);
            }
        }
    }

    public void ProgressMenu(GameObject menu)
    {
        StartCoroutine(ProgressMenuCoroutine(menu));
    }
    public void TurnMaskBoolOn(bool result)
    {
        maskOn = result;
    }

    public void TurnMaskOn(bool mask)
    {
        if (mask)
        {
            Vector3 pos = maskTransform.position;
            pos.x = maskOnPosX;
            maskTransform.position = pos;

            Vector2 size = maskTransform.sizeDelta;
            size.x = maskOnWidth;
            maskTransform.sizeDelta = size;
        }
        else
        {
            Vector3 pos = maskTransform.position;
            pos.x = maskOffPosX;
            maskTransform.position = pos;

            Vector2 size = maskTransform.sizeDelta;
            size.x = maskOffWidth;
            maskTransform.sizeDelta = size;
        }
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        transitionPlayer.FeedbacksList[0].Play(Vector3.zero);

        yield return new WaitForSeconds(
            transitionPlayer.FeedbacksList[0].FeedbackDuration
        );

        SceneManager.LoadScene(sceneName);

    }

    public void LoadLevelScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    
    private IEnumerator ProgressMenuCoroutine(GameObject menu)
    {
        // Fade in to black
        transitionPlayer.FeedbacksList[0].Play(Vector3.zero);

        // Wait for fade duration
        yield return new WaitForSeconds(
            transitionPlayer.FeedbacksList[0].FeedbackDuration
        );

        // Switch menus
        if (currentMenu != null)
            currentMenu.SetActive(false);

        TurnMaskOn(maskOn);

        menu.SetActive(true);
        currentMenu = menu;

        yield return null;

        MenuPage page = menu.GetComponent<MenuPage>();

        if(page != null && page.defaultButton != null)
        {
            EventSystem.current.SetSelectedGameObject(
                page.defaultButton.gameObject);
        }

        // Fade back out
        transitionPlayer.FeedbacksList[1].Play(Vector3.zero);
    }
}
