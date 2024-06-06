using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    GameObject menu;
    GameObject text_game_state;
    GameObject button_continue;
    public void Initialize()
    {
        menu = GameObject.Find("Menu");
        text_game_state = GameObject.Find("Text_GameState");
        button_continue = GameObject.Find("Button_Continue");
        menu.SetActive(false);
    }

    public bool IsMenuActive()
    {
        return menu.activeSelf;
    }
    public void ShowMenu(bool show)
    {
        menu.SetActive(show);
    }

    public string game_state = "";
    public void SetGameState(string game_state)
    {
        this.game_state = game_state;

        if (text_game_state != null)
            text_game_state.GetComponent<TextMeshProUGUI>().text = game_state;
        if (button_continue != null)
            button_continue.SetActive(game_state == "Pause");
    }

    public void Continue()
    {
        var wi = GameObject.Find("WorldInterface");
        wi.GetComponent<WorldInterface>().OnPauseGame();
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
