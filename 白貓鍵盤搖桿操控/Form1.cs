using EventHook;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using WindowsInput;
using static System.Threading.Thread;

namespace 白貓鍵盤操控
{
    public partial class Form1 : Form
    {
        #region Windwos API   
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
        private static extern void SetForegroundWindow(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern short GetWindowRect(IntPtr hwnd, out RECT Rect);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        #endregion
        
        #region 變數
        const int SKILLSLEEPTIME = 300; //技能施放後的休息時間
        bool close = false, work = false;
        #region 判斷已按下的按鈕
        bool isD1Press = false;
        bool isD2Press = false;
        bool isD3Press = false;
        bool isSpacePress = false;
        bool isAttackPress = false;
        bool isSkill1Press = false;
        bool isSkill2Press = false;
        bool isSkill3Press = false;
        bool isMoveUpPress = false;
        bool isResetWindows = false;
        bool isMoveDownPress = false;
        bool isMoveLeftPress = false;
        bool isMoveRightPress = false;
        bool isResetWindowsPress = false;
        bool isMouseLeftButtonPress = false;
        #endregion
        #region XY位置
        int midX, midY;
        int upX, upY;
        int downX, downY;
        int leftX, leftY;
        int rightX, rightY;
        int sk1X, sk1Y;
        int sk2X, sk2Y;
        int sk3X, sk3Y;
        #endregion
        Controller xboxController = new Controller(UserIndex.One); //XBox搖桿
        List<Process> processList = new List<Process>(); //程式清單
        IntPtr windowsPtr; //視窗代碼
        IMouseSimulator mouse = new InputSimulator().Mouse; //滑鼠模擬
        Regex regex = new Regex("(NumPad[1234568]$)|([QWERS]$)|(Space)$(D[123]$)|");
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.Clear(); 
            processList.Clear();
            listBox1.Items.Clear();
            //取得現在所有執行的程式
            foreach (Process item in Process.GetProcesses())
            {
                //Try來排除遇到權限問題而導致的錯誤
                try
                {
                    //如果視窗標題不是空的
                    if (item.MainWindowTitle != "")
                    {
                        //ListBox新增一個項目，命名規則為"[程式的名稱] [程式的PID]: 程式的標題"
                        listBox1.Items.Add("[" + item.MainModule.ModuleName + "] [" + item.Id.ToString() + "]: " + item.MainWindowTitle);
                        //新增到程式清單裡
                        processList.Add(item);
                    }
                }
                catch (Exception) { }
            }
            if (xboxController.IsConnected) { WriteLine("手把已連接"); cb_ControllerPlay.Enabled = true; cb_ControllerPlay.Checked = true; cb_Move.Checked = true; }
            else { WriteLine("手把未連接"); cb_ControllerPlay.Enabled = false; cb_ControllerPlay.Checked = false; }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            close = true;
        }

        private void btn_Reset_Click(object sender, EventArgs e)
        {
            Form1_Load(sender, e);
            btn_Start.Enabled = false;
        }

        private void cb_TopShow_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = cb_TopShow.Checked;
        }

        private void cb_Move_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_Move.Checked && xboxController.IsConnected) { cb_ControllerPlay.Checked = true; cb_ControllerPlay.Enabled = true; }
            else { cb_ControllerPlay.Enabled = false; cb_ControllerPlay.Checked = false; }
        }

        private void cb_ControllerPlay_CheckedChanged(object sender, EventArgs e)
        {
            if (!xboxController.IsConnected)
            {
                cb_ControllerPlay.Checked = false; cb_ControllerPlay.Enabled = false;
                return;
            }
            if (cb_ControllerPlay.Checked) { cb_Move.Enabled = false; cb_Move.Checked = true; }
            else { cb_Move.Enabled = true; }
        }

        private void OnKeyInput(object sender, KeyInputEventArgs e)
        {
            if (GetForegroundWindow() != windowsPtr) return; //如果前景視窗的控制代碼跟選擇的視窗代碼不一樣，就不執行判斷
            string KeyName = e.KeyData.Keyname;
            if (!regex.Match(KeyName).Success) return; //如果按鍵名稱比對後的結果不一樣，就不處理
           
            if (e.KeyData.EventType == KeyEvent.down) //如果按鍵狀態是按下的
            {               
                if (Regex.Match(KeyName, "NumPad[8546]").Success) //如果是移動按鍵
                {
                    if (KeyName == "NumPad8") isMoveUpPress = true; //上
                    else if (KeyName == "NumPad5") isMoveDownPress = true; //下
                    else if (KeyName == "NumPad4") isMoveLeftPress = true; //左
                    else if (KeyName == "NumPad6") isMoveRightPress = true; //右
                }
                else if (Regex.Match(KeyName, "([QWERS]$)|NumPad[123]").Success) //如果是技能按鍵
                {
                    if (KeyName == "R") isResetWindowsPress = true; //重設視窗
                    if (KeyName == "W") isAttackPress = true; //攻擊
                    if (KeyName == "Q" || KeyName == "NumPad1") isSkill1Press = true; //技能1
                    if (KeyName == "E" || KeyName == "NumPad3") isSkill2Press = true; //技能2
                    if (KeyName == (cb_Move.Checked ? "S" : "W") || KeyName == "NumPad2") isSkill3Press = true; //武器技能
                }
                else if (Regex.Match(KeyName, "(D[123]$)").Success) //如果是功能按鍵
                {
                    if (KeyName == "D1") isD1Press = true; //暫停
                    if (KeyName == "D2") isD2Press = true; //繼續
                    if (KeyName == "D3") isD3Press = true; //停止
                }
                else if (KeyName == "Space") isSpacePress = true; //如果是空白鍵
            }            
            else if (e.KeyData.EventType == KeyEvent.up) //否則如果是彈起的
            {
                if (Regex.Match(KeyName, "NumPad[8546]").Success) //如果是移動按鍵
                {
                    if (KeyName == "NumPad8") isMoveUpPress = false; //上
                    if (KeyName == "NumPad5") isMoveDownPress = false; //下
                    if (KeyName == "NumPad4") isMoveLeftPress = false; //左
                    if (KeyName == "NumPad6") isMoveRightPress = false; //右
                }
                else if (Regex.Match(KeyName, "([QWERS]$)|NumPad[123]").Success) //如果是技能按鍵
                {
                    if (KeyName == "R") isResetWindowsPress = false; //重設視窗
                    if (KeyName == "W") isAttackPress = false; //攻擊
                    if (KeyName == "Q" || KeyName == "NumPad1") isSkill1Press = false; //技能1
                    if (KeyName == "E" || KeyName == "NumPad3") isSkill2Press = false; //技能2
                    if (KeyName == (cb_Move.Checked ? "S" : "W") || KeyName == "NumPad2") isSkill3Press = false; //武器技能
                }
                else if (Regex.Match(KeyName, "(D[123]$)").Success) //如果是功能按鍵
                {
                    if (KeyName == "D1") isD1Press = false; //暫停
                    if (KeyName == "D2") isD2Press = false; //繼續
                    if (KeyName == "D3") isD3Press = false; //停止
                }
                else if (KeyName == "Space") isSpacePress = false; //如果是空白鍵
            }
        }

        private void OnMouseInput(object sender, EventHook.MouseEventArgs e)
        {
            if (GetForegroundWindow() != windowsPtr) return; //如果前景視窗的控制代碼跟選擇的視窗代碼不一樣，就不執行判斷
            if (!e.Message.ToString().StartsWith("WM_LBUTTON")) return; //如果按下的按鈕不是滑鼠左鍵，就不判斷
            if (e.Message.ToString().EndsWith("DOWN")) isMouseLeftButtonPress = true; //按下
            else isMouseLeftButtonPress = false; //彈起
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("請確定以下資訊是否正確，否則有可能會出問題，如果出現問題" +
                "\r\n先嘗試按按看鍵盤上排的數字3或搖桿的BACK鍵，都不能的話在嘗試強制關閉或重新開機" +
                "\r\n選擇的程式PID: " + processList[listBox1.SelectedIndex].Id.ToString() +
                "\r\n選擇的視窗標題: " + processList[listBox1.SelectedIndex].MainWindowTitle
                , "", MessageBoxButtons.YesNo) == DialogResult.No) return;

            close = false;
            WorkSwitch(false);
            work = true;
            richTextBox1.Clear();

            #region 宣告與視窗處理
            const int LOOPSLEEP = 10;
            const int MOVESLEEP = 15;
            int moveAftelSleep = 15;
            GamepadButtonFlags button;
            bool stop = false, isMove = false, isSkillLongPress = false, isSkillPress = false, isAttack = false, isRoll = false;
            windowsPtr = processList[listBox1.SelectedIndex].MainWindowHandle;
            SetForegroundWindow(windowsPtr);
            ResetWindowsRect();
            #endregion

            #region 重設已按下的按鍵
            isD1Press = false;
            isD2Press = false;
            isD3Press = false;
            isSpacePress = false;
            isAttackPress = false;
            isSkill1Press = false;
            isSkill2Press = false;
            isSkill3Press = false;
            isMoveUpPress = false;
            isResetWindows = false;
            isMoveDownPress = false;
            isMoveLeftPress = false;
            isMoveRightPress = false;
            isResetWindowsPress = false;
            isMouseLeftButtonPress = false;
            #endregion

            #region 開新執行緒以防止介面卡死
            new Thread(new ThreadStart(delegate
            {                
                if (xboxController.IsConnected && cb_ControllerPlay.Checked) //如果搖桿已連接，並且使用搖桿遊玩
                {
                    #region 前置作業
                    WriteLine("XBox搖桿已連接，電量: " + GetBatteryChinese(xboxController.GetBatteryInformation(BatteryDeviceType.Gamepad).BatteryLevel));
                    WriteLine("即將偵測蘑菇頭置中時的XY軸，請確定蘑菇頭已置中");
                    int i = 2;
                    while (i != 0)
                    {
                        WriteLine(i.ToString() + "秒後偵測");
                        Sleep(1000);
                        i--;
                    }
                    State state;
                    state = xboxController.GetState(); //取得搖桿目前的狀態
                    int contLThumX = state.Gamepad.LeftThumbX, contLThumY = state.Gamepad.LeftThumbY; //設定左蘑菇頭置中時的XY
                    int contRThumX = state.Gamepad.RightThumbX, contRThumY = state.Gamepad.RightThumbY; //設定右蘑菇頭置中時的XY
                    moveAftelSleep = 50; //設定翻滾後休息50毫秒
                    WriteLine("左蘑菇頭置中時的XY為: " + contLThumX.ToString() + " , " + contLThumY.ToString());
                    WriteLine("右蘑菇頭置中時的XY為: " + contRThumX.ToString() + " , " + contRThumY.ToString());
                    #endregion
                    while (!stop && !close)
                    {
                        if (!xboxController.IsConnected) { WriteLine("搖桿已中斷連結"); break; } //如果搖桿斷線，就結束
                        if (GetForegroundWindow() != windowsPtr) { Sleep(250); continue; } //如果前景視窗的控制代碼跟選擇的視窗代碼不一樣，就不執行判斷
                        Sleep(LOOPSLEEP); //休息
                        state = xboxController.GetState(); //取得搖桿目前的狀態
                        button = state.Gamepad.Buttons; //取得已按下的按鈕

                        //DPad=十字按鍵(搖桿左下的)
                        if (button == GamepadButtonFlags.DPadUp)
                        {
                            if (!isRoll)
                            {
                                if (isMouseLeftButtonPress) mouse.LeftButtonUp();
                                WriteLine("上滑");
                                int y = -5;
                                isRoll = true;
                                mouse.LeftButtonDown();
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 10);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 15);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 20);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 25);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 30);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 35);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 40);
                                Sleep(moveAftelSleep);
                                mouse.LeftButtonUp();
                                SetCursorPos(midX, midY);
                            }
                        }
                        else if (button == GamepadButtonFlags.DPadDown)
                        {
                            if (!isRoll)
                            {
                                int y = 5;
                                isRoll = true;
                                mouse.LeftButtonDown();
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 10);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 15);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 20);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 25);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 30);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 35);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX, midY + y * 40);
                                Sleep(moveAftelSleep);
                                mouse.LeftButtonUp();
                                SetCursorPos(midX, midY);
                            }
                        }
                        else if (button == GamepadButtonFlags.DPadLeft)
                        {
                            if (!isRoll)
                            {
                                int x = -5;
                                isRoll = true;
                                mouse.LeftButtonDown();
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 10, midY);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 15, midY);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 20, midY);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 25, midY);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 30, midY);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 35, midY);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 40, midY);
                                Sleep(moveAftelSleep);
                                mouse.LeftButtonUp();
                                SetCursorPos(midX, midY);
                            }
                        }
                        else if (button == GamepadButtonFlags.DPadRight)
                        {
                            if (!isRoll)
                            {
                                int x = 5;
                                isRoll = true;
                                mouse.LeftButtonDown();
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 10, midY);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 15, midY);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 20, midY);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 25, midY);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 30, midY);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 35, midY);
                                Sleep(MOVESLEEP);
                                SetCursorPos(midX + x * 40, midY);
                                Sleep(moveAftelSleep);
                                mouse.LeftButtonUp();
                                SetCursorPos(midX, midY);
                            }
                        }
                        else { isRoll = false; }

                        //重設視窗位置
                        if (button == GamepadButtonFlags.Y)
                        {
                            if (!isResetWindows)
                            {
                                isResetWindows = true;
                                ResetWindowsRect();
                            }
                        }
                        else { isResetWindows = false; }

                        //攻擊與施放集氣技能(Shoulder=搖桿後方的短按鈕，Trigger=搖桿後方的長按鈕)
                        if (button.ToString() == "A" || button.ToString() == "LeftShoulder, A" || button.ToString() == "RightShoulder, A")
                        {
                            if (!isAttack)
                            {
                                mouse.LeftButtonDown();
                                isAttack = true;
                                WriteLine("攻擊");
                            }
                            else if (button.ToString() == "LeftShoulder, A")
                            {
                                if (!isSkillLongPress)
                                {
                                    isSkillLongPress = true;
                                    WriteLine("(集氣)技能1");
                                    SetCursorPos(midX, midY);
                                    SetCursorPos(sk1X, sk1Y);
                                    mouse.LeftButtonUp();
                                    Sleep(SKILLSLEEPTIME);
                                    SetCursorPos(midX, midY);
                                }
                            }
                            else if (button.ToString() == "RightShoulder, A")
                            {
                                if (!isSkillLongPress)
                                {
                                    isSkillLongPress = true;
                                    WriteLine("(集氣)技能2");
                                    SetCursorPos(midX, midY);
                                    SetCursorPos(sk2X, sk2Y);
                                    mouse.LeftButtonUp();
                                    Sleep(SKILLSLEEPTIME);
                                    SetCursorPos(midX, midY);
                                }
                            }
                            else if (xboxController.GetState().Gamepad.RightTrigger >= 128 || xboxController.GetState().Gamepad.LeftTrigger >= 128)
                            {
                                if (!isSkillLongPress)
                                {
                                    isSkillLongPress = true;
                                    WriteLine("(集氣)武器技能");
                                    SetCursorPos(midX, midY);
                                    SetCursorPos(sk3X, sk3Y);
                                    mouse.LeftButtonUp();
                                    Sleep(SKILLSLEEPTIME);
                                    SetCursorPos(midX, midY);
                                }
                            }
                        }
                        else { isAttack = false; isSkillLongPress = false; }

                        //施放技能
                        if (button == GamepadButtonFlags.LeftShoulder)
                        {
                            if (!isSkillPress && !isSkillLongPress)
                            {
                                isSkillPress = true;
                                WriteLine("技能1");
                                SetCursorPos(midX, midY);
                                mouse.LeftButtonDown();
                                Sleep(SKILLSLEEPTIME);
                                SetCursorPos(sk1X, sk1Y);
                                mouse.LeftButtonUp();
                                SetCursorPos(midX, midY);
                            }
                        }
                        else if (button == GamepadButtonFlags.RightShoulder)
                        {
                            if (!isSkillPress && !isSkillLongPress)
                            {
                                isSkillPress = true;
                                WriteLine("技能2");
                                SetCursorPos(midX, midY);
                                mouse.LeftButtonDown();
                                Sleep(SKILLSLEEPTIME);
                                SetCursorPos(sk2X, sk2Y);
                                mouse.LeftButtonUp();
                                SetCursorPos(midX, midY);
                            }
                        }
                        else if (xboxController.GetState().Gamepad.RightTrigger >= 128 || xboxController.GetState().Gamepad.LeftTrigger >= 128)
                        {
                            if (!isSkillPress && !isSkillLongPress)
                            {
                                isSkillPress = true;
                                WriteLine("武器技能");
                                SetCursorPos(midX, midY);
                                mouse.LeftButtonDown();
                                Sleep(SKILLSLEEPTIME);
                                SetCursorPos(sk3X, sk3Y);
                                mouse.LeftButtonUp();
                                SetCursorPos(midX, midY);
                            }
                        }
                        else isSkillPress = false;

                        //移動
                        if ((state.Gamepad.LeftThumbX <= contLThumX - 1000 || state.Gamepad.LeftThumbX >= contLThumX + 1000) || state.Gamepad.LeftThumbY <= contLThumY - 1000 || state.Gamepad.LeftThumbY >= contLThumY + 1000)
                        {
                            if (!isMove && !isMouseLeftButtonPress) { SetCursorPos(midX, midY); Sleep(50); mouse.LeftButtonDown(); isMove = true; }
                            Sleep(5);
                            if (!isAttack) SetCursorPos(midX - (contLThumX - state.Gamepad.LeftThumbX) / 5000 * 25, midY + (contLThumY - state.Gamepad.LeftThumbY) / 5000 * 20);
                            else SetCursorPos(midX - (contLThumX - state.Gamepad.LeftThumbX) / 5000 * 15, midY + (contLThumY - state.Gamepad.LeftThumbY) / 5000 * 10);
                        }
                        else if (isMove) { isMove = false; }
                        else { SetCursorPos(midX, midY); if (!isAttack) mouse.LeftButtonUp(); }

                        //暫停
                        if (button == GamepadButtonFlags.X)
                        {
                            WriteLine("暫停");
                            while (true)
                            {
                                if (!xboxController.IsConnected) { WriteLine("搖桿已中斷連結"); stop = true; break; }
                                button = xboxController.GetState().Gamepad.Buttons;
                                if (button == GamepadButtonFlags.B) { WriteLine("繼續"); break; }
                                if (button == GamepadButtonFlags.Back || close) { stop = true; break; }                                
                                Sleep(50);
                            }
                        }

                        //結束
                        if (button == GamepadButtonFlags.Back) stop = true;
                    }
                }
                else //否則使用鍵盤遊玩
                {
                    if (!xboxController.IsConnected) WriteLine("XBox搖桿未連接，使用鍵盤設置");
                    else WriteLine("已關閉搖桿操作，使用鍵盤設置");
                    if (!cb_Move.Checked) { WriteLine("移動控制已關閉，現在只有施放技能的作用"); WriteLine("武器技能施放案件已改成W"); }
                    KeyboardWatcher.Start(); //開始鍵盤偵測
                    KeyboardWatcher.OnKeyInput += OnKeyInput; //附加鍵盤事件
                    MouseWatcher.Start(); //開始滑鼠偵測
                    MouseWatcher.OnMouseInput += OnMouseInput; //附加滑鼠事件
                    while (!stop && !close)
                    {
                        if (GetForegroundWindow() != windowsPtr) { Sleep(500); continue; }//如果前景視窗的控制代碼跟選擇的視窗代碼不一樣，就不執行判斷
                        Sleep(LOOPSLEEP); //休息

                        //如果開啟移動控制
                        if (cb_Move.Checked)
                        {
                            //移動
                            if (!isSpacePress && IsMoveKeyPress())
                            {
                                if (!isMove && !isMouseLeftButtonPress) { SetCursorPos(midX, midY); Sleep(50); mouse.LeftButtonDown(); isMove = true; WriteLine("已按下"); }
                                if (isMoveLeftPress && isMoveUpPress) { SetCursorPos((isAttack ? leftX + 50 : leftX), (isAttack ? upY - 50 : upY)); WriteLine("左上"); }
                                else if (isMoveRightPress && isMoveUpPress) { SetCursorPos((isAttack ? rightX - 50 : rightX), (isAttack ? upY - 50 : upY)); WriteLine("右上"); }
                                else if (isMoveLeftPress && isMoveDownPress) { SetCursorPos(leftX, downY); WriteLine("左下"); }
                                else if (isMoveRightPress && isMoveDownPress) { SetCursorPos(rightX, downY); WriteLine("右下"); }
                                else if (isMoveUpPress) { SetCursorPos(upX, upY); WriteLine("上"); }
                                else if (isMoveDownPress) { SetCursorPos(downX, downY); WriteLine("下"); }
                                else if (isMoveLeftPress) { SetCursorPos(leftX, leftY); WriteLine("左"); }
                                else if (isMoveRightPress) { SetCursorPos(rightX, rightY); WriteLine("右"); }
                            }
                            else if (isMove) { isMove = false; }

                            //翻滾
                            else if (isSpacePress)
                            {
                                if (isMoveUpPress)
                                {
                                    if (!isRoll)
                                    {
                                        if (isMouseLeftButtonPress) mouse.LeftButtonUp();
                                        WriteLine("上滑");
                                        int y = -5;
                                        isRoll = true;
                                        mouse.LeftButtonDown();
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 10);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 15);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 20);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 25);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 30);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 35);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 40);
                                        Sleep(moveAftelSleep);
                                        mouse.LeftButtonUp();
                                        SetCursorPos(midX, midY);
                                    }
                                }
                                else if (isMoveDownPress)
                                {
                                    if (!isRoll)
                                    {
                                        if (isMouseLeftButtonPress) mouse.LeftButtonUp();
                                        WriteLine("下滑");
                                        int y = 5;
                                        isRoll = true;
                                        mouse.LeftButtonDown();
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 10);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 15);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 20);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 25);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 30);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 35);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX, midY + y * 40);
                                        Sleep(moveAftelSleep);
                                        mouse.LeftButtonUp();
                                        SetCursorPos(midX, midY);
                                    }
                                }
                                else if (isMoveLeftPress)
                                {
                                    if (!isRoll)
                                    {
                                        if (isMouseLeftButtonPress) mouse.LeftButtonUp();
                                        WriteLine("左滑");
                                        int x = -5;
                                        isRoll = true;
                                        mouse.LeftButtonDown();
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 10, midY);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 15, midY);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 20, midY);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 25, midY);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 30, midY);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 35, midY);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 40, midY);
                                        Sleep(moveAftelSleep);
                                        mouse.LeftButtonUp();
                                        SetCursorPos(midX, midY);
                                    }
                                }
                                else if (isMoveRightPress)
                                {
                                    if (!isRoll)
                                    {
                                        if (isMouseLeftButtonPress) mouse.LeftButtonUp();
                                        WriteLine("右滑");
                                        int x = 5;
                                        isRoll = true;
                                        mouse.LeftButtonDown();
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 10, midY);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 15, midY);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 20, midY);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 25, midY);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 30, midY);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 35, midY);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(midX + x * 40, midY);
                                        Sleep(moveAftelSleep);
                                        mouse.LeftButtonUp();
                                        SetCursorPos(midX, midY);
                                    }
                                }
                                else { isRoll = false; }
                            }
                            else if (isRoll) { isRoll = false; }

                            else { SetCursorPos(midX, midY); if (!isAttackPress && isMouseLeftButtonPress) mouse.LeftButtonUp(); }
                        }

                        //攻擊
                        if (isAttackPress)
                        {
                            if (!isAttack)
                            {
                                mouse.LeftButtonDown();
                                isAttack = true;
                                WriteLine("攻擊");
                            }
                        }
                        else { isAttack = false; }

                        //技能
                        if (isSkill1Press)
                        {
                            if (!isSkillPress)
                            {
                                isSkillPress = true;
                                Point old_point = Cursor.Position;
                                if (isAttack) WriteLine("(集氣)技能1");
                                else
                                {
                                    if (isMouseLeftButtonPress) mouse.LeftButtonUp();
                                    WriteLine("技能1");
                                    SetCursorPos(midX, midY);
                                    mouse.LeftButtonDown();
                                    Sleep(SKILLSLEEPTIME);
                                }
                                SetCursorPos(sk1X, sk1Y);
                                mouse.LeftButtonUp();
                                if (!cb_Move.Checked) SetCursorPos(old_point.X, old_point.Y);
                            }
                        }
                        else if (isSkill2Press)
                        {
                            if (!isSkillPress)
                            {
                                isSkillPress = true;
                                Point old_point = Cursor.Position;
                                if (isAttack) WriteLine("(集氣)技能2");
                                else
                                {
                                    if (isMouseLeftButtonPress) mouse.LeftButtonUp();
                                    WriteLine("技能2");
                                    SetCursorPos(midX, midY);
                                    mouse.LeftButtonDown();
                                    Sleep(SKILLSLEEPTIME);
                                }
                                SetCursorPos(sk2X, sk2Y);
                                mouse.LeftButtonUp();
                                if (!cb_Move.Checked) SetCursorPos(old_point.X, old_point.Y);
                            }
                        }
                        else if (isSkill3Press)
                        {
                            if (!isSkillPress)
                            {
                                isSkillPress = true;
                                Point old_point = Cursor.Position;
                                if (isAttack) WriteLine("(集氣)武器技能");
                                else
                                {
                                    if (isMouseLeftButtonPress) mouse.LeftButtonUp();
                                    WriteLine("武器技能");
                                    SetCursorPos(midX, midY);
                                    mouse.LeftButtonDown();
                                    Sleep(SKILLSLEEPTIME);
                                }
                                SetCursorPos(sk3X, sk3Y);
                                mouse.LeftButtonUp();
                                if (!cb_Move.Checked) SetCursorPos(old_point.X, old_point.Y);
                            }
                        }
                        else { isSkillPress = false; }

                        //重設視窗
                        if (isResetWindowsPress) { if (!isResetWindows) { isResetWindows = true; ResetWindowsRect(); } }
                        else isResetWindows = false;

                        //暫停
                        if (isD1Press)
                        {
                            WriteLine("暫停");
                            while (true)
                            {
                                if (isD2Press) { WriteLine("繼續"); break; }
                                if (isD3Press || close) { stop = true; break; }
                                Sleep(50);
                            }
                        }

                        //停止
                        if (isD3Press) stop = true;
                    }
                    KeyboardWatcher.Stop(); //停止鍵盤偵測
                    MouseWatcher.Stop(); //停止滑鼠偵測
                }
                WriteLine("已結束");
                work = false;
                WorkSwitch(true);
            })).Start();
            #endregion
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            lab_PID.Text = "選擇的程式PID: " + processList[listBox1.SelectedIndex].Id.ToString();
            lab_PTitle.Text = "選擇的視窗標題: " + processList[listBox1.SelectedIndex].MainWindowTitle;
            btn_Start.Enabled = !work;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (IsDisposed) return;
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();
        }

        string LastStr = "";
        /// <summary>
        /// 寫入指定字串到RichTextBox
        /// </summary>
        /// <param name="Text">字串</param>
        private void WriteLine(string Text)
        {
            if (IsDisposed) return;

            Text += "\r\n"; //加上分行符號
            if (LastStr == Text) return; //如果最後的字串跟現在的字串一樣，就跳過(防止重複輸出)

            //輸出格式"[現在時間] 字串"
            if (InvokeRequired) BeginInvoke(new Action(delegate { richTextBox1.AppendText("[" + DateTime.Now.ToString("hh-mm-ss") + "] " + Text); }));
            else richTextBox1.AppendText("[" + DateTime.Now.ToString("hh-mm-ss") + "] " + Text);

            LastStr = Text;
        }

        /// <summary>
        /// 設定游標的位置
        /// </summary>
        /// <param name="x">X軸</param>
        /// <param name="y">Y軸</param>
        private void SetCursorPos(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }
        
        /// <summary>
        /// 切換控制項的啟用開關
        /// </summary>
        /// <param name="enable">True為開，False為關</param>
        private void WorkSwitch(bool enable)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(new Action(delegate
            {
                btn_Start.Enabled = enable; //開始按鈕
                btn_Reset.Enabled = enable; //重設按鈕
                cb_ControllerPlay.Enabled = xboxController.IsConnected && enable; //搖桿控制方塊
                cb_Move.Enabled = enable && !cb_ControllerPlay.Enabled; //移動控制方塊
            }));
            else
            {
                btn_Start.Enabled = enable; //開始按鈕
                btn_Reset.Enabled = enable; //重設按鈕
                cb_ControllerPlay.Enabled = xboxController.IsConnected && enable; //搖桿控制方塊
                cb_Move.Enabled = enable && !cb_ControllerPlay.Enabled; //移動控制方塊
            }
        }

        /// <summary>
        /// 取得電量的中文
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private string GetBatteryChinese(BatteryLevel level)
        {
            switch (level)
            {
                case BatteryLevel.Empty:
                    return "空";
                case BatteryLevel.Low:
                    return "低";
                case BatteryLevel.Medium:
                    return "中";
                case BatteryLevel.Full:
                    return "高";
                default:
                    return "錯誤";
            }
        }

        /// <summary>
        /// 重設視窗大小
        /// </summary>
        private void ResetWindowsRect()
        {
            GetWindowRect(windowsPtr, out RECT rect);
            midX = (rect.Left + rect.Right) / 2;
            midY = (rect.Top + rect.Bottom) / 2;
            upX = midX;
            upY = midY - 100;
            downX = midX;
            downY = midY + 100;
            leftX = midX - 100;
            leftY = midY;
            rightX = midX + 100;
            rightY = midY;
            sk1X = midX - 100;
            sk1Y = midY - 110;
            sk2X = midX + 100;
            sk2Y = midY - 110;
            sk3X = midX;
            sk3Y = midY + 180;
            WriteLine("已重設視窗位置");
            WriteLine("上: " + rect.Top + " 下: " + rect.Bottom);
            WriteLine("左: " + rect.Left + " 右: " + rect.Right);
        }
        
        /// <summary>
        /// 取得移動按鍵是否被按下
        /// </summary>
        /// <returns></returns>
        private bool IsMoveKeyPress()
        {
            return isMoveUpPress || isMoveDownPress || isMoveLeftPress || isMoveRightPress;
        }
    }
}