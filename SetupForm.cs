using System;
using System.Windows.Forms;

namespace LunarAimbot
{
    public partial class SetupForm : Form
    {
        private NumericUpDown xySensitivity;
        private NumericUpDown targetingSensitivity;
        private Button saveButton;

        public SetupForm()
        {
            InitializeComponents();
            LoadCurrentConfig();
        }

        private void InitializeComponents()
        {
            this.Size = new System.Drawing.Size(400, 200);
            this.Text = "Lunar Aimbot Setup";
            
            var xyLabel = new Label
            {
                Text = "X-Axis and Y-Axis Sensitivity:",
                Location = new System.Drawing.Point(10, 20),
                Size = new System.Drawing.Size(200, 20)
            };

            xySensitivity = new NumericUpDown
            {
                Location = new System.Drawing.Point(220, 20),
                Size = new System.Drawing.Size(100, 20),
                DecimalPlaces = 1,
                Minimum = 0.1m,
                Maximum = 100m,
                Value = 6.9m
            };

            var targetLabel = new Label
            {
                Text = "Targeting Sensitivity:",
                Location = new System.Drawing.Point(10, 60),
                Size = new System.Drawing.Size(200, 20)
            };

            targetingSensitivity = new NumericUpDown
            {
                Location = new System.Drawing.Point(220, 60),
                Size = new System.Drawing.Size(100, 20),
                DecimalPlaces = 1,
                Minimum = 0.1m,
                Maximum = 100m,
                Value = 6.9m
            };

            saveButton = new Button
            {
                Text = "Save Configuration",
                Location = new System.Drawing.Point(120, 100),
                Size = new System.Drawing.Size(150, 30)
            };
            saveButton.Click += SaveButton_Click;

            this.Controls.AddRange(new Control[] { 
                xyLabel, xySensitivity, 
                targetLabel, targetingSensitivity,
                saveButton 
            });
        }

        private void LoadCurrentConfig()
        {
            var config = Config.Load();
            xySensitivity.Value = (decimal)config.XYSensitivity;
            targetingSensitivity.Value = (decimal)config.TargetingSensitivity;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            var config = new Config
            {
                XYSensitivity = (float)xySensitivity.Value,
                TargetingSensitivity = (float)targetingSensitivity.Value,
                XYScale = 10f/(float)xySensitivity.Value,
                TargetingScale = 1000f/((float)targetingSensitivity.Value * (float)xySensitivity.Value)
            };
            
            config.Save();
            MessageBox.Show("Configuration saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}