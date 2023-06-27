using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Immutable;
using static System.Windows.Forms.LinkLabel;
using MtcEast.Properties;
using System.Security.Policy;

namespace MtcEast
{
    public partial class Setting : Form
    {
        public const int VK_非常 = -1;

        public const int VK_Enter = 0x0d;
        public const int VK_BackSpace = 0x08;
        public const int VK_Space = 0x20;
        public const int VK_Left = 0x25;
        public const int VK_Up = 0x26;
        public const int VK_Right = 0x27;
        public const int VK_Down = 0x28;
        public const int VK_Esc = 0x1B;
        public const int VK_SHIFT = 0x10;



        public static int[] A = new int[3];
        public static int[] AA = new int[3];
        public static int[] B = new int[3];
        public static int[] C = new int[3];
        public static int[] D = new int[3];
        public static int[] ATS = new int[3];
        public static int[] Sel = new int[3];
        public static int[] Start = new int[3];
        public static int[] 上 = new int[3];
        public static int[] 左 = new int[3];
        public static int[] 右 = new int[3];
        public static int[] 下 = new int[3];

        public static int[] SA = new int[3];
        public static int[] SAA = new int[3];
        public static int[] SB = new int[3];
        public static int[] SC = new int[3];
        public static int[] SD = new int[3];
        public static int[] SSel = new int[3];
        public static int[] SStart = new int[3];
        public static int[] S上 = new int[3];
        public static int[] S左 = new int[3];
        public static int[] S右 = new int[3];
        public static int[] S下 = new int[3];

        public const String DEFURL = @"steam://rungameid/2111630";

        public static String URL = DEFURL;

        private class DefSetting
        {
            public int[]? r;
            public String T1 = "", T2 = "", T3 = "";
            public int V1 = 0, V2 = 0, V3 = 0;
        }

        private static DefSetting[] m_DefSettings;

        private class ComboSetting
        {
            public int[]? r;
            public ComboBox? c1 = null, c2 = null, c3 = null;
        }

        private ComboSetting[]? m_ComboSetting;

        static Setting()
        {
            m_DefSettings = new DefSetting[]
            {
                new DefSetting{ r = A      , T1 = "電笛 Enter"   , V1 = VK_Enter       },
                new DefSetting{ r = AA     , T1 = "空笛 Back"    , V1 = VK_BackSpace   },
                new DefSetting{ r = B      , T1 = "B ブザー"     , V1 = 'B'            },
                new DefSetting{ r = C      , T1 = "非常ブレーキ" , V1 = VK_非常        },
                new DefSetting{ r = D      , T1 = "W 定速"       , V1 = 'W'            },
                new DefSetting{ r = ATS    , T1 = "ATS確認"      , V1 = VK_Space       , T2 = "E EBリセット" , V2 = 'E' },
                new DefSetting{ r = Sel    , T1 = "C 運転台表示" , V1 = 'C'            },
                new DefSetting{ r = Start  , T1 = "Esc ポーズ"   , V1 = VK_Esc         },
                new DefSetting{ r = 上     , T1 = "↑上"         , V1 = VK_Up          },
                new DefSetting{ r = 左     , T1 = "←左"         , V1 = VK_Left        },
                new DefSetting{ r = 右     , T1 = "→右"         , V1 = VK_Right       },
                new DefSetting{ r = 下     , T1 = "↓下"         , V1 = VK_Down        },

                new DefSetting{ r = SA     , T1 = "X 警報持続"  , V1 = 'X' },
                new DefSetting{ r = SAA    , T1 = "X 警報持続"  , V1 = 'X' },
                new DefSetting{ r = SB     , T1 = "Y 復帰常用"  , V1 = 'Y' },
                new DefSetting{ r = SC     , T1 = "U 復帰非常"  , V1 = 'U' },
                new DefSetting{ r = SD     , T1 = "D 抑速１"    , V1 = 'D' },
                new DefSetting{ r = SSel   , T1 = "V HUD表示"   , V1 = 'V' },
                new DefSetting{ r = SStart , T1 = "Shift 視点切替" , V1=VK_SHIFT} ,
                new DefSetting{ r = S上    , T1 = "K 勾配起動"  , V1 = 'K' },
                new DefSetting{ r = S左    , T1 = "T TASK切"    , V1 = 'T' },
                new DefSetting{ r = S右    } ,
                new DefSetting{ r = S下    , T1 = "I インチング", V1 = 'I' },
            };

            InitSetting();
            ReadUserSetting();
        }


        public Setting()
        {
            InitializeComponent();

            m_ComboSetting = new ComboSetting[]
            {
                new ComboSetting{ r = A      , c1 = cbA1      , c2 = cbA2      , c3 = cbA3     },
                new ComboSetting{ r = AA     , c1 = cbAA1     , c2 = cbAA2     , c3 = cbAA3    },
                new ComboSetting{ r = B      , c1 = cbB1      , c2 = cbB2      , c3 = cbB3     },
                new ComboSetting{ r = C      , c1 = cbC1      , c2 = cbC2      , c3 = cbC3     },
                new ComboSetting{ r = D      , c1 = cbD1      , c2 = cbD2      , c3 = cbD3     },
                new ComboSetting{ r = ATS    , c1 = cbS1      , c2 = cbS2      , c3 = cbS3     },
                new ComboSetting{ r = Sel    , c1 = cbSel1    , c2 = cbSel2    , c3 = cbSel3   },
                new ComboSetting{ r = Start  , c1 = cbStart1  , c2 = cbStart2  , c3 = cbStart3 },
                new ComboSetting{ r = 上     , c1 = cbUp1     , c2 = cbUp2     , c3 = cbUp3    },
                new ComboSetting{ r = 左     , c1 = cbLeft1   , c2 = cbLeft2   , c3 = cbLeft3  },
                new ComboSetting{ r = 右     , c1 = cbRight1  , c2 = cbRight2  , c3 = cbRight3 },
                new ComboSetting{ r = 下     , c1 = cbDown1   , c2 = cbDown2   , c3 = cbDown3  },

                new ComboSetting{ r = SA     , c1 = cbSA1     , c2 = cbSA2     , c3 = cbSA3    },
                new ComboSetting{ r = SAA    , c1 = cbSAA1    , c2 = cbSAA2    , c3 = cbSAA3   },
                new ComboSetting{ r = SB     , c1 = cbSB1     , c2 = cbSB2     , c3 = cbSB3    },
                new ComboSetting{ r = SC     , c1 = cbSC1     , c2 = cbSC2     , c3 = cbSC3    },
                new ComboSetting{ r = SD     , c1 = cbSD1     , c2 = cbSD2     , c3 = cbSD3    },
                new ComboSetting{ r = SSel   , c1 = cbSSel1   , c2 = cbSSel2   , c3 = cbSSel3  },
                new ComboSetting{ r = SStart , c1 = cbSStart1 , c2 = cbSStart2 , c3 = cbSStart3},
                new ComboSetting{ r = S上    , c1 = cbSUp1    , c2 = cbSUp2    , c3 = cbSUp3   },
                new ComboSetting{ r = S左    , c1 = cbSLeft1  , c2 = cbSLeft2  , c3 = cbSLeft3 },
                new ComboSetting{ r = S右    , c1 = cbSRight1 , c2 = cbSRight2 , c3 = cbSRight3},
                new ComboSetting{ r = S下    , c1 = cbSDown1  , c2 = cbSDown2  , c3 = cbSDown3 },
            };
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBoxURL.Text = URL;


            List<object> list = new List<object>();
            list.Add(new { T = "", V = 0 });

            foreach (var item in m_DefSettings)
            {
                if (item.T1 != "" && item.V1 != 0) list.Add(new { T = item.T1, V = item.V1 });
                if (item.T2 != "" && item.V2 != 0) list.Add(new { T = item.T2, V = item.V2 });
                if (item.T3 != "" && item.V3 != 0) list.Add(new { T = item.T3, V = item.V3 });
            }
            list = list.Distinct().ToList();

            if(m_ComboSetting != null)
            {
                foreach (var item in m_ComboSetting)
                {
                    if (item.c1 != null)
                    {
                        List<object> tmp = new List<object>();
                        tmp.AddRange(list);

                        item.c1.DisplayMember = "T";
                        item.c1.ValueMember = "V";
                        item.c1.DataSource = tmp;
                        if (item.r != null) item.c1.SelectedValue = item.r[0];
                    }

                    if (item.c2 != null)
                    {
                        List<object> tmp = new List<object>();
                        tmp.AddRange(list);

                        item.c2.DisplayMember = "T";
                        item.c2.ValueMember = "V";
                        item.c2.DataSource = tmp;
                        if (item.r != null) item.c2.SelectedValue = item.r[1];
                    }

                    if (item.c3 != null)
                    {
                        List<object> tmp = new List<object>();
                        tmp.AddRange(list);

                        item.c3.DisplayMember = "T";
                        item.c3.ValueMember = "V";
                        item.c3.DataSource = tmp;
                        if (item.r != null) item.c3.SelectedValue = item.r[2];                        
                    }
                }
            }           
        }


        private static void InitSetting()
        {
            foreach (var item in m_DefSettings)
            {
                if (item.r != null)
                {
                    item.r[0] = item.V1;
                    item.r[1] = item.V2;
                    item.r[2] = item.V3;
                }
            }
        }

        private static void ReadUserSetting()
        {
            try
            {
                string s = Settings.Default.ボタン設定.Trim();

                if (m_DefSettings != null && s != "")
                {
                    s = s.Replace("\r\n", "\n").Replace("\r", "\n");
                    string[] lines = s.Split('\n');

                    int i = 0;
                    for (i = 0; (i < lines.Length) && (i < m_DefSettings.Length); i++)
                    {
                        string[] cells = lines[i].Trim().Split(',');

                        var item = m_DefSettings[i];

                        if (item.r != null)
                        {
                            item.r[0] = int.Parse(cells[0]);
                            item.r[1] = int.Parse(cells[1]);
                            item.r[2] = int.Parse(cells[2]);
                        }
                    }
                }



                s = Settings.Default.URL.Trim();
                if (s != "") URL = s;

            }
            catch (Exception)
            {
            }
        }




        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_ComboSetting != null)
            {
                foreach (var item in m_ComboSetting)
                {
                    if (item.c1 != null && item.r != null) item.r[0] = (int)item.c1.SelectedValue;
                    if (item.c2 != null && item.r != null) item.r[1] = (int)item.c2.SelectedValue;
                    if (item.c3 != null && item.r != null) item.r[2] = (int)item.c3.SelectedValue;
                }
            }

            string s = "";
            foreach (var item in m_DefSettings)
            {
                if(item.r != null)
                {
                    s += $"{item.r[0]},{item.r[1]},{item.r[2]}\n";
                }
                else
                {
                    s += $"0,0,0\n";
                }


                URL = textBoxURL.Text.Trim();

                Settings.Default.URL = URL;
                Settings.Default.ボタン設定 = s;
                Settings.Default.Save();
            }

        }

        private void buttonInit_Click(object sender, EventArgs e)
        {
            textBoxURL.Text = URL = DEFURL;
            InitSetting();


            if (m_ComboSetting != null)
            {
                foreach (var item in m_ComboSetting)
                {
                    if (item.c1 != null && item.r != null) item.c1.SelectedValue = item.r[0];
                    if (item.c2 != null && item.r != null) item.c2.SelectedValue = item.r[1];
                    if (item.c3 != null && item.r != null) item.c3.SelectedValue = item.r[2];

                }
            }
        }
    }
}
