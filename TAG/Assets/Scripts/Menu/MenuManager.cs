using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public MMF_Player transitionPlayer;
    [SerializeField] List<MMF_Player> feedbackPlayOnStart;

    public GameObject currentMenu;

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
