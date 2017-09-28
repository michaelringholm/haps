using UnityEngine;
using System.Collections;
using Assets.Script;
using Commands;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using TMPro;
using DG.Tweening;


// Quick
//  Good info when out of credits

// http://www.bidragsportalen.dk/danske-hjaelpeorganisationer.html
// http://www.hjaelpeorganisationer.dk/oversigt-over-hjaelpeorganisationer/
//- BØRNEfonden
//- Dyrenes Beskyttelse
//- Kræftens Bekæmpelse
//- Red Barnet
//- WWF Verdensnaturfonden

// TODO
//   Chat
//   Scratch
//   Tell up front if there's no money to be won! Check back soon. Need some notification!
//   Questions, multiple choice. Math, other?

//   Page with prices (maybe the startpage?)
//   Get a picture of chicken. Get drawing of chicken with animated mouth? Get chicken sound.
//   More xp goodies

// Neural net learning how player plays. Autoplay.

// Effects
//  - pairs of particle/antiparticle show up and spirals into eachother and dies (symmetric?)
//  - Game of life of some sort

public class Main : MonoBehaviour
{
    //string IOSAdsId = "1205587";
    //string AndroidAdsId = "1205586";

    [System.NonSerialized]
    public int[] WinLineCells = new int[] { 3, 4, 5 };

    public Button[] HoldButtons = new Button[3];
    public Button ButtonStep;
    public Button ButtonSpin;
    public GameObject IconGhost;
    public TextMeshProUGUI CreditsText;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI XpText;
    public TextMeshProUGUI WinInfo;
    public GameObject FloatingTextProto;
    public GameObject GameCanvas;
    public GameObject MenuCanvas;
    public GameObject OfflineCanvas;
    public GameObject HelpCanvas;
    public GameObject DonationCanvas;
    public GameObject AdResultCanvas;
    public GameObject PanelDarken;
    public Image XpBarImage;

    public ParticleSystem ParticlesXp;
    public ParticleSystem ParticlesCredits;
    public ParticleSystem ParticlesConfetti;
    public ParticleSystem ParticlesHearts;
    public ParticleSystem ParticlesLevel;
    public ParticleSystem ParticleBigSmiley;

    public WinHint WinHint;

    private Vector3 MenuCanvasPos;
    private Vector3 OfflineCanvasPos;
    private Vector3 HelpCanvasPos;
    private Vector3 DonationCanvasPos;

    private string userId_;
    private int credits_;
    private int xp_;
    private Levels levels_ = new Levels();

    public enum GameStateEnum { GetUserId, FullSpinRequired, Spinning, ShowWin, AwaitingUserDecisions };
    [System.NonSerialized]
    public GameStateEnum GameState = GameStateEnum.GetUserId;
     
    public enum GlobalState { Playing, Other };
    [System.NonSerialized]
    public static GlobalState State = Main.GlobalState.Other;

    private WinLine winLine_;

    [System.NonSerialized]
    public static LocalData LocalData = new LocalData();
    [System.NonSerialized]
    public static IServer Server;

    private Sequencer sequencer_ = new Sequencer();

    private Wheel wheel_;
    private bool[] holdFlags_ = new bool[3];
    private bool lastSpinWasFull_;

    private Color textColorNormal;
    private Color textColorDisabled;

    int showingOverlay_ = 0;

    void OnServerRequestSuccess(bool success)
    {
        ShowErrorCanvas(!success);
    }

    void ShowOverlayCanvas(GameObject canvas, Vector3 basePosition, Vector2 targetPosition, bool doDarkening, Action onClose = null)
    {
        StartCoroutine(ShowOverlayCanvasCo(canvas, basePosition, targetPosition, doDarkening, onClose));
    }

    IEnumerator ShowOverlayCanvasCo(GameObject canvas, Vector3 basePosition, Vector2 targetPosition, bool doDarkening, Action onClose)
    {
        // First close any existing overlay, ex. when selecting a menu item that opens another overlay
        State = GlobalState.Playing;
        yield return null;

        showingOverlay_++;

        canvas.transform.position = basePosition;

        const float Speed = 0.25f;
        State = GlobalState.Other;
        Vector3 target3 = targetPosition;
        target3.z = basePosition.z;
        canvas.transform.DOMove(target3, Speed);
        if (doDarkening)
        {
            PanelDarken.SetActive(true);
            PanelDarken.GetComponent<Image>().DOFade(0.8f, Speed);
        }

        // Wait until returning to game
        while (State == GlobalState.Other)
            yield return null;

        if (doDarkening)
        {
            PanelDarken.GetComponent<Image>().DOFade(0.0f, Speed);
        }

        yield return canvas.transform.DOMove(basePosition, Speed).WaitForCompletion();
        PanelDarken.SetActive(false);

        showingOverlay_--;

        if (onClose != null)
            onClose();
    }

    public void ShowMenu()
    {
        if (GameState != GameStateEnum.AwaitingUserDecisions && GameState != GameStateEnum.FullSpinRequired)
            return;

        ShowOverlayCanvas(MenuCanvas, MenuCanvasPos, Vector2.zero, true);
    }

    public void ShowHelp()
    {
        ShowOverlayCanvas(HelpCanvas, HelpCanvasPos, Vector2.zero, false);
    }

    public void ShowDonations()
    {
        ShowOverlayCanvas(DonationCanvas, DonationCanvasPos, Vector2.zero, false);
    }

    void ShowErrorCanvas(bool show)
    {
        if (show)
        {
            if (showingOverlay_ == 0)
                ShowOverlayCanvas(OfflineCanvas, OfflineCanvasPos, new Vector2(0.0f, -2.0f), true);
        }
        else
        {
            State = GlobalState.Playing;
        }
    }

    // Startup:
    //  1) get userId (either local storage or new from server)
    //  2) get sequence (and distribution since distId is 0)
    //  3) play
    void Start()
    {
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);

        wheel_ = GameObject.Find("FruitGrid").GetComponent<Wheel>();

        Server = new Server(OnServerRequestSuccess);

        textColorNormal = ButtonSpin.gameObject.GetComponent<Image>().color;
        textColorDisabled = Color.gray;

        WinHint.gameObject.SetActive(false);

        MenuCanvasPos = MenuCanvas.transform.position;
        HelpCanvasPos = HelpCanvas.transform.position;
        OfflineCanvasPos = OfflineCanvas.transform.position;
        DonationCanvasPos = DonationCanvas.transform.position;

        ShowButtons(fullSpinRequired: true);

        // Winfo is visible since its nice to see in the designer, remove it
        var tmpCol = WinInfo.color;
        tmpCol.a = 0.0f;
        WinInfo.color = tmpCol;
        WinInfo.gameObject.SetActive(false);

        StartCoroutine(GameLoop());
    }

    void InitializeLocalData()
    {
        PlayerPrefs.SetInt(LocalData.CreditsKey, 50);
        PlayerPrefs.SetInt(LocalData.XpKey, 0);
        PlayerPrefs.Save();
    }

    void LoadLocalData()
    {
        credits_ = PlayerPrefs.GetInt(LocalData.CreditsKey, 0);
        xp_ = PlayerPrefs.GetInt(LocalData.XpKey, 0);
    }

    IEnumerator GetUserId()
    {
        // Missing user id is used to determine if this is first run on install
        string id = PlayerPrefs.GetString(LocalData.PlayerIdKey, string.Empty);
        id = "";
        if (id != string.Empty)
        {
            GotUserId(id);
        }
        else
        {
            InitializeLocalData();
            StartCoroutine(Server.CreateUser("proto", GotUserId));
        }

        // Wait until we have a user id
        while (string.IsNullOrEmpty(userId_))
            yield return null;
    }

    void GotUserId(string id)
    {
        userId_ = id;
        PlayerPrefs.SetString(LocalData.PlayerIdKey, id);
        PlayerPrefs.Save();

        StartCoroutine(Server.RequestSequence(userId_, GotSequence));

        LoadLocalData();
        UpdateCredits();
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
        XpText.text = string.Format("{0} / {1}", levels_.XpThisLevel, levels_.RequiredXpThisLevel);
        if (change != 0)
        {
            XpBarImage.DOColor(Color.white, 0.1f).SetEase(Ease.Flash).SetLoops(2, LoopType.Yoyo);

            bool levelUp = levels_.CurrentLevel != 1 && prevLevel != levels_.CurrentLevel;
            if (levelUp)
            {
                LevelText.transform.DOPunchScale(new Vector2(0.2f, 0.2f), 3.0f, 2, 0.2f);
                ParticlesLevel.Play();
            }
        }

        LevelText.text = levels_.CurrentLevel.ToString();

        if (save)
            PlayerPrefs.Save();
    }

    private void UpdateCredits(int change = 0, bool save = false)
    {
        credits_ += change;
        if (credits_ < 0)
            credits_ = 0;

        if (save)
            PlayerPrefs.Save();

        CreditsText.text = credits_.ToString();
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
        GameState = GameStateEnum.GetUserId;

        // First get userId
        yield return GetUserId();

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
            var spinLabelTween = spinLabel.transform.DOPunchScale(new Vector2(0.2f, 0.2f), 2.0f, 0, 0.0f).SetLoops(-1);

            // Wait for user doing a full spin
            yield return WaitForState(GameStateEnum.Spinning);
            spinLabelTween.Kill();
            spinLabel.transform.DOScale(1.0f, 0.1f);

            CheckForWinHint(hide: true);

            SetWinLine(new int[] { 3, 4, 5}); // Default middle

            char winChar;
            bool playerInterfered = false;
            while (!playerInterfered)
            {
                ShowButtons(fullSpinRequired: false);

                // Wait for spin to be done
                while (wheel_.IsSpinning)
                    yield return null;

                if (CheckWin(out winChar))
                {
                    // The player won something on initial spin, celebrate a bit, then back to full spin
                    SetGameState(GameStateEnum.ShowWin);
                    yield return ShowWin(winChar);
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

            if (CheckWin(out winChar))
            {
                // The player won something on secondary spin, celebrate a bit, then back to full spin
                SetGameState(GameStateEnum.ShowWin);
                yield return ShowWin(winChar);
                continue;
            }

            // The player didn't win anything this round. Back to square one.
        }
    }

    private IEnumerator ShowWin(char winChar)
    {
        Win win = Win.GetWin(winChar);

        if (win.Type == WinType.Credits)
        {
            yield return ShowCreditWin(winChar, win.Amount);
        }
        else if (win.Type == WinType.Xp)
        {
            yield return ShowXpWin(winChar, win.Amount);
        }
        else if (win.Type == WinType.Money)
        {
            yield return ShowMoneyWin(winChar, win.Amount);
        }
    }

    SpriteRenderer CreateGhost(SpriteRenderer source)
    {
        var ghost = Instantiate(IconGhost);
        ghost.transform.position = source.transform.position;
        ghost.transform.localScale = source.transform.lossyScale;
        var r = ghost.GetComponent<SpriteRenderer>();
        r.sortingOrder = 100;
        r.sprite = source.sprite;
        return r;
    }

    // TODO coords are weird (parent scale etc.)
    private void FireFloatingText(string msg, Vector2 pos)
    {
        const float FloatTime = 3.0f;
        var obj = (GameObject)Instantiate(FloatingTextProto, GameCanvas.transform);
        obj.transform.position = pos;
        obj.transform.DOMoveY(pos.y + 1.0f, FloatTime).SetEase(Ease.Linear);
        obj.transform.localScale = new Vector2(2.0f, 2.0f);
        var textObj = obj.GetComponent<TextMeshProUGUI>();
        textObj.text = msg;
        var color = textObj.color;
        color.a = 0.0f;
        textObj.color = color;
        const float FadeTime = 0.5f;

        textObj.DOFade(1.0f, FadeTime);
        textObj.DOFade(0.0f, FadeTime).SetDelay(FloatTime - FadeTime);

        GameObject.Destroy(obj, FloatTime);
    }

    private IEnumerator FireGhosts(float ghostRemoveDelay)
    {
        var renderers = wheel_.GetRenderers(WinLineCells);
        List<SpriteRenderer> ghosts = new List<SpriteRenderer>();

        foreach (var srcRenderer in renderers)
            ghosts.Add(CreateGhost(srcRenderer));

        // Blink match line
        var mat = ghosts[0].sharedMaterial;
        var tween = DOTween.To(() => mat.GetFloat("_FlashAmount"), x => mat.SetFloat("_FlashAmount", x), 0.8f, 0.2f).SetEase(Ease.InFlash).SetLoops(6, LoopType.Yoyo);

        yield return tween.WaitForCompletion();

        // Move ghosts to top
        for (int i = 0; i < 3; ++i)
        {
            const float Delay = 0;
            var ghost = ghosts[i];

            const float MoveTime = 0.4f;
            const float Size = 0.6f;
            const float SeperationDelay = 0.1f;

            ghost.transform.DOMove(new Vector2(-Size + (i * Size), 3.2f), MoveTime).SetDelay(Delay + i * SeperationDelay).SetEase(Ease.OutCubic);
            ghost.transform.DOScale(Size, MoveTime).SetDelay(Delay + i * SeperationDelay);
        }

        // Trigger async delayed ghost removal
        StartCoroutine(FadeAwayCreditWin(ghostRemoveDelay, ghosts, WinInfo));
    }

    private IEnumerator ShowCreditWin(char winChar, int amount)
    {
        const float Delay = 0.25f;
        float GhostRemoveDelay = 2.0f + ((amount / 2) * Delay);
        yield return FireGhosts(GhostRemoveDelay);

        // Show win info line
        WinInfo.text = string.Format("+{0} Kredit", amount);
        WinInfo.gameObject.SetActive(true);

        yield return WinInfo.DOFade(1.0f, 0.5f).SetDelay(0.5f).WaitForCompletion();

        for (int i = amount - 2; i >= 0; i -= 2) // 4, 10, 20 -> 2, 5, 10
        {
            ParticlesCredits.Emit(5);

            WinInfo.text = string.Format("+{0} Kredit", i < 0 ? 0 : i);
            WinInfo.transform.DOPunchScale(new Vector2(0.3f, 0.3f), Delay, 2, 0.2f);
            CreditsText.transform.DOPunchScale(new Vector2(0.2f, 0.2f), Delay, 2, 0.2f);

            UpdateCredits(2);

            yield return new WaitForSeconds(Delay);
        }

        UpdateCredits(0, save: true);
    }

    private IEnumerator ShowXpWin(char winChar, int amount)
    {
        const float Delay = 0.25f;
        float GhostRemoveDelay = 2.0f + ((amount / 10) * Delay);
        yield return FireGhosts(GhostRemoveDelay);

        // Show win info line
        WinInfo.text = string.Format("+{0} XP", amount);
        WinInfo.gameObject.SetActive(true);

        yield return WinInfo.DOFade(1.0f, 0.5f).SetDelay(0.5f).WaitForCompletion();
        for (int i = amount - 10; i >= 0; i -= 10) // 10, 50, 100 -> 1, 5, 10
        {
            ParticlesXp.Emit(5);

            WinInfo.text =  string.Format("+{0} XP", i);
            WinInfo.transform.DOPunchScale(new Vector2(0.3f, 0.3f), Delay, 2, 0.2f);

            UpdateXp(10);
            yield return new WaitForSeconds(Delay);
        }

        UpdateXp(0, save: true);
    }

    private IEnumerator ShowMoneyWin(char winChar, int amount)
    {
        string requirementText = Win.GetRequirementText(winChar, levels_.CurrentLevel);
        string winText = Win.GetWinText(winChar);

        bool requirementsMet = requirementText == "";
        if (!requirementsMet)
        {
            // Level too low
            WinInfo.text = requirementText;
            WinInfo.transform.DOPunchScale(new Vector2(0.1f, 0.1f), 1.0f, 2, 0.2f);
        }
        else
        {
            // User won money!
            WinInfo.text = winText;
        }

        float GhostRemoveDelay = requirementText == "" ? 3.0f : 5.0f;
        yield return FireGhosts(GhostRemoveDelay);

        // Show win info line
        WinInfo.gameObject.SetActive(true);
        yield return WinInfo.DOFade(1.0f, 0.5f).SetDelay(0.5f).WaitForCompletion();

        if (requirementsMet)
        {
            ParticlesConfetti.Play();
            // TODO: Update stats
        }
    }

    private void ShowOutOfCredits()
    {
        WinInfo.text = "Du er loebet toer for kredit! Se en reklame for at få flere.";
        WinInfo.transform.DOPunchScale(new Vector2(0.1f, 0.1f), 1.0f, 2, 0.2f);
    }

    IEnumerator FadeAwayCreditWin(float delay, List<SpriteRenderer> ghosts, TextMeshProUGUI infoText)
    {
        // Wait for spinning, menu or timeout
        yield return WaitForState(GameStateEnum.Spinning, requirePlaying: true, timeout: delay);

        // Fade away
        const float FadeTime = 0.5f;
        WinInfo.DOFade(0.0f, FadeTime);
        foreach (var ghost in ghosts)
            ghost.DOFade(0.0f, FadeTime);

        yield return new WaitForSeconds(FadeTime);

        infoText.gameObject.SetActive(false);

        foreach (var ghost in ghosts)
            GameObject.Destroy(ghost.gameObject);
    }

    private bool CheckWin(out char winChar)
    {
        winChar = '\0';
        var final9 = wheel_.GetFinal9();
        int id0 = WinLineCells[0];
        int id1 = WinLineCells[1];
        int id2 = WinLineCells[2];
        if ((final9[id0] == final9[id1]) && (final9[id1] == final9[id2]))
        {
            // All 3 are equal
            winChar = final9[id0];
            return true;
        }

        return false;
    }

    void GotSequence(string seq)
    {
        var sequence = Util.FromJson<ItemSequence>(seq);
        sequencer_.SetSequence(sequence);
    }

    void SetGameState(GameStateEnum newState)
    {
        GameState = newState;
        GameStateChanged(newState);
    }

    void GameStateChanged(GameStateEnum newState)
    {
//        Debug.LogFormat("GameState changed to: {0}", newState.ToString());
    }

    private void StartSpin(bool fullSpin)
    {
        if (!sequencer_.NeedsUpdate())
        {
            var row0 = new List<char>();
            var row1 = new List<char>();
            var row2 = new List<char>();
            sequencer_.Get(fullSpin, row0, row1, row2);

            wheel_.SetNextSpin(row0, row1, row2);
            wheel_.DoSpin(fullSpin, holdFlags_);

            lastSpinWasFull_ = fullSpin;

            SetGameState(GameStateEnum.Spinning);
        }
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

        if (credits_ > 0)
        {
            UpdateCredits(-1);
            StartSpin(fullSpin: true);
        }
        else
        {
            ShowOutOfCredits();
        }
    }

    public void OnOneDownClick()
    {
        if (GameState != GameStateEnum.AwaitingUserDecisions)
            return;

        if (credits_ > 0)
        {
            UpdateCredits(-1);
            StartSpin(fullSpin: false);
        }
        else
        {
            ShowOutOfCredits();
        }
    }

    void OnEnable()
    {
        winLine_ = GameObject.Find("WinLine").GetComponent<WinLine>();
    }

    void CheckForWinHint(bool hide = false)
    {
        if (hide || GameState != GameStateEnum.AwaitingUserDecisions)
        {
            ShowHint(false);
            return;
        }

        var final9 = wheel_.GetFinal9();
        int id0 = WinLineCells[0];
        int id1 = WinLineCells[1];
        int id2 = WinLineCells[2];
        char c0 = final9[id0];
        char c1 = final9[id1];
        char c2 = final9[id2];

        char matchChar = '-';
        if (c0 == c1)
            matchChar = c0;
        else if (c0 == c2)
            matchChar = c0;
        else if (c1 == c2)
            matchChar = c1;

        bool showHint = matchChar != '-';
        if (showHint)
        {
            WinHint.UpdateHint(wheel_.GetSpriteForIcon(matchChar), matchChar, levels_.CurrentLevel);
        }

        ShowHint(showHint);
    }

    void ShowHint(bool show)
    {
        if (WinHint.gameObject.activeInHierarchy != show)
        {
            WinHint.gameObject.SetActive(show);
        }
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

    public void ShowAd()
    {
#if UNITY_ADS
        if (Advertisement.IsReady())
        {
            var options = new ShowOptions { resultCallback = HandleShowAdResult };
            Advertisement.Show("", options);
            Advertisement.Show();
        }
        else
        {
            Debug.LogError("Ad fail");
        }
#endif
    }

#if UNITY_ADS
    private void HandleShowAdResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                // Exploiting offline canvas position
                ShowOverlayCanvas(AdResultCanvas, OfflineCanvasPos, new Vector2(0.0f, -2.0f), true, () => {
                    UpdateCredits(20, true);
                    CreditsText.transform.DOScale(1.5f, 0.5f).SetLoops(6, LoopType.Yoyo).SetEase(Ease.InOutCubic);
                    ParticleBigSmiley.Emit(1);
                });
                // No reward when pressing escape. Need event (lambda).
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }
#endif

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
        Server.Stop();
    }
}
