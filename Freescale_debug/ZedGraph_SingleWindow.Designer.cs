using System.ComponentModel;
using System.Windows.Forms;
using ZedGraph;
using Label = System.Windows.Forms.Label;

namespace Freescale_debug
{
    partial class ZedGraphSingleWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZedGraphSingleWindow));
            this.zedGraph_Single = new ZedGraph.ZedGraphControl();
            this.button_ClearData = new System.Windows.Forms.Button();
            this.timer_fresh = new System.Windows.Forms.Timer(this.components);
            this.button_ShowRange = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_pause = new System.Windows.Forms.Button();
            this.buttonSetting = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // zedGraph_Single
            // 
            this.zedGraph_Single.IsShowHScrollBar = true;
            this.zedGraph_Single.IsShowVScrollBar = true;
            this.zedGraph_Single.Location = new System.Drawing.Point(0, 0);
            this.zedGraph_Single.Name = "zedGraph_Single";
            this.zedGraph_Single.ScrollGrace = 0D;
            this.zedGraph_Single.ScrollMaxX = 0D;
            this.zedGraph_Single.ScrollMaxY = 0D;
            this.zedGraph_Single.ScrollMaxY2 = 0D;
            this.zedGraph_Single.ScrollMinX = 0D;
            this.zedGraph_Single.ScrollMinY = 0D;
            this.zedGraph_Single.ScrollMinY2 = 0D;
            this.zedGraph_Single.Size = new System.Drawing.Size(522, 403);
            this.zedGraph_Single.TabIndex = 0;
            // 
            // button_ClearData
            // 
            this.button_ClearData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_ClearData.Location = new System.Drawing.Point(560, 173);
            this.button_ClearData.Name = "button_ClearData";
            this.button_ClearData.Size = new System.Drawing.Size(63, 23);
            this.button_ClearData.TabIndex = 1;
            this.button_ClearData.Text = "清除数据";
            this.button_ClearData.UseVisualStyleBackColor = true;
            this.button_ClearData.Click += new System.EventHandler(this.button_ClearData_Click);
            // 
            // timer_fresh
            // 
            this.timer_fresh.Interval = 50;
            this.timer_fresh.Tick += new System.EventHandler(this.timer_fresh_Tick);
            // 
            // button_ShowRange
            // 
            this.button_ShowRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_ShowRange.Location = new System.Drawing.Point(560, 92);
            this.button_ShowRange.Name = "button_ShowRange";
            this.button_ShowRange.Size = new System.Drawing.Size(63, 23);
            this.button_ShowRange.TabIndex = 2;
            this.button_ShowRange.Text = "确认";
            this.button_ShowRange.UseVisualStyleBackColor = true;
            this.button_ShowRange.Click += new System.EventHandler(this.button_ShowRange_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.textBox1.Location = new System.Drawing.Point(528, 44);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(95, 21);
            this.textBox1.TabIndex = 3;
            this.textBox1.Text = "请输入X轴宽度";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox1.Enter += new System.EventHandler(this.textBox1_Enter);
            this.textBox1.Leave += new System.EventHandler(this.textBox1_Leave);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(528, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "0表示无宽度限制";
            // 
            // button_pause
            // 
            this.button_pause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_pause.Location = new System.Drawing.Point(560, 133);
            this.button_pause.Name = "button_pause";
            this.button_pause.Size = new System.Drawing.Size(63, 23);
            this.button_pause.TabIndex = 5;
            this.button_pause.Text = "暂停";
            this.button_pause.UseVisualStyleBackColor = true;
            this.button_pause.Click += new System.EventHandler(this.button_pause_Click);
            // 
            // buttonSetting
            // 
            this.buttonSetting.Location = new System.Drawing.Point(547, 358);
            this.buttonSetting.Name = "buttonSetting";
            this.buttonSetting.Size = new System.Drawing.Size(76, 23);
            this.buttonSetting.TabIndex = 6;
            this.buttonSetting.Text = "设置（无）";
            this.buttonSetting.UseVisualStyleBackColor = true;
            this.buttonSetting.Click += new System.EventHandler(this.buttonSetting_Click);
            // 
            // ZedGraphSingleWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(635, 403);
            this.Controls.Add(this.buttonSetting);
            this.Controls.Add(this.button_pause);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button_ShowRange);
            this.Controls.Add(this.button_ClearData);
            this.Controls.Add(this.zedGraph_Single);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ZedGraphSingleWindow";
            this.Text = "Graph";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ZedGraph_SingleWindow_FormClosed);
            this.Load += new System.EventHandler(this.ZedGraph_SingleWindow_Load);
            this.Resize += new System.EventHandler(this.ZedGraph_SingleWindow_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ZedGraphControl zedGraph_Single;
        private Button button_ClearData;
        private Timer timer_fresh;
        private Button button_ShowRange;
        private TextBox textBox1;
        private Label label1;
        private Button button_pause;
        private Button buttonSetting;
    }
}