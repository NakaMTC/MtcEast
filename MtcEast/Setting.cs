using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MtcEast.Setting;

namespace MtcEast
{
    public partial class Setting : Form
    {

        const string IniPath = @".\Settings.ini";

        private const string DefURL = @"steam://rungameid/2111630";

        /// <summary> デフォルトのボタン設定</summary>
        private static Dictionary<Idx, VK操作> Def操作 = new()
        {
            {Idx.A強   , VK操作.Back_空笛          },
            {Idx.A     , VK操作.Enter_電笛         },
            {Idx.B     , VK操作.E_EBリセット       },
            {Idx.C     , VK操作.W_定速             },
            {Idx.D     , VK操作.D_抑速1            },
            {Idx.ATS   , VK操作.SP_ATS確認         },
            {Idx.左    , VK操作.左                 },
            {Idx.右    , VK操作.右                 },
            {Idx.上    , VK操作.上                 },
            {Idx.下    , VK操作.下                 },
            {Idx.selA  , VK操作.C_運転台表示       },
            {Idx.selB  , VK操作.B_ブザー           },
            {Idx.selC  , VK操作.非常               },
            {Idx.selD  , VK操作.V_HUD表示          },
            {Idx.selATS, VK操作.X_警報持続         },
            {Idx.sel左 , VK操作.SHIFT_視点         },
            {Idx.sel右 , VK操作.T_TASK             },
            {Idx.sel上 , VK操作.I_インチング       },
            {Idx.sel下 , VK操作.K_勾配起動         },
            {Idx.StA   , VK操作.Y_復帰常用         },
            {Idx.StB   , VK操作.U_復帰非常         },
            {Idx.StC   , VK操作.WinG_GameBar       },
            {Idx.StD   , VK操作.WinAltR_録画       },
            {Idx.StATS , VK操作.X_警報持続         },
            {Idx.St左  , VK操作.SHIFT_視点         },
            {Idx.St右  , VK操作.T_TASK             },
            {Idx.St上  , VK操作.I_インチング       },
            {Idx.St下  , VK操作.K_勾配起動         },
            {Idx.selST , VK操作.Esc_ポーズ         },
        };

        /// <summary> コンストラクタ、コンボボックス一覧を取得する </summary>
        public Setting()
        {
            InitializeComponent();

            // コンボボックス一覧を取得する
            m_CbList = new ComboBox[(int)Idx.selST + 1];
            m_CbList[(int)Idx.A強] = cbA強;
            m_CbList[(int)Idx.A] = cbA;
            m_CbList[(int)Idx.B] = cbB;
            m_CbList[(int)Idx.C] = cbC;
            m_CbList[(int)Idx.D] = cbD;
            m_CbList[(int)Idx.ATS] = cbATS;
            m_CbList[(int)Idx.左] = cb左;
            m_CbList[(int)Idx.右] = cb右;
            m_CbList[(int)Idx.上] = cb上;
            m_CbList[(int)Idx.下] = cb下;
            m_CbList[(int)Idx.selA] = cbSelA;
            m_CbList[(int)Idx.selB] = cbSelB;
            m_CbList[(int)Idx.selC] = cbSelC;
            m_CbList[(int)Idx.selD] = cbSelD;
            m_CbList[(int)Idx.selATS] = cbSelATS;
            m_CbList[(int)Idx.sel左] = cbSel左;
            m_CbList[(int)Idx.sel右] = cbSel右;
            m_CbList[(int)Idx.sel上] = cbSel上;
            m_CbList[(int)Idx.sel下] = cbSel下;
            m_CbList[(int)Idx.StA] = cbStA;
            m_CbList[(int)Idx.StB] = cbStB;
            m_CbList[(int)Idx.StC] = cbStC;
            m_CbList[(int)Idx.StD] = cbStD;
            m_CbList[(int)Idx.StATS] = cbStATS;
            m_CbList[(int)Idx.St左] = cbSt左;
            m_CbList[(int)Idx.St右] = cbSt右;
            m_CbList[(int)Idx.St上] = cbSt上;
            m_CbList[(int)Idx.St下] = cbSt下;
            m_CbList[(int)Idx.selST] = cbSelST;
        }
        private ComboBox[] m_CbList;

        /// <summary> 各ボタンのビット </summary>
        private enum Bit
        {
            A   =     0x0400,
            AA  =     0x0c00,
            B   =     0x1000,
            C   =     0x2000,
            D   =     0x0200,
            ATS =     0x0100,
            ST  = 0x00010000,
            Se  = 0x00020000,
            上  = 0x00040000,
            下  = 0x00080000,
            左  = 0x00100000,
            右  = 0x00200000,
        }

        public static string URL = DefURL;
        private static int[] 操作リスト = new int[(int)Idx.selST + 1];


        /// <summary> 設定の読み込み ： 呼び出し元 ＝ システム起動時 </summary>
        public static void IniRead()
        {
            // 初期化の設定の読み込み
            URL = DefURL;
            foreach (var item in Def操作)
            {
                操作リスト[(int)item.Key] = (int)item.Value;
            }

            // 初期化の設定の読み込み
            try
            {
                if (File.Exists(IniPath))
                {
                    string[] lines = File.ReadAllLines(IniPath);

                    // 1行目 : URL
                    if (lines.Length > 0) URL = lines[0].Trim();

                    // 2行目 ： 操作リスト （int ・ カンマ区切り）
                    if (lines.Length > 1)
                    {
                        string[] cells = lines[1].Trim().Split(',');
                        for (int i = 0; i < Math.Min(cells.Length, 操作リスト.Length); i++)
                        {
                            if (int.TryParse(cells[i], out int n)) 操作リスト[i] = n;
                        }
                    }

                }
            }
            catch { }
        }

        /// <summary> 設定の保存 </summary>
        public static void IniSave()
        {            
            string[] lines = new string[2];
            lines[0] = URL;
            lines[1] = string.Join(",", 操作リスト);
            if (File.Exists(IniPath)) File.Delete(IniPath);
            File.WriteAllLines(IniPath, lines, Encoding.UTF8);
        }


        /// <summary> 仮想ボタン一覧（同時押し対応・インデックス）</summary>
        public enum Idx
        {
            A強 = 0,
            A = 1,
            B = 2,
            C = 3,
            D = 4,
            ATS = 5,
            左 = 6,
            右 = 7,
            上 = 8,
            下 = 9,
            selA = 10,
            selB = 11,
            selC = 12,
            selD = 13,
            selATS = 14,
            sel左 = 15,
            sel右 = 16,
            sel上 = 17,
            sel下 = 18,
            StA = 19,
            StB = 20,
            StC = 21,
            StD = 22,
            StATS = 23,
            St左 = 24,
            St右 = 25,
            St上 = 26,
            St下 = 27,
            selST = 28,
        }

        /// <summary> 仮想キー コード </summary>
        internal enum VK
        {
            Enter = 0x0d,
            BackSpace = 0x08,
            Space = 0x20,
            左 = 0x25,
            上 = 0x26,
            右 = 0x27,
            下 = 0x28,
            Esc = 0x1B,
            SHIFT = 0x10,
            Win = 0x5b,
            Alt = 0xA4,
        }
        internal enum VK特殊
        {
            非常 = -1,
            WinAltR_録画 = -2,
            WinG_GameBar = -3,
        }
        public enum VK操作
        {
            Back_空笛 = VK.BackSpace,
            Enter_電笛 = VK.Enter,
            E_EBリセット = 'E',
            W_定速 = 'W',
            D_抑速1 = 'D',
            SP_ATS確認 = VK.Space,
            左 = VK.左,
            上 = VK.上,
            右 = VK.右,
            下 = VK.下,
            B_ブザー = 'B',
            非常 = VK特殊.非常,
            C_運転台表示 = 'C',
            V_HUD表示 = 'V',
            X_警報持続 = 'X',
            T_TASK = 'T',
            I_インチング = 'I',
            K_勾配起動 = 'K',
            Esc_ポーズ = VK.Esc,
            Y_復帰常用 = 'Y',
            U_復帰非常 = 'U',
            SHIFT_視点 = VK.SHIFT,
            WinAltR_録画 = VK特殊.WinAltR_録画,
            WinG_GameBar = VK特殊.WinG_GameBar,
        }

        public class C_選択肢
        {
            public int val { get; set; }
            public string txt { get; set; }

            public C_選択肢(int val, string txt)
            {
                this.val = val;
                this.txt = txt;
            }
        }



        private void Setting_Load(object sender, EventArgs e)
        {
            // コンボボックスの選択肢の設定（URL用）
            cbSelURL.Items.Add(DefURL);
            cbSelURL.Text = URL;

            // コンボボックスの選択肢の設定（VK操作用）            
            List<C_選択肢> 選択肢 = new();
            選択肢.Add(new (0, ""));
            foreach (VK操作 vk操作 in Enum.GetValues(typeof(VK操作)))
            {
                選択肢.Add(new((int)vk操作, vk操作.ToString()));
            }


            // コンボボックス用のDataを作成


            for (int i = 0; i < m_CbList.Length; i++)
            {
                m_CbList[i].DisplayMember = "txt";
                m_CbList[i].ValueMember = "val";
                m_CbList[i].DataSource = 選択肢.ToArray();
                m_CbList[i].SelectedValue = 操作リスト[i];
            }
        }


        private void buttonInit_Click(object sender, EventArgs e)
        {
            foreach (var item in Def操作)
            {
                ComboBox cb = m_CbList[(int)item.Key];
                cb.SelectedValue = (int)item.Value;
            }
        }


        private void buttonCancel_Click(object sender, EventArgs e) => this.Close();

        private void buttonOK_Click(object sender, EventArgs e)
        {
            foreach (var item in Def操作)
            {
                ComboBox cb = m_CbList[(int)item.Key];
                操作リスト[(int)item.Key] = cb.SelectedValue as int? ?? 0;
            }
            URL = cbSelURL.Text;
            IniSave();
            Close();
        }


        enum eModeStSel
        {
            sel, St, selSt, none, enable, disable
        }


        private static eModeStSel m_ModeStSel = 0;


        public static void GetKyes(uint val32, List<int> Keys1, List<int> Funcs)
        {
            bool mtcA   = CheckBit(val32, (uint)Bit.A   );
            bool mtcAA  = CheckBit(val32, (uint)Bit.AA  );
            bool mtcB   = CheckBit(val32, (uint)Bit.B   );
            bool mtcC   = CheckBit(val32, (uint)Bit.C   );
            bool mtcD   = CheckBit(val32, (uint)Bit.D   );
            bool mtcATS = CheckBit(val32, (uint)Bit.ATS );
            bool mtc上  = CheckBit(val32, (uint)Bit.上  );
            bool mtc下  = CheckBit(val32, (uint)Bit.下  );
            bool mtc左  = CheckBit(val32, (uint)Bit.左  );
            bool mtc右  = CheckBit(val32, (uint)Bit.右  );
            bool mtcST = CheckBit(val32, (uint)Bit.ST);
            bool mtcSe = CheckBit(val32, (uint)Bit.Se);


            if (!mtcA && !mtcAA && !mtcB && !mtcD && !mtcATS && !mtc上 && !mtc下 && !mtc左 && !mtc右 && !mtcST && !mtcSe)
            {   // どのボタンも押されていない → 次回はどのモードも友好
                m_ModeStSel = eModeStSel.enable;
            }
            else if (!mtcA && !mtcAA && !mtcB && !mtcD && !mtcATS && !mtc上 && !mtc下 && !mtc左 && !mtc右 && mtcST && mtcSe)            
            {   // Start + mtcSe の同時押し
                m_ModeStSel = eModeStSel.selSt;
            }
            else if (mtcST && !mtcSe && (m_ModeStSel == eModeStSel.enable || m_ModeStSel == eModeStSel.St))
            {   // Start + mtcSe の同時押し
                m_ModeStSel = eModeStSel.St;
            }
            else if (!mtcST && mtcSe && (m_ModeStSel == eModeStSel.enable || m_ModeStSel == eModeStSel.sel))
            {   // Start + mtcSe の同時押し
                m_ModeStSel = eModeStSel.sel;
            }
            else if (!mtcST && !mtcSe && (m_ModeStSel == eModeStSel.enable || m_ModeStSel == eModeStSel.none))
            {   // Start + mtcSe の同時押し
                m_ModeStSel = eModeStSel.none;
            }
            else
            {
                m_ModeStSel = eModeStSel.disable;
            }


            if (m_ModeStSel==eModeStSel.selSt ) //if (mtcST && mtcSe && (m_ModeStSel==0 || m_ModeStSel==1))
            {   // Start + Select の同時押し
                Keys1.Add(操作リスト[(int)Idx.selST]);
            }
            else if (m_ModeStSel == eModeStSel.St) //if (mtcST)
            {   // Start の同時押し
                if (mtcA || mtcAA )    Keys1.Add(操作リスト[(int)Idx.StA   ]);
                if (mtcB    )           Keys1.Add(操作リスト[(int)Idx.StB   ]);
                if (mtcC    )           Keys1.Add(操作リスト[(int)Idx.StC   ]);
                if (mtcD    )           Keys1.Add(操作リスト[(int)Idx.StD   ]);
                if (mtcATS  )           Keys1.Add(操作リスト[(int)Idx.StATS ]);
                if (mtc左   )           Keys1.Add(操作リスト[(int)Idx.St左  ]);
                if (mtc右   )           Keys1.Add(操作リスト[(int)Idx.St右  ]);
                if (mtc上   )           Keys1.Add(操作リスト[(int)Idx.St上  ]);
                if (mtc下   )           Keys1.Add(操作リスト[(int)Idx.St下  ]);
            }
            else if (m_ModeStSel == eModeStSel.sel) // if (mtcSe && (m_ModeStSel == 0 || m_ModeStSel == 3))
            {
                if (mtcA || mtcAA )    Keys1.Add(操作リスト[(int)Idx.selA  ]);
                if (mtcB    )           Keys1.Add(操作リスト[(int)Idx.selB  ]);
                if (mtcC    )           Keys1.Add(操作リスト[(int)Idx.selC  ]);
                if (mtcD    )           Keys1.Add(操作リスト[(int)Idx.selD  ]);
                if (mtcATS  )           Keys1.Add(操作リスト[(int)Idx.selATS]);
                if (mtc左   )           Keys1.Add(操作リスト[(int)Idx.sel左 ]);
                if (mtc右   )           Keys1.Add(操作リスト[(int)Idx.sel右 ]);
                if (mtc上   )           Keys1.Add(操作リスト[(int)Idx.sel上 ]);
                if (mtc下   )           Keys1.Add(操作リスト[(int)Idx.sel下 ]);
            }
            else if (m_ModeStSel == eModeStSel.none) //if ( m_ModeStSel == 0 || m_ModeStSel == 4)
            {
                if (mtcAA   )           Keys1.Add(操作リスト[(int)Idx.A強  ]);
                if (mtcA    )           Keys1.Add(操作リスト[(int)Idx.A    ]);
                if (mtcB    )           Keys1.Add(操作リスト[(int)Idx.B    ]);
                if (mtcC    )           Keys1.Add(操作リスト[(int)Idx.C    ]);
                if (mtcD    )           Keys1.Add(操作リスト[(int)Idx.D    ]);
                if (mtcATS  )           Keys1.Add(操作リスト[(int)Idx.ATS  ]);
                if (mtc左   )           Keys1.Add(操作リスト[(int)Idx.左   ]);
                if (mtc右   )           Keys1.Add(操作リスト[(int)Idx.右   ]);
                if (mtc上   )           Keys1.Add(操作リスト[(int)Idx.上   ]);
                if (mtc下   )           Keys1.Add(操作リスト[(int)Idx.下   ]);                
            }

            if (Keys1.Contains((int)VK特殊.非常))
            {
                Funcs.Add((int)VK特殊.非常);
            }
            if (Keys1.Contains((int)VK特殊.WinAltR_録画))
            {
                Keys1.Add((int)VK.Win);
                Keys1.Add((int)VK.Alt);
                Keys1.Add('R');
            }
            if (Keys1.Contains((int)VK特殊.WinG_GameBar))
            {
                Keys1.Add((int)VK.Win);
                Keys1.Add('G');
            }
            Keys1 = Keys1.Distinct().Where(x => x > 0).ToList();
        }

        private static bool CheckBit(uint bits, uint mask) => ((bits & mask) == mask);

    }
}

