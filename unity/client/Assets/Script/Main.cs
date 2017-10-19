using UnityEngine;
using System.Collections;
using Assets.Script;
using HapsCommands;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Facebook.Unity;

public class Main : MonoBehaviour
{
    //string IOSAdsId = "1205587";
    //string AndroidAdsId = "1205586";

    [System.NonSerialized]
    public int[] WinLineCells = new int[] { 3, 4, 5 };

    public Button[] HoldButtons = new Button[3];
    public Button ButtonStep;
    public Button ButtonSpin;
    public GameObject GameCanvas;
    public Image XpBarImage;

    public ParticleSystem ParticlesXp;
    public ParticleSystem ParticlesConfetti;
    public ParticleSystem ParticlesHearts;
    public ParticleSystem ParticlesLevel;
    public ParticleSystem ParticleBigSmiley;

    private int xp_;
    private Levels levels_ = new Levels();

    public enum GameStateEnum { None, FullSpinRequired, Spinning, ShowWin, AwaitingUserDecisions, CircleWave };
//    [System.NonSerialized]
    public GameStateEnum GameState = GameStateEnum.None;
     
    public enum GlobalState { Playing, Other };
    [System.NonSerialized]
    public static GlobalState State = Main.GlobalState.Other;

    private WinLine winLine_;

    [System.NonSerialized]
    public static IServer Server;

    private Sequencer sequencer_ = new Sequencer();

    private Wheel wheel_;
    private bool[] holdFlags_ = new bool[3];
    private bool lastSpinWasFull_;

    private Color textColorNormal;
    private Color textColorDisabled;

    void OnServerRequestSuccess(bool success)
    {
    }

    void Start()
    {
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);

        wheel_ = GameObject.Find("FruitGrid").GetComponent<Wheel>();

        Server = new Server(OnServerRequestSuccess);

        textColorNormal = ButtonSpin.gameObject.GetComponent<Image>().color;
        textColorDisabled = Color.gray;

        ShowButtons(fullSpinRequired: true);

        StartCoroutine(GameLoop());

        StartCoroutine(Server.RequestSequence("me", GotSequence));
    }

    void InitializeLocalData()
    {
        PlayerPrefs.SetInt(LocalData.XpKey, 0);
        PlayerPrefs.Save();
    }

    void LoadLocalData()
    {
        xp_ = PlayerPrefs.GetInt(LocalData.XpKey, 0);
    }

    void StartGame()
    {
        LoadLocalData();
        UpdateXp();

        State = GlobalState.Playing;
        SetGameState(GameStateEnum.FullSpinRequired);
    }

    private void UpdateXp(int change = 0, bool save = false)
    {
        xp_ += change;

        int prevLevel = levels_.CurrentLevel;
        levels_.UpdateXp(xp_);
        XpBarImage.fillAmount = levels_.XpBarPos;
        if (change != 0)
        {
            XpBarImage.DOColor(Color.white, 0.1f).SetEase(Ease.Flash).SetLoops(2, LoopType.Yoyo);

            bool levelUp = levels_.CurrentLevel != 1 && prevLevel != levels_.CurrentLevel;
            if (levelUp)
            {
                ParticlesLevel.Play();
            }
        }

        if (save)
            PlayerPrefs.Save();
    }

    IEnumerator WaitForState(GameStateEnum state, bool requirePlaying = false, float timeout = -1)
    {
        float endTime = Time.time + timeout;
        while (GameState != state)
        {
            if (requirePlaying && State != GlobalState.Playing)
                break;

            if (timeout != -1 && Time.time >= endTime)
                break;
            yield return null;
        }
    }

    void ShowButtons(bool fullSpinRequired)
    {
        float speed = 0.5f;
        if (fullSpinRequired)
        {
            HoldButtons[0].gameObject.GetComponent<Image>().DOColor(textColorDisabled, speed);
            HoldButtons[1].gameObject.GetComponent<Image>().DOColor(textColorDisabled, speed);
            HoldButtons[2].gameObject.GetComponent<Image>().DOColor(textColorDisabled, speed);
            ButtonStep.gameObject.GetComponent<Image>().DOColor(textColorDisabled, speed);
        }
        else
        {
            HoldButtons[0].gameObject.GetComponent<Image>().DOColor(textColorNormal, speed);
            HoldButtons[1].gameObject.GetComponent<Image>().DOColor(textColorNormal, speed);
            HoldButtons[2].gameObject.GetComponent<Image>().DOColor(textColorNormal, speed);
            ButtonStep.gameObject.GetComponent<Image>().DOColor(textColorNormal, speed);
        }
    }

    IEnumerator GameLoop()
    {
        State = GlobalState.Other;
        GameState = GameStateEnum.None;

        StartGame();

        // Outer game loop
        while (true)
        {
NewRound:
            ShowButtons(fullSpinRequired: true);

            SetGameState(GameStateEnum.FullSpinRequired);

            holdFlags_[0] = false;
            holdFlags_[1] = false;
            holdFlags_[2] = false;
            UpdateHoldButtons();

            var spinLabel = ButtonSpin.GetComponentInChildren<TextMeshProUGUI>();
            var spinLabelTween = spinLabel.transform.DOPunchScale(new Vector2(0.3f, 0.3f), 1.0f, 2, 1).SetLoops(-1);

            // Wait for user doing a full spin
            yield return WaitForState(GameStateEnum.Spinning);
            spinLabelTween.Kill();
            spinLabel.transform.DOScale(1.0f, 0.1f);

            CheckForWinHint(hide: true);

            SetWinLine(new int[] { 3, 4, 5}); // Default middle

            int winIdx;
            bool playerInterfered = false;
            while (!playerInterfered)
            {
                ShowButtons(fullSpinRequired: false);

                // Wait for spin to be done
                while (wheel_.IsSpinning)
                    yield return null;

                if (CheckWin(out winIdx))
                {
                    // The player won something on initial spin, celebrate a bit, then back to full spin
                    SetGameState(GameStateEnum.ShowWin);
                    yield return ShowWin(winIdx);
                    goto NewRound;
                }

                // Player did not win on initial spin, wait for decisions
                SetGameState(GameStateEnum.AwaitingUserDecisions);
                CheckForWinHint();
                yield return WaitForState(GameStateEnum.Spinning);

                CheckForWinHint(hide: true);

                bool playerUsedHold = holdFlags_[0] != false || holdFlags_[1] != false || holdFlags_[2] != false;
                // Keep looping full spins until player holds or nudges
                playerInterfered = playerUsedHold || !lastSpinWasFull_;

                // Reset win line to default if player didn't interfere, since it doesn't matter where it is
                //if (!playerInterfered)
                //    SetWinLine(new int[] { 3, 4, 5 }); // Default middle
            }

            // Player used hold or nudge. Wait for spin to be done
            while (wheel_.IsSpinning)
                yield return null;

            if (CheckWin(out winIdx))
            {
                // The player won something on secondary spin, celebrate a bit, then back to full spin
                SetGameState(GameStateEnum.ShowWin);
                yield return ShowWin(winIdx);
                continue;
            }

            // The player didn't win anything this round. Back to square one.
        }
    }

    private IEnumerator ShowWin(int winIdx)
    {
        Win win = Win.GetWin(winIdx);

        if (win.Type == WinType.Xp)
        {
            yield return ShowXpWin(winIdx, win.Amount);
        }
        else if (win.Type == WinType.Money)
        {
            yield return ShowMoneyWin(winIdx, win.Amount);
        }
    }

    private IEnumerator ShowXpWin(int winIdx, int amount)
    {
        const float Delay = 0.25f;
        float GhostRemoveDelay = 2.0f + ((amount / 10) * Delay);

        for (int i = amount - 10; i >= 0; i -= 10) // 10, 50, 100 -> 1, 5, 10
        {
            ParticlesXp.Emit(5);

            UpdateXp(10);
            yield return new WaitForSeconds(Delay);
        }

        UpdateXp(0, save: true);
    }

    private IEnumerator ShowMoneyWin(int winIdx, int amount)
    {
        string requirementText = Win.GetRequirementText(winIdx, levels_.CurrentLevel);
        string winText = Win.GetWinText(winIdx);

        bool requirementsMet = requirementText == "";
        if (!requirementsMet)
        {
            // Level too low
        }
        else
        {
            // User won money!
        }

        if (requirementsMet)
        {
            ParticlesConfetti.Play();
            // TODO: Update stats
        }

        yield return null;
    }

    private bool CheckWin(out int winIdx)
    {
        winIdx = -1;
        var final9 = wheel_.GetFinal9();
        int id0 = WinLineCells[0];
        int id1 = WinLineCells[1];
        int id2 = WinLineCells[2];
        if ((final9[id0] == final9[id1]) && (final9[id1] == final9[id2]))
        {
            // All 3 are equal
            winIdx = final9[id0];
            AppLog.LogLine("Matched 3: {0}", winIdx);
            return true;
        }

        return false;
    }

    void GotSequence(ItemSequence seq)
    {
        sequencer_.SetSequence(seq);
    }

    void SetGameState(GameStateEnum newState)
    {
        AppLog.LogLine("Setting new game state: {0}", newState);

        GameState = newState;
        GameStateChanged(newState);
    }

    void GameStateChanged(GameStateEnum newState)
    {
    }

    private void StartSpin(bool fullSpin)
    {
        if (!fullSpin)
        {
            wheel_.DoOneDown(holdFlags_);
        }
        else
        {
            if (!sequencer_.NeedsUpdate())
            {
                var row0 = new List<int>();
                var row1 = new List<int>();
                var row2 = new List<int>();
                sequencer_.Get(fullSpin, row0, row1, row2);

                wheel_.SetNextSpin(row0, row1, row2);
                wheel_.DoFullSpin(holdFlags_);
            }
        }

        lastSpinWasFull_ = fullSpin;
        SetGameState(GameStateEnum.Spinning);
    }

    public void OnHold0Click()
    {
        if (GameState != GameStateEnum.AwaitingUserDecisions)
            return;

        bool bothOtherAreOn = holdFlags_[1] && holdFlags_[2];
        holdFlags_[0] = !holdFlags_[0] && !bothOtherAreOn;
        UpdateHoldButton(0);
    }

    public void OnHold1Click()
    {
        if (GameState != GameStateEnum.AwaitingUserDecisions)
            return;

        bool bothOtherAreOn = holdFlags_[0] && holdFlags_[2];
        holdFlags_[1] = !holdFlags_[1] && !bothOtherAreOn;
        UpdateHoldButton(1);
    }

    public void OnHold2Click()
    {
        if (GameState != GameStateEnum.AwaitingUserDecisions)
            return;

        bool bothOtherAreOn = holdFlags_[0] && holdFlags_[1];
        holdFlags_[2] = !holdFlags_[2] && !bothOtherAreOn;
        UpdateHoldButton(2);
    }

    void UpdateHoldButton(int idx)
    {
        Color color = holdFlags_[idx] ? Color.gray : Color.white;
        var colorBlock = HoldButtons[idx].colors;
        colorBlock.normalColor = color;
        colorBlock.highlightedColor = color;
        HoldButtons[idx].colors = colorBlock;
    }

    void UpdateHoldButtons()
    {
        UpdateHoldButton(0);
        UpdateHoldButton(1);
        UpdateHoldButton(2);
    }

    public void OnGoClick()
    {
        if (GameState != GameStateEnum.AwaitingUserDecisions && GameState != GameStateEnum.FullSpinRequired)
            return;

        StartSpin(fullSpin: true);
    }

    public void OnOneDownClick()
    {
        if (GameState != GameStateEnum.AwaitingUserDecisions)
            return;

        StartSpin(fullSpin: false);
    }

    public void OnPopButtonClick()
    {

    }

    void OnEnable()
    {
        winLine_ = GameObject.Find("WinLine").GetComponent<WinLine>();
    }

    void CheckForWinHint(bool hide = false)
    {
        if (hide || GameState != GameStateEnum.AwaitingUserDecisions)
        {
            return;
        }

        var final9 = wheel_.GetFinal9();
        int id0 = WinLineCells[0];
        int id1 = WinLineCells[1];
        int id2 = WinLineCells[2];
        int c0 = final9[id0];
        int c1 = final9[id1];
        int c2 = final9[id2];

        int matchCode = -1;
        if (c0 == c1)
            matchCode = c0;
        else if (c0 == c2)
            matchCode = c0;
        else if (c1 == c2)
            matchCode = c1;
    }

    public void SetWinLine(int[] cells)
    {
        WinLineCells = cells;
        winLine_.UpdateTarget(cells, 50);

        CheckForWinHint();
    }

    void Back()
    {
        State = GlobalState.Playing;
    }

    public void AdDialogClosed()
    {
        Back();
    }

    public void Update()
    {
        if (State == GlobalState.Other && Input.GetKeyDown(KeyCode.Escape))
        {
            Back();
        }
    }

    public void OnDestroy()
    {
    }

    private void OnApplicationPause(bool pause)
    {
        AppLog.LogLine("Main: OnApplicationPause, pause = {0}", pause);

        bool resumed = !pause;
        if (resumed)
        {
            if (!FB.IsInitialized)
            {
                Util.LoadSplashScene();
            }
            else
            {
                FB.ActivateApp();
            }
        }
    }
}
