using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChapterMgr : MonoSingleton<ChapterMgr>
{
    // 当前章节数据
    public Chapter CurrentChapter { get; private set; }
    public string CurrentChapterName { get; private set; }
    // 所有章节标题
    public List<Image> allTitles;
    // 所有章节介绍
    public List<Image> allIntros;
    // 所有章节介绍跳过按钮
    public List<Button> allPassIntroButton;
 
    // 双字典存储（名称+ID）
    private readonly Dictionary<string, Chapter> _nameToChapter = new Dictionary<string, Chapter>();
    private readonly Dictionary<int, Chapter> _idToChapter = new Dictionary<int, Chapter>();

    /// <summary>
    /// 注册章节到管理器
    /// </summary>
    public void Register(Chapter chapter)
    {
        if (chapter == null)
        {
            Debug.LogError("无法注册空章节");
            return;
        }

        if (!_nameToChapter.ContainsKey(chapter.Name))
        {
            _nameToChapter.Add(chapter.Name, chapter);
            _idToChapter.Add(chapter.ID, chapter);
            Debug.Log($"注册章节: {chapter.Name} (ID: {chapter.ID})");
        }
        else
        {
            Debug.LogWarning($"章节已存在: {chapter.Name}");
        }
    }

    /// <summary>
    /// 通过名称获取章节
    /// </summary>
    public Chapter GetChapter(string chapterName)
    {
        if (_nameToChapter.TryGetValue(chapterName, out Chapter chapter))
        {
            return chapter;
        }
        Debug.LogError($"未找到章节: {chapterName}");
        return null;
    }

    /// <summary>
    /// 通过ID获取章节
    /// </summary>
    public Chapter GetChapter(int chapterId)
    {
        if (_idToChapter.TryGetValue(chapterId, out Chapter chapter))
        {
            return chapter;
        }
        Debug.LogError($"未找到章节ID: {chapterId}");
        return null;
    }

    /// <summary>
    /// 获取当前章节
    /// </summary>
    /// <returns></returns>
    public Chapter GetCurrentChapter()
    {
        return CurrentChapter;
    }

    /// <summary>
    /// 选择章节（外部调用入口保持不变）
    /// </summary>
    public void SelectChapter(string chapterName)
    {
        Chapter chapter = GetChapter(chapterName);
        if (chapter == null || chapter.Checkpoints.Count == 0)
        {
            Debug.LogError($"无效章节或关卡列表为空: {chapterName}");
            return;
        }

        CurrentChapter = chapter;
        CurrentChapterName = chapterName;

        // 从SaveManager中取出数据（这里暂时先启动该章节的第一个关卡）
        int checkpointId = 0;

        CheckpointMgr.Instance.StartCheckpoint(chapter.Checkpoints[checkpointId]);
        CanvasMgr.Instance.GameCanvas.gameObject.SetActive(true);

        // 保留原有的背景管理调用（已注释）
        // BackGroundManager.Instance.SetBackGroungSprite(chapterName);

    }

    /// <summary>
    /// 设置当前章节（不启动关卡，仅更新状态）
    /// </summary>
    /// <param name="chapterName">章节名称</param>
    /// <returns>是否设置成功</returns>
    public bool SetCurrentChapter(string chapterName)
    {
        if (string.IsNullOrEmpty(chapterName))
        {
            Debug.LogError("章节名称不能为空");
            return false;
        }

        if (!_nameToChapter.TryGetValue(chapterName, out Chapter chapter))
        {
            Debug.LogError($"章节不存在: {chapterName}");
            return false;
        }

        CurrentChapter = chapter;
        CurrentChapterName = chapterName;
        Debug.Log($"当前章节设置为: {chapterName} (ID: {chapter.ID})");

        // 可选：触发章节变更事件
        // OnChapterChanged?.Invoke(chapter);

        return true;
    }

    /// <summary>
    /// 通过ID设置当前章节
    /// </summary>
    public bool SetCurrentChapter(int chapterId)
    {
        if (!_idToChapter.TryGetValue(chapterId, out Chapter chapter))
        {
            Debug.LogError($"章节ID不存在: {chapterId}");
            return false;
        }

        return SetCurrentChapter(chapter.Name);
    }

    /// <summary>
    /// 通过实例设置当前章节
    /// </summary>
    public bool SetCurrentChapter(Chapter chapter)
    {
        if (!_idToChapter.ContainsKey(chapter.ID))
        {
            Debug.LogError($"章节不存在: {chapter.Name}");
            return false;
        }

        return SetCurrentChapter(chapter.Name);
    }

    // 进入当前章节，显现章节标题和章节介绍
    public IEnumerator DoShowTitle()
    {
        Image title = allTitles[CurrentChapter.ID - 1];
        Image intro = allIntros[CurrentChapter.ID - 1];

        title.gameObject.SetActive(true);
        intro.gameObject.SetActive(false);
        title.GetComponent<UIFade>().FadeIn(0.8f);

        yield return new WaitForSeconds(2.8f);
        title.GetComponent<UIFade>().FadeOut(0.8f);

        yield return new WaitForSeconds(1.0f);
        title.gameObject.SetActive(false);
        intro.gameObject.SetActive(true);
        intro.GetComponent<UIFade>().FadeIn(0.8f);

        // yield return new WaitForSeconds(1.0f);

        yield return new WaitForSeconds(2.8f);
        allPassIntroButton[CurrentChapter.ID - 1].gameObject.SetActive(true);
        allPassIntroButton[CurrentChapter.ID - 1].GetComponent<UIFade>().FadeIn(0.4f);
    }

    public void OnClickPassChapterIntro(GameObject passButton)
    {
        allPassIntroButton[CurrentChapter.ID - 1].interactable = false;

        MonoMgr.StartGlobalCoroutine(DoPassIntro(passButton));
    }

    IEnumerator DoPassIntro(GameObject passButton)
    {
        Image intro = allIntros[CurrentChapter.ID - 1];

        allPassIntroButton[CurrentChapter.ID - 1].GetComponent<UIFade>().FadeOut(0.4f);
     
        yield return new WaitForSeconds(0.8f);
        intro.GetComponent<UIFade>().FadeOut(0.8f);

        yield return new WaitForSeconds(1.0f);
        intro.gameObject.SetActive(false);
        allPassIntroButton[CurrentChapter.ID - 1].gameObject.SetActive(false);
        SelectChapter(CurrentChapterName);

        yield return new WaitForSeconds(3.0f);
        allPassIntroButton[CurrentChapter.ID - 1].gameObject.SetActive(true);
        allPassIntroButton[CurrentChapter.ID - 1].interactable = true;
    }

}