using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
/*
* https://blog.csdn.net/m0_37283423/article/details/73185391?utm_medium=distribute.pc_relevant.none-task-blog-BlogCommendFromMachineLearnPai2-1.channel_param&depth_1-utm_source=distribute.pc_relevant.none-task-blog-BlogCommendFromMachineLearnPai2-1.channel_param
* 使用方法：将该脚本附在镜头下面，然后切换场景时镜头不销毁
* 要调用的时候获得该脚本，然后调用StartSplash（）
*/

//@author joi 
public class SceneFadeInOut : MonoBehaviour
{
    public int guiDepth = 0;

    //将要加载的场景序号
    public int levelToLoadInt;
    public string levelToLoadString;
    //切换场景的纹理
    public Texture2D splashLogo;
    //淡入淡出的速度
    public float fadeSpeed = 0.8f;
    //保持纹理最高透明度的时间
    public float waitTime = 0f;
    //是否要等待输入
    public bool waitForInput = false;

    public bool startAutomatically = false;
    public bool IsDealPlayer = false;
    private float timeFadingInFinished = 0.0f;
    //处理切换场景后需要实现的事件
    public delegate void EventHandler();


    public event EventHandler trigger;
    public delegate void EventHandler2nd();


    public event EventHandler2nd triggerAtLoading;

    private bool ifAdditive = false;
    public bool FadeSound = true;
    //淡入淡出方式
    public enum SplashType
    {
        LoadNextLevelThenFadeOut,
        FadeOutThenLoadNextLevel
    }
    public SplashType splashType;

    private float alpha = 0.0f;
    //纹理的状态
    private enum FadeStatus
    {
        Paused,
        FadeIn,
        FadeWaiting,
        FadeOut
    }
    private FadeStatus status = FadeStatus.Paused;

    private Rect splashLogoPos = new Rect();
    //是否要自适应屏幕大小
    public enum LogoPositioning
    {
        Centered,
        Stretched
    }
    public LogoPositioning logoPositioning;

    private bool loadingNextLevel = false;

    void Start()
    {

        if (logoPositioning == LogoPositioning.Centered)
        {
            splashLogoPos.x = (Screen.width * 0.5f) - (splashLogo.width * 0.5f);
            splashLogoPos.y = (Screen.height * 0.5f) - (splashLogo.height * 0.5f);

            splashLogoPos.width = splashLogo.width;
            splashLogoPos.height = splashLogo.height;
        }
        else
        {
            splashLogoPos.x = 0;
            splashLogoPos.y = 0;

            splashLogoPos.width = Screen.width;
            splashLogoPos.height = Screen.height;
        }

        if (splashType == SplashType.LoadNextLevelThenFadeOut)
        {
            //var cameraList = FindObjectsOfType<Camera>();
            //if(cameraList.Length > 1)
            //Destroy(cameraList[cameraList.Length - 1].transform.gameObject);
            DontDestroyOnLoad(this.gameObject);
        }
        if (SceneManager.sceneCount <= 1)
        {
            //Debug.LogWarning("Invalid levelToLoad value.");
            //print(SceneManager.sceneCount);
        }
    }

    //开始切换，要跳转的场景和设置对应的切换速率
    public void StartSplash(int level, int i)
    {
        status = FadeStatus.FadeIn;
        levelToLoadInt = level;
        SetValue(i);
    }
    public void StartSplashScene(int level)
    {
        status = FadeStatus.FadeIn;
        levelToLoadInt = level;
    }
    public void StartSplashSceneAdditive(string name)
    {
        status = FadeStatus.FadeIn;
        levelToLoadString = name;
        ifAdditive = true;
    }
    public void StartSplashNextScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        status = FadeStatus.FadeIn;
        //print("SceneManager.sceneCountInBuildSettings" + SceneManager.sceneCountInBuildSettings);
        if (sceneIndex + 1 >= SceneManager.sceneCountInBuildSettings)
        {
            levelToLoadInt = 0;
        }
        else levelToLoadInt = sceneIndex + 1;
    }
    public void setIsDealPlayer(bool isDealPlayer)
    {
        IsDealPlayer = isDealPlayer;
    }
    public void SetValue(int i)
    {
        switch (i)
        {
            case 1:
                fadeSpeed = 0.8f;
                waitTime = 1.0f;
                break;
            case 2:
                fadeSpeed = 0.8f;
                waitTime = 1.0f;
                break;
            case 3:
                fadeSpeed = 0.8f;
                waitTime = 1.0f;
                break;
        }
    }
    void Update()
    {
        switch (status)
        {
            case FadeStatus.FadeIn:
                alpha += fadeSpeed * Time.deltaTime;
                if (FadeSound)
                {
                    //新增 AudioListener 淡入淡出控制
                    AudioListener.volume -= fadeSpeed * Time.deltaTime;
                }

                break;
            case FadeStatus.FadeWaiting:
                if ((!waitForInput && Time.time >= timeFadingInFinished + waitTime) || (waitForInput && Input.anyKey))
                {
                    status = FadeStatus.FadeOut;
                }
                break;
            case FadeStatus.FadeOut:
                alpha += -fadeSpeed * Time.deltaTime * 2;
                if (FadeSound)
                {
                    //新增 AudioListener 淡入淡出控制
                    AudioListener.volume += fadeSpeed * Time.deltaTime * 2;
                }

                if (alpha <= 0.0f)
                {
                    if (trigger != null)
                    {
                        trigger();
                    }

                    status = FadeStatus.Paused;
                    //新增：在FadeOut后删除前一个场景的相机 防止两个相机出现问题
                    var cameraList = FindObjectsOfType<Camera>();
                    if (cameraList.Length > 1) Destroy(this.gameObject);
                    //Additive只生效一次
                    ifAdditive = false;
                }
                break;
        }
    }

    void OnGUI()
    {

        GUI.depth = guiDepth;
        if (splashLogo != null)
        {
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, Mathf.Clamp01(alpha));
            GUI.DrawTexture(splashLogoPos, splashLogo);
        }
        if (alpha > 1.0f)
        {

            status = FadeStatus.FadeWaiting;

            timeFadingInFinished = Time.time;
            alpha = 1.0f;
            if (splashType == SplashType.LoadNextLevelThenFadeOut)
            {

                loadingNextLevel = true;
                if ((SceneManager.sceneCount) >= 1)
                {
                    LoadSceneAction();
                    if (triggerAtLoading != null)
                        triggerAtLoading();
                }
            }
        }
        if (alpha < 0.0f)
        {
            if (splashType == SplashType.FadeOutThenLoadNextLevel)
            {
                if (SceneManager.sceneCount >= 1)
                {
                    LoadSceneAction();
                }
            }
        }
    }

    private void LoadSceneAction()
    {
        //LoadScene
        if (ifAdditive)
        {
            SceneManager.LoadScene(levelToLoadString, LoadSceneMode.Additive);
        }
        else SceneManager.LoadScene(levelToLoadInt);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}