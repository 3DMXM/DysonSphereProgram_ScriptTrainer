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
    [BepInPlugin("aoe.top.ScriptTrainer", "[戴森球计划] 内置修改器 By:小莫", "1.0.1")]
    public class ScriptTrainer : BaseUnityPlugin
    {


        public bool DisplayingWindow;
        private bool ShowAddItemWindow = true;
        private bool ShowOtherWindow = false;
        private bool ShowAddlocktechWindow = false;
        private int AddItemNum = 1000;

        private string searchItem = "";

        private Rect HeaderTitleRect;
        private Rect HeaderTableRect;
        private Rect windowRect;
        private Rect AddItemTableRect;
        private Rect AddlocktechRect;
        private Rect OtherRect;
        private Vector2 scrollPosition;


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

            //计算区域
            this.ComputeRect();
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
                windowRect = GUI.Window(20210219, windowRect, DoMyWindow, "", myWindowStyle);
            }
            // 保存修改
            this.ChangeToGame();
        }
        // 初始样式
        void ComputeRect()
        {
            // 主窗口居中
            int num = Mathf.Min(Screen.width, 700);
            int num2 = (Screen.height < 400) ? Screen.height : (400);
            int num3 = Mathf.RoundToInt((float)(Screen.width - num) / 2f);
            int num4 = Mathf.RoundToInt((float)(Screen.height - num2) / 2f);
            this.windowRect = new Rect((float)num3, (float)num4, (float)num, (float)num2);

            // 头部
            this.HeaderTitleRect = this.windowRect;
            this.HeaderTitleRect.position = Vector2.zero;
            // 选项卡按钮
            //this.HeaderTableRect.width = 150f;
            this.HeaderTableRect = new Rect(0, 40, 700, 40);
            // 添加物品样式
            this.AddItemTableRect = new Rect(0, 90, 700, 300);
            // 解锁科技
            this.AddlocktechRect = new Rect(0, 90, 700, 300);
            // 其他内容
            this.OtherRect = new Rect(0, 90, 700, 300);
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
                        this.ShowAddItemWindow = true;
                        this.ShowOtherWindow = false;
                        this.ShowAddlocktechWindow = false;
                    }
                    if (GUILayout.Button("解锁科技", guistyle))
                    {
                        this.ShowAddlocktechWindow = true;
                        this.ShowAddItemWindow = false;
                        this.ShowOtherWindow = false;
                    }

                    if (GUILayout.Button("其他", guistyle))
                    {
                        this.ShowOtherWindow = true;
                        this.ShowAddItemWindow = false;
                        this.ShowAddlocktechWindow = false;
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
                GUILayout.Label("请先进入游戏", new GUIStyle
                {
                    fontSize = 26,
                    fixedWidth = 700,
                    fixedHeight = 300,
                    alignment = TextAnchor.MiddleCenter
                });
                return;
            }


            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture2D.SetPixel(0, 0, new Color32(30, 144, 255, 255));    // rgba(30, 144, 255,1.0)
            texture2D.Apply();
            Texture2D texture2D2 = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture2D2.SetPixel(0, 0, new Color32(112, 161, 255, 255));  // rgba(112, 161, 255,1.0)
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
                fixedWidth = 90,
                margin = new RectOffset(5, 7, 0, 5),
            };

            // 物品列表
            ItemProto[] dataArray = LDB.items.dataArray;
            GUILayout.BeginArea(AddItemTableRect);
            {

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(700), GUILayout.Height(300));
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

                        int listNum = 7;
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

        // 解锁科技
        void AddlocktechTable(Rect AddlocktechRect)
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


            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture2D.SetPixel(0, 0, new Color32(30, 144, 255, 255));    // rgba(30, 144, 255,1.0)
            texture2D.Apply();
            Texture2D texture2D2 = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture2D2.SetPixel(0, 0, new Color32(112, 161, 255, 255));  // rgba(112, 161, 255,1.0)
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
                fixedWidth = 90,
                margin = new RectOffset(5, 7, 0, 5),
            };

            // 科技列表
            TechProto[] dataArray = LDB.techs.dataArray;

            GUILayout.BeginArea(AddlocktechRect);
            {

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(700), GUILayout.Height(300));
                {
                    GUILayout.BeginHorizontal(new GUIStyle { alignment = TextAnchor.UpperLeft });
                    {
                        if (GUILayout.Button("解锁全部", guistyle))
                        {
                            for (int i = 0; i < dataArray.Length; i++)
                            {
                                var item = dataArray[i];
                                GameMain.history.UnlockTech(item.ID);
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(new GUIStyle { alignment = TextAnchor.UpperLeft });
                    for (int i = 0; i < dataArray.Length; i++)
                    {
                        var item = dataArray[i];

                        if (searchItem == "")
                        {
                            // 普通模式
                            if (GUILayout.Button(item.name, guistyle))
                            {
                                //int num = AddItemNum;
                                //int res = GameMain.mainPlayer.package.AddItemStacked(item.ID, num);
                                //UIItemup.Up(item.ID, num);
                                // 解锁科技
                                GameMain.history.UnlockTech(item.ID);
                            }
                        }
                        else
                        {
                            // 如果用户输入搜索
                            if (item.name.Contains(searchItem))
                            {
                                if (GUILayout.Button(item.name, guistyle))
                                {
                                    //int num = AddItemNum;
                                    //int res = GameMain.mainPlayer.package.AddItemStacked(item.ID, num);
                                    //UIItemup.Up(item.ID, num);

                                    // 解锁科技
                                    GameMain.history.UnlockTech(item.ID);
                                }
                            }
                        }

                        int listNum = 7;
                        if ((i + 1) % listNum == 0)
                        {
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal(new GUIStyle { alignment = TextAnchor.UpperLeft });
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }


        // 无人机数量
        int droneCount = 3;
        int newDroneCount = 3;
        // 无人机速度
        float droneSpeed = 5;
        float newDroneSpeed = 5;
        // 跳跃高度
        float jumpSpeed = 32;
        float newjumpSpeed = 32;
        // 机甲移动速度
        float walkSpeed = 5;
        float newWalkSpeed = 5;
        // 挖掘速度倍率
        float miningSpeed = 1;
        float newMiningSpeed = 1;
        // 制作速度
        float replicateSpeed = 1;
        float newreplicateSpeed = 1;
        // 保存修改
        void ChangeToGame()
        {
            // 第一行
            {
                if (droneCount != newDroneCount)
                {
                    GameMain.mainPlayer.mecha.droneCount = newDroneCount;
                    Debug.Log("无人机数量修改为" + newDroneCount);
                    droneCount = newDroneCount;
                }
                if (droneSpeed != newDroneSpeed)
                {
                    GameMain.mainPlayer.mecha.droneSpeed = newDroneSpeed;
                    Debug.Log("无人机速度修改为" + newDroneSpeed);
                    droneSpeed = newDroneSpeed;
                }
                if (jumpSpeed != newjumpSpeed)
                {
                    GameMain.mainPlayer.mecha.jumpSpeed = newjumpSpeed;
                    Debug.Log("跳跃高度修改为" + newjumpSpeed);
                    jumpSpeed = newjumpSpeed;
                }
            }
            // 第二行
            {
                if (walkSpeed != newWalkSpeed)
                {
                    GameMain.mainPlayer.mecha.walkSpeed = newWalkSpeed;
                    Debug.Log("航行速度修改为" + newWalkSpeed);
                    walkSpeed = newWalkSpeed;
                }
                if (miningSpeed != newMiningSpeed)
                {
                    GameMain.mainPlayer.mecha.miningSpeed = newMiningSpeed;
                    Debug.Log("挖掘速度倍率修改为" + newMiningSpeed);
                    miningSpeed = newMiningSpeed;
                }
                if (replicateSpeed != newreplicateSpeed)
                {
                    GameMain.mainPlayer.mecha.researchPower = newreplicateSpeed;
                    Debug.Log("制作速度修改为" + newreplicateSpeed);
                    replicateSpeed = newreplicateSpeed;
                }
            }

        }
        // 其他
        void OtherTable(Rect OtherRect)
        {
            // GUILayout.Button(item.name, guistyle)
            
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture2D.SetPixel(0, 0, new Color32(30, 144, 255, 255));    // rgba(30, 144, 255,1.0)
            texture2D.Apply();
            Texture2D texture2D2 = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture2D2.SetPixel(0, 0, new Color32(112, 161, 255, 255));  // rgba(112, 161, 255,1.0)
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
                fixedWidth = 90,
                margin = new RectOffset(5, 7, 0, 5),
            };
            
            

            GUILayout.BeginArea(OtherRect);
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUILayout.Width(700), GUILayout.Height(300));
                {
                    GUILayout.BeginHorizontal(new GUIStyle { alignment = TextAnchor.UpperLeft });
                    {
                        // 无人机数量
                        {
                            GUILayout.Label("无人机数量:", new GUIStyle
                            {
                                fixedWidth = 80,
                                fixedHeight = 40,
                                alignment = TextAnchor.MiddleRight,
                                normal = new GUIStyleState
                                {
                                    textColor = Color.white
                                }
                            });
                            var ItemText = GUILayout.TextField(newDroneCount.ToString(), new GUIStyle
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
                                if (ItemText != null && ItemText.Length < 10 && ItemText.Length != 0)
                                {
                                    newDroneCount = Int32.Parse(ItemText);
                                }
                                else
                                {
                                    ItemText = newDroneCount.ToString();
                                }
                            }
                            catch (Exception) { throw; }
                        }
                        // 无人机速度
                        {
                            GUILayout.Label("无人机速度:", new GUIStyle
                            {
                                fixedWidth = 80,
                                fixedHeight = 40,
                                alignment = TextAnchor.MiddleRight,
                                normal = new GUIStyleState
                                {
                                    textColor = Color.white
                                }
                            });
                            var ItemText = GUILayout.TextField(newDroneSpeed.ToString(), new GUIStyle
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
                                if (ItemText != null && ItemText.Length < 10 && ItemText.Length != 0)
                                {
                                    newDroneSpeed = Int32.Parse(ItemText);
                                }
                                else
                                {
                                    ItemText = newDroneSpeed.ToString();
                                }
                            }
                            catch (Exception) { throw; }
                        }
                        // 跳跃高度
                        {
                            GUILayout.Label("跳跃高度:", new GUIStyle
                            {
                                fixedWidth = 80,
                                fixedHeight = 40,
                                alignment = TextAnchor.MiddleRight,
                                normal = new GUIStyleState
                                {
                                    textColor = Color.white
                                }
                            });
                            var ItemText = GUILayout.TextField(newjumpSpeed.ToString(), new GUIStyle
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
                                if (ItemText != null && ItemText.Length < 10 && ItemText.Length != 0)
                                {
                                    newjumpSpeed = Int32.Parse(ItemText);
                                }
                                else
                                {
                                    ItemText = newjumpSpeed.ToString();
                                }
                            }
                            catch (Exception) { throw; }
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(new GUIStyle { alignment = TextAnchor.UpperLeft });
                    {

                        // 挖掘速度倍率
                        {
                            GUILayout.Label("挖掘速度:", new GUIStyle
                            {
                                fixedWidth = 80,
                                fixedHeight = 40,
                                alignment = TextAnchor.MiddleRight,
                                normal = new GUIStyleState
                                {
                                    textColor = Color.white
                                }
                            });
                            var ItemText = GUILayout.TextField(newMiningSpeed.ToString(), new GUIStyle
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
                                if (ItemText != null && ItemText.Length < 10 && ItemText.Length != 0)
                                {
                                    newMiningSpeed = Int32.Parse(ItemText);
                                }
                                else
                                {
                                    ItemText = newMiningSpeed.ToString();
                                }
                            }
                            catch (Exception) { throw; }
                        }
                        // 机甲移动速度
                        {
                            GUILayout.Label("机甲移动速度:", new GUIStyle
                            {
                                fixedWidth = 80,
                                fixedHeight = 40,
                                alignment = TextAnchor.MiddleRight,
                                normal = new GUIStyleState
                                {
                                    textColor = Color.white
                                }
                            });
                            var ItemText = GUILayout.TextField(newWalkSpeed.ToString(), new GUIStyle
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
                                if (ItemText != null && ItemText.Length < 10 && ItemText.Length != 0)
                                {
                                    newWalkSpeed = Int32.Parse(ItemText);
                                }
                                else
                                {
                                    ItemText = newWalkSpeed.ToString();
                                }
                            }
                            catch (Exception) { throw; }
                        }
                        // 制作速度
                        {
                            GUILayout.Label("制作速度:", new GUIStyle
                            {
                                fixedWidth = 80,
                                fixedHeight = 40,
                                alignment = TextAnchor.MiddleRight,
                                normal = new GUIStyleState
                                {
                                    textColor = Color.white
                                }
                            });
                            var ItemText = GUILayout.TextField(newreplicateSpeed.ToString(), new GUIStyle
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
                                if (ItemText != null && ItemText.Length < 10 && ItemText.Length != 0)
                                {
                                    newreplicateSpeed = Int32.Parse(ItemText);
                                }
                                else
                                {
                                    ItemText = newreplicateSpeed.ToString();
                                }
                            }
                            catch (Exception) { throw; }
                        }
                    }
                    GUILayout.EndHorizontal();

                    //GUILayout.BeginHorizontal(new GUIStyle { alignment = TextAnchor.UpperLeft });
                    //{
                    //    if (GUILayout.Button("潮汐锁定", guistyle))
                    //    {
                    //        GameMain.mainPlayer.planetData.singularity = EPlanetSingularity.TidalLocked;
                    //        // Load
                    //        GameMain.mainPlayer.planetData.Load();
                    //    }
                    //    if (GUILayout.Button("行星类型",guistyle))
                    //    {
                    //        GameMain.mainPlayer.planetData.type = EPlanetType.Ice;
                    //        GameMain.mainPlayer.planetData.Load();
                    //    }
                    //}
                    //GUILayout.EndHorizontal();
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
            if (this.ShowAddItemWindow)
            {
                this.AddItemTable(this.AddItemTableRect);
            }
            // 解锁科技
            if (this.ShowAddlocktechWindow)
            {
                this.AddlocktechTable(this.AddlocktechRect);
            }

            // 其他
            if (this.ShowOtherWindow)
            {
                this.OtherTable(this.OtherRect);
            }

            GUI.DragWindow();
        }

    }


}