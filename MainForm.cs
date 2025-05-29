using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using Yolov8Net;
using Yolov8Net.Inference;

namespace LunarAimbot
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        private bool isRunning = false;
        private readonly YoloModel model;
        private Thread aimThread;
        private readonly int screenWidth;
        private readonly int screenHeight;
        private readonly int screenCenterX;
        private readonly int screenCenterY;
        private float aimHeight = 10f;
        private float confidence = 0.45f;
        private bool useTriggerBot = true;
        private float xyScale = 1.0f;
        private float targetingScale = 1.0f;

        public MainForm()
        {
            InitializeComponent();
            
            screenWidth = GetSystemMetrics(SM_CXSCREEN);
            screenHeight = GetSystemMetrics(SM_CYSCREEN);
            screenCenterX = screenWidth / 2;
            screenCenterY = screenHeight / 2;

            model = new YoloModel("models/best.onnx");
            KeyboardHook.KeyPressed += OnKeyPressed;
            
            SetupUI();
            LoadConfig();
        }

        private void SetupUI()
        {
            Text = "Lunar Aimbot";
            Size = new Size(400, 300);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Label statusLabel = new Label
            {
                Text = "Status: DISABLED",
                Location = new Point(10, 10),
                AutoSize = true,
                ForeColor = Color.Red
            };

            Controls.Add(statusLabel);
        }

        private void LoadConfig()
        {
            var config = Config.Load();
            xyScale = config.XYScale;
            targetingScale = config.TargetingScale;
        }

        private void OnKeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Key == Keys.F1)
            {
                ToggleAimbot();
            }
            else if (e.Key == Keys.F2)
            {
                Application.Exit();
            }
        }

        private void ToggleAimbot()
        {
            isRunning = !isRunning;
            if (isRunning)
            {
                aimThread = new Thread(AimbotLoop)
                {
                    IsBackground = true
                };
                aimThread.Start();
            }
        }

        private void AimbotLoop()
        {
            while (isRunning)
            {
                try
                {
                    using (var screenshot = CaptureScreen())
                    {
                        var predictions = model.Predict(screenshot);
                        
                        foreach (var prediction in predictions)
                        {
                            if (prediction.Confidence < confidence)
                                continue;

                            var targetX = prediction.Box.Center.X;
                            var targetY = prediction.Box.Center.Y - (prediction.Box.Height / aimHeight);

                            if (IsTargetValid(targetX, targetY))
                            {
                                MoveCrosshair(targetX, targetY);

                                if (useTriggerBot && IsTargetLocked(targetX, targetY))
                                {
                                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in aimbot loop: {ex.Message}");
                    isRunning = false;
                    break;
                }

                Thread.Sleep(1);
            }
        }

        private Bitmap CaptureScreen()
        {
            var bounds = Screen.PrimaryScreen.Bounds;
            var bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            }
            return bitmap;
        }

        private bool IsTargetValid(float x, float y)
        {
            return x >= 0 && x < screenWidth && y >= 0 && y < screenHeight;
        }

        private void MoveCrosshair(float targetX, float targetY)
        {
            GetCursorPos(out POINT currentPos);
            
            float dx = (targetX - currentPos.X) * xyScale;
            float dy = (targetY - currentPos.Y) * targetingScale;

            mouse_event(MOUSEEVENTF_MOVE, (int)dx, (int)dy, 0, 0);
        }

        private bool IsTargetLocked(float x, float y)
        {
            GetCursorPos(out POINT currentPos);
            var distance = Math.Sqrt(Math.Pow(x - currentPos.X, 2) + Math.Pow(y - currentPos.Y, 2));
            return distance < 5;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isRunning = false;
            if (aimThread?.IsAlive == true)
            {
                aimThread.Join(1000);
            }
            KeyboardHook.Dispose();
            base.OnFormClosing(e);
        }
    }
}