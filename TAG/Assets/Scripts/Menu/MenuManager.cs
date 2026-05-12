using MoreMountains.Feedbacks;
using System.Collections;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public MMF_Player transitionPlayer;

    public GameObject currentMenu;

    public void ProgressMenu(GameObject menu)
    {
        StartCoroutine(ProgressMenuCoroutine(menu));
    }

    public void LoadLevelScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
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

        // Fade back out
        transitionPlayer.FeedbacksList[1].Play(Vector3.zero);
    }
}
