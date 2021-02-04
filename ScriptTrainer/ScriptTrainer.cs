using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Collections;
using ConfigurationManager;
using UnityEngine.Experimental.Rendering;
using System.Reflection;
using BepInEx.Logging;
using System.Text.RegularExpressions;

namespace ScriptTrainer
{
    [BepInPlugin("aoe.top.ScriptTrainer", "[戴森球计划] 内置修改器 By:小莫", "1.0.0")]
    public class ScriptTrainer : BaseUnityPlugin
    {


        public bool DisplayingWindow;
        private bool ShowAddItemwindow = true;
        private bool ShowOtherWindow = false;
        private bool ShowUnlockResearchWindow = false;
        private int AddItemNum = 1000;

        private string searchItem = "";

        private Rect HeaderTitleRect;
        private Rect HeaderTableRect;
        private Rect windowRect;
        private Rect AddItemTableRect;
        private Rect UnlockResearchTableRect;
        private GUIStyle ButtonStyle;
        Vector2 scrollPosition;
        Vector2 scrollPosition2;

        // 启动按键
        private ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter { get; set; }
        private static ConfigEntry<int> userCount;

        [Obsolete]
        // 注入脚本时会自动调用Start()方法 执行在Awake()方法后面
        public void Start()
        {
            // 允许用户自定义启动快捷键
            ShowCounter = Config.AddSetting("修改器快捷键", "Key", new BepInEx.Configuration.KeyboardShortcut(KeyCode.F9));

            // 自定义默认给予物品数量
            userCount = Config.AddSetting("启动游戏时默认给予数量", "val:", AddItemNum, new ConfigDescription("你可以根据自己的需求,自由的调整默认给予物品的数量", new AcceptableValueRange<int>(10, 100000)));
            AddItemNum = userCount.Value;

            // 日志输出
            Debug.Log("脚本已启动");
        }

        public void Update()
        {
            if (ShowCounter.Value.IsDown())
            {
                //Debug.Log("按下按键");
                DisplayingWindow = !DisplayingWindow;
                if (DisplayingWindow)
                {
                    Debug.Log("打开窗口");
                }
                else
                {
                    Debug.Log("关闭窗口");
                }
            }
        }
        
        // GUI函数
        private void OnGUI()
        {
            if (this.DisplayingWindow)
            {
                //计算区域
                this.ComputeRect();

                Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                // rgba(116, 125, 140,1.0)
                texture2D.SetPixel(0, 0, new Color32(116, 125, 140, 255));
                texture2D.Apply();
                GUIStyle myWindowStyle = new GUIStyle
                {                    
                    normal = new GUIStyleState  // 正常样式
                    {
                        textColor = new Color32(47, 53, 66, 1),
                        background = texture2D
                    },
                    wordWrap = true,    // 自动换行
                    alignment = TextAnchor.UpperCenter,  //对其方式
                };
                // 定义一个新窗口
                //windowRect = new Rect(200, 200, 700, 400);
                windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "", myWindowStyle);

            }
        }
        // 初始样式
        void ComputeRect()
        {
            int num = Mathf.Min(Screen.width, 800);
            int num2 = (Screen.height < 400) ? Screen.height : (400);
            int num3 = Mathf.RoundToInt((float)(Screen.width - num) / 2f);
            int num4 = Mathf.RoundToInt((float)(Screen.height - num2) / 2f);
            this.windowRect = new Rect((float)num3, (float)num4, (float)num, (float)num2);

            // 头部
            this.HeaderTitleRect = this.windowRect;
            this.HeaderTitleRect.position = Vector2.zero;
            // 选项卡按钮
            //this.HeaderTableRect.width = 150f;
            this.HeaderTableRect = new Rect(0,40, 800, 40);
            // 添加物品样式
            this.AddItemTableRect = new Rect(0, 90, 800, 300);
            // 解锁公式
            this.UnlockResearchTableRect = new Rect(0, 90, 800, 300);

            // 按钮样式
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture2D.SetPixel(0, 0, new Color32(30, 144, 255, 255));    // rgba(30, 144, 255,1.0)
            texture2D.Apply();
            Texture2D texture2D2 = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture2D2.SetPixel(0, 0, new Color32(112, 161, 255, 255));  // rgba(112, 161, 255,1.0)
            texture2D2.Apply();
            ButtonStyle = new GUIStyle
            {
                normal = new GUIStyleState  // 正常样式
                {
                    textColor = Color.white,
                    background = texture2D
                },
                active = new GUIStyleState  // 点击样式
                {
                    textColor = Color.white,
                    background = texture2D2
                },
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 40,
                fixedWidth = 90,
                margin = new RectOffset(5, 7, 0, 5),
            };
        }
        // 头部标题
        void HeaderTitle(Rect HeaderTitleRect)
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            // rgba(255, 99, 72,1.0)
            texture2D.SetPixel(0, 0, new Color32(255, 99, 72, 255));
            texture2D.Apply();
            GUIStyle guistyle = new GUIStyle
            {
                normal = new GUIStyleState
                {
                    textColor = Color.white,
                    background = texture2D
                },
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 30,
                fontSize = 16
            };
            
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginArea(HeaderTitleRect);
                GUILayout.Label("[戴森球计划] 内置修改器 By:小莫", guistyle);               
            }

            // 结尾
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        // 头部选项卡
        void HeaderTable(Rect HeaderTableRect)
        {            
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);            
            texture2D.SetPixel(0, 0, new Color32(255, 99, 72, 255));    // rgba(255, 99, 72,1.0)
            texture2D.Apply();
            Texture2D texture2D2 = new Texture2D(1, 1, TextureFormat.RGBA32, false);            
            texture2D2.SetPixel(0, 0, new Color32(255, 127, 80, 255));  // rgba(255, 127, 80,1.0)
            texture2D2.Apply();

            // 按钮样式
            GUIStyle guistyle = new GUIStyle
            {
                normal = new GUIStyleState  // 正常样式
                {
                    textColor = Color.white,
                    background = texture2D
                },
                active = new GUIStyleState  // 点击样式
                {
                    textColor = Color.white,
                    background = texture2D2
                },
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 40,
                fixedWidth = 120,
                margin = new RectOffset(5, 5, 0, 0),
                border = new RectOffset(5, 5, 5, 5),
            };

            GUILayout.BeginArea(HeaderTableRect);
            {
                GUILayout.BeginHorizontal(new GUIStyle { alignment = TextAnchor.UpperLeft });
                {
                    if (GUILayout.Button("获取物品", guistyle))
                    {
                        this.ShowAddItemwindow = true;
                        this.ShowOtherWindow = false;
                        this.ShowUnlockResearchWindow = false;
                    }
                    if (GUILayout.Button("解锁公式", guistyle))
                    {
                        this.ShowUnlockResearchWindow = true;
                        this.ShowAddItemwindow = false;
                        this.ShowOtherWindow = false;
                    }
                    if (GUILayout.Button("其他", guistyle))
                    {
                        this.ShowOtherWindow = true;
                        this.ShowAddItemwindow = false;
                        this.ShowUnlockResearchWindow = false;
                    }

                    // 用户自定义获取数量
                    {
                        GUILayout.Label("获取数量:", new GUIStyle
                        {
                            fixedWidth = 80,
                            fixedHeight = 40,
                            alignment = TextAnchor.MiddleRight,
                            normal = new GUIStyleState
                            {
                                textColor = Color.white
                            }
                        });
                        var ItemText = GUILayout.TextField(AddItemNum.ToString(), new GUIStyle
                        {
                            fixedWidth = 100,
                            fixedHeight = 40,
                            alignment = TextAnchor.MiddleLeft,
                            margin = new RectOffset(5, 0, 0, 0),
                            normal = new GUIStyleState
                            {
                                textColor = Color.white
                            }
                        });
                        ItemText = Regex.Replace(ItemText, @"[^0-9.]", "");
                        try
                        {
                            if (ItemText != null && ItemText.Length < 10)
                            {
                                AddItemNum = Int32.Parse(ItemText);
                            }
                            else
                            {
                                ItemText = 1000.ToString();
                            }
                        }
                        catch (Exception) { throw; }
                    }

                    // 搜索物品
                    {
                        GUILayout.Label("搜索物品:", new GUIStyle
                        {
                            fixedWidth = 80,
                            fixedHeight = 40,
                            alignment = TextAnchor.MiddleRight,
                            normal = new GUIStyleState
                            {
                                textColor = Color.white
                            }
                        });
                        searchItem = GUILayout.TextField(searchItem, new GUIStyle
                        {
                            fixedWidth = 100,
                            fixedHeight = 40,
                            alignment = TextAnchor.MiddleLeft,
                            margin = new RectOffset(5, 0, 0, 0),
                            normal = new GUIStyleState
                            {
                                textColor = Color.white
                            }
                        });
                    }
                }
                GUILayout.EndHorizontal();
            }            
            GUILayout.EndArea();
        }
        
        // 添加物品
        void AddItemTable(Rect AddItemTableRect)
        {
            if (GameMain.mainPlayer == null)
            {
                GUILayout.Label("请先进入游戏",new GUIStyle {
                    fontSize = 26,
                    fixedWidth = 700,
                    fixedHeight = 300,
                    alignment = TextAnchor.MiddleCenter
                });
                return;
            }

            // 按钮样式
            GUIStyle guistyle = ButtonStyle;
            ItemProto[] dataArray = LDB.items.dataArray;
                       
            GUILayout.BeginArea(AddItemTableRect);
            {
               
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(800), GUILayout.Height(300));
                {
                    GUILayout.BeginHorizontal(new GUIStyle { alignment = TextAnchor.UpperLeft });                                       
                    for (int i = 0; i < dataArray.Length; i++)
                    {
                        var item = dataArray[i];
                        if (searchItem == "")
                        {
                            // 普通模式
                            if (GUILayout.Button(item.name, guistyle))
                            {
                                int num = AddItemNum;
                                int res = GameMain.mainPlayer.package.AddItemStacked(item.ID, num);
                                UIItemup.Up(item.ID, num);

                                //// 添加物品代码
                                //int ID = 6001;
                                //int num = 1000;
                                //int res = GameMain.mainPlayer.package.AddItemStacked(ID, num);
                                //UIItemup.Up(ID, num);
                            }                            
                        }
                        else
                        {
                            // 如果用户输入搜索
                            if (item.name.Contains(searchItem))
                            {
                                if (GUILayout.Button(item.name, guistyle))
                                {
                                    int num = AddItemNum;
                                    int res = GameMain.mainPlayer.package.AddItemStacked(item.ID, num);
                                    UIItemup.Up(item.ID, num);
                                }
                            }
                        }

                        int listNum = 8;
                        if ((i + 1) % listNum == 0)
                        {
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal(new GUIStyle { alignment = TextAnchor.UpperLeft });
                        }
                    }
                    if (searchItem == "" || "沙土".Contains(searchItem))
                    {
                        if (GUILayout.Button("沙土", guistyle))
                        {
                            GameMain.mainPlayer.SetSandCount(GameMain.mainPlayer.sandCount + AddItemNum);
                        }
                    }
                        
                    GUILayout.EndHorizontal();
                }                
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        // 解锁研究
        void UnlockResearchTable(Rect UnlockResearchTableRect)
        {
            if (GameMain.mainPlayer == null)
            {
                GUILayout.Label("请先进入游戏", new GUIStyle
                {
                    fontSize = 26,
                    fixedWidth = 700,
                    fixedHeight = 300,
                    alignment = TextAnchor.MiddleCenter
                });
                return;
            }
            
            GUIStyle guistyle = ButtonStyle;                    // 按钮样式
            RecipeProto[] dataArray = LDB.recipes.dataArray;    //研究列表

            GUILayout.BeginArea(UnlockResearchTableRect);
            {
                scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2, false, false, GUILayout.Width(800), GUILayout.Height(300));
                {
                    GUILayout.BeginHorizontal(new GUIStyle { alignment = TextAnchor.UpperLeft });
                    {
                        for (int i = 0; i < dataArray.Length; i++)
                        {
                            var item = dataArray[i];
                            if (searchItem == "")
                            {
                                // 普通模式
                                if (GUILayout.Button(item.name, guistyle))
                                {
                                    GameMain.history.UnlockRecipe(item.ID);
                                }
                            }
                            else
                            {
                                // 如果用户输入搜索
                                if (item.name.Contains(searchItem))
                                {
                                    if (GUILayout.Button(item.name, guistyle))
                                    {
                                        GameMain.history.UnlockRecipe(item.ID);
                                    }
                                }
                            }

                            int listNum = 8;
                            if ((i + 1) % listNum == 0)
                            {
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal(new GUIStyle { alignment = TextAnchor.UpperLeft });
                            }
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        // 显示窗口
        void DoMyWindow(int winId)
        {

            // 渲染头部标题
            this.HeaderTitle(HeaderTitleRect);

            // 渲染头部选项卡
            this.HeaderTable(HeaderTableRect);

            // 显示物品列表
            if (this.ShowAddItemwindow)
            {               
                this.AddItemTable(AddItemTableRect);               
            }

            // 解锁研究
            if (this.ShowUnlockResearchWindow)
            {
                GUILayout.Space(5f);
                this.UnlockResearchTable(UnlockResearchTableRect);
            }
            

            // 其他
            if (this.ShowOtherWindow)
            {

            }

        }

    }





    //[BepInPlugin("aoe.top.ScriptTrainer", "[戴森球计划] 内置修改器 By:小莫", "1.0.0")]
    //public class ScriptTrainer : BaseUnityPlugin
    //{
    //    public bool Trainer = false;

    //    private ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter { get; set; }

    //    // 脚本停止时执行 OnDestroy() 函数
    //    void OnDestroy()
    //    {

    //        Debug.Log("内置修改器重载！");
    //    }

    //    // 启动脚本时执行 Awake() 函数

    //    public void Awake()
    //    {

    //    }

    //    [Obsolete]
    //    // 注入脚本时会自动调用Start()方法 执行在Awake()方法后面
    //    public void Start()
    //    {
    //        // 初始化UI
    //        ShowCounter = Config.AddSetting("修改器快捷键", "Key", new BepInEx.Configuration.KeyboardShortcut(KeyCode.F9));

    //        // 注入补丁
    //        Harmony.CreateAndPatchAll(typeof(ScriptTrainer), null);

    //    }

    //    // 插件将自动循环Update()方法中的内容
    //    public void Update()
    //    {
    //        // 如果用户修改了资源倍数的值
    //        if (ShowCounter.Value.IsDown())
    //        {
    //            if (Trainer)
    //            {
    //                Debug.Log("关闭修改器");
    //            }
    //            else
    //            {
    //                Debug.Log("启动修改器");
    //            }

    //            UIAdvisorTipMsg();
    //            //GetList();
    //            Trainer = !Trainer;
    //        }
    //    }


    //    public UIMechaWindow mechaWindow;
    //    [NonSerialized]
    //    public UIRoot uiRoot;
    //    [NonSerialized]
    //    public UIGame uiGame;
    //    public void UIAdvisorTipMsg()
    //    {
    //        //Assert.False(false, "传送带输入接口已满");
    //        //int ID = 6001;
    //        //int num = 1000;
    //        //int res = GameMain.mainPlayer.package.AddItemStacked(ID, num);
    //        //UIItemup.Up(ID, num);

    //        // 创建并初始化UI
    //        //optionWindow._RegEvent();
    //        //GameData gameData = ManualBehaviour.data;
    //        //var mecha = gameData.mainPlayer;

    //        //UIMeshLine Ow = new UIMeshLine();

    //        //Ow.init(null);

    //        //mechaWindow._Init(GameMain.mainPlayer.mecha);
    //        //mechaWindow._Create();
    //        //mechaWindow._Open();

    //        uiGame = uiRoot.uiGame;
    //        uiGame.OpenMechaWindow();
    //    }


    //    public void GetList()
    //    {
    //        List<string> myList = new List<string>();
    //        ItemProto[] dataArray = LDB.items.dataArray;

    //        for (int j = 0; j < dataArray.Length; j++)
    //        {
    //            var item = dataArray[j];
    //            string[] strArray = new string[] {
    //                "ID:" + item.ID.ToString(),
    //                "\tname:" + item.name,
    //                "\tHeatValue:" + item.HeatValue.ToString(),
    //                "\tModelCount:" + item.ModelCount.ToString()
    //            };
    //            myList.Add(string.Join(",", strArray));
    //        }


    //        string[] names = myList.ToArray(); ;

    //        using (StreamWriter sw = new StreamWriter("names.txt"))
    //        {
    //            foreach (string s in names)
    //            {
    //                sw.WriteLine(s);
    //            }
    //            sw.Close();
    //        }
    //        Debug.Log("写入物品数据完成");
    //    }




    //    [HarmonyPrefix]
    //    [HarmonyPatch(typeof(Mecha), "SetForNewGame")]
    //    public static bool Mecha_SetForNewGame_Prefix(Mecha __instance)
    //    {
    //        // 这里写入我们自己的内容        
    //        ModeConfig freeMode = Configs.freeMode;
    //        __instance.coreEnergyCap = freeMode.mechaCoreEnergyCap;
    //        __instance.coreEnergy = __instance.coreEnergyCap;
    //        __instance.corePowerGen = freeMode.mechaCorePowerGen;
    //        __instance.reactorPowerGen = freeMode.mechaReactorPowerGen;
    //        __instance.reactorEnergy = 0.0;
    //        __instance.reactorItemId = 0;

    //        // 返回 true为继续执执行游戏原函数，返回 false为不执行游戏原函数,
    //        return true;
    //    }

    //    [HarmonyPostfix]
    //    [HarmonyPatch(typeof(Mecha), "SetForNewGame")]
    //    public static void Mecha_SetForNewGame_Postfix(Mecha __instance)
    //    {
    //        // 这里写入我们自己的内容            
    //        Debug.Log("这里的内容需要等待游戏原函数执行完后才会执行");


    //        var _droneCount = Traverse.Create(__instance).Field("_droneCount").GetValue<int>();


    //        // 调用方法 public void GameTick(long time, float dt)
    //        FunObj obj = new FunObj();
    //        obj.time = 0;
    //        obj.dt = 0;
    //        var Free = Traverse.Create(__instance).Method("GameTick", obj).GetValue();
    //    }

    //    struct FunObj
    //    {
    //        public long time;
    //        public float dt;
    //    }


    //}
}
