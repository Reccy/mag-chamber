﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MonsterLove.StateMachine;

public class MainMenu : MonoBehaviour {

    //State Machine
    enum MenuState {MainMenu, LevelSelect};
    StateMachine<MenuState> menuState;

    //Panels
    public GameObject mainMenuPanel, levelSelectPanel;
    Button[] mainMenuButtons;

    //Levels
    public LevelData[] levels;
    int selectedLevelIndex = 0;

    void Awake()
    {
        //Gets level data
        UpdateLevelData(selectedLevelIndex);

        //Disable panels
        levelSelectPanel.SetActive(false);

        //Get list of buttons
        mainMenuButtons = mainMenuPanel.GetComponentsInChildren<Button>();

        //Init FSM
        menuState = StateMachine<MenuState>.Initialize(this);
        menuState.ChangeState(MenuState.MainMenu);
    }

    /*
     * State management
     */

    //Main Menu
    void MainMenu_Enter()
    {
        foreach(Button button in mainMenuButtons)
        {
            button.interactable = true;
        }
    }
    
    void MainMenu_Exit()
    {
        foreach(Button button in mainMenuButtons)
        {
            button.interactable = false;
        }
    }

    //Level Select
    void LevelSelect_Enter()
    {
        levelSelectPanel.SetActive(true);
    }

    void LevelSelect_Exit()
    {
        levelSelectPanel.SetActive(false);
    }

    /*
     * Button management
     */

    //Main Menu
    public void PlayGame()
    {
        menuState.ChangeState(MenuState.LevelSelect);
    }

	public void QuitGame()
    {
        Application.Quit();
    }

    //Level Select
    public void BackToMenu()
    {
        menuState.ChangeState(MenuState.MainMenu);
    }

    public void NextLevel()
    {
        if(selectedLevelIndex == 4)
        {
            selectedLevelIndex = 0;
        }
        else
        {
            selectedLevelIndex++;
        }
        UpdateLevelData(selectedLevelIndex);
    }

    public void PrevLevel()
    {
        if(selectedLevelIndex == 0)
        {
            selectedLevelIndex = 4;
        }
        else
        {
            selectedLevelIndex--;
        }
        UpdateLevelData(selectedLevelIndex);
    }

    /*
     * Level Select Methods
     */

    void UpdateLevelData(int index)
    {
        levelSelectPanel.transform.Find("LevelNumber").GetComponent<Text>().text = "Level " + (index + 1);
        levelSelectPanel.transform.Find("LevelName").GetComponent<Text>().text = levels[index].levelName;
        levelSelectPanel.transform.Find("LevelImage").GetComponent<Image>().sprite = levels[index].levelImage;
        levelSelectPanel.transform.Find("HighScore").GetComponent<Text>().text = "High Score:\n" + levels[index].highScore;
        levelSelectPanel.transform.Find("DevScore").GetComponent<Text>().text = "Dev Score:\n" + levels[index].devScore;
        levelSelectPanel.transform.Find("StartButton").GetComponent<Button>().interactable = levels[index].isUnlocked;
    }
}

//Level Data
[System.Serializable]
public class LevelData
{
    public string levelName;
    public Sprite levelImage;
    public int highScore;
    public int devScore;
    public bool isUnlocked;
}