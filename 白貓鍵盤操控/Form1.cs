using System;
using System.Threading;
using static System.Threading.Thread;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WindowsInput;
using System.Drawing;
using System.Text.RegularExpressions;
using SharpDX.XInput;
using EventHook;

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

        const int SKILLSLEEPTIME = 300;

        #region 變數
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
        bool close = false, work = false;
        int mid_x, mid_y;
        int up_x, up_y;
        int down_x, down_y;
        int left_x, left_y;
        int right_x, right_y;
        int sk1_x, sk1_y;
        int sk2_x, sk2_y;
        int sk3_x, sk3_y;        
        Controller xboxController = new Controller(UserIndex.One);
        List<Process> processlist = new List<Process>();
        IntPtr WindowsPtr;
        IMouseSimulator Mouse = new InputSimulator().Mouse;
        Regex regex = new Regex("(NumPad[1234568]$)|([QWERS]$)|(Space)$(D[123]$)|");
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            processlist.Clear();
            listBox1.Items.Clear();
            foreach (Process item in Process.GetProcesses())
            {
                try
                {
                    if (item.MainWindowTitle != "") { listBox1.Items.Add("[" + item.MainModule.ModuleName + "] [" + item.Id.ToString() + "]: " + item.MainWindowTitle); processlist.Add(item); }
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
            if (GetForegroundWindow() != WindowsPtr) return;
            string KeyName = e.KeyData.Keyname;
            if (!regex.Match(KeyName).Success) return;
            if (e.KeyData.EventType == KeyEvent.down)
            {
                if (Regex.Match(KeyName, "NumPad[8546]").Success)
                {
                    if (KeyName == "NumPad8") isMoveUpPress = true;
                    else if (KeyName == "NumPad5") isMoveDownPress = true;
                    else if (KeyName == "NumPad4") isMoveLeftPress = true;
                    else if (KeyName == "NumPad6") isMoveRightPress = true;
                }
                else if (Regex.Match(KeyName, "([QWERS]$)|NumPad[123]").Success)
                {
                    if (KeyName == "R") isResetWindowsPress = true;
                    if (KeyName == "W") isAttackPress = true;
                    if (KeyName == "Q" || KeyName == "NumPad1") isSkill1Press = true;
                    if (KeyName == "E" || KeyName == "NumPad3") isSkill2Press = true;
                    if (KeyName == (cb_Move.Checked ? "S" : "W") || KeyName == "NumPad2") isSkill3Press = true;
                }
                else if (Regex.Match(KeyName, "(D[123]$)").Success)
                {
                    if (KeyName == "D1") isD1Press = true;
                    if (KeyName == "D2") isD2Press = true;
                    if (KeyName == "D3") isD3Press = true;
                }
                else if (KeyName == "Space") isSpacePress = true;
            }
            else if (e.KeyData.EventType == KeyEvent.up)
            {
                if (Regex.Match(KeyName, "NumPad[8546]").Success)
                {
                    if (KeyName == "NumPad8") isMoveUpPress = false;
                    if (KeyName == "NumPad5") isMoveDownPress = false;
                    if (KeyName == "NumPad4") isMoveLeftPress = false;
                    if (KeyName == "NumPad6") isMoveRightPress = false;
                }
                else if (Regex.Match(KeyName, "([QWERS]$)|NumPad[123]").Success)
                {
                    if (KeyName == "R") isResetWindowsPress = false;
                    if (KeyName == "W") isAttackPress = false;
                    if (KeyName == "Q" || KeyName == "NumPad1") isSkill1Press = false;
                    if (KeyName == "E" || KeyName == "NumPad3") isSkill2Press = false;
                    if (KeyName == (cb_Move.Checked ? "S" : "W") || KeyName == "NumPad2") isSkill3Press = false;
                }
                else if (Regex.Match(KeyName, "(D[123]$)").Success)
                {
                    if (KeyName == "D1") isD1Press = false;
                    if (KeyName == "D2") isD2Press = false;
                    if (KeyName == "D3") isD3Press = false;
                }
                else if (KeyName == "Space") isSpacePress = false;
            }
        }

        private void OnMouseInput(object sender, EventHook.MouseEventArgs e)
        {
            if (GetForegroundWindow() != WindowsPtr) return;
            if (!e.Message.ToString().StartsWith("WM_LBUTTON")) return;
            if (e.Message.ToString().EndsWith("DOWN")) isMouseLeftButtonPress = true;
            else isMouseLeftButtonPress = false;
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            close = false;
            if (MessageBox.Show("請確定以下資訊是否正確，否則有可能會出問題，如果出現問題" +
                "\r\n先嘗試按按看鍵盤上排的數字3或搖桿的BACK鍵，都不能的話在嘗試強制關閉或重新開機" +
                "\r\n選擇的程式PID: " + processlist[listBox1.SelectedIndex].Id.ToString() +
                "\r\n選擇的視窗標題: " + processlist[listBox1.SelectedIndex].MainWindowTitle
                , "", MessageBoxButtons.YesNo) == DialogResult.No) return;

            WorkSwitch(false);
            work = true;
            richTextBox1.Clear();

            #region 宣告與視窗處理
            const int LOOPSLEEP = 10;
            const int MOVESLEEP = 15;
            int moveAftelSleep = 15;
            GamepadButtonFlags button;
            bool stop = false, isMove = false, isSkillLongPress = false, isSkillPress = false, isAttack = false, isRoll = false;
            WindowsPtr = processlist[listBox1.SelectedIndex].MainWindowHandle;
            SetForegroundWindow(WindowsPtr);
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
                        if (!xboxController.IsConnected) { WriteLine("搖桿已中斷連結"); break; }
                        if (GetForegroundWindow() != WindowsPtr) { Sleep(250); continue; } //如果前景視窗的控制代碼跟選擇的視窗代碼不一樣，就不執行判斷
                        Sleep(LOOPSLEEP);
                        state = xboxController.GetState();
                        button = state.Gamepad.Buttons;
                        if (button == GamepadButtonFlags.DPadUp)
                        {
                            if (!isRoll)
                            {
                                if (isMouseLeftButtonPress) Mouse.LeftButtonUp();
                                WriteLine("上滑");
                                int y = -5;
                                isRoll = true;
                                Mouse.LeftButtonDown();
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 10);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 15);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 20);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 25);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 30);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 35);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 40);
                                Sleep(moveAftelSleep);
                                Mouse.LeftButtonUp();
                                SetCursorPos(mid_x, mid_y);
                            }
                        }
                        else if (button == GamepadButtonFlags.DPadDown)
                        {
                            if (!isRoll)
                            {
                                int y = 5;
                                isRoll = true;
                                Mouse.LeftButtonDown();
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 10);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 15);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 20);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 25);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 30);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 35);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x, mid_y + y * 40);
                                Sleep(moveAftelSleep);
                                Mouse.LeftButtonUp();
                                SetCursorPos(mid_x, mid_y);
                            }
                        }
                        else if (button == GamepadButtonFlags.DPadLeft)
                        {
                            if (!isRoll)
                            {
                                int x = -5;
                                isRoll = true;
                                Mouse.LeftButtonDown();
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 10, mid_y);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 15, mid_y);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 20, mid_y);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 25, mid_y);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 30, mid_y);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 35, mid_y);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 40, mid_y);
                                Sleep(moveAftelSleep);
                                Mouse.LeftButtonUp();
                                SetCursorPos(mid_x, mid_y);
                            }
                        }
                        else if (button == GamepadButtonFlags.DPadRight)
                        {
                            if (!isRoll)
                            {
                                int x = 5;
                                isRoll = true;
                                Mouse.LeftButtonDown();
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 10, mid_y);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 15, mid_y);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 20, mid_y);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 25, mid_y);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 30, mid_y);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 35, mid_y);
                                Sleep(MOVESLEEP);
                                SetCursorPos(mid_x + x * 40, mid_y);
                                Sleep(moveAftelSleep);
                                Mouse.LeftButtonUp();
                                SetCursorPos(mid_x, mid_y);
                            }
                        }
                        else { isRoll = false; }
                        if (button == GamepadButtonFlags.Y)
                        {
                            if (!isResetWindows)
                            {
                                isResetWindows = true;
                                ResetWindowsRect();
                            }
                        }
                        else { isResetWindows = false; }
                        if (button.ToString() == "A" || button.ToString() == "LeftShoulder, A" || button.ToString() == "RightShoulder, A")
                        {
                            if (!isAttack)
                            {
                                Mouse.LeftButtonDown();
                                isAttack = true;
                                WriteLine("攻擊");
                            }
                            else if (button.ToString() == "LeftShoulder, A")
                            {
                                if (!isSkillLongPress)
                                {
                                    isSkillLongPress = true;
                                    WriteLine("(集氣)技能1");
                                    SetCursorPos(mid_x, mid_y);
                                    SetCursorPos(sk1_x, sk1_y);
                                    Mouse.LeftButtonUp();
                                    Sleep(SKILLSLEEPTIME);
                                    SetCursorPos(mid_x, mid_y);
                                }
                            }
                            else if (button.ToString() == "RightShoulder, A")
                            {
                                if (!isSkillLongPress)
                                {
                                    isSkillLongPress = true;
                                    WriteLine("(集氣)技能2");
                                    SetCursorPos(mid_x, mid_y);
                                    SetCursorPos(sk2_x, sk2_y);
                                    Mouse.LeftButtonUp();
                                    Sleep(SKILLSLEEPTIME);
                                    SetCursorPos(mid_x, mid_y);
                                }
                            }
                            else if (xboxController.GetState().Gamepad.RightTrigger >= 128 || xboxController.GetState().Gamepad.LeftTrigger >= 128)
                            {
                                if (!isSkillLongPress)
                                {
                                    isSkillLongPress = true;
                                    WriteLine("(集氣)武器技能");
                                    SetCursorPos(mid_x, mid_y);
                                    SetCursorPos(sk3_x, sk3_y);
                                    Mouse.LeftButtonUp();
                                    Sleep(SKILLSLEEPTIME);
                                    SetCursorPos(mid_x, mid_y);
                                }
                            }
                        }
                        else { isAttack = false; isSkillLongPress = false; }
                        if (button == GamepadButtonFlags.LeftShoulder)
                        {
                            if (!isSkillPress && !isSkillLongPress)
                            {
                                isSkillPress = true;
                                WriteLine("技能1");
                                SetCursorPos(mid_x, mid_y);
                                Mouse.LeftButtonDown();
                                Sleep(SKILLSLEEPTIME);
                                SetCursorPos(sk1_x, sk1_y);
                                Mouse.LeftButtonUp();
                                SetCursorPos(mid_x, mid_y);
                            }
                        }
                        else if (button == GamepadButtonFlags.RightShoulder)
                        {
                            if (!isSkillPress && !isSkillLongPress)
                            {
                                isSkillPress = true;
                                WriteLine("技能2");
                                SetCursorPos(mid_x, mid_y);
                                Mouse.LeftButtonDown();
                                Sleep(SKILLSLEEPTIME);
                                SetCursorPos(sk2_x, sk2_y);
                                Mouse.LeftButtonUp();
                                SetCursorPos(mid_x, mid_y);
                            }
                        }
                        else if (xboxController.GetState().Gamepad.RightTrigger >= 128 || xboxController.GetState().Gamepad.LeftTrigger >= 128)
                        {
                            if (!isSkillPress && !isSkillLongPress)
                            {
                                isSkillPress = true;
                                WriteLine("武器技能");
                                SetCursorPos(mid_x, mid_y);
                                Mouse.LeftButtonDown();
                                Sleep(SKILLSLEEPTIME);
                                SetCursorPos(sk3_x, sk3_y);
                                Mouse.LeftButtonUp();
                                SetCursorPos(mid_x, mid_y);
                            }
                        }
                        else isSkillPress = false;
                        if ((state.Gamepad.LeftThumbX <= contLThumX - 1000 || state.Gamepad.LeftThumbX >= contLThumX + 1000) || state.Gamepad.LeftThumbY <= contLThumY - 1000 || state.Gamepad.LeftThumbY >= contLThumY + 1000)
                        {
                            if (!isMove) { SetCursorPos(mid_x, mid_y); Sleep(50); Mouse.LeftButtonDown(); isMove = true; }
                            Sleep(5);
                            if (!isAttack) SetCursorPos(mid_x - (contLThumX - state.Gamepad.LeftThumbX) / 5000 * 25, mid_y + (contLThumY - state.Gamepad.LeftThumbY) / 5000 * 20);
                            else SetCursorPos(mid_x - (contLThumX - state.Gamepad.LeftThumbX) / 5000 * 15, mid_y + (contLThumY - state.Gamepad.LeftThumbY) / 5000 * 10);
                        }
                        else if (isMove) { isMove = false; }
                        else { SetCursorPos(mid_x, mid_y); if (!isAttack) Mouse.LeftButtonUp(); }
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
                        if (GetForegroundWindow() != WindowsPtr) { Sleep(500); continue; }//如果前景視窗的控制代碼跟選擇的視窗代碼不一樣，就不執行判斷
                        Sleep(LOOPSLEEP); //休息
                        if (cb_Move.Checked)
                        {
                            if (!isSpacePress && IsMoveKeyPress())
                            {
                                if (!isMove) { SetCursorPos(mid_x, mid_y); Sleep(50); Mouse.LeftButtonDown(); isMove = true; WriteLine("已按下"); }
                                if (isMoveLeftPress && isMoveUpPress) { SetCursorPos((isAttack ? left_x + 50 : left_x), (isAttack ? up_y - 50 : up_y)); WriteLine("左上"); }
                                else if (isMoveRightPress && isMoveUpPress) { SetCursorPos((isAttack ? right_x - 50 : right_x), (isAttack ? up_y - 50 : up_y)); WriteLine("右上"); }
                                else if (isMoveLeftPress && isMoveDownPress) { SetCursorPos(left_x, down_y); WriteLine("左下"); }
                                else if (isMoveRightPress && isMoveDownPress) { SetCursorPos(right_x, down_y); WriteLine("右下"); }
                                else if (isMoveUpPress) { SetCursorPos(up_x, up_y); WriteLine("上"); }
                                else if (isMoveDownPress) { SetCursorPos(down_x, down_y); WriteLine("下"); }
                                else if (isMoveLeftPress) { SetCursorPos(left_x, left_y); WriteLine("左"); }
                                else if (isMoveRightPress) { SetCursorPos(right_x, right_y); WriteLine("右"); }
                            }
                            else if (isMove) { isMove = false; }
                            else if (isSpacePress)
                            {
                                if (isMoveUpPress)
                                {
                                    if (!isRoll)
                                    {
                                        if (isMouseLeftButtonPress) Mouse.LeftButtonUp();
                                        WriteLine("上滑");
                                        int y = -5;
                                        isRoll = true;
                                        Mouse.LeftButtonDown();
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 10);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 15);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 20);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 25);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 30);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 35);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 40);
                                        Sleep(moveAftelSleep);
                                        Mouse.LeftButtonUp();
                                        SetCursorPos(mid_x, mid_y);
                                    }
                                }
                                else if (isMoveDownPress)
                                {
                                    if (!isRoll)
                                    {
                                        if (isMouseLeftButtonPress) Mouse.LeftButtonUp();
                                        WriteLine("下滑");
                                        int y = 5;
                                        isRoll = true;
                                        Mouse.LeftButtonDown();
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 10);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 15);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 20);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 25);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 30);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 35);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x, mid_y + y * 40);
                                        Sleep(moveAftelSleep);
                                        Mouse.LeftButtonUp();
                                        SetCursorPos(mid_x, mid_y);
                                    }
                                }
                                else if (isMoveLeftPress)
                                {
                                    if (!isRoll)
                                    {
                                        if (isMouseLeftButtonPress) Mouse.LeftButtonUp();
                                        WriteLine("左滑");
                                        int x = -5;
                                        isRoll = true;
                                        Mouse.LeftButtonDown();
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 10, mid_y);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 15, mid_y);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 20, mid_y);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 25, mid_y);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 30, mid_y);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 35, mid_y);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 40, mid_y);
                                        Sleep(moveAftelSleep);
                                        Mouse.LeftButtonUp();
                                        SetCursorPos(mid_x, mid_y);
                                    }
                                }
                                else if (isMoveRightPress)
                                {
                                    if (!isRoll)
                                    {
                                        if (isMouseLeftButtonPress) Mouse.LeftButtonUp();
                                        WriteLine("右滑");
                                        int x = 5;
                                        isRoll = true;
                                        Mouse.LeftButtonDown();
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 10, mid_y);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 15, mid_y);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 20, mid_y);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 25, mid_y);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 30, mid_y);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 35, mid_y);
                                        Sleep(MOVESLEEP);
                                        SetCursorPos(mid_x + x * 40, mid_y);
                                        Sleep(moveAftelSleep);
                                        Mouse.LeftButtonUp();
                                        SetCursorPos(mid_x, mid_y);
                                    }
                                }
                                else { isRoll = false; }
                            }
                            else if (isRoll) { isRoll = false; }
                            else { SetCursorPos(mid_x, mid_y); if (!isAttackPress && isMouseLeftButtonPress) Mouse.LeftButtonUp(); }
                        }
                        if (isAttackPress)
                        {
                            if (!isAttack)
                            {
                                Mouse.LeftButtonDown();
                                isAttack = true;
                                WriteLine("攻擊");
                            }
                        }
                        else { isAttack = false; }
                        if (isSkill1Press)
                        {
                            if (!isSkillPress)
                            {
                                isSkillPress = true;
                                Point old_point = Cursor.Position;
                                if (isAttack) WriteLine("(集氣)技能1");
                                else
                                {
                                    if (isMouseLeftButtonPress) Mouse.LeftButtonUp();
                                    WriteLine("技能1");
                                    SetCursorPos(mid_x, mid_y);
                                    Mouse.LeftButtonDown();
                                    Sleep(SKILLSLEEPTIME);
                                }
                                SetCursorPos(sk1_x, sk1_y);
                                Mouse.LeftButtonUp();
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
                                    if (isMouseLeftButtonPress) Mouse.LeftButtonUp();
                                    WriteLine("技能2");
                                    SetCursorPos(mid_x, mid_y);
                                    Mouse.LeftButtonDown();
                                    Sleep(SKILLSLEEPTIME);
                                }
                                SetCursorPos(sk2_x, sk2_y);
                                Mouse.LeftButtonUp();
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
                                    if (isMouseLeftButtonPress) Mouse.LeftButtonUp();
                                    WriteLine("武器技能");
                                    SetCursorPos(mid_x, mid_y);
                                    Mouse.LeftButtonDown();
                                    Sleep(SKILLSLEEPTIME);
                                }
                                SetCursorPos(sk3_x, sk3_y);
                                Mouse.LeftButtonUp();
                                if (!cb_Move.Checked) SetCursorPos(old_point.X, old_point.Y);
                            }
                        }
                        else { isSkillPress = false; }
                        if (isResetWindowsPress) { if (!isResetWindows) { isResetWindows = true; ResetWindowsRect(); } }
                        else isResetWindows = false;
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
                        if (isD3Press) stop = true;
                    }
                    KeyboardWatcher.Stop();
                    MouseWatcher.Stop();
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
            lab_PID.Text = "選擇的程式PID: " + processlist[listBox1.SelectedIndex].Id.ToString();
            lab_PTitle.Text = "選擇的視窗標題: " + processlist[listBox1.SelectedIndex].MainWindowTitle;
            btn_Start.Enabled = !work;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (IsDisposed) return;
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();
        }

        private void WriteLine(string Text)
        {
            if (IsDisposed) return;
            Text += "\r\n";
            if (InvokeRequired) BeginInvoke(new Action(delegate { richTextBox1.AppendText("[" + DateTime.Now.ToString("hh-mm-ss") + "] " + Text); }));
            else richTextBox1.AppendText("[" + DateTime.Now.ToString("hh-mm-ss") + "] " + Text);
        }

        private void SetCursorPos(int x, int y)
        {
            Cursor.Position = new System.Drawing.Point(x, y);
        }
        
        private void WorkSwitch(bool enable)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(new Action(delegate
            {
                btn_Start.Enabled = enable;
                btn_Reset.Enabled = enable;
                cb_ControllerPlay.Enabled = xboxController.IsConnected && enable;
                cb_Move.Enabled = enable;
            }));
            else
            {
                btn_Start.Enabled = enable;
                btn_Reset.Enabled = enable;
                cb_ControllerPlay.Enabled = xboxController.IsConnected && enable;
                cb_Move.Enabled = enable;
            }
        }

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

        private void ResetWindowsRect()
        {
            GetWindowRect(WindowsPtr, out RECT rect);
            mid_x = (rect.Left + rect.Right) / 2;
            mid_y = (rect.Top + rect.Bottom) / 2;
            up_x = mid_x;
            up_y = mid_y - 100;
            down_x = mid_x;
            down_y = mid_y + 100;
            left_x = mid_x - 100;
            left_y = mid_y;
            right_x = mid_x + 100;
            right_y = mid_y;
            sk1_x = mid_x - 100;
            sk1_y = mid_y - 110;
            sk2_x = mid_x + 100;
            sk2_y = mid_y - 110;
            sk3_x = mid_x;
            sk3_y = mid_y + 180;
            WriteLine("已重設視窗位置");
            WriteLine("上: " + rect.Top + " 下: " + rect.Bottom);
            WriteLine("左: " + rect.Left + " 右: " + rect.Right);
        }
        
        private bool IsMoveKeyPress()
        {
            return isMoveUpPress || isMoveDownPress || isMoveLeftPress || isMoveRightPress;
        }
    }
}