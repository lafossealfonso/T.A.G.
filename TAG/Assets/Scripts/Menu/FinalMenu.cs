using MoreMountains.Feedbacks;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class FinalMenu : MonoBehaviour
{
    [Tooltip("The final menu object (has the MenuPage). Leave empty if this script is on it.")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private string mainMenuScene = "Menu";

    [SerializeField] MMF_Player finalMenuMMFPlayer;


    private bool isLeaving;

    private static bool IsOnline
    {
        get { return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening; }
    }

    // --- Showing -----------------------------------------------------------

    public void Show()
    {
        if (menuRoot == null) menuRoot = gameObject;
        menuRoot.SetActive(true);
        finalMenuMMFPlayer.PlayFeedbacks();
        StartCoroutine(SelectDefaultButton());
    }

    // Same idea as MenuManager: wait a frame, then select the page's default button.
    private IEnumerator SelectDefaultButton()
    {
        yield return null;

        MenuPage page = menuRoot.GetComponent<MenuPage>();
        if (page != null && page.defaultButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(page.defaultButton.gameObject);
    }

    // --- Replay ----------------------------------------------------------------

    // Wire the Replay button to this one; it picks the right version.
    public void Replay()
    {
        if (IsOnline) OnlineReplay();
        else LocalReplay();
    }

    public void LocalReplay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnlineReplay()
    {
        if (NetworkedGameManager.Instance == null) return;
        NetworkedGameManager.Instance.RequestReplayServerRpc();
    }

    // --- Main menu -------------------------------------------------------------

    // Wire the Main Menu button to this. Also used when the session ends under us.
    public void MainMenu()
    {
        if (isLeaving) return;
        isLeaving = true;

        if (NetworkManager.Singleton != null) StartCoroutine(LeaveSessionRoutine());
        else SceneManager.LoadScene(mainMenuScene);
    }

    private IEnumerator LeaveSessionRoutine()
    {
        NetworkManager nm = NetworkManager.Singleton; // keep our own reference

        if (nm.IsListening)
        {
            nm.Shutdown();
            while (nm != null && nm.ShutdownInProgress) yield return null; // shutdown isn't instant
        }

        // These persist across scenes. Remove them so the Menu scene's own copies
        // don't end up duplicated.
        if (nm != null) Destroy(nm.gameObject);

        NetworkTestUI ui = FindFirstObjectByType<NetworkTestUI>();
        if (ui != null) Destroy(ui.gameObject);

        SceneManager.LoadScene(mainMenuScene);
    }
}