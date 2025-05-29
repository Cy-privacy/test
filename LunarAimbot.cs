using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using System.Drawing.Imaging;

namespace LunarAimbot
{
    public partial class MainForm : Form
    {
        // Win32 API imports for mouse control
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;

        private bool isRunning = false;
        private float sensitivity = 1.0f;
        private float aimHeight = 10f;
        private float confidence = 0.45f;
        private bool useTriggerBot = true;

        private InferenceSession yoloSession;
        private Thread aimThread;

        public MainForm()
        {
            InitializeComponent();
            LoadYoloModel();
            SetupControls();
        }

        private void LoadYoloModel()
        {
            try
            {
                // Load embedded YOLO model
                yoloSession = new InferenceSession("models/best.onnx");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading YOLO model: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void SetupControls()
        {
            // Create basic UI controls
            var startButton = new Button
            {
                Text = "Start",
                Location = new Point(10, 10),
                Size = new Size(100, 30)
            };
            startButton.Click += StartButton_Click;

            var sensitivityLabel = new Label
            {
                Text = "Sensitivity:",
                Location = new Point(10, 50),
                Size = new Size(100, 20)
            };

            var sensitivityTrackbar = new TrackBar
            {
                Location = new Point(110, 50),
                Size = new Size(200, 45),
                Minimum = 1,
                Maximum = 100,
                Value = (int)(sensitivity * 10)
            };
            sensitivityTrackbar.ValueChanged += (s, e) => sensitivity = sensitivityTrackbar.Value / 10f;

            Controls.AddRange(new Control[] { startButton, sensitivityLabel, sensitivityTrackbar });
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (!isRunning)
            {
                isRunning = true;
                aimThread = new Thread(AimbotLoop)
                {
                    IsBackground = true
                };
                aimThread.Start();
                ((Button)sender).Text = "Stop";
            }
            else
            {
                isRunning = false;
                ((Button)sender).Text = "Start";
            }
        }

        private void AimbotLoop()
        {
            while (isRunning)
            {
                try
                {
                    // Capture screen
                    using (var bitmap = CaptureScreen())
                    {
                        // Process image with YOLO
                        var detections = ProcessImage(bitmap);

                        // Find closest target
                        var target = FindClosestTarget(detections);

                        if (target != null)
                        {
                            // Move mouse to target
                            MoveMouseToTarget(target.Value);

                            // Trigger bot
                            if (useTriggerBot && IsTargetLocked(target.Value))
                            {
                                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                            }
                        }
                    }

                    Thread.Sleep(1); // Prevent excessive CPU usage
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in aimbot loop: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isRunning = false;
                    break;
                }
            }
        }

        private Bitmap CaptureScreen()
        {
            var screenBounds = Screen.PrimaryScreen.Bounds;
            var bitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(Point.Empty, Point.Empty, screenBounds.Size);
            }
            return bitmap;
        }

        private Rectangle[] ProcessImage(Bitmap bitmap)
        {
            // Convert bitmap to tensor and run inference
            // This is a simplified version - you'll need to implement proper
            // image preprocessing and YOLO post-processing
            return new Rectangle[0]; // Placeholder
        }

        private Point? FindClosestTarget(Rectangle[] detections)
        {
            var screenCenter = new Point(Screen.PrimaryScreen.Bounds.Width / 2,
                                       Screen.PrimaryScreen.Bounds.Height / 2);
            
            Point? closest = null;
            float closestDistance = float.MaxValue;

            foreach (var detection in detections)
            {
                var targetPoint = new Point(detection.X + detection.Width / 2,
                                          detection.Y + detection.Height / 2);
                
                var distance = Distance(screenCenter, targetPoint);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = targetPoint;
                }
            }

            return closest;
        }

        private float Distance(Point a, Point b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private void MoveMouseToTarget(Point target)
        {
            var screenCenter = new Point(Screen.PrimaryScreen.Bounds.Width / 2,
                                       Screen.PrimaryScreen.Bounds.Height / 2);
            
            var dx = (target.X - screenCenter.X) * sensitivity;
            var dy = (target.Y - screenCenter.Y) * sensitivity;

            mouse_event(MOUSEEVENTF_MOVE, (uint)dx, (uint)dy, 0, 0);
        }

        private bool IsTargetLocked(Point target)
        {
            var screenCenter = new Point(Screen.PrimaryScreen.Bounds.Width / 2,
                                       Screen.PrimaryScreen.Bounds.Height / 2);
            
            return Distance(screenCenter, target) < 5;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isRunning = false;
            if (aimThread?.IsAlive == true)
            {
                aimThread.Join(1000);
            }
            base.OnFormClosing(e);
        }
    }
}