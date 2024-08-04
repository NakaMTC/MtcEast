using Accessibility;
using System.Diagnostics;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MtcEast
{
    public struct MtcInfo
    {
        public bool Enable = false;
        public int Min = 0, Max = 0;
        public int Val = 0, Val0 = 0;
        public int FR = 0, FR0 = 1;

        public bool A = false, AA = false, B = false, C = false, D = false;
        public bool ATS = false;
        public bool Start = false, Sel = false;
        public bool 左 = false, 上 = false, 下 = false, 右 = false;

        public bool SA = false, SAA = false, SB = false, SC = false, SD = false;
        public bool SStart = false, SSel = false;
        public bool S左 = false, S上 = false, S下 = false, S右 = false;

        public bool pendAA = false;
        public bool pendSA = false, pendSAA = false, pendSB = false, pendSC = false, pendSD = false;
        public bool pendSStart = false, pendSSel = false;
        public bool pendS左 = false, pendS上 = false, pendS下 = false, pendS右 = false;


        public MtcInfo()
        {
        }
    }





    public partial class Form1 : Form
    {

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUPDOWN = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const byte VK_UP = 0x26;
        private const byte VK_DOWN = 0x28;


        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const int MOUSEEVENTF_WHEEL = 0x0800;




        /// <summary> USBのReader </summary>
        private UsbEndpointReader? m_UsbEndpointReader = null;

        /// <summary>　実行中のタスク </summary>
        Task mTask;

        /// <summary>　実行中のタスクを修了させるかどうか？ </summary>
        bool mEndFlag = false;

        /// <summary>　MTCを有効にするかどうか？ </summary>
        bool mEnable = false;

        public Form1()
        {
            InitializeComponent();

            mTask = new Task(TaskRun);
            mTask.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }


        private void buttonSettings_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = mEnable = false;

            using (var f = new Setting())
            {
                f.ShowDialog();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var res = MessageBox.Show("修了しますか？", "修了確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes)
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
                

                if (mTask != null)
                {
                    mEndFlag = true;
                    mTask.Wait();
                }
            }
        }


        private void TaskRun()
        {
             
            uint m_PrevUint32 = 0; // 前回読み込み時のUSBのバイト列→32ビットに変換したもの

            MtcInfo mtc = new MtcInfo();
            List<int> Keys0 = new List<int>();

            while (mEndFlag == false)
            {
                try
                {
                    //--- USBをOPEN ---
                    if (m_UsbEndpointReader == null)    // UsbEndpointReader の OPEN
                    {
                        UsbDevice? device = null;

                        mtc.Max = mtc.Min = 0;

                        foreach (UsbRegistry reg in UsbDevice.AllDevices)
                        {
                            if (reg.Vid == 0x0AE4 && reg.Pid == 0x0101 && reg.Rev == 0400) { mtc.Max = 4; mtc.Min = -7; }   // P4 B6 (非常を含めて-7)
                            if (reg.Vid == 0x0AE4 && reg.Pid == 0x0101 && reg.Rev == 0300) { mtc.Max = 4; mtc.Min = -8; }   // P4 B7 (非常を含めて-8)
                            if (reg.Vid == 0x1C06 && reg.Pid == 0x77A7 && reg.Rev == 0202) { mtc.Max = 5; mtc.Min = -6; }   // P4 B5 (非常を含めて-6)
                            if (reg.Vid == 0x0AE4 && reg.Pid == 0x0101 && reg.Rev == 0800) { mtc.Max = 5; mtc.Min = -8; }   // P5 B7 (非常を含めて-8)
                            if (reg.Vid == 0x0AE4 && reg.Pid == 0x0101 && reg.Rev == 0000) { mtc.Max = 13; mtc.Min = -8; }  // P5 B7 (非常を含めて-8)

                            if (reg.Vid == 0x0AE4 && reg.Pid == 0x0004 && reg.Rev == 0100) { mtc.Max = 5; mtc.Min = -9; }  // P5 B9 (非常を含めて-9)


                            if (mtc.Max == 5 && mtc.Min == -9)
                            {
                                


                                UsbDeviceFinder finder = new UsbDeviceFinder(reg.Pid, reg.Vid, reg.Rev);
                                device = UsbDevice.OpenUsbDevice(finder);


                                if (reg.Open(out device) && device != null) break;  // 接続に成功 → 次のステップに進む
                            }
                            else if (mtc.Max > 0 && mtc.Min < 0)
                            {
                                if (reg.Open(out device) && device != null) break;  // 接続に成功 → 次のステップに進む
                                else throw new Exception($"接続失敗\r\n{reg.Name}\r\n{UsbDevice.LastErrorString}");
                            }
                        }

                        if (device == null) return;
                        m_UsbEndpointReader = device.OpenEndpointReader(ReadEndpointID.Ep01);
                    }


                    // 8バイトバッファへの読み込み
                    byte[] buff = new byte[8];  // 8バイトバッファ
                    int readLen = 0;
                    ErrorCode code = m_UsbEndpointReader.Read(buff, 2000, out readLen);


                    uint tmp;

                    if (readLen > 0 && mEnable)
                    {
                        tmp = (uint)buff[1] << 00 | (uint)buff[2] << 08 | (uint)buff[3] << 16 | (uint)buff[4] << 24;
                    }
                    else
                    {
                        tmp = 0;
                    }

                    if (m_PrevUint32 != tmp)                    
                    {
                        uint bitA = 0x0400;
                        uint bitAA = 0x0c00;
                        uint bitB = 0x1000;
                        uint bitC = 0x2000;
                        uint bitD = 0x0200;

                        uint bitS = 0x0100;
                        uint bitSt = 0x00010000;
                        uint bitSe = 0x00020000;

                        uint bitUp = 0x00040000;
                        uint bitDown = 0x00080000;
                        uint bitLeft = 0x00100000;
                        uint bitRight = 0x00200000;


                        mtc.Enable = (tmp != 0);

                        if (mtc.Enable)
                        {
                            Debug.WriteLine($"0x{tmp:x}");

                            if (mtc.Max == 13)  // P13～Bxの場合
                            {
                                int fr = (int)((tmp & 0xE0) >> 4);
                                if (fr == 0) mtc.FR = 0;
                                else if (fr == 4) mtc.FR = -1;
                                if (fr == 8) mtc.FR = 1;

                                mtc.Val = (int)(tmp & 0x1F) + mtc.Min - 1;
                            }
                            else if (mtc.Min == -6)  // P5～B5 (非常を含めてB6) の場合
                            {
                                int fr = (int)(tmp & 0x30);
                                if (fr == 0) mtc.FR = 0;
                                else if (fr == 16) mtc.FR = -1;
                                if (fr == 32) mtc.FR = 1;
                                //Debug.WriteLine($"tmp=0x{tmp:x} 0x30 0x{tmp & 0x30:x} fr={mtc.FR}");

                                mtc.Val = (int)(tmp & 0x0F) + mtc.Min - 1;
                                //Debug.WriteLine($"mtc.Val={mtc.Val}");


                                // キハ54用にブレーキの段数を変更
                                if (mtc.Val == 0 || mtc.Val == -1) mtc.Val = 0;          // B0～B1 → B0 運転 に変更
                                else if (mtc.Val == -2 || mtc.Val == -3) mtc.Val = -1;   // B2～B3 → B1 重り に変更
                                else if (mtc.Val == -4 || mtc.Val == -5) mtc.Val = -2;   // B4～B5 → B2 制動 に変更
                                else if (mtc.Val == -6) mtc.Val = -3;                    // B6     → B3 非常 に変更
                                Debug.WriteLine($"mtc.Val={mtc.Val}");
                            }
                            else
                            {
                                int fr = (int)((tmp & 0xF0) >> 4);
                                if (fr == 0) mtc.FR = 0;
                                else if (fr == 4) mtc.FR = -1;
                                if (fr == 8) mtc.FR = 1;

                                mtc.Val = (int)(tmp & 0x0F) + mtc.Min - 1;
                            }
                        }

                        // 各ボタンの状態を保持                        
                        mtc.A = CheckBit(tmp, bitA);
                        mtc.AA = CheckBit(tmp, bitAA);
                        mtc.B = CheckBit(tmp, bitB);
                        mtc.C = CheckBit(tmp, bitC);
                        mtc.D = CheckBit(tmp, bitD);
                        mtc.ATS = CheckBit(tmp, bitS);
                        mtc.Start = CheckBit(tmp, bitSt);
                        mtc.Sel = CheckBit(tmp, bitSe);
                        mtc.左 = CheckBit(tmp, bitLeft);
                        mtc.上 = CheckBit(tmp, bitUp);
                        mtc.下 = CheckBit(tmp, bitDown);
                        mtc.右 = CheckBit(tmp, bitRight);

                        // ATS同時のチェック
                        mtc.SA = mtc.ATS & mtc.A;
                        mtc.SAA = mtc.ATS & mtc.AA;
                        mtc.SB = mtc.ATS & mtc.B;
                        mtc.SC = mtc.ATS & mtc.C;
                        mtc.SD = mtc.ATS & mtc.D;
                        mtc.SStart = mtc.ATS & mtc.Start;
                        mtc.SSel = mtc.ATS & mtc.Sel;
                        mtc.S左 = mtc.ATS & mtc.左;
                        mtc.S上 = mtc.ATS & mtc.上;
                        mtc.S下 = mtc.ATS & mtc.下;
                        mtc.S右 = mtc.ATS & mtc.右;

                        // Pending
                        if (mtc.AA || mtc.SAA) mtc.pendAA = true;
                        //if (mtc.ATS)
                        //{
                            if (mtc.SA) mtc.pendSA = true;
                            if (mtc.SAA) mtc.pendSAA = true;
                            if (mtc.SB) mtc.pendSB = true;
                            if (mtc.SC) mtc.pendSC = true;
                            if (mtc.SD) mtc.pendSD = true;
                            if (mtc.SStart) mtc.pendSStart = true;
                            if (mtc.SSel) mtc.pendSSel = true;
                            if (mtc.S左) mtc.pendS左 = true;
                            if (mtc.S上) mtc.pendS上 = true;
                            if (mtc.S下) mtc.pendS下 = true;
                            if (mtc.S右) mtc.pendS右 = true;
                        //}

                        // Pending解除
                        if (!mtc.A && !mtc.AA) mtc.pendAA = false;
                        if (!mtc.ATS)
                        {
                            if (!mtc.A && !mtc.AA) mtc.pendSA = mtc.pendSAA = false;
                            if (!mtc.B) mtc.pendSB = false;
                            if (!mtc.C) mtc.pendSC = false;
                            if (!mtc.D) mtc.pendSD = false;
                            if (!mtc.Start) mtc.pendSStart = false;
                            if (!mtc.Sel) mtc.pendSSel = false;
                            if (!mtc.左) mtc.pendS左 = false;
                            if (!mtc.上) mtc.pendS上 = false;
                            if (!mtc.下) mtc.pendS下 = false;
                            if (!mtc.右) mtc.pendS右 = false;
                        }


                        if (mtc.pendAA) mtc.A = mtc.SA = false;
                        if (mtc.pendSA) mtc.ATS = mtc.A = mtc.AA = false;
                        if (mtc.pendSAA) mtc.ATS = mtc.A = mtc.AA = mtc.SA = false;
                        if (mtc.pendSB) mtc.ATS = mtc.B = false;
                        if (mtc.pendSC) mtc.ATS = mtc.C = false;
                        if (mtc.pendSD) mtc.ATS = mtc.D = false;
                        if (mtc.pendSStart) mtc.ATS = mtc.Start = false;
                        if (mtc.pendSSel) mtc.ATS = mtc.Sel = false;
                        if (mtc.pendS左) mtc.ATS = mtc.左 = false;
                        if (mtc.pendS右) mtc.ATS = mtc.右 = false;
                        if (mtc.pendS上) mtc.ATS = mtc.上 = false;
                        if (mtc.pendS下) mtc.ATS = mtc.下 = false;

                        

                        // 前回の状態を保持
                        m_PrevUint32 = tmp;


                        //Debug.WriteLine($"{m_PrevUint32}  max={mtc.Max} min={mtc.Min} Value={mtc.Val} FR={mtc.FR}");
                        //Debug.WriteLine($"A={(mtc.A ? 1 : 0)} A2={(mtc.A2 ? 1 : 0)} B={(mtc.B ? 1 : 0)} C={(mtc.C ? 1 : 0)} D={(mtc.D ? 1 : 0)} ATS={(mtc.ATS ? 1 : 0)}");
                        //Debug.WriteLine($"Start={(mtc.Start ? 1 : 0)} Select={(mtc.Sel ? 1 : 0)} ↑={(mtc.上 ? 1 : 0)} ↓={(mtc.下 ? 1 : 0)} ←={(mtc.左 ? 1 : 0)} →={(mtc.右 ? 1 : 0)}");


                        // ボタンの変更をチェック
                        this.Invoke(new Action (()=>{
                            labelA.Visible = mtc.A;
                            labelAA.Visible = mtc.AA;
                            labelB.Visible = mtc.B;
                            labelC.Visible = mtc.C;
                            labelD.Visible = mtc.D;
                            labelATS.Visible = mtc.ATS;
                            labelStart.Visible = mtc.Start;
                            labelSel.Visible = mtc.Sel;
                            label上.Visible = mtc.上;
                            label下.Visible = mtc.下;
                            label右.Visible = mtc.右;
                            label左.Visible = mtc.左;


                            labelSA.Visible = mtc.SA;
                            labelSAA.Visible = mtc.SAA;
                            labelSB.Visible = mtc.SB;
                            labelSC.Visible = mtc.SC;
                            labelSD.Visible = mtc.SD;
                            labelSStart.Visible = mtc.SStart;
                            labelSSel.Visible = mtc.SSel;
                            labelS上.Visible = mtc.S上;
                            labelS下.Visible = mtc.S下;
                            labelS右.Visible = mtc.S右;
                            labelS左.Visible = mtc.S左;
                        }));

                        // キー入力
                        List<int> Keys1 = new List<int>();
                        List<int> Funcs = new List<int>();

                        if (mtc.A) Keys1.AddRange(Setting.A);
                        if (mtc.AA) Keys1.AddRange(Setting.AA);
                        if (mtc.B) Keys1.AddRange(Setting.B);
                        if (mtc.C) Keys1.AddRange(Setting.C);
                        if (mtc.D) Keys1.AddRange(Setting.D);
                        if (mtc.ATS) Keys1.AddRange(Setting.ATS);
                        if (mtc.Start) Keys1.AddRange(Setting.Start);
                        if (mtc.Sel) Keys1.AddRange(Setting.Sel);
                        if (mtc.上) Keys1.AddRange(Setting.上);
                        if (mtc.下) Keys1.AddRange(Setting.下);
                        if (mtc.右) Keys1.AddRange(Setting.右);
                        if (mtc.左) Keys1.AddRange(Setting.左);

                        if (mtc.SA) Keys1.AddRange(Setting.SA);
                        if (mtc.SAA) Keys1.AddRange(Setting.SAA);
                        if (mtc.SB) Keys1.AddRange(Setting.SB);
                        if (mtc.SC) Keys1.AddRange(Setting.SC);
                        if (mtc.SD) Keys1.AddRange(Setting.SD);
                        if (mtc.SStart) Keys1.AddRange(Setting.SStart);
                        if (mtc.SSel) Keys1.AddRange(Setting.SSel);
                        if (mtc.S上) Keys1.AddRange(Setting.S上);
                        if (mtc.S下) Keys1.AddRange(Setting.S下);
                        if (mtc.S右) Keys1.AddRange(Setting.S右);
                        if (mtc.S左) Keys1.AddRange(Setting.S左);

                        Keys1 = Keys1.Distinct().ToList();
                        Funcs = Keys1.Where(x => x < 0).ToList();
                        Keys1 = Keys1.Where(x => x > 0).ToList();


                        // OFF→ONをチェック
                        List<int> on = Keys1.Where(x => Keys0.Contains(x) == false).ToList();
                        List<int> off = Keys0.Where(x => Keys1.Contains(x) == false).ToList();
                        if (on.Count > 0)
                        {
                            foreach (var item in on)
                            {
                                keybd_event((byte)item, 0, KEYEVENTF_KEYDOWN, 0);
                                Keys0.Add(item);
                            }
                        }
                        if (off.Count > 0)
                        {
                            foreach (var item in off)
                            {
                                keybd_event((byte)item, 0, KEYEVENTF_KEYUP, 0);
                                Keys0.Remove(item);
                            }
                            
                        }

                        // ノッチの移動をチェック                        
                        if (mtc.Val <= mtc.Min && (Funcs.Contains(Setting.VK_非常) || mtc.Val0 == -9)) mtc.Val = -9;
                        if (mtc.Val0 != mtc.Val)
                        {
                            int move;
                            if (mtc.Val == -9)
                            {
                                move = -30;
                            }
                            //else if(mtc.Val == mtc.Max)
                            //{
                            //    move = 30;
                            //}
                            else  
                            {
                                move = mtc.Val - mtc.Val0;
                            }


                            if (mtc.Val == 0)
                            {
                                mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                                Thread.Sleep(10);
                                mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                                Thread.Sleep(10);
                                mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                                Thread.Sleep(10);
                                mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                            }
                            else if (move != 0)
                            {
                                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -move * 120, 0);
                            }

                            Debug.WriteLine($"ノッチ：{mtc.Val0} → {mtc.Val} move:{move}");




                            mtc.Val0 = mtc.Val;
                        }


                        //Debug.WriteLine($"レバーサー：{mtc.FR}");

                        // 前後交代をチェック
                        if (mtc.FR0 != mtc.FR)
                        {
                            Debug.WriteLine($"レバーサー：{mtc.FR0} → {mtc.FR}");

                            // キハ54対応 B5 （非常を含めるとB6）の場合、↑↓（前後切替）の代わりに RF（直変切替）とする
                            byte FR_UP = (mtc.Min == -6 ? (byte)'R' : VK_UP);
                            byte FR_DOWN = (mtc.Min == -6 ? (byte)'F' : VK_DOWN);
                            


                            if (mtc.FR > 0)
                            {   // OnFrChanged : 前進 ↑↑
                                keybd_event(FR_UP, 0, KEYEVENTF_KEYUPDOWN, 0);
                                Thread.Sleep(10);
                                keybd_event(FR_UP, 0, KEYEVENTF_KEYUPDOWN, 0);
                            }
                            else if(mtc.FR < 0)
                            {   // OnFrChanged : 後退 ↓↓
                                keybd_event(FR_DOWN, 0, KEYEVENTF_KEYUPDOWN, 0);
                                Thread.Sleep(10);
                                keybd_event(FR_DOWN, 0, KEYEVENTF_KEYUPDOWN, 0);
                            }
                            else if (mtc.FR0 > 0)
                            {   // OnFrChanged : 前進→中立 ↓
                                keybd_event(FR_DOWN, 0, KEYEVENTF_KEYUPDOWN, 0);
                            }
                            else if (mtc.FR0 < 0)
                            {   // OnFrChanged : 後退→中立 ↑
                                keybd_event(FR_UP, 0, KEYEVENTF_KEYUPDOWN, 0);
                            }

                            mtc.FR0 = mtc.FR;
                        }
                    }
                }
                catch (Exception)
                {
                }
            }


            //--- 修了処理 ---
            try
            {
                if(m_UsbEndpointReader != null)
                {
                    m_UsbEndpointReader.Device.Close();
                    m_UsbEndpointReader.Dispose();
                }
            }
            catch (Exception)
            {
                m_UsbEndpointReader = null;
                m_PrevUint32 = 0;
            }
        }




        private static bool CheckBit(uint bits, uint mask)
        {
            return ((bits & mask) == mask);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Select();
            mEnable = checkBox1.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            


            if (m_UsbEndpointReader　!= null)
            {
                checkBox1.Checked = true;
                textBox1.Select();


                var info = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = Setting.URL,
                };
                Process.Start(info);



                //ProcessStartInfo pInfo = new ProcessStartInfo();
                //pInfo.FileName = Setting.URL;

                //Process.Start(pInfo);
            }
        }
    }
}