using System;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using ZedGraphControl = ZedGraph.ZedGraphControl;

namespace Freescale_debug
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TabControl tabControl1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tabPage_Serial = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button_countclear = new System.Windows.Forms.Button();
            this.label_receiveCount = new System.Windows.Forms.Label();
            this.label_sendCount = new System.Windows.Forms.Label();
            this.groupBox_send = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_sendPeriod = new System.Windows.Forms.TextBox();
            this.checkBox_sendAuto = new System.Windows.Forms.CheckBox();
            this.button_sendmessage = new System.Windows.Forms.Button();
            this.textBox_send = new System.Windows.Forms.TextBox();
            this.groupBox_Receive = new System.Windows.Forms.GroupBox();
            this.checkBox_showRecvTime = new System.Windows.Forms.CheckBox();
            this.checkBox_receiveAnotherline = new System.Windows.Forms.CheckBox();
            this.checkBox_receiveHex = new System.Windows.Forms.CheckBox();
            this.button_receiveclear = new System.Windows.Forms.Button();
            this.button_receivepause = new System.Windows.Forms.Button();
            this.textBox_receive = new System.Windows.Forms.TextBox();
            this.tabPage_Scope = new System.Windows.Forms.TabPage();
            this.button_ClearDrawing = new System.Windows.Forms.Button();
            this.panel_Scope = new System.Windows.Forms.Panel();
            this.zedGraph_local = new ZedGraph.ZedGraphControl();
            this.tabPage_PIDSettings = new System.Windows.Forms.TabPage();
            this.label26 = new System.Windows.Forms.Label();
            this.groupBox_BalanceCar = new System.Windows.Forms.GroupBox();
            this.groupBox_Balance_Direction = new System.Windows.Forms.GroupBox();
            this.button_ModifyPID_Direction = new System.Windows.Forms.Button();
            this.textBox_Direction_D = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.textBox_Direction_I = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.textBox_Direction_P = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.groupBox_Balance_Speed = new System.Windows.Forms.GroupBox();
            this.button_ModifyPID_Speed = new System.Windows.Forms.Button();
            this.textBox_Speed_D = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.textBox_Speed_I = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.textBox_Speed_P = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.groupBox_Balance_Stand = new System.Windows.Forms.GroupBox();
            this.button_ModifyPID_Stand = new System.Windows.Forms.Button();
            this.textBox_Stand_D = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.textBox_Stand_I = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.textBox_Stand_P = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.groupBox_fourWheels = new System.Windows.Forms.GroupBox();
            this.groupBox_Steer = new System.Windows.Forms.GroupBox();
            this.button_ModifyPID_Steer = new System.Windows.Forms.Button();
            this.textBox_Steer_D = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_Steer_I = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_Steer_P = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox_Motor = new System.Windows.Forms.GroupBox();
            this.button_ModifyPID_Motor = new System.Windows.Forms.Button();
            this.textBox_Motor_D = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_Motor_I = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.textBox_Motor_P = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.groupBox_carType = new System.Windows.Forms.GroupBox();
            this.checkBox_IsUsePID = new System.Windows.Forms.CheckBox();
            this.radioButton_BalanceCar = new System.Windows.Forms.RadioButton();
            this.radioButton_FourWheel = new System.Windows.Forms.RadioButton();
            this.groupBox_sensorType = new System.Windows.Forms.GroupBox();
            this.radioButton_CCD = new System.Windows.Forms.RadioButton();
            this.radioButton_camera = new System.Windows.Forms.RadioButton();
            this.radioButton_electromagnetism = new System.Windows.Forms.RadioButton();
            this.tabPage_DIY_Algorithm = new System.Windows.Forms.TabPage();
            this.button_DIY_NumConfirm = new System.Windows.Forms.Button();
            this.label22 = new System.Windows.Forms.Label();
            this.textBox_DIY_Number = new System.Windows.Forms.TextBox();
            this.panel_add_DIYControls = new System.Windows.Forms.Panel();
            this.button_DIY_SendModify = new System.Windows.Forms.Button();
            this.tabPage_DAC_Value = new System.Windows.Forms.TabPage();
            this.button_electricity_NumConfirm = new System.Windows.Forms.Button();
            this.label23 = new System.Windows.Forms.Label();
            this.textBox_Electricity_Number = new System.Windows.Forms.TextBox();
            this.panel_Electricity = new System.Windows.Forms.Panel();
            this.tabPage_Camera = new System.Windows.Forms.TabPage();
            this.checkBox_Camera_ONOFF = new System.Windows.Forms.CheckBox();
            this.label25 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.pictureBox_CameraActual = new System.Windows.Forms.PictureBox();
            this.tabPage_CCD = new System.Windows.Forms.TabPage();
            this.label32 = new System.Windows.Forms.Label();
            this.pictureBox_CCD3 = new System.Windows.Forms.PictureBox();
            this.label31 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBox_CCD_ONOFF = new System.Windows.Forms.CheckBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label_CCD_Width = new System.Windows.Forms.Label();
            this.pictureBox_CCD_deal = new System.Windows.Forms.PictureBox();
            this.label27 = new System.Windows.Forms.Label();
            this.pictureBox_CCD_Path = new System.Windows.Forms.PictureBox();
            this.pictureBox_CCD_Actual = new System.Windows.Forms.PictureBox();
            this.label28 = new System.Windows.Forms.Label();
            this.mySerialPort = new System.IO.Ports.SerialPort(this.components);
            this.timer_autoSend = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox_port = new System.Windows.Forms.ComboBox();
            this.comboBox_baudrate = new System.Windows.Forms.ComboBox();
            this.comboBox_parity = new System.Windows.Forms.ComboBox();
            this.comboBox_databit = new System.Windows.Forms.ComboBox();
            this.comboBox_stopbit = new System.Windows.Forms.ComboBox();
            this.button_openPort = new System.Windows.Forms.Button();
            this.button_freshPort = new System.Windows.Forms.Button();
            this.groupBox_portSetting = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_force_stop = new System.Windows.Forms.Button();
            this.button_ExportMatlab = new System.Windows.Forms.Button();
            this.button_loadHistory = new System.Windows.Forms.Button();
            this.button_History = new System.Windows.Forms.Button();
            this.button_ReadAllSettings = new System.Windows.Forms.Button();
            this.saveFileDialog_History = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog_History = new System.Windows.Forms.OpenFileDialog();
            this.timer_Send2GetEcho = new System.Windows.Forms.Timer(this.components);
            this.timer_fresh = new System.Windows.Forms.Timer(this.components);
            this.label29 = new System.Windows.Forms.Label();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabControl1.SuspendLayout();
            this.tabPage_Serial.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox_send.SuspendLayout();
            this.groupBox_Receive.SuspendLayout();
            this.tabPage_Scope.SuspendLayout();
            this.tabPage_PIDSettings.SuspendLayout();
            this.groupBox_BalanceCar.SuspendLayout();
            this.groupBox_Balance_Direction.SuspendLayout();
            this.groupBox_Balance_Speed.SuspendLayout();
            this.groupBox_Balance_Stand.SuspendLayout();
            this.groupBox_fourWheels.SuspendLayout();
            this.groupBox_Steer.SuspendLayout();
            this.groupBox_Motor.SuspendLayout();
            this.groupBox_carType.SuspendLayout();
            this.groupBox_sensorType.SuspendLayout();
            this.tabPage_DIY_Algorithm.SuspendLayout();
            this.tabPage_DAC_Value.SuspendLayout();
            this.tabPage_Camera.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_CameraActual)).BeginInit();
            this.tabPage_CCD.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_CCD3)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_CCD_deal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_CCD_Path)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_CCD_Actual)).BeginInit();
            this.groupBox_portSetting.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            tabControl1.Controls.Add(this.tabPage_Serial);
            tabControl1.Controls.Add(this.tabPage_Scope);
            tabControl1.Controls.Add(this.tabPage_PIDSettings);
            tabControl1.Controls.Add(this.tabPage_DIY_Algorithm);
            tabControl1.Controls.Add(this.tabPage_DAC_Value);
            tabControl1.Controls.Add(this.tabPage_Camera);
            tabControl1.Controls.Add(this.tabPage_CCD);
            tabControl1.Location = new System.Drawing.Point(246, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(744, 565);
            tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            tabControl1.TabIndex = 0;
            // 
            // tabPage_Serial
            // 
            this.tabPage_Serial.Controls.Add(this.groupBox2);
            this.tabPage_Serial.Controls.Add(this.groupBox_send);
            this.tabPage_Serial.Controls.Add(this.groupBox_Receive);
            this.tabPage_Serial.Location = new System.Drawing.Point(4, 22);
            this.tabPage_Serial.Name = "tabPage_Serial";
            this.tabPage_Serial.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_Serial.Size = new System.Drawing.Size(736, 539);
            this.tabPage_Serial.TabIndex = 0;
            this.tabPage_Serial.Text = "串口配置";
            this.tabPage_Serial.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.button_countclear);
            this.groupBox2.Controls.Add(this.label_receiveCount);
            this.groupBox2.Controls.Add(this.label_sendCount);
            this.groupBox2.Location = new System.Drawing.Point(6, 487);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(589, 37);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "收发统计";
            // 
            // button_countclear
            // 
            this.button_countclear.Location = new System.Drawing.Point(470, 11);
            this.button_countclear.Name = "button_countclear";
            this.button_countclear.Size = new System.Drawing.Size(75, 23);
            this.button_countclear.TabIndex = 2;
            this.button_countclear.Text = "计数清零";
            this.button_countclear.UseVisualStyleBackColor = true;
            this.button_countclear.Click += new System.EventHandler(this.button_countclear_Click);
            // 
            // label_receiveCount
            // 
            this.label_receiveCount.AutoSize = true;
            this.label_receiveCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_receiveCount.Location = new System.Drawing.Point(227, 16);
            this.label_receiveCount.Name = "label_receiveCount";
            this.label_receiveCount.Size = new System.Drawing.Size(121, 14);
            this.label_receiveCount.TabIndex = 1;
            this.label_receiveCount.Text = "接受区计数：0      ";
            // 
            // label_sendCount
            // 
            this.label_sendCount.AutoSize = true;
            this.label_sendCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_sendCount.Location = new System.Drawing.Point(65, 16);
            this.label_sendCount.Name = "label_sendCount";
            this.label_sendCount.Size = new System.Drawing.Size(115, 14);
            this.label_sendCount.TabIndex = 0;
            this.label_sendCount.Text = "发送区计数：0     ";
            // 
            // groupBox_send
            // 
            this.groupBox_send.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_send.Controls.Add(this.label6);
            this.groupBox_send.Controls.Add(this.textBox_sendPeriod);
            this.groupBox_send.Controls.Add(this.checkBox_sendAuto);
            this.groupBox_send.Controls.Add(this.button_sendmessage);
            this.groupBox_send.Controls.Add(this.textBox_send);
            this.groupBox_send.Location = new System.Drawing.Point(6, 285);
            this.groupBox_send.Name = "groupBox_send";
            this.groupBox_send.Size = new System.Drawing.Size(588, 196);
            this.groupBox_send.TabIndex = 4;
            this.groupBox_send.TabStop = false;
            this.groupBox_send.Text = "串口发送区";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(359, 70);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(125, 12);
            this.label6.TabIndex = 6;
            this.label6.Text = "自动重发周期（ms）：";
            // 
            // textBox_sendPeriod
            // 
            this.textBox_sendPeriod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sendPeriod.Location = new System.Drawing.Point(482, 66);
            this.textBox_sendPeriod.Name = "textBox_sendPeriod";
            this.textBox_sendPeriod.Size = new System.Drawing.Size(49, 21);
            this.textBox_sendPeriod.TabIndex = 5;
            this.textBox_sendPeriod.Text = "500";
            // 
            // checkBox_sendAuto
            // 
            this.checkBox_sendAuto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_sendAuto.AutoSize = true;
            this.checkBox_sendAuto.Location = new System.Drawing.Point(361, 33);
            this.checkBox_sendAuto.Name = "checkBox_sendAuto";
            this.checkBox_sendAuto.Size = new System.Drawing.Size(96, 16);
            this.checkBox_sendAuto.TabIndex = 4;
            this.checkBox_sendAuto.Text = "自动循环发送";
            this.checkBox_sendAuto.UseVisualStyleBackColor = true;
            this.checkBox_sendAuto.CheckedChanged += new System.EventHandler(this.checkBox_sendAuto_CheckedChanged);
            // 
            // button_sendmessage
            // 
            this.button_sendmessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_sendmessage.Location = new System.Drawing.Point(361, 111);
            this.button_sendmessage.Name = "button_sendmessage";
            this.button_sendmessage.Size = new System.Drawing.Size(75, 23);
            this.button_sendmessage.TabIndex = 3;
            this.button_sendmessage.Text = "发送";
            this.button_sendmessage.UseVisualStyleBackColor = true;
            this.button_sendmessage.Click += new System.EventHandler(this.button_sendmessage_Click);
            // 
            // textBox_send
            // 
            this.textBox_send.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_send.Location = new System.Drawing.Point(12, 20);
            this.textBox_send.Multiline = true;
            this.textBox_send.Name = "textBox_send";
            this.textBox_send.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_send.Size = new System.Drawing.Size(313, 157);
            this.textBox_send.TabIndex = 2;
            // 
            // groupBox_Receive
            // 
            this.groupBox_Receive.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_Receive.Controls.Add(this.checkBox_showRecvTime);
            this.groupBox_Receive.Controls.Add(this.checkBox_receiveAnotherline);
            this.groupBox_Receive.Controls.Add(this.checkBox_receiveHex);
            this.groupBox_Receive.Controls.Add(this.button_receiveclear);
            this.groupBox_Receive.Controls.Add(this.button_receivepause);
            this.groupBox_Receive.Controls.Add(this.textBox_receive);
            this.groupBox_Receive.Location = new System.Drawing.Point(6, 6);
            this.groupBox_Receive.Name = "groupBox_Receive";
            this.groupBox_Receive.Size = new System.Drawing.Size(588, 263);
            this.groupBox_Receive.TabIndex = 3;
            this.groupBox_Receive.TabStop = false;
            this.groupBox_Receive.Text = "串口接收区";
            // 
            // checkBox_showRecvTime
            // 
            this.checkBox_showRecvTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_showRecvTime.AutoSize = true;
            this.checkBox_showRecvTime.Location = new System.Drawing.Point(361, 88);
            this.checkBox_showRecvTime.Name = "checkBox_showRecvTime";
            this.checkBox_showRecvTime.Size = new System.Drawing.Size(84, 16);
            this.checkBox_showRecvTime.TabIndex = 6;
            this.checkBox_showRecvTime.Text = "显示时间戳";
            this.checkBox_showRecvTime.UseVisualStyleBackColor = true;
            // 
            // checkBox_receiveAnotherline
            // 
            this.checkBox_receiveAnotherline.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_receiveAnotherline.AutoSize = true;
            this.checkBox_receiveAnotherline.Location = new System.Drawing.Point(361, 57);
            this.checkBox_receiveAnotherline.Name = "checkBox_receiveAnotherline";
            this.checkBox_receiveAnotherline.Size = new System.Drawing.Size(96, 16);
            this.checkBox_receiveAnotherline.TabIndex = 5;
            this.checkBox_receiveAnotherline.Text = "自动换行显示";
            this.checkBox_receiveAnotherline.UseVisualStyleBackColor = true;
            // 
            // checkBox_receiveHex
            // 
            this.checkBox_receiveHex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_receiveHex.AutoSize = true;
            this.checkBox_receiveHex.Location = new System.Drawing.Point(361, 28);
            this.checkBox_receiveHex.Name = "checkBox_receiveHex";
            this.checkBox_receiveHex.Size = new System.Drawing.Size(96, 16);
            this.checkBox_receiveHex.TabIndex = 4;
            this.checkBox_receiveHex.Text = "十六进制显示";
            this.checkBox_receiveHex.UseVisualStyleBackColor = true;
            this.checkBox_receiveHex.CheckedChanged += new System.EventHandler(this.checkBox_receiveHex_CheckedChanged);
            // 
            // button_receiveclear
            // 
            this.button_receiveclear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_receiveclear.Location = new System.Drawing.Point(493, 187);
            this.button_receiveclear.Name = "button_receiveclear";
            this.button_receiveclear.Size = new System.Drawing.Size(75, 23);
            this.button_receiveclear.TabIndex = 3;
            this.button_receiveclear.Text = "清空接受区";
            this.button_receiveclear.UseVisualStyleBackColor = true;
            this.button_receiveclear.Click += new System.EventHandler(this.button_receiveclear_Click);
            // 
            // button_receivepause
            // 
            this.button_receivepause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_receivepause.Location = new System.Drawing.Point(482, 148);
            this.button_receivepause.Name = "button_receivepause";
            this.button_receivepause.Size = new System.Drawing.Size(86, 23);
            this.button_receivepause.TabIndex = 2;
            this.button_receivepause.Text = "暂停接收显示";
            this.button_receivepause.UseVisualStyleBackColor = true;
            this.button_receivepause.Click += new System.EventHandler(this.button_receivepause_Click);
            // 
            // textBox_receive
            // 
            this.textBox_receive.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_receive.Location = new System.Drawing.Point(12, 14);
            this.textBox_receive.Multiline = true;
            this.textBox_receive.Name = "textBox_receive";
            this.textBox_receive.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_receive.Size = new System.Drawing.Size(313, 236);
            this.textBox_receive.TabIndex = 1;
            this.textBox_receive.TextChanged += new System.EventHandler(this.textBox_receive_TextChanged);
            // 
            // tabPage_Scope
            // 
            this.tabPage_Scope.Controls.Add(this.button_ClearDrawing);
            this.tabPage_Scope.Controls.Add(this.panel_Scope);
            this.tabPage_Scope.Controls.Add(this.zedGraph_local);
            this.tabPage_Scope.Location = new System.Drawing.Point(4, 22);
            this.tabPage_Scope.Name = "tabPage_Scope";
            this.tabPage_Scope.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_Scope.Size = new System.Drawing.Size(736, 539);
            this.tabPage_Scope.TabIndex = 4;
            this.tabPage_Scope.Text = "虚拟示波器";
            this.tabPage_Scope.UseVisualStyleBackColor = true;
            // 
            // button_ClearDrawing
            // 
            this.button_ClearDrawing.Location = new System.Drawing.Point(6, 89);
            this.button_ClearDrawing.Name = "button_ClearDrawing";
            this.button_ClearDrawing.Size = new System.Drawing.Size(75, 23);
            this.button_ClearDrawing.TabIndex = 0;
            this.button_ClearDrawing.Text = "清除图像";
            this.button_ClearDrawing.UseVisualStyleBackColor = true;
            this.button_ClearDrawing.Click += new System.EventHandler(this.button_ClearDrawing_Click);
            // 
            // panel_Scope
            // 
            this.panel_Scope.Location = new System.Drawing.Point(7, 7);
            this.panel_Scope.Name = "panel_Scope";
            this.panel_Scope.Size = new System.Drawing.Size(678, 76);
            this.panel_Scope.TabIndex = 1;
            // 
            // zedGraph_local
            // 
            this.zedGraph_local.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.zedGraph_local.IsShowHScrollBar = true;
            this.zedGraph_local.IsShowPointValues = true;
            this.zedGraph_local.IsShowVScrollBar = true;
            this.zedGraph_local.Location = new System.Drawing.Point(6, 119);
            this.zedGraph_local.Name = "zedGraph_local";
            this.zedGraph_local.ScrollGrace = 0D;
            this.zedGraph_local.ScrollMaxX = 0D;
            this.zedGraph_local.ScrollMaxY = 0D;
            this.zedGraph_local.ScrollMaxY2 = 0D;
            this.zedGraph_local.ScrollMinX = 0D;
            this.zedGraph_local.ScrollMinY = 0D;
            this.zedGraph_local.ScrollMinY2 = 0D;
            this.zedGraph_local.Size = new System.Drawing.Size(679, 393);
            this.zedGraph_local.TabIndex = 0;
            // 
            // tabPage_PIDSettings
            // 
            this.tabPage_PIDSettings.Controls.Add(this.label26);
            this.tabPage_PIDSettings.Controls.Add(this.groupBox_BalanceCar);
            this.tabPage_PIDSettings.Controls.Add(this.groupBox_fourWheels);
            this.tabPage_PIDSettings.Controls.Add(this.groupBox_carType);
            this.tabPage_PIDSettings.Controls.Add(this.groupBox_sensorType);
            this.tabPage_PIDSettings.Location = new System.Drawing.Point(4, 22);
            this.tabPage_PIDSettings.Name = "tabPage_PIDSettings";
            this.tabPage_PIDSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_PIDSettings.Size = new System.Drawing.Size(736, 539);
            this.tabPage_PIDSettings.TabIndex = 1;
            this.tabPage_PIDSettings.Text = "PID参数设置";
            this.tabPage_PIDSettings.UseVisualStyleBackColor = true;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(487, 46);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(143, 12);
            this.label26.TabIndex = 12;
            this.label26.Text = "所有精确到小数点后3位！";
            // 
            // groupBox_BalanceCar
            // 
            this.groupBox_BalanceCar.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.groupBox_BalanceCar.AutoSize = true;
            this.groupBox_BalanceCar.Controls.Add(this.groupBox_Balance_Direction);
            this.groupBox_BalanceCar.Controls.Add(this.groupBox_Balance_Speed);
            this.groupBox_BalanceCar.Controls.Add(this.groupBox_Balance_Stand);
            this.groupBox_BalanceCar.Location = new System.Drawing.Point(33, 197);
            this.groupBox_BalanceCar.Name = "groupBox_BalanceCar";
            this.groupBox_BalanceCar.Size = new System.Drawing.Size(563, 227);
            this.groupBox_BalanceCar.TabIndex = 8;
            this.groupBox_BalanceCar.TabStop = false;
            this.groupBox_BalanceCar.Text = "两轮平衡车";
            // 
            // groupBox_Balance_Direction
            // 
            this.groupBox_Balance_Direction.Controls.Add(this.button_ModifyPID_Direction);
            this.groupBox_Balance_Direction.Controls.Add(this.textBox_Direction_D);
            this.groupBox_Balance_Direction.Controls.Add(this.label19);
            this.groupBox_Balance_Direction.Controls.Add(this.textBox_Direction_I);
            this.groupBox_Balance_Direction.Controls.Add(this.label20);
            this.groupBox_Balance_Direction.Controls.Add(this.textBox_Direction_P);
            this.groupBox_Balance_Direction.Controls.Add(this.label21);
            this.groupBox_Balance_Direction.Location = new System.Drawing.Point(392, 32);
            this.groupBox_Balance_Direction.Name = "groupBox_Balance_Direction";
            this.groupBox_Balance_Direction.Size = new System.Drawing.Size(154, 169);
            this.groupBox_Balance_Direction.TabIndex = 8;
            this.groupBox_Balance_Direction.TabStop = false;
            this.groupBox_Balance_Direction.Text = "方向PID";
            // 
            // button_ModifyPID_Direction
            // 
            this.button_ModifyPID_Direction.Location = new System.Drawing.Point(45, 128);
            this.button_ModifyPID_Direction.Name = "button_ModifyPID_Direction";
            this.button_ModifyPID_Direction.Size = new System.Drawing.Size(75, 23);
            this.button_ModifyPID_Direction.TabIndex = 8;
            this.button_ModifyPID_Direction.Text = "修改参数";
            this.button_ModifyPID_Direction.UseVisualStyleBackColor = true;
            this.button_ModifyPID_Direction.Click += new System.EventHandler(this.button_ModifyPID_Direction_Click);
            // 
            // textBox_Direction_D
            // 
            this.textBox_Direction_D.Location = new System.Drawing.Point(53, 90);
            this.textBox_Direction_D.Name = "textBox_Direction_D";
            this.textBox_Direction_D.Size = new System.Drawing.Size(67, 21);
            this.textBox_Direction_D.TabIndex = 5;
            this.textBox_Direction_D.Text = "1.0";
            this.textBox_Direction_D.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(30, 98);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(17, 12);
            this.label19.TabIndex = 4;
            this.label19.Text = "D:";
            // 
            // textBox_Direction_I
            // 
            this.textBox_Direction_I.Location = new System.Drawing.Point(53, 54);
            this.textBox_Direction_I.Name = "textBox_Direction_I";
            this.textBox_Direction_I.Size = new System.Drawing.Size(67, 21);
            this.textBox_Direction_I.TabIndex = 3;
            this.textBox_Direction_I.Text = "1.0";
            this.textBox_Direction_I.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(30, 59);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(17, 12);
            this.label20.TabIndex = 2;
            this.label20.Text = "I:";
            // 
            // textBox_Direction_P
            // 
            this.textBox_Direction_P.Location = new System.Drawing.Point(53, 23);
            this.textBox_Direction_P.Name = "textBox_Direction_P";
            this.textBox_Direction_P.Size = new System.Drawing.Size(67, 21);
            this.textBox_Direction_P.TabIndex = 1;
            this.textBox_Direction_P.Text = "1.0";
            this.textBox_Direction_P.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(30, 27);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(17, 12);
            this.label21.TabIndex = 0;
            this.label21.Text = "P:";
            // 
            // groupBox_Balance_Speed
            // 
            this.groupBox_Balance_Speed.Controls.Add(this.button_ModifyPID_Speed);
            this.groupBox_Balance_Speed.Controls.Add(this.textBox_Speed_D);
            this.groupBox_Balance_Speed.Controls.Add(this.label16);
            this.groupBox_Balance_Speed.Controls.Add(this.textBox_Speed_I);
            this.groupBox_Balance_Speed.Controls.Add(this.label17);
            this.groupBox_Balance_Speed.Controls.Add(this.textBox_Speed_P);
            this.groupBox_Balance_Speed.Controls.Add(this.label18);
            this.groupBox_Balance_Speed.Location = new System.Drawing.Point(208, 32);
            this.groupBox_Balance_Speed.Name = "groupBox_Balance_Speed";
            this.groupBox_Balance_Speed.Size = new System.Drawing.Size(154, 169);
            this.groupBox_Balance_Speed.TabIndex = 8;
            this.groupBox_Balance_Speed.TabStop = false;
            this.groupBox_Balance_Speed.Text = "速度PID";
            // 
            // button_ModifyPID_Speed
            // 
            this.button_ModifyPID_Speed.Location = new System.Drawing.Point(45, 128);
            this.button_ModifyPID_Speed.Name = "button_ModifyPID_Speed";
            this.button_ModifyPID_Speed.Size = new System.Drawing.Size(75, 23);
            this.button_ModifyPID_Speed.TabIndex = 7;
            this.button_ModifyPID_Speed.Text = "修改参数";
            this.button_ModifyPID_Speed.UseVisualStyleBackColor = true;
            this.button_ModifyPID_Speed.Click += new System.EventHandler(this.button_ModifyPID_Speed_Click);
            // 
            // textBox_Speed_D
            // 
            this.textBox_Speed_D.Location = new System.Drawing.Point(53, 90);
            this.textBox_Speed_D.Name = "textBox_Speed_D";
            this.textBox_Speed_D.Size = new System.Drawing.Size(67, 21);
            this.textBox_Speed_D.TabIndex = 5;
            this.textBox_Speed_D.Text = "1.0";
            this.textBox_Speed_D.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(30, 98);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(17, 12);
            this.label16.TabIndex = 4;
            this.label16.Text = "D:";
            // 
            // textBox_Speed_I
            // 
            this.textBox_Speed_I.Location = new System.Drawing.Point(53, 54);
            this.textBox_Speed_I.Name = "textBox_Speed_I";
            this.textBox_Speed_I.Size = new System.Drawing.Size(67, 21);
            this.textBox_Speed_I.TabIndex = 3;
            this.textBox_Speed_I.Text = "1.0";
            this.textBox_Speed_I.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(30, 59);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(17, 12);
            this.label17.TabIndex = 2;
            this.label17.Text = "I:";
            // 
            // textBox_Speed_P
            // 
            this.textBox_Speed_P.Location = new System.Drawing.Point(53, 23);
            this.textBox_Speed_P.Name = "textBox_Speed_P";
            this.textBox_Speed_P.Size = new System.Drawing.Size(67, 21);
            this.textBox_Speed_P.TabIndex = 1;
            this.textBox_Speed_P.Text = "1.0";
            this.textBox_Speed_P.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(30, 27);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(17, 12);
            this.label18.TabIndex = 0;
            this.label18.Text = "P:";
            // 
            // groupBox_Balance_Stand
            // 
            this.groupBox_Balance_Stand.Controls.Add(this.button_ModifyPID_Stand);
            this.groupBox_Balance_Stand.Controls.Add(this.textBox_Stand_D);
            this.groupBox_Balance_Stand.Controls.Add(this.label13);
            this.groupBox_Balance_Stand.Controls.Add(this.textBox_Stand_I);
            this.groupBox_Balance_Stand.Controls.Add(this.label14);
            this.groupBox_Balance_Stand.Controls.Add(this.textBox_Stand_P);
            this.groupBox_Balance_Stand.Controls.Add(this.label15);
            this.groupBox_Balance_Stand.Location = new System.Drawing.Point(22, 32);
            this.groupBox_Balance_Stand.Name = "groupBox_Balance_Stand";
            this.groupBox_Balance_Stand.Size = new System.Drawing.Size(154, 169);
            this.groupBox_Balance_Stand.TabIndex = 7;
            this.groupBox_Balance_Stand.TabStop = false;
            this.groupBox_Balance_Stand.Text = "直立PID";
            // 
            // button_ModifyPID_Stand
            // 
            this.button_ModifyPID_Stand.Location = new System.Drawing.Point(47, 128);
            this.button_ModifyPID_Stand.Name = "button_ModifyPID_Stand";
            this.button_ModifyPID_Stand.Size = new System.Drawing.Size(75, 23);
            this.button_ModifyPID_Stand.TabIndex = 6;
            this.button_ModifyPID_Stand.Text = "修改参数";
            this.button_ModifyPID_Stand.UseVisualStyleBackColor = true;
            this.button_ModifyPID_Stand.Click += new System.EventHandler(this.button_ModifyPID_Stand_Click);
            // 
            // textBox_Stand_D
            // 
            this.textBox_Stand_D.Location = new System.Drawing.Point(53, 90);
            this.textBox_Stand_D.Name = "textBox_Stand_D";
            this.textBox_Stand_D.Size = new System.Drawing.Size(67, 21);
            this.textBox_Stand_D.TabIndex = 5;
            this.textBox_Stand_D.Text = "1.0";
            this.textBox_Stand_D.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(30, 98);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(17, 12);
            this.label13.TabIndex = 4;
            this.label13.Text = "D:";
            // 
            // textBox_Stand_I
            // 
            this.textBox_Stand_I.Location = new System.Drawing.Point(53, 54);
            this.textBox_Stand_I.Name = "textBox_Stand_I";
            this.textBox_Stand_I.Size = new System.Drawing.Size(67, 21);
            this.textBox_Stand_I.TabIndex = 3;
            this.textBox_Stand_I.Text = "1.0";
            this.textBox_Stand_I.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(30, 59);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(17, 12);
            this.label14.TabIndex = 2;
            this.label14.Text = "I:";
            // 
            // textBox_Stand_P
            // 
            this.textBox_Stand_P.Location = new System.Drawing.Point(53, 23);
            this.textBox_Stand_P.Name = "textBox_Stand_P";
            this.textBox_Stand_P.Size = new System.Drawing.Size(67, 21);
            this.textBox_Stand_P.TabIndex = 1;
            this.textBox_Stand_P.Text = "1.0";
            this.textBox_Stand_P.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(30, 27);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(17, 12);
            this.label15.TabIndex = 0;
            this.label15.Text = "P:";
            // 
            // groupBox_fourWheels
            // 
            this.groupBox_fourWheels.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.groupBox_fourWheels.Controls.Add(this.groupBox_Steer);
            this.groupBox_fourWheels.Controls.Add(this.groupBox_Motor);
            this.groupBox_fourWheels.Location = new System.Drawing.Point(100, 197);
            this.groupBox_fourWheels.Name = "groupBox_fourWheels";
            this.groupBox_fourWheels.Size = new System.Drawing.Size(422, 204);
            this.groupBox_fourWheels.TabIndex = 9;
            this.groupBox_fourWheels.TabStop = false;
            this.groupBox_fourWheels.Text = "四轮车";
            // 
            // groupBox_Steer
            // 
            this.groupBox_Steer.Controls.Add(this.button_ModifyPID_Steer);
            this.groupBox_Steer.Controls.Add(this.textBox_Steer_D);
            this.groupBox_Steer.Controls.Add(this.label9);
            this.groupBox_Steer.Controls.Add(this.textBox_Steer_I);
            this.groupBox_Steer.Controls.Add(this.label8);
            this.groupBox_Steer.Controls.Add(this.textBox_Steer_P);
            this.groupBox_Steer.Controls.Add(this.label7);
            this.groupBox_Steer.Location = new System.Drawing.Point(48, 23);
            this.groupBox_Steer.Name = "groupBox_Steer";
            this.groupBox_Steer.Size = new System.Drawing.Size(154, 165);
            this.groupBox_Steer.TabIndex = 1;
            this.groupBox_Steer.TabStop = false;
            this.groupBox_Steer.Text = "舵机PID";
            // 
            // button_ModifyPID_Steer
            // 
            this.button_ModifyPID_Steer.Location = new System.Drawing.Point(45, 126);
            this.button_ModifyPID_Steer.Name = "button_ModifyPID_Steer";
            this.button_ModifyPID_Steer.Size = new System.Drawing.Size(75, 23);
            this.button_ModifyPID_Steer.TabIndex = 6;
            this.button_ModifyPID_Steer.Text = "修改参数";
            this.button_ModifyPID_Steer.UseVisualStyleBackColor = true;
            this.button_ModifyPID_Steer.Click += new System.EventHandler(this.button_ModifyPID_Steer_Click);
            // 
            // textBox_Steer_D
            // 
            this.textBox_Steer_D.Location = new System.Drawing.Point(53, 90);
            this.textBox_Steer_D.Name = "textBox_Steer_D";
            this.textBox_Steer_D.Size = new System.Drawing.Size(67, 21);
            this.textBox_Steer_D.TabIndex = 5;
            this.textBox_Steer_D.Text = "1.0";
            this.textBox_Steer_D.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(30, 98);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(17, 12);
            this.label9.TabIndex = 4;
            this.label9.Text = "D:";
            // 
            // textBox_Steer_I
            // 
            this.textBox_Steer_I.Location = new System.Drawing.Point(53, 54);
            this.textBox_Steer_I.Name = "textBox_Steer_I";
            this.textBox_Steer_I.Size = new System.Drawing.Size(67, 21);
            this.textBox_Steer_I.TabIndex = 3;
            this.textBox_Steer_I.Text = "1.0";
            this.textBox_Steer_I.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(30, 59);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 12);
            this.label8.TabIndex = 2;
            this.label8.Text = "I:";
            // 
            // textBox_Steer_P
            // 
            this.textBox_Steer_P.Location = new System.Drawing.Point(53, 23);
            this.textBox_Steer_P.Name = "textBox_Steer_P";
            this.textBox_Steer_P.Size = new System.Drawing.Size(67, 21);
            this.textBox_Steer_P.TabIndex = 1;
            this.textBox_Steer_P.Text = "1.0";
            this.textBox_Steer_P.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(30, 27);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 12);
            this.label7.TabIndex = 0;
            this.label7.Text = "P:";
            // 
            // groupBox_Motor
            // 
            this.groupBox_Motor.Controls.Add(this.button_ModifyPID_Motor);
            this.groupBox_Motor.Controls.Add(this.textBox_Motor_D);
            this.groupBox_Motor.Controls.Add(this.label10);
            this.groupBox_Motor.Controls.Add(this.textBox_Motor_I);
            this.groupBox_Motor.Controls.Add(this.label11);
            this.groupBox_Motor.Controls.Add(this.textBox_Motor_P);
            this.groupBox_Motor.Controls.Add(this.label12);
            this.groupBox_Motor.Location = new System.Drawing.Point(232, 23);
            this.groupBox_Motor.Name = "groupBox_Motor";
            this.groupBox_Motor.Size = new System.Drawing.Size(154, 165);
            this.groupBox_Motor.TabIndex = 6;
            this.groupBox_Motor.TabStop = false;
            this.groupBox_Motor.Text = "电机PID";
            // 
            // button_ModifyPID_Motor
            // 
            this.button_ModifyPID_Motor.Location = new System.Drawing.Point(45, 126);
            this.button_ModifyPID_Motor.Name = "button_ModifyPID_Motor";
            this.button_ModifyPID_Motor.Size = new System.Drawing.Size(75, 23);
            this.button_ModifyPID_Motor.TabIndex = 7;
            this.button_ModifyPID_Motor.Text = "修改参数";
            this.button_ModifyPID_Motor.UseVisualStyleBackColor = true;
            this.button_ModifyPID_Motor.Click += new System.EventHandler(this.button_ModifyPID_Motor_Click);
            // 
            // textBox_Motor_D
            // 
            this.textBox_Motor_D.Location = new System.Drawing.Point(53, 90);
            this.textBox_Motor_D.Name = "textBox_Motor_D";
            this.textBox_Motor_D.Size = new System.Drawing.Size(67, 21);
            this.textBox_Motor_D.TabIndex = 5;
            this.textBox_Motor_D.Text = "1.0";
            this.textBox_Motor_D.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(30, 98);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(17, 12);
            this.label10.TabIndex = 4;
            this.label10.Text = "D:";
            // 
            // textBox_Motor_I
            // 
            this.textBox_Motor_I.Location = new System.Drawing.Point(53, 54);
            this.textBox_Motor_I.Name = "textBox_Motor_I";
            this.textBox_Motor_I.Size = new System.Drawing.Size(67, 21);
            this.textBox_Motor_I.TabIndex = 3;
            this.textBox_Motor_I.Text = "1.0";
            this.textBox_Motor_I.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(30, 59);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(17, 12);
            this.label11.TabIndex = 2;
            this.label11.Text = "I:";
            // 
            // textBox_Motor_P
            // 
            this.textBox_Motor_P.Location = new System.Drawing.Point(53, 23);
            this.textBox_Motor_P.Name = "textBox_Motor_P";
            this.textBox_Motor_P.Size = new System.Drawing.Size(67, 21);
            this.textBox_Motor_P.TabIndex = 1;
            this.textBox_Motor_P.Text = "1.0";
            this.textBox_Motor_P.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(30, 27);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(17, 12);
            this.label12.TabIndex = 0;
            this.label12.Text = "P:";
            // 
            // groupBox_carType
            // 
            this.groupBox_carType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.groupBox_carType.Controls.Add(this.checkBox_IsUsePID);
            this.groupBox_carType.Controls.Add(this.radioButton_BalanceCar);
            this.groupBox_carType.Controls.Add(this.radioButton_FourWheel);
            this.groupBox_carType.Location = new System.Drawing.Point(170, 12);
            this.groupBox_carType.Name = "groupBox_carType";
            this.groupBox_carType.Size = new System.Drawing.Size(132, 151);
            this.groupBox_carType.TabIndex = 10;
            this.groupBox_carType.TabStop = false;
            this.groupBox_carType.Text = "车型选择";
            // 
            // checkBox_IsUsePID
            // 
            this.checkBox_IsUsePID.AutoSize = true;
            this.checkBox_IsUsePID.Checked = true;
            this.checkBox_IsUsePID.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_IsUsePID.Location = new System.Drawing.Point(20, 30);
            this.checkBox_IsUsePID.Name = "checkBox_IsUsePID";
            this.checkBox_IsUsePID.Size = new System.Drawing.Size(90, 16);
            this.checkBox_IsUsePID.TabIndex = 5;
            this.checkBox_IsUsePID.Text = "使用PID参数";
            this.checkBox_IsUsePID.UseVisualStyleBackColor = true;
            this.checkBox_IsUsePID.CheckedChanged += new System.EventHandler(this.checkBox_IsUsePID_CheckedChanged);
            // 
            // radioButton_BalanceCar
            // 
            this.radioButton_BalanceCar.AutoSize = true;
            this.radioButton_BalanceCar.Location = new System.Drawing.Point(20, 93);
            this.radioButton_BalanceCar.Name = "radioButton_BalanceCar";
            this.radioButton_BalanceCar.Size = new System.Drawing.Size(83, 16);
            this.radioButton_BalanceCar.TabIndex = 4;
            this.radioButton_BalanceCar.Text = "两轮平衡车";
            this.radioButton_BalanceCar.UseVisualStyleBackColor = true;
            this.radioButton_BalanceCar.CheckedChanged += new System.EventHandler(this.radioButton_BalanceCar_CheckedChanged);
            // 
            // radioButton_FourWheel
            // 
            this.radioButton_FourWheel.AutoSize = true;
            this.radioButton_FourWheel.Checked = true;
            this.radioButton_FourWheel.Location = new System.Drawing.Point(20, 61);
            this.radioButton_FourWheel.Name = "radioButton_FourWheel";
            this.radioButton_FourWheel.Size = new System.Drawing.Size(59, 16);
            this.radioButton_FourWheel.TabIndex = 3;
            this.radioButton_FourWheel.TabStop = true;
            this.radioButton_FourWheel.Text = "四轮车";
            this.radioButton_FourWheel.UseVisualStyleBackColor = true;
            this.radioButton_FourWheel.Click += new System.EventHandler(this.radioButton_FourWheel_Click);
            // 
            // groupBox_sensorType
            // 
            this.groupBox_sensorType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.groupBox_sensorType.BackColor = System.Drawing.Color.White;
            this.groupBox_sensorType.Controls.Add(this.radioButton_CCD);
            this.groupBox_sensorType.Controls.Add(this.radioButton_camera);
            this.groupBox_sensorType.Controls.Add(this.radioButton_electromagnetism);
            this.groupBox_sensorType.Location = new System.Drawing.Point(18, 12);
            this.groupBox_sensorType.Name = "groupBox_sensorType";
            this.groupBox_sensorType.Size = new System.Drawing.Size(136, 151);
            this.groupBox_sensorType.TabIndex = 0;
            this.groupBox_sensorType.TabStop = false;
            this.groupBox_sensorType.Text = "传感器类型（无）";
            // 
            // radioButton_CCD
            // 
            this.radioButton_CCD.AutoSize = true;
            this.radioButton_CCD.Location = new System.Drawing.Point(38, 94);
            this.radioButton_CCD.Name = "radioButton_CCD";
            this.radioButton_CCD.Size = new System.Drawing.Size(47, 16);
            this.radioButton_CCD.TabIndex = 2;
            this.radioButton_CCD.Text = "光电";
            this.radioButton_CCD.UseVisualStyleBackColor = true;
            // 
            // radioButton_camera
            // 
            this.radioButton_camera.AutoSize = true;
            this.radioButton_camera.Location = new System.Drawing.Point(38, 61);
            this.radioButton_camera.Name = "radioButton_camera";
            this.radioButton_camera.Size = new System.Drawing.Size(59, 16);
            this.radioButton_camera.TabIndex = 1;
            this.radioButton_camera.Text = "摄像头";
            this.radioButton_camera.UseVisualStyleBackColor = true;
            // 
            // radioButton_electromagnetism
            // 
            this.radioButton_electromagnetism.AutoSize = true;
            this.radioButton_electromagnetism.Checked = true;
            this.radioButton_electromagnetism.Location = new System.Drawing.Point(38, 29);
            this.radioButton_electromagnetism.Name = "radioButton_electromagnetism";
            this.radioButton_electromagnetism.Size = new System.Drawing.Size(47, 16);
            this.radioButton_electromagnetism.TabIndex = 0;
            this.radioButton_electromagnetism.TabStop = true;
            this.radioButton_electromagnetism.Text = "电磁";
            this.radioButton_electromagnetism.UseVisualStyleBackColor = true;
            // 
            // tabPage_DIY_Algorithm
            // 
            this.tabPage_DIY_Algorithm.Controls.Add(this.button_DIY_NumConfirm);
            this.tabPage_DIY_Algorithm.Controls.Add(this.label22);
            this.tabPage_DIY_Algorithm.Controls.Add(this.textBox_DIY_Number);
            this.tabPage_DIY_Algorithm.Controls.Add(this.panel_add_DIYControls);
            this.tabPage_DIY_Algorithm.Controls.Add(this.button_DIY_SendModify);
            this.tabPage_DIY_Algorithm.Location = new System.Drawing.Point(4, 22);
            this.tabPage_DIY_Algorithm.Name = "tabPage_DIY_Algorithm";
            this.tabPage_DIY_Algorithm.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_DIY_Algorithm.Size = new System.Drawing.Size(736, 539);
            this.tabPage_DIY_Algorithm.TabIndex = 5;
            this.tabPage_DIY_Algorithm.Text = "自定义算法参数";
            this.tabPage_DIY_Algorithm.UseVisualStyleBackColor = true;
            // 
            // button_DIY_NumConfirm
            // 
            this.button_DIY_NumConfirm.Location = new System.Drawing.Point(263, 21);
            this.button_DIY_NumConfirm.Name = "button_DIY_NumConfirm";
            this.button_DIY_NumConfirm.Size = new System.Drawing.Size(57, 23);
            this.button_DIY_NumConfirm.TabIndex = 36;
            this.button_DIY_NumConfirm.Text = "确认";
            this.button_DIY_NumConfirm.UseVisualStyleBackColor = true;
            this.button_DIY_NumConfirm.Click += new System.EventHandler(this.button_DIY_NumConfirm_Click);
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(28, 26);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(113, 12);
            this.label22.TabIndex = 35;
            this.label22.Text = "请输入自定义数量：";
            // 
            // textBox_DIY_Number
            // 
            this.textBox_DIY_Number.Location = new System.Drawing.Point(147, 23);
            this.textBox_DIY_Number.Name = "textBox_DIY_Number";
            this.textBox_DIY_Number.Size = new System.Drawing.Size(100, 21);
            this.textBox_DIY_Number.TabIndex = 34;
            this.textBox_DIY_Number.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // panel_add_DIYControls
            // 
            this.panel_add_DIYControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel_add_DIYControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_add_DIYControls.Location = new System.Drawing.Point(30, 63);
            this.panel_add_DIYControls.Name = "panel_add_DIYControls";
            this.panel_add_DIYControls.Size = new System.Drawing.Size(541, 408);
            this.panel_add_DIYControls.TabIndex = 33;
            // 
            // button_DIY_SendModify
            // 
            this.button_DIY_SendModify.Location = new System.Drawing.Point(576, 478);
            this.button_DIY_SendModify.Name = "button_DIY_SendModify";
            this.button_DIY_SendModify.Size = new System.Drawing.Size(75, 23);
            this.button_DIY_SendModify.TabIndex = 32;
            this.button_DIY_SendModify.Text = "修改参数";
            this.button_DIY_SendModify.UseVisualStyleBackColor = true;
            this.button_DIY_SendModify.Visible = false;
            this.button_DIY_SendModify.Click += new System.EventHandler(this.button_DIY_Modify_Click);
            // 
            // tabPage_DAC_Value
            // 
            this.tabPage_DAC_Value.Controls.Add(this.button_electricity_NumConfirm);
            this.tabPage_DAC_Value.Controls.Add(this.label23);
            this.tabPage_DAC_Value.Controls.Add(this.textBox_Electricity_Number);
            this.tabPage_DAC_Value.Controls.Add(this.panel_Electricity);
            this.tabPage_DAC_Value.Location = new System.Drawing.Point(4, 22);
            this.tabPage_DAC_Value.Name = "tabPage_DAC_Value";
            this.tabPage_DAC_Value.Size = new System.Drawing.Size(736, 539);
            this.tabPage_DAC_Value.TabIndex = 6;
            this.tabPage_DAC_Value.Text = "实时变量值";
            this.tabPage_DAC_Value.UseVisualStyleBackColor = true;
            // 
            // button_electricity_NumConfirm
            // 
            this.button_electricity_NumConfirm.Location = new System.Drawing.Point(268, 34);
            this.button_electricity_NumConfirm.Name = "button_electricity_NumConfirm";
            this.button_electricity_NumConfirm.Size = new System.Drawing.Size(57, 23);
            this.button_electricity_NumConfirm.TabIndex = 40;
            this.button_electricity_NumConfirm.Text = "确认";
            this.button_electricity_NumConfirm.UseVisualStyleBackColor = true;
            this.button_electricity_NumConfirm.Click += new System.EventHandler(this.button_electricity_NumConfirm_Click);
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(42, 39);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(101, 12);
            this.label23.TabIndex = 39;
            this.label23.Text = "请输入变量数量：";
            // 
            // textBox_Electricity_Number
            // 
            this.textBox_Electricity_Number.Location = new System.Drawing.Point(149, 35);
            this.textBox_Electricity_Number.Name = "textBox_Electricity_Number";
            this.textBox_Electricity_Number.Size = new System.Drawing.Size(100, 21);
            this.textBox_Electricity_Number.TabIndex = 38;
            this.textBox_Electricity_Number.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // panel_Electricity
            // 
            this.panel_Electricity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel_Electricity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_Electricity.Location = new System.Drawing.Point(44, 76);
            this.panel_Electricity.Name = "panel_Electricity";
            this.panel_Electricity.Size = new System.Drawing.Size(541, 408);
            this.panel_Electricity.TabIndex = 37;
            // 
            // tabPage_Camera
            // 
            this.tabPage_Camera.Controls.Add(this.checkBox_Camera_ONOFF);
            this.tabPage_Camera.Controls.Add(this.label25);
            this.tabPage_Camera.Controls.Add(this.label24);
            this.tabPage_Camera.Controls.Add(this.textBox2);
            this.tabPage_Camera.Controls.Add(this.textBox1);
            this.tabPage_Camera.Controls.Add(this.numericUpDown1);
            this.tabPage_Camera.Controls.Add(this.pictureBox_CameraActual);
            this.tabPage_Camera.Location = new System.Drawing.Point(4, 22);
            this.tabPage_Camera.Name = "tabPage_Camera";
            this.tabPage_Camera.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_Camera.Size = new System.Drawing.Size(736, 539);
            this.tabPage_Camera.TabIndex = 2;
            this.tabPage_Camera.Text = "摄像头图像（无）";
            this.tabPage_Camera.UseVisualStyleBackColor = true;
            // 
            // checkBox_Camera_ONOFF
            // 
            this.checkBox_Camera_ONOFF.AutoSize = true;
            this.checkBox_Camera_ONOFF.Location = new System.Drawing.Point(21, 23);
            this.checkBox_Camera_ONOFF.Name = "checkBox_Camera_ONOFF";
            this.checkBox_Camera_ONOFF.Size = new System.Drawing.Size(108, 16);
            this.checkBox_Camera_ONOFF.TabIndex = 6;
            this.checkBox_Camera_ONOFF.Text = "摄像头显示开关";
            this.checkBox_Camera_ONOFF.UseVisualStyleBackColor = true;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(19, 104);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(29, 12);
            this.label25.TabIndex = 5;
            this.label25.Text = "高：";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(19, 70);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(29, 12);
            this.label24.TabIndex = 4;
            this.label24.Text = "宽：";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(72, 105);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(54, 21);
            this.textBox2.TabIndex = 3;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(72, 67);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(54, 21);
            this.textBox1.TabIndex = 2;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(58, 160);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(68, 21);
            this.numericUpDown1.TabIndex = 1;
            // 
            // pictureBox_CameraActual
            // 
            this.pictureBox_CameraActual.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_CameraActual.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox_CameraActual.Location = new System.Drawing.Point(145, 129);
            this.pictureBox_CameraActual.Name = "pictureBox_CameraActual";
            this.pictureBox_CameraActual.Size = new System.Drawing.Size(527, 273);
            this.pictureBox_CameraActual.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_CameraActual.TabIndex = 0;
            this.pictureBox_CameraActual.TabStop = false;
            // 
            // tabPage_CCD
            // 
            this.tabPage_CCD.Controls.Add(this.label32);
            this.tabPage_CCD.Controls.Add(this.pictureBox_CCD3);
            this.tabPage_CCD.Controls.Add(this.label31);
            this.tabPage_CCD.Controls.Add(this.label30);
            this.tabPage_CCD.Controls.Add(this.groupBox3);
            this.tabPage_CCD.Controls.Add(this.pictureBox_CCD_deal);
            this.tabPage_CCD.Controls.Add(this.label27);
            this.tabPage_CCD.Controls.Add(this.pictureBox_CCD_Path);
            this.tabPage_CCD.Controls.Add(this.pictureBox_CCD_Actual);
            this.tabPage_CCD.Location = new System.Drawing.Point(4, 22);
            this.tabPage_CCD.Name = "tabPage_CCD";
            this.tabPage_CCD.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_CCD.Size = new System.Drawing.Size(736, 539);
            this.tabPage_CCD.TabIndex = 3;
            this.tabPage_CCD.Text = "CCD图像（无）";
            this.tabPage_CCD.UseVisualStyleBackColor = true;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(181, 236);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(47, 12);
            this.label32.TabIndex = 11;
            this.label32.Text = "图像3：";
            // 
            // pictureBox_CCD3
            // 
            this.pictureBox_CCD3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_CCD3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox_CCD3.Location = new System.Drawing.Point(246, 193);
            this.pictureBox_CCD3.Name = "pictureBox_CCD3";
            this.pictureBox_CCD3.Size = new System.Drawing.Size(484, 88);
            this.pictureBox_CCD3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_CCD3.TabIndex = 10;
            this.pictureBox_CCD3.TabStop = false;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(157, 318);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(71, 12);
            this.label31.TabIndex = 9;
            this.label31.Text = "图像1路径：";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(181, 124);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(47, 12);
            this.label30.TabIndex = 8;
            this.label30.Text = "图像2：";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBox_CCD_ONOFF);
            this.groupBox3.Controls.Add(this.comboBox1);
            this.groupBox3.Controls.Add(this.label_CCD_Width);
            this.groupBox3.Location = new System.Drawing.Point(10, 11);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(118, 205);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox3";
            // 
            // checkBox_CCD_ONOFF
            // 
            this.checkBox_CCD_ONOFF.AutoSize = true;
            this.checkBox_CCD_ONOFF.Location = new System.Drawing.Point(22, 24);
            this.checkBox_CCD_ONOFF.Name = "checkBox_CCD_ONOFF";
            this.checkBox_CCD_ONOFF.Size = new System.Drawing.Size(90, 16);
            this.checkBox_CCD_ONOFF.TabIndex = 4;
            this.checkBox_CCD_ONOFF.Text = "CCD显示开关";
            this.checkBox_CCD_ONOFF.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "1",
            "2",
            "3"});
            this.comboBox1.Location = new System.Drawing.Point(22, 52);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(67, 20);
            this.comboBox1.TabIndex = 2;
            // 
            // label_CCD_Width
            // 
            this.label_CCD_Width.AutoSize = true;
            this.label_CCD_Width.Location = new System.Drawing.Point(20, 89);
            this.label_CCD_Width.Name = "label_CCD_Width";
            this.label_CCD_Width.Size = new System.Drawing.Size(83, 12);
            this.label_CCD_Width.TabIndex = 3;
            this.label_CCD_Width.Text = "CCD宽度：None";
            // 
            // pictureBox_CCD_deal
            // 
            this.pictureBox_CCD_deal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_CCD_deal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox_CCD_deal.Location = new System.Drawing.Point(246, 99);
            this.pictureBox_CCD_deal.Name = "pictureBox_CCD_deal";
            this.pictureBox_CCD_deal.Size = new System.Drawing.Size(484, 88);
            this.pictureBox_CCD_deal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_CCD_deal.TabIndex = 6;
            this.pictureBox_CCD_deal.TabStop = false;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(181, 39);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(47, 12);
            this.label27.TabIndex = 5;
            this.label27.Text = "图像1：";
            // 
            // pictureBox_CCD_Path
            // 
            this.pictureBox_CCD_Path.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_CCD_Path.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox_CCD_Path.Location = new System.Drawing.Point(246, 290);
            this.pictureBox_CCD_Path.Name = "pictureBox_CCD_Path";
            this.pictureBox_CCD_Path.Size = new System.Drawing.Size(484, 217);
            this.pictureBox_CCD_Path.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_CCD_Path.TabIndex = 1;
            this.pictureBox_CCD_Path.TabStop = false;
            // 
            // pictureBox_CCD_Actual
            // 
            this.pictureBox_CCD_Actual.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_CCD_Actual.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox_CCD_Actual.Location = new System.Drawing.Point(246, 6);
            this.pictureBox_CCD_Actual.Name = "pictureBox_CCD_Actual";
            this.pictureBox_CCD_Actual.Size = new System.Drawing.Size(484, 87);
            this.pictureBox_CCD_Actual.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_CCD_Actual.TabIndex = 0;
            this.pictureBox_CCD_Actual.TabStop = false;
            // 
            // label28
            // 
            this.label28.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(59, 554);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(41, 12);
            this.label28.TabIndex = 13;
            this.label28.Text = "Status";
            // 
            // timer_autoSend
            // 
            this.timer_autoSend.Interval = 500;
            this.timer_autoSend.Tick += new System.EventHandler(this.timer_autoSend_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "串口号：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "波特率：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "校验位：";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 127);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "数据位：";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 160);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "停止位：";
            // 
            // comboBox_port
            // 
            this.comboBox_port.FormattingEnabled = true;
            this.comboBox_port.Location = new System.Drawing.Point(79, 25);
            this.comboBox_port.Name = "comboBox_port";
            this.comboBox_port.Size = new System.Drawing.Size(121, 20);
            this.comboBox_port.TabIndex = 5;
            // 
            // comboBox_baudrate
            // 
            this.comboBox_baudrate.FormattingEnabled = true;
            this.comboBox_baudrate.Items.AddRange(new object[] {
            "300",
            "600",
            "1200",
            "2400",
            "4800",
            "9600",
            "14400",
            "19200",
            "28800",
            "38400",
            "56000",
            "115200",
            "128000",
            "256000"});
            this.comboBox_baudrate.Location = new System.Drawing.Point(79, 57);
            this.comboBox_baudrate.Name = "comboBox_baudrate";
            this.comboBox_baudrate.Size = new System.Drawing.Size(121, 20);
            this.comboBox_baudrate.TabIndex = 6;
            // 
            // comboBox_parity
            // 
            this.comboBox_parity.FormattingEnabled = true;
            this.comboBox_parity.Items.AddRange(new object[] {
            "无校验(None)",
            "偶校验(Even)",
            "奇校验(Odd)",
            "保留为0(Space)",
            "保留为1(Mark)"});
            this.comboBox_parity.Location = new System.Drawing.Point(79, 85);
            this.comboBox_parity.Name = "comboBox_parity";
            this.comboBox_parity.Size = new System.Drawing.Size(121, 20);
            this.comboBox_parity.TabIndex = 7;
            // 
            // comboBox_databit
            // 
            this.comboBox_databit.FormattingEnabled = true;
            this.comboBox_databit.Items.AddRange(new object[] {
            "8",
            "7",
            "6",
            "5"});
            this.comboBox_databit.Location = new System.Drawing.Point(81, 118);
            this.comboBox_databit.Name = "comboBox_databit";
            this.comboBox_databit.Size = new System.Drawing.Size(121, 20);
            this.comboBox_databit.TabIndex = 8;
            // 
            // comboBox_stopbit
            // 
            this.comboBox_stopbit.FormattingEnabled = true;
            this.comboBox_stopbit.Items.AddRange(new object[] {
            "1",
            "1.5",
            "2"});
            this.comboBox_stopbit.Location = new System.Drawing.Point(79, 151);
            this.comboBox_stopbit.Name = "comboBox_stopbit";
            this.comboBox_stopbit.Size = new System.Drawing.Size(121, 20);
            this.comboBox_stopbit.TabIndex = 9;
            // 
            // button_openPort
            // 
            this.button_openPort.Location = new System.Drawing.Point(125, 187);
            this.button_openPort.Name = "button_openPort";
            this.button_openPort.Size = new System.Drawing.Size(75, 23);
            this.button_openPort.TabIndex = 10;
            this.button_openPort.Text = "打开串口";
            this.button_openPort.UseVisualStyleBackColor = true;
            this.button_openPort.Click += new System.EventHandler(this.button_openPort_Click);
            // 
            // button_freshPort
            // 
            this.button_freshPort.Location = new System.Drawing.Point(25, 187);
            this.button_freshPort.Name = "button_freshPort";
            this.button_freshPort.Size = new System.Drawing.Size(75, 23);
            this.button_freshPort.TabIndex = 11;
            this.button_freshPort.Text = "刷新串口";
            this.button_freshPort.UseVisualStyleBackColor = true;
            this.button_freshPort.Click += new System.EventHandler(this.button_freshPort_Click);
            // 
            // groupBox_portSetting
            // 
            this.groupBox_portSetting.Controls.Add(this.button_freshPort);
            this.groupBox_portSetting.Controls.Add(this.button_openPort);
            this.groupBox_portSetting.Controls.Add(this.comboBox_stopbit);
            this.groupBox_portSetting.Controls.Add(this.comboBox_databit);
            this.groupBox_portSetting.Controls.Add(this.comboBox_parity);
            this.groupBox_portSetting.Controls.Add(this.comboBox_baudrate);
            this.groupBox_portSetting.Controls.Add(this.comboBox_port);
            this.groupBox_portSetting.Controls.Add(this.label5);
            this.groupBox_portSetting.Controls.Add(this.label4);
            this.groupBox_portSetting.Controls.Add(this.label3);
            this.groupBox_portSetting.Controls.Add(this.label2);
            this.groupBox_portSetting.Controls.Add(this.label1);
            this.groupBox_portSetting.Location = new System.Drawing.Point(12, 40);
            this.groupBox_portSetting.Name = "groupBox_portSetting";
            this.groupBox_portSetting.Size = new System.Drawing.Size(228, 223);
            this.groupBox_portSetting.TabIndex = 0;
            this.groupBox_portSetting.TabStop = false;
            this.groupBox_portSetting.Text = "串口设置";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.button_force_stop);
            this.groupBox1.Controls.Add(this.button_ExportMatlab);
            this.groupBox1.Controls.Add(this.button_loadHistory);
            this.groupBox1.Controls.Add(this.button_History);
            this.groupBox1.Controls.Add(this.button_ReadAllSettings);
            this.groupBox1.Location = new System.Drawing.Point(12, 270);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(228, 271);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "全局参数设置";
            // 
            // button_force_stop
            // 
            this.button_force_stop.BackColor = System.Drawing.SystemColors.Highlight;
            this.button_force_stop.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_force_stop.Location = new System.Drawing.Point(18, 72);
            this.button_force_stop.Name = "button_force_stop";
            this.button_force_stop.Size = new System.Drawing.Size(159, 93);
            this.button_force_stop.TabIndex = 5;
            this.button_force_stop.Text = "紧急停车";
            this.button_force_stop.UseVisualStyleBackColor = false;
            this.button_force_stop.Click += new System.EventHandler(this.button_force_stop_Click);
            // 
            // button_ExportMatlab
            // 
            this.button_ExportMatlab.Location = new System.Drawing.Point(25, 235);
            this.button_ExportMatlab.Name = "button_ExportMatlab";
            this.button_ExportMatlab.Size = new System.Drawing.Size(145, 23);
            this.button_ExportMatlab.TabIndex = 4;
            this.button_ExportMatlab.Text = "数据导出到matlab(无)";
            this.button_ExportMatlab.UseVisualStyleBackColor = true;
            // 
            // button_loadHistory
            // 
            this.button_loadHistory.Location = new System.Drawing.Point(54, 206);
            this.button_loadHistory.Name = "button_loadHistory";
            this.button_loadHistory.Size = new System.Drawing.Size(86, 23);
            this.button_loadHistory.TabIndex = 3;
            this.button_loadHistory.Text = "导入历史数据";
            this.button_loadHistory.UseVisualStyleBackColor = true;
            this.button_loadHistory.Click += new System.EventHandler(this.button_loadHistory_Click);
            // 
            // button_History
            // 
            this.button_History.Location = new System.Drawing.Point(60, 177);
            this.button_History.Name = "button_History";
            this.button_History.Size = new System.Drawing.Size(75, 23);
            this.button_History.TabIndex = 2;
            this.button_History.Text = "保存当前运行值";
            this.button_History.UseVisualStyleBackColor = true;
            this.button_History.Click += new System.EventHandler(this.button_History_Click);
            // 
            // button_ReadAllSettings
            // 
            this.button_ReadAllSettings.Location = new System.Drawing.Point(54, 35);
            this.button_ReadAllSettings.Name = "button_ReadAllSettings";
            this.button_ReadAllSettings.Size = new System.Drawing.Size(87, 23);
            this.button_ReadAllSettings.TabIndex = 0;
            this.button_ReadAllSettings.Text = "读取全局参数";
            this.button_ReadAllSettings.UseVisualStyleBackColor = true;
            this.button_ReadAllSettings.Click += new System.EventHandler(this.button_ReadAllSettings_Click);
            // 
            // openFileDialog_History
            // 
            this.openFileDialog_History.FileName = "openFileDialog1";
            // 
            // timer_Send2GetEcho
            // 
            this.timer_Send2GetEcho.Tick += new System.EventHandler(this.timer_Send2GetEcho_Tick);
            // 
            // timer_fresh
            // 
            this.timer_fresh.Tick += new System.EventHandler(this.timer_fresh_Tick);
            // 
            // label29
            // 
            this.label29.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(8, 554);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(47, 12);
            this.label29.TabIndex = 14;
            this.label29.Text = "Status:";
            // 
            // Form1
            // 
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1002, 589);
            this.Controls.Add(this.label29);
            this.Controls.Add(this.label28);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(tabControl1);
            this.Controls.Add(this.groupBox_portSetting);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "飞思卡尔调试平台 V1.1";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            tabControl1.ResumeLayout(false);
            this.tabPage_Serial.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox_send.ResumeLayout(false);
            this.groupBox_send.PerformLayout();
            this.groupBox_Receive.ResumeLayout(false);
            this.groupBox_Receive.PerformLayout();
            this.tabPage_Scope.ResumeLayout(false);
            this.tabPage_PIDSettings.ResumeLayout(false);
            this.tabPage_PIDSettings.PerformLayout();
            this.groupBox_BalanceCar.ResumeLayout(false);
            this.groupBox_Balance_Direction.ResumeLayout(false);
            this.groupBox_Balance_Direction.PerformLayout();
            this.groupBox_Balance_Speed.ResumeLayout(false);
            this.groupBox_Balance_Speed.PerformLayout();
            this.groupBox_Balance_Stand.ResumeLayout(false);
            this.groupBox_Balance_Stand.PerformLayout();
            this.groupBox_fourWheels.ResumeLayout(false);
            this.groupBox_Steer.ResumeLayout(false);
            this.groupBox_Steer.PerformLayout();
            this.groupBox_Motor.ResumeLayout(false);
            this.groupBox_Motor.PerformLayout();
            this.groupBox_carType.ResumeLayout(false);
            this.groupBox_carType.PerformLayout();
            this.groupBox_sensorType.ResumeLayout(false);
            this.groupBox_sensorType.PerformLayout();
            this.tabPage_DIY_Algorithm.ResumeLayout(false);
            this.tabPage_DIY_Algorithm.PerformLayout();
            this.tabPage_DAC_Value.ResumeLayout(false);
            this.tabPage_DAC_Value.PerformLayout();
            this.tabPage_Camera.ResumeLayout(false);
            this.tabPage_Camera.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_CameraActual)).EndInit();
            this.tabPage_CCD.ResumeLayout(false);
            this.tabPage_CCD.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_CCD3)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_CCD_deal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_CCD_Path)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_CCD_Actual)).EndInit();
            this.groupBox_portSetting.ResumeLayout(false);
            this.groupBox_portSetting.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TabPage tabPage_Serial;
        private TabPage tabPage_PIDSettings;
        private TabPage tabPage_Camera;
        private TabPage tabPage_CCD;
        private TabPage tabPage_Scope;
        private GroupBox groupBox_send;
        private TextBox textBox_send;
        private GroupBox groupBox_Receive;
        private TextBox textBox_receive;
        private Label label6;
        private TextBox textBox_sendPeriod;
        private CheckBox checkBox_sendAuto;
        private Button button_sendmessage;
        private Button button_receiveclear;
        private Button button_receivepause;
        private GroupBox groupBox2;
        private Label label_receiveCount;
        private Label label_sendCount;
        private CheckBox checkBox_receiveAnotherline;
        private CheckBox checkBox_receiveHex;
        private Button button_countclear;
        private SerialPort mySerialPort;
        private CheckBox checkBox_showRecvTime;
        private Timer timer_autoSend;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private ComboBox comboBox_port;
        private ComboBox comboBox_baudrate;
        private ComboBox comboBox_parity;
        private ComboBox comboBox_databit;
        private ComboBox comboBox_stopbit;
        private Button button_openPort;
        private Button button_freshPort;
        private GroupBox groupBox_portSetting;
        private GroupBox groupBox_Steer;
        private GroupBox groupBox_sensorType;
        private RadioButton radioButton_CCD;
        private RadioButton radioButton_camera;
        private RadioButton radioButton_electromagnetism;
        private PictureBox pictureBox_CameraActual;
        private PictureBox pictureBox_CCD_Path;
        private PictureBox pictureBox_CCD_Actual;
        private GroupBox groupBox_BalanceCar;
        private GroupBox groupBox_Balance_Direction;
        private TextBox textBox_Direction_D;
        private Label label19;
        private TextBox textBox_Direction_I;
        private Label label20;
        private TextBox textBox_Direction_P;
        private Label label21;
        private GroupBox groupBox_Balance_Speed;
        private TextBox textBox_Speed_D;
        private Label label16;
        private TextBox textBox_Speed_I;
        private Label label17;
        private TextBox textBox_Speed_P;
        private Label label18;
        private GroupBox groupBox_Balance_Stand;
        private TextBox textBox_Stand_D;
        private Label label13;
        private TextBox textBox_Stand_I;
        private Label label14;
        private TextBox textBox_Stand_P;
        private Label label15;
        private GroupBox groupBox_Motor;
        private TextBox textBox_Motor_D;
        private Label label10;
        private TextBox textBox_Motor_I;
        private Label label11;
        private TextBox textBox_Motor_P;
        private Label label12;
        private TextBox textBox_Steer_D;
        private Label label9;
        private TextBox textBox_Steer_I;
        private Label label8;
        private TextBox textBox_Steer_P;
        private Label label7;
        private TabPage tabPage_DIY_Algorithm;
        private TabPage tabPage_DAC_Value;
        private GroupBox groupBox_carType;
        private RadioButton radioButton_BalanceCar;
        private RadioButton radioButton_FourWheel;
        private GroupBox groupBox_fourWheels;
        private GroupBox groupBox1;
        private Button button_ReadAllSettings;
        private Panel panel_add_DIYControls;
        private Button button_DIY_SendModify;
        private Button button_DIY_NumConfirm;
        private Label label22;
        private TextBox textBox_DIY_Number;
        private Button button_electricity_NumConfirm;
        private Label label23;
        private TextBox textBox_Electricity_Number;
        private Panel panel_Electricity;
        private ComboBox comboBox1;
        private ZedGraphControl zedGraph_local;
        private Panel panel_Scope;
        private Label label25;
        private Label label24;
        private TextBox textBox2;
        private TextBox textBox1;
        private NumericUpDown numericUpDown1;
        private Label label_CCD_Width;
        private Button button_History;
        private CheckBox checkBox_IsUsePID;
        private SaveFileDialog saveFileDialog_History;
        private OpenFileDialog openFileDialog_History;
        private Button button_loadHistory;
        private CheckBox checkBox_Camera_ONOFF;
        private CheckBox checkBox_CCD_ONOFF;
        private Label label26;
        private Timer timer_Send2GetEcho;
        private Button button_ModifyPID_Direction;
        private Button button_ModifyPID_Speed;
        private Button button_ModifyPID_Stand;
        private Button button_ModifyPID_Steer;
        private Button button_ModifyPID_Motor;
        private Timer timer_fresh;
        private Label label27;
        private Label label28;
        private Button button_ExportMatlab;
        private Button button_ClearDrawing;
        private Label label29;
        private Button button_force_stop;
        private PictureBox pictureBox_CCD_deal;
        private Label label30;
        private GroupBox groupBox3;
        private Label label31;
        private Label label32;
        private PictureBox pictureBox_CCD3;
    }
}

