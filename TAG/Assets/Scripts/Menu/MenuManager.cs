using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject currentMenu;
    public void ProgressMenu(GameObject menu)
    {
        if (currentMenu != null)
            currentMenu.SetActive(false);
        menu.SetActive(true);
        currentMenu = menu;
    }

}
