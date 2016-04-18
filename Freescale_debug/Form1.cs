#define MULTITHREAD //多线程收发模式，注释本句则使用单线程模式
//相对单线收发模式，占用系统资源稍微大些，但是执行效果更好，尤其是在大数据收发时的UI反应尤其明显       

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using leomon;
using TestLog4Net;
using ZedGraph;

namespace Freescale_debug
{
    public partial class Form1 : Form
    {
        public delegate void UpdateAcceptTextBoxTextHandler(string text);

        //ZedGraph
        private const int ScopeNumber = 8; //示波器能画曲线的数量
        private const string SavefileName = "串口助手配置.xml";

        private readonly Color[] _colorLine =
        {
            Color.Green, Color.DodgerBlue, Color.Brown, Color.Chartreuse,
            Color.CornflowerBlue, Color.Red, Color.Yellow, Color.Gray
        };

        //发送串口数据的队列，直到收到有效数据为止
        private readonly Queue _queueEchoControl = new Queue(); //根据这个来判断是不是要进行重发
        private readonly List<int> _recieveBuff = new List<int>();
        private readonly double _xminScale = 0;
        private Bitmap _bitmapOld;

        //SerialPort Flags
        private bool _closing; //是否正在关闭串口，执行Application.DoEvents，并阻止再次

        private bool _isLoadHistory;
        //自定义参数的一些变量
        private bool _listening; //是否没有执行完invoke相关操作  
        private int _xmaxScale = 100;
        private double _ymaxScale = 100;
        private double _yminScale = -10;
        //ZedGraph 窗体间传参
        public CallObject[] coOb = new CallObject[ScopeNumber];

        private ReceivedDataType myReceivedDataType = ReceivedDataType.CharType;
        private SendDataType mySendDataType = SendDataType.CharType;
        private string RecievedStringAdd = ""; //接收到的数据
        private int retryCount;
        private bool showInfo = true;

        private GetEchoForm tmpSendHandle = new GetEchoForm(1, "");
        private int totalReceivedBytes;
        public UpdateAcceptTextBoxTextHandler UpdateTextHandler;
        private readonly ZedGrpahName[] zedGrpahNames = new ZedGrpahName[ScopeNumber];

        //线程的委托//使线程可以改变控件的值
        private delegate void DoWorkUiThreadDelegate(string recvString, List<int> recvByte);

        private enum ReceivedDataType
        {
            CharType,
            HexType
        }

        private enum SendDataType
        {
            CharType,
            HexType
        }

        #region Form

        public Form1()
        {
            InitializeComponent();
            UpdateTextHandler = UpdateText;
            textBox_receive.ScrollToCaret();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _bitmapOld = new Bitmap(128, pictureBox_CCD_Path.Height);
            for (var i = 0; i < 128; i++)
            {
                for (var j = 0; j < pictureBox_CCD_Path.Height; j++)
                {
                    _bitmapOld.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                }
            }

            CheckAvailablePorts();

            LoadConfig(SavefileName);

            Init_TablePagePIDSettings();
            Init_CustomPara_DynamicControls();
            InitRealtime();
            InitzedGraph();
            Init_pane_Scope();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _closing = true;
            while (_listening) Application.DoEvents();

            timer_Send2GetEcho.Stop();

            SaveConfig(SavefileName);
        }

        #endregion

        #region 串口设置相关 1

        private int _totalSendBytes;
        private bool _autoSend;

        private void button_openPort_Click(object sender, EventArgs e) //打开关闭串口
        {
            if (button_openPort.Text == @"打开串口")
            {
                button_freshPort.Enabled = false;

                textBox_receive.Clear();
                OpenSelectedPort();
                //更改配置信息
                SetSerialPortPropertiesBeforeSending();
            }
            else
            {
                timer_Send2GetEcho.Stop();

                //清除队列中所有元素
                label28.Text = @"QueueLength Cleared";
                _queueEchoControl.Clear();
                retryCount = 0;

                CloseCurrentPort();
                button_freshPort.Enabled = true;
            }
        }

        private void button_freshPort_Click(object sender, EventArgs e)
        {
            comboBox_port.Items.Clear();
            var allAvailablePorts = SerialPort.GetPortNames();
            //判断是否有可用的端口
            if (allAvailablePorts.Length > 0)
            {
                hasPorts = true;
                //使能控件portNamesComboBox，openOrCloseButton
                button_openPort.Enabled = true;
                comboBox_port.Enabled = true;
                comboBox_baudrate.Enabled = true;
                comboBox_databit.Enabled = true;
                comboBox_parity.Enabled = true;
                comboBox_stopbit.Enabled = true;
                //依次添加可用的串口
                comboBox_port.Items.AddRange(allAvailablePorts);
                //默认选中第一个项
                comboBox_port.SelectedIndex = 0;
                //显示相应的状态信息
                //statusDisplayToolStripStatusLabel.Text = string.Format("  欢迎使用！自动查找到该计算机可用端口数：{0}，当前选中端口号{1}  :)",
                //    allAvailablePorts.Length, portNameComboBox.SelectedItem.ToString());
            }
            else
            {
                hasPorts = false;
                checkBox_sendAuto.Enabled = false;
                button_sendmessage.Enabled = false;
                //stopSendButton.Enabled = false;
                button_openPort.Enabled = false;
                //清空所有项
                comboBox_port.Items.Clear();
                comboBox_port.Enabled = false;
                //statusDisplayToolStripStatusLabel.Text = "  抱歉，未查找到当前计算机中可用端口。";
                //同时弹出警告对话框，提示是否进行再次检查？!
                ShowWarningMessageBox();
            }
        }

        private void button_countclear_Click(object sender, EventArgs e)
        {
            _totalSendBytes = 0;
            totalReceivedBytes = 0;

            label_receiveCount.Text = @"接受区计数：" + totalReceivedBytes;
            label_sendCount.Text = @"发送区计数：" + _totalSendBytes;
        }

        private void UpdateText(string text)
        {
            //if (showInfo)
            {
                textBox_receive.AppendText(text);

                if (textBox_receive.GetLineFromCharIndex(textBox_receive.TextLength) > 100) //超过100行清零
                {
                    textBox_receive.Clear();
                }

                if (checkBox_receiveAnotherline.Checked) //自动添加换行符
                    textBox_receive.Text += @"\r\n";

                label_receiveCount.Text = @"接受区计数：" + totalReceivedBytes;
            }
        }

        /// <summary>
        ///     每次从SerialPort接收数据时发生，由于运行在辅助线程
        ///     所以必须要通过委托来实现跨线程。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mySerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_closing) return; //如果正在关闭，忽略操作，直接返回，尽快的完成串口监听线程的一次循环  
            try
            {
                _listening = true;
                var sp = (SerialPort) sender;
                var text = string.Empty;
                var size = sp.BytesToRead;
                if (myReceivedDataType == ReceivedDataType.HexType)
                {
                    for (var i = 0; i < size; i++)
                    {
                        var tempByte = sp.ReadByte();
                        var tempStr = "0X";
                        if (tempByte <= 0X0F)
                        {
                            tempStr = "0X0";
                        }
                        text +=
                            tempStr + Convert.ToString(tempByte, 16).ToUpper() + " ";
                    }
                }
                else
                {
                    var n = sp.BytesToRead;
                    var builder = new StringBuilder();
                    var buf = new byte[n];
                    sp.Read(buf, 0, n);
                    builder.Append(Encoding.ASCII.GetString(buf));

                    text = builder.ToString();
                    RecievedStringAdd += text;

                    for (var i = 0; i < n; i++)
                    {
                        _recieveBuff.Add(buf.ElementAt(i));
                    }

                    //在这里判断是不是收到了合适的数据
                    //判断数据头尾
                    if (RecievedStringAdd.Contains("#|") && RecievedStringAdd.Contains("|$"))
                    {
                        var headCheck = RecievedStringAdd.IndexOf('#');
                        var endCheck = RecievedStringAdd.LastIndexOf('$');
                        if (headCheck != -1 && endCheck != -1 && endCheck - headCheck > 10)
                        {
                            if (InvokeRequired)
                            {
                                Invoke(new DoWorkUiThreadDelegate(UnPakegeReceived), RecievedStringAdd, _recieveBuff);
                            }
                            RecievedStringAdd = null;
                            _recieveBuff.Clear();
                        }
                    }
                }
                totalReceivedBytes += size;
                Invoke(UpdateTextHandler, text);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof (Form1), ex);
            }

            finally
            {
                _listening = false; //我用完了，ui可以关闭串口了。  
            }
        }

        #region 数据包格式解析部分

        public class GetEchoForm
        {
            public GetEchoForm()
            {
                Messgae = "";
                Flag = 0;
            }

            public GetEchoForm(int flag, string msg)
            {
                Flag = flag;
                Messgae = msg;
            }

            public string Messgae { get; set; }
            public int Flag { get; set; }
        }

        private void timer_Send2GetEcho_Tick(object sender, EventArgs e)
        {
            try
            {
                DequeueAndSendport();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void DequeueAndSendport()
        {
            if (tmpSendHandle.Flag == 1 && _queueEchoControl.Count > 0)
            {
                var send_message = (GetEchoForm) _queueEchoControl.Dequeue();
                tmpSendHandle = send_message;
            }

            if (tmpSendHandle.Flag == 0)
            {
                mySerialPort.Write(tmpSendHandle.Messgae);
                label28.Text = @"Loading..." + retryCount + @" Times" +
                               @"队列长度：" + _queueEchoControl.Count;
                retryCount++;
            }
        }

        private void UnPakegeReceived(string recvStr, List<int> recBuff)
        {
            // 1 2 3    45 6 7               8
            //#|1|3.31|0||1|1|P1200I2100D3310|$
            try
            {
                if (ReceiveValidateCheck(recvStr))
                {
                    var headCheck = recvStr.IndexOf("#|", StringComparison.Ordinal);
                    var endCheck = recvStr.LastIndexOf("|$", StringComparison.Ordinal);
                    var messagePack = recvStr.Substring(headCheck, endCheck - headCheck + 2);

                    var splittedMessage = messagePack.Split('|');

                    if (SingleMessageValidateCheck(splittedMessage))
                    {
                        DeliveryReceivedToControls(splittedMessage, messagePack, recBuff);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(Form1), ex);
            }
        }

        private bool ReceiveValidateCheck(string recvStr)
        {
            var headCheck = recvStr.IndexOf("#|", StringComparison.Ordinal);
            var endCheck = recvStr.LastIndexOf("|$", StringComparison.Ordinal);
            return headCheck != -1 && endCheck != -1;
        }

        private bool SingleMessageValidateCheck(string[] splittedMessage)
        {
            for (var i = 0; 4*(i + 1) < splittedMessage.Length; i++)
            {
                //加入是空字符串，舍弃本次
                if (splittedMessage.ElementAt(1 + 4*i).Trim() == "" ||
                    splittedMessage.ElementAt(2 + 4*i).Trim() == "")
                    return false;

                int father = Convert.ToInt16(splittedMessage.ElementAt(1 + 4*i));
                int child = Convert.ToInt16(splittedMessage.ElementAt(2 + 4*i));

                //表示不符合数据格式要求
                if ((father < 0 || father > 8) && (child < 0 || child > 200))
                {
                    return false;
                }
            }
            return true;
        }

        private void DeliveryReceivedToControls(string[] splittedMessage, string neededStr, List<int> recBuff)
        {
            var countLength = 0;
            const int EACH_MESSAGE_NUMBER = 4;
            countLength += neededStr.Where((t, i) => neededStr.ElementAt(i) == '|').Count();
            countLength = countLength/EACH_MESSAGE_NUMBER;

            for (var k = 0; k < countLength; k++)
            {
                int father = Convert.ToInt16(splittedMessage.ElementAt(1 + EACH_MESSAGE_NUMBER*k));

                if (father == 1 && checkBox_Camera_ONOFF.Checked)
                    //摄像头数据
                {
                    var cameraAlgorithm = new CameraAlgorithm(neededStr);

                    cameraAlgorithm.ApartMessage();

                    cameraAlgorithm.DrawCameraPicture(pictureBox_CameraActual);
                }
                else if (father == 2 && checkBox_CCD_ONOFF.Checked)
                    //CCD数据
                {
                    var ccdAlgorithm = new CCDAlgorithm(neededStr, recBuff);

                    ccdAlgorithm.ApartMessage();

                    var length = ccdAlgorithm.GetCCDLength();
                    label_CCD_Width.Text = @"CCD宽度：" + length;

                    int child = Convert.ToInt16(splittedMessage.ElementAt(2 + 4*k));
                    if (child == 1) //CCD1
                    {
                        ccdAlgorithm.DrawCCDPicture(pictureBox_CCD1);

                        //绘制路径曲线
                        ccdAlgorithm.DrawCCDPath(_bitmapOld, pictureBox_CCD_Path);
                    }
                    else if (child == 2) //CCD2
                    {
                        ccdAlgorithm.DrawCCDPicture(pictureBox_CCD2);
                    }
                    else if (child == 3) //CCD3
                    {
                        ccdAlgorithm.DrawCCDPicture(pictureBox_CCD3);
                    }
                }
                else if (father == 3) //实时参数
                {
                    int Elect_num = Convert.ToInt16(splittedMessage.ElementAt(2 + 4*k));
                    var realTimeValue = splittedMessage.ElementAt(3 + 4*k);

                    ReadRealtimeFromChip(Elect_num, realTimeValue);
                }
                else if (father == 4) //传送到示波器的数据
                {
                    int id = Convert.ToInt16(splittedMessage.ElementAt(2 + 4*k));
                    var value = Convert.ToDouble(splittedMessage.ElementAt(3 + 4*k))/1000;

                    ReadFromChipAndDrawToScope(id, value);
                }
                else if (father == 5) //读取参数值(PID)
                {
                    int type = Convert.ToInt16(splittedMessage.ElementAt(2 + 4*k));
                    var valuePID = splittedMessage.ElementAt(3 + 4*k);

                    ReadPIDFromChip(type, valuePID);
                }
                else if (father == 6) //读取参数值(CustomPara)
                {
                    int child = Convert.ToInt16(splittedMessage.ElementAt(2 + 4*k));
                    var valueCustomPara =
                        (Convert.ToDouble(splittedMessage.ElementAt(3 + 4*k))/1000.0).ToString();

                    ReadCustomParaFromChip(child, valueCustomPara);
                }
                else if (father == 7 || //PID返回的设置信息
                         father == 8 || //自定义参数返回的设置信息
                         father == 9) //紧急停车
                {
                    // ReSharper disable once InconsistentNaming
                    var isContainsACK = splittedMessage.ElementAt(3 + 4*k).Contains("ACK");

                    if (isContainsACK)
                    {
                        StopSendCurrentQueueAndUpdateState();
                    }
                }

                if (father == 5 || //PID返回的ECHO信息
                    father == 6) //自定义参数返回的ECHO信息
                {
                    StopSendCurrentQueueAndUpdateState();
                }
            }
        }

        private void ReadRealtimeFromChip(int electNum, string realTimeStr)
        {
            int totalContolsNum = Convert.ToInt16(textBox_Realtime_Number.Text);
            if (electNum <= totalContolsNum) //发送的传感器数据比现有的小
            {
                var txtBox = (TextBox)
                    panel_Electricity.Controls.Find(
                        "txtElectValue" + Convert.ToString(electNum), true)[0];

                //将参数赋值到相应的版块
                var value = Convert.ToDouble(realTimeStr)/1000;
                txtBox.Text = value.ToString();
            }
        }

        private void ReadFromChipAndDrawToScope(int id, double value)
        {
            var total = ScopeNumber;
            var checkDrawing = new CheckBox[ScopeNumber];

            if (_isLoadHistory)
            {
                ClearCurves();
            }

            InitCheckboxAndStartReflesh(checkDrawing, total);

            ChangeAxisAndPlotToScope(id, value, checkDrawing);
        }

        private void InitCheckboxAndStartReflesh(CheckBox[] checkDrawing, int total)
        {
            for (var i = 0; i < total; i++)
            {
                checkDrawing[i] =
                    (CheckBox)
                        panel_Scope.Controls.Find("checkBox_Def" + Convert.ToString(i + 1), true)[0];

                if (checkDrawing[i].Checked && !timer_fresh.Enabled)
                {
                    timer_fresh.Start();
                }
            }
        }

        private void ChangeAxisAndPlotToScope(int id, double value, CheckBox[] checkDrawing)
        {
            var total = ScopeNumber;
            for (var i = 0; i < total; i++)
            {
                if (checkDrawing[i].Checked &&
                    id == i + 1)
                {
                    zedGrpahNames[i].ValueZed = value;

                    ChangeAxis(i);
                    DrawScope(i, value);
                }
            }
        }

        private void ChangeAxis(int i)
        {
            if (_xmaxScale - zedGrpahNames[i].x < _xmaxScale/5) //改变XMax坐标轴范围
            {
                _xmaxScale += _xmaxScale/5;
                zedGraph_local.GraphPane.XAxis.Scale.Max = _xmaxScale;
            }
            if (_ymaxScale - zedGrpahNames[i].ValueZed < _ymaxScale/5) //改变YMax坐标轴范围
            {
                _ymaxScale += -_ymaxScale + 1.3*zedGrpahNames[i].ValueZed;
                zedGraph_local.GraphPane.YAxis.Scale.Max = _ymaxScale;
            }
            if (_yminScale - zedGrpahNames[i].ValueZed > _yminScale/5) //改变YMin坐标轴范围
            {
                _yminScale += _yminScale/5;
                zedGraph_local.GraphPane.YAxis.Scale.Min = _yminScale;
            }
        }

        private void DrawScope(int i, double value)
        {
            zedGrpahNames[i].x += 1;

            zedGrpahNames[i].listZed.Add(Convert.ToDouble(zedGrpahNames[i].x), value);
            zedGrpahNames[i].ZedPoint.ZedListX.Add(Convert.ToDouble(zedGrpahNames[i].x));
            zedGrpahNames[i].ZedPoint.ZedListY.Add(value);

            if (zedGrpahNames[i].IsSingleWindowShowed)
                coOb[i].CallEvent(zedGrpahNames[i].x, value);
        }

        private void ClearCurves()
        {
            _isLoadHistory = false;
            for (var i = 0; i < ScopeNumber; i++) //首先清除所有的曲线数据
            {
                zedGrpahNames[i].listZed.RemoveRange(0, zedGrpahNames[i].listZed.Count);
                zedGrpahNames[i].x = -1;
            }
        }

        private void ReadPIDFromChip(int type, string PIDStrings)
        {
            var indexP = PIDStrings.IndexOf('P');
            var indexI = PIDStrings.IndexOf("I", StringComparison.Ordinal);
            var indexD = PIDStrings.IndexOf("D", StringComparison.Ordinal);

            if (indexP != -1 && indexI != -1 && indexD != -1)
            {
                var valueP =
                    (Convert.ToInt16(PIDStrings.Substring(indexP + 1, indexI - indexP - 1))/1000.0)
                        .ToString(CultureInfo.InvariantCulture);
                var valueI =
                    (Convert.ToInt16(PIDStrings.Substring(indexI + 1, indexD - indexI - 1))/1000.0)
                        .ToString(CultureInfo.InvariantCulture);
                var valueD =
                    (Convert.ToInt16(PIDStrings.Substring(indexD + 1))/1000.0).ToString(
                        CultureInfo.InvariantCulture);

                if (type == 1)
                {
                    textBox_Steer_P.Text = valueP;
                    textBox_Steer_I.Text = valueI;
                    textBox_Steer_D.Text = valueD;
                }
                else if (type == 2)
                {
                    textBox_Motor_P.Text = valueP;
                    textBox_Motor_I.Text = valueI;
                    textBox_Motor_D.Text = valueD;
                }
                else if (type == 3)
                {
                    textBox_Stand_P.Text = valueP;
                    textBox_Stand_I.Text = valueI;
                    textBox_Stand_D.Text = valueD;
                }
                else if (type == 4)
                {
                    textBox_Speed_P.Text = valueP;
                    textBox_Speed_I.Text = valueI;
                    textBox_Speed_D.Text = valueD;
                }
                else if (type == 5)
                {
                    textBox_Direction_P.Text = valueP;
                    textBox_Direction_I.Text = valueI;
                    textBox_Direction_D.Text = valueD;
                }
            }
        }

        private void ReadCustomParaFromChip(int child, string CustomParaStrings)
        {
            var total = Convert.ToInt32(textBox_DIY_Number.Text);

            if (child < total)
            {
                var checkboxSelect = (CheckBox) panel_add_DIYControls.Controls.Find("checkBox_Def" +
                                                                                    Convert.ToString(child), true)[0];
                var txtBox = (TextBox) panel_add_DIYControls.Controls.Find("txtValue" +
                                                                           Convert.ToString(child), true)[0];

                if (checkboxSelect.Checked)
                {
                    txtBox.Text = CustomParaStrings;
                }
            }
        }

        private void StopSendCurrentQueueAndUpdateState()
        {
            if (_queueEchoControl.Count == 0) //队列中无元素
            {
                tmpSendHandle.Flag = 1;
                timer_Send2GetEcho.Stop();
            }
            else
            {
                tmpSendHandle.Flag = 1;
            }

            label28.Text = "OK!! + Remain:" + _queueEchoControl.Count;
            retryCount = 0;
        }

        #endregion

        private void CloseCurrentPort()
        {
            _autoSend = false;
            checkBox_sendAuto.Checked = false;

            _closing = true;
            while (_listening) Application.DoEvents();

            try
            {
                //关闭选中串口
                mySerialPort.Close();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
            //do something
            button_openPort.Text = "打开串口";
            //更新状态栏的显示
            //statusDisplayToolStripStatusLabel.Text = string.Format("  关闭端口 {0}成功！",
            //    mySerialPort.PortName);
            //所有设置控件非使能态
            CloseSelectedPortSuccessfully();
        }

        private void CloseSelectedPortSuccessfully()
        {
            comboBox_port.Enabled = true;
            textBox_receive.Enabled = true;
            textBox_send.Enabled = true;
            comboBox_baudrate.Enabled = true;
            comboBox_databit.Enabled = true;
            comboBox_stopbit.Enabled = true;
            comboBox_parity.Enabled = true;
            //sendSettingGroupBox.Enabled = false;
            totalReceivedBytes = 0;
            _totalSendBytes = 0;
        }

        private void SetSerialPortPropertiesBeforeSending()
        {
            mySerialPort.Encoding = Encoding.Default;
            //设置成为选中的波特率
            mySerialPort.BaudRate = GetSelectedBaudRate();
            //设置成为选中的奇偶校验位
            mySerialPort.Parity = GetSelectedParity();
            //设置成为选中的数据位
            mySerialPort.DataBits = GetSelectedDataBits();
            //设置成为选中的端口停止位
            try
            {
                mySerialPort.StopBits = GetSelectedStopBits();
            }
            catch (IOException ee)
            {
                MessageBox.Show(ee.Message + "已经将 停止位 设置为 默认一位 了！",
                    "提示！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                mySerialPort.StopBits = StopBits.One;
                comboBox_stopbit.SelectedItem = "1";
            }
        }

        private StopBits GetSelectedStopBits()
        {
            var stopBits = StopBits.One;
            switch (comboBox_stopbit.SelectedItem.ToString())
            {
                case "1":
                {
                    stopBits = StopBits.One;
                }
                    break;
                case "2":
                {
                    stopBits = StopBits.Two;
                }
                    break;
                case "1.5":
                {
                    stopBits = StopBits.OnePointFive;
                }
                    break;
                default:
                    stopBits = StopBits.One;
                    break;
            }
            return stopBits;
        }

        private int GetSelectedBaudRate()
        {
            var baudRate = 0;
            if (!int.TryParse(comboBox_baudrate.SelectedItem.ToString(), out baudRate))
            {
                baudRate = 9600;
            }
            return baudRate;
        }

        private int GetSelectedDataBits()
        {
            var dataBits = 8;
            if (!int.TryParse(comboBox_databit.SelectedItem.ToString(), out dataBits))
            {
                MessageBox.Show("转换失败！");
            }
            return dataBits;
        }

        private Parity GetSelectedParity()
        {
            var parity = Parity.None;
            var selectedParityWay = comboBox_parity.SelectedItem.ToString();
            switch (selectedParityWay)
            {
                case "偶校验(Even)":
                {
                    parity = Parity.Even;
                }
                    break;
                case "奇校验(Odd)":
                {
                    parity = Parity.Odd;
                }
                    break;
                case "保留为0(Space)":
                {
                    parity = Parity.Space;
                }
                    break;
                case "保留为1(Mark)":
                {
                    parity = Parity.Mark;
                }
                    break;
                default:
                {
                    parity = Parity.None;
                }
                    break;
            }
            return parity;
        }

        private void OpenSelectedPort()
        {
            _closing = false;
            try
            {
                //if(mySerialPort == null)
                //    mySerialPort = new SerialPort();

                //设置打开的端口号
                mySerialPort.PortName = comboBox_port.SelectedItem.ToString();
                //打开选中串口
                mySerialPort.Open();

                mySerialPort.NewLine = "/r/n";
                mySerialPort.RtsEnable = true;
                mySerialPort.DataReceived += mySerialPort_DataReceived;

                //更新状态栏的显示
                //statusDisplayToolStripStatusLabel.Text = string.Format("  打开端口 {0}成功！",
                //    mySerialPort.PortName);
                button_openPort.Text = "关闭串口";
                //打开串口成功后
                OpenSelectedPortSuccessfully();
            }
            catch (Exception ee)
            {
                mySerialPort = new SerialPort();
                MessageBox.Show(ee.Message);
            }
        }

        private void OpenSelectedPortSuccessfully()
        {
            comboBox_port.Enabled = false;
            comboBox_databit.Enabled = false;
            comboBox_stopbit.Enabled = false;
            comboBox_parity.Enabled = false;
            comboBox_baudrate.Enabled = false;
            groupBox_send.Enabled = true;
            //acceptZoneSettingGroupBox.Enabled = true;
            //sendZoneSettingGroupBox.Enabled = true;
            textBox_receive.Enabled = true;
            textBox_send.Enabled = true;
        }

        private bool hasPorts;

        /// <summary>
        ///     获取可用的端口名，并添加到选择框中，同时设置相关
        ///     默认项。
        /// </summary>
        private void CheckAvailablePorts()
        {
            comboBox_port.Items.Clear();
            var allAvailablePorts = SerialPort.GetPortNames();
            //判断是否有可用的端口
            if (allAvailablePorts.Length > 0)
            {
                hasPorts = true;
                //使能控件portNamesComboBox，openOrCloseButton
                button_openPort.Enabled = true;
                comboBox_port.Enabled = true;
                //依次添加可用的串口
                comboBox_port.Items.AddRange(allAvailablePorts);
                //默认选中第一个项
                comboBox_port.SelectedIndex = 0;
                //显示相应的状态信息
                //statusDisplayToolStripStatusLabel.Text = string.Format("  欢迎使用！自动查找到该计算机可用端口数：{0}，当前选中端口号{1}  :)",
                //    allAvailablePorts.Length, portNameComboBox.SelectedItem.ToString());
            }
            else
            {
                hasPorts = false;
                checkBox_sendAuto.Enabled = false;
                button_sendmessage.Enabled = false;
                //stopSendButton.Enabled = false;
                button_openPort.Enabled = false;
                //清空所有项
                comboBox_port.Items.Clear();
                comboBox_port.Enabled = false;
                //statusDisplayToolStripStatusLabel.Text = "  抱歉，未查找到当前计算机中可用端口。";
                //同时弹出警告对话框，提示是否进行再次检查？!
                ShowWarningMessageBox();
            }
        }

        private void ShowWarningMessageBox()
        {
            var result = new DialogResult();
            result = MessageBox.Show("抱歉，没有检测到当前计算机中可用端口，请插入相关设备或者检查有关驱动是否安装？" +
                                     Environment.NewLine + "提示：您可以取消后单击“查找可用端口”按钮重新查找。",
                "自动查找计算机可用端口", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Retry)
            {
                //重新运行检测方法
                CheckAvailablePorts();
            }
        }

        private void button_receivepause_Click(object sender, EventArgs e)
        {
            if (button_receivepause.Text == "暂停接收显示")
            {
                showInfo = false;
                button_receivepause.Text = "继续接收显示";
            }
            else
            {
                showInfo = true;
                button_receivepause.Text = "暂停接收显示";
                //
            }
        }

        private void textBox_receive_TextChanged(object sender, EventArgs e)
        {
            //textBox_receive.SelectionStart = textBox_receive.Text.Length;

            //textBox_receive.ScrollToCaret();
        }

        private void button_receiveclear_Click(object sender, EventArgs e)
        {
            textBox_receive.Clear();
            //acceptStatusLabel.Text = "0字节";
            totalReceivedBytes = 0;
        }

        private void checkBox_receiveHex_CheckedChanged(object sender, EventArgs e)
        {
            textBox_receive.Clear();
            var rb = sender as CheckBox;
            if (rb != null)
            {
                if (!rb.Checked)
                {
                    myReceivedDataType = ReceivedDataType.CharType;
                }
                else
                {
                    myReceivedDataType = ReceivedDataType.HexType;
                }
            }
        }

        private void button_sendmessage_Click(object sender, EventArgs e)
        {
            if (textBox_send.TextLength != 0)
            {
                if (checkBox_sendAuto.Checked)
                {
                    _autoSend = true;
                    checkBox_sendAuto.Enabled = false;
                    button_sendmessage.Text = "自动发送中";

                    timer_autoSend.Enabled = true;
                    timer_autoSend.Interval = Convert.ToInt32(textBox_sendPeriod.Text);
                    timer_autoSend.Start();
                }
                else
                {
                    _autoSend = false;
                    checkBox_sendAuto.Enabled = true;
                    button_sendmessage.Text = "发送数据";
                    try
                    {
                        SerialPortSendChar(textBox_send.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("发送区为空");
            }
        }

        private void SerialPortSendChar(string str)
        {
            var ch = str.ToCharArray();
            foreach (var item in ch)
            {
                if (item <= 255)
                {
                    _totalSendBytes += 1;
                }
                else
                {
                    _totalSendBytes += 2;
                }
            }
            if (mySerialPort == null || mySerialPort.IsOpen == false)
            {
                MessageBox.Show("拒绝操作，没有任何串口被开启，无法发送数据！", "发送数据");
            }
            else
            {
                try
                {
                    mySerialPort.Write(ch, 0, ch.Length);
                    label_sendCount.Text = "发送区计数：" + _totalSendBytes;
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }
            }
        }

        private void timer_autoSend_Tick(object sender, EventArgs e)
        {
            if (_autoSend)
            {
                try
                {
                    SerialPortSendChar(textBox_send.Text);
                }
                catch
                {
                }
            }
        }

        private void checkBox_sendAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (textBox_send.TextLength == 0)
            {
                checkBox_sendAuto.Checked = false;
                _autoSend = false;
                MessageBox.Show("没有需要发送的内容 >_<");
            }
        }

        #endregion

        #region PID参数设置 2

        private void Init_TablePagePIDSettings()
        {
            //车的类型选择
            if (radioButton_FourWheel.Checked)
            {
                groupBox_BalanceCar.Visible = false;
                groupBox_fourWheels.Visible = true;
            }
            if (radioButton_BalanceCar.Checked)
            {
                groupBox_BalanceCar.Visible = true;
                groupBox_fourWheels.Visible = false;
            }
        }

        private void checkBox_IsUsePID_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_IsUsePID.Checked)
            {
                groupBox_BalanceCar.Enabled = true;
                groupBox_fourWheels.Enabled = true;
            }
            else
            {
                groupBox_BalanceCar.Enabled = false;
                groupBox_fourWheels.Enabled = false;
            }
        }

        private void radioButton_FourWheel_Click(object sender, EventArgs e)
        {
            var rb = (RadioButton) sender;
            if (rb.Checked)
            {
                groupBox_BalanceCar.Visible = false;
                groupBox_fourWheels.Visible = true;
            }
            else
            {
                groupBox_BalanceCar.Visible = true;
                groupBox_fourWheels.Visible = false;
            }
        }

        private void radioButton_BalanceCar_CheckedChanged(object sender, EventArgs e)
        {
            var rb = (RadioButton) sender;
            if (!rb.Checked)
            {
                groupBox_BalanceCar.Visible = false;
                groupBox_fourWheels.Visible = true;
            }
            else
            {
                groupBox_BalanceCar.Visible = true;
                groupBox_fourWheels.Visible = false;
            }
        }

        //生成需要发送控制指令的字符串

        private void button_ModifyPID_Motor_Click(object sender, EventArgs e)
        {
            //电机PID参数
            var Motor_P = Convert.ToDouble(textBox_Motor_P.Text)*1000;
            var Motor_I = Convert.ToDouble(textBox_Motor_I.Text)*1000;
            var Motor_D = Convert.ToDouble(textBox_Motor_D.Text)*1000;


            var tmpMessageSteer = "P" + Math.Floor(Motor_P) +
                                  "I" + Math.Floor(Motor_I) +
                                  "D" + Math.Floor(Motor_D);

            try
            {
                SaveConfig(SavefileName);
                SendMessageAndEnqueue(1, 2, tmpMessageSteer);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void button_ModifyPID_Steer_Click(object sender, EventArgs e)
        {
            //舵机PID参数
            var steer_P = Convert.ToDouble(textBox_Steer_P.Text)*1000;
            var steer_I = Convert.ToDouble(textBox_Steer_I.Text)*1000;
            var steer_D = Convert.ToDouble(textBox_Steer_D.Text)*1000;


            var tmpMessageSteer = "P" + Math.Floor(steer_P).ToString(CultureInfo.InvariantCulture) +
                                  "I" + Math.Floor(steer_I).ToString(CultureInfo.InvariantCulture) +
                                  "D" + Math.Floor(steer_D).ToString(CultureInfo.InvariantCulture);

            try
            {
                SaveConfig(SavefileName);
                SendMessageAndEnqueue(1, 1, tmpMessageSteer);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void button_ModifyPID_Stand_Click(object sender, EventArgs e)
        {
            //直立PID
            var stand_P = Convert.ToDouble(textBox_Stand_P.Text)*1000;
            var stand_I = Convert.ToDouble(textBox_Stand_I.Text)*1000;
            var stand_D = Convert.ToDouble(textBox_Stand_D.Text)*1000;

            var tmpMessageStand = "P" + Math.Floor(stand_P) +
                                  "I" + Math.Floor(stand_I) +
                                  "D" + Math.Floor(stand_D);

            try
            {
                SaveConfig(SavefileName);
                SendMessageAndEnqueue(1, 3, tmpMessageStand);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void button_ModifyPID_Speed_Click(object sender, EventArgs e)
        {
            //速度PID
            var speed_P = Convert.ToDouble(textBox_Speed_P.Text)*1000;
            var speed_I = Convert.ToDouble(textBox_Speed_I.Text)*1000;
            var speed_D = Convert.ToDouble(textBox_Speed_D.Text)*1000;

            var tmpMessageSpeed = "P" + Math.Floor(speed_P) +
                                  "I" + Math.Floor(speed_I) +
                                  "D" + Math.Floor(speed_D);
            try
            {
                SaveConfig(SavefileName);
                SendMessageAndEnqueue(1, 4, tmpMessageSpeed);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void button_ModifyPID_Direction_Click(object sender, EventArgs e)
        {
            //方向PID
            var direction_P = Convert.ToDouble(textBox_Direction_P.Text)*1000;
            var direction_I = Convert.ToDouble(textBox_Direction_I.Text)*1000;
            var direction_D = Convert.ToDouble(textBox_Direction_D.Text)*1000;

            var tmpMessageDirection = "P" + Math.Floor(direction_P) +
                                      "I" + Math.Floor(direction_I) +
                                      "D" + Math.Floor(direction_D);

            try
            {
                SaveConfig(SavefileName);
                SendMessageAndEnqueue(1, 5, tmpMessageDirection);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        #endregion

        #region 3. 自定义算法参数

        private void button_DIY_NumConfirm_Click(object sender, EventArgs e)
        {
            if (isCustomParaValidate())
            {
                var result = MessageBox.Show(@"确认更改数量？", @"修改", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    List<bool> state = new List<bool>();
                    List<string> names = new List<string>();
                    List<string> value = new List<string>();
                    GetCustomOldNames(state, names, value);

                    DrawCustomParaPannel(false);

                    SetCustomNewNames(state, names, value);
                }
                SaveConfig(SavefileName);
            }
        }

        private void button_DIY_ResetNames_Click(object sender, EventArgs e)
        {
            if (isCustomParaValidate())
            {
                var result = MessageBox.Show(@"确认更改数量？", @"修改", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    DrawCustomParaPannel(false);
                }
                SaveConfig(SavefileName);
            }
        }

        private void GetCustomOldNames(List<bool> state, List<string> names, List<string> value)
        {
            SharedPreferences sp = new SharedPreferences(SavefileName);
            int oldNumber = 0;
            if(sp.ConfigFileExists)
                oldNumber = sp.GetInt32("DIY_Number", 1);

            for (var i = 0; i < oldNumber; i++)
            {
                var checkboxSelect = (CheckBox)panel_add_DIYControls.Controls.Find("checkBox_Def" +
                    Convert.ToString(i + 1), true)[0];
                state.Add(checkboxSelect.Checked);

                var txtBoxValue = (TextBox) panel_add_DIYControls.Controls.Find("txtValue" +
                    Convert.ToString(i + 1), true)[0];
                value.Add(txtBoxValue.Text);

                var txtBoxName = (TextBox)panel_add_DIYControls.Controls.Find("txtName" +
                    Convert.ToString(i + 1), true)[0];
                names.Add(txtBoxName.Text);
            }
        }

        private void SetCustomNewNames(List<bool> state, List<string> names, List<string> value)
        {
            var total = int.Parse(textBox_DIY_Number.Text);

            if (state.Count < total)
            {
                total = state.Count;
            }

            for (var i = 0; i < total; i++)
            {
                var checkboxSelect = (CheckBox)panel_add_DIYControls.Controls.Find("checkBox_Def" +
                    Convert.ToString(i + 1), true)[0];
                checkboxSelect.Checked = state[i];

                var txtBoxValue = (TextBox)panel_add_DIYControls.Controls.Find("txtValue" +
                    Convert.ToString(i + 1), true)[0];
                txtBoxValue.Text = value[i];

                var txtBoxName = (TextBox)panel_add_DIYControls.Controls.Find("txtName" +
                    Convert.ToString(i + 1), true)[0];
                txtBoxName.Text = names[i];
            }
        }

        private void Init_CustomPara_DynamicControls()
        {
            var sp = new SharedPreferences(SavefileName);
            if (sp.ConfigFileExists && isCustomParaValidate())
            {
                DrawCustomParaPannel(true);
            }
        }

        private void DrawCustomParaPannel(bool byInit)
        {
            panel_add_DIYControls.AutoScroll = true;
            panel_add_DIYControls.Controls.Clear();
            var total = int.Parse(textBox_DIY_Number.Text);

            var sp = new SharedPreferences(SavefileName);
            if (sp.ConfigFileExists)
            {
                AddControlsToPannel(sp, 0, total, byInit);
            }
        }

        private Boolean isCustomParaValidate()
        {
            var total = int.Parse(textBox_DIY_Number.Text);
            if (total < 1)
            {
                MessageBox.Show(@"请输入需要的参数数量！", @"错误");
                return false;
            }
            return true;
        }

        private void textBox_DIY_Number_TextChanged(object sender, EventArgs e)
        {
            var t = sender as TextBox;

            if (t != null && !IsIntegerCheck(t.Text))
                t.Text = @"10";
        }

        private void AddControlsToPannel(SharedPreferences sp, int start, int end, bool byInit)
        {
            for (var i = start; i < end; i++)
            {
                var checkboxSelect = new CheckBox();
                checkboxSelect.Size = new Size(50, 20);
                checkboxSelect.Location = new Point(30 + 70*0, 30*(i + 1));
                checkboxSelect.Name = "checkBox_Def" + Convert.ToString(i + 1);
                checkboxSelect.Text = (i + 1).ToString();
                if (byInit)
                {
                    var checkState = string.Format("DIY_CheckState{0}", i + 1);
                    checkboxSelect.Checked = sp.GetBoolean(checkState, true);
                }
                checkboxSelect.CheckedChanged += CheckboxSelectOnCheckedChanged;

                var txtBoxName = new TextBox();
                txtBoxName.Size = new Size(50, 50); //textbox大小                   
                txtBoxName.Location = new Point(30 + 70*1, 30*(i + 1));
                txtBoxName.Name = @"txtName" + Convert.ToString(i + 1);
                txtBoxName.Text = @"Name" + Convert.ToString(i + 1);
                txtBoxName.TextAlign = HorizontalAlignment.Center;
                if (byInit)
                {
                    var txtName_CustomPara = string.Format("DIY_TextName{0}", i + 1);
                    txtBoxName.Text = sp.GetString(txtName_CustomPara, "数据");
                }
                else
                {
                    txtBoxName.Enabled = false;
                }

                var txtBoxValue = new TextBox();
                txtBoxValue.Size = new Size(50, 50); //textbox大小                   
                txtBoxValue.Location = new Point(30 + 70*2, 30*(i + 1));
                txtBoxValue.Name = "txtValue" + Convert.ToString(i + 1);
                txtBoxValue.Text = @"1.0";
                txtBoxValue.TextAlign = HorizontalAlignment.Center;
                if (byInit)
                {
                    var txtValue_CustomPara = string.Format("DIY_TextValue{0}", i + 1);
                    txtBoxValue.Text = sp.GetString(txtValue_CustomPara, "1.0");
                }
                else
                {
                    txtBoxValue.Enabled = false;
                }
                txtBoxValue.TextChanged += DIYTextboxValueChanged;

                var submitButton = new Button();
                submitButton.Size = new Size(50, 20); //textbox大小                   
                submitButton.Location = new Point(30 + 70*3, 30*(i + 1));
                submitButton.Name = "buttonSubmit" + Convert.ToString(i + 1);
                submitButton.Text = @"修改";
                submitButton.Click += SubmitButtonOnClick;
                if (!byInit)
                {
                    submitButton.Enabled = false;
                }

                if (!checkboxSelect.Checked)
                {
                    txtBoxName.Enabled = false;
                    txtBoxValue.Enabled = false;
                    submitButton.Enabled = false;
                }

                panel_add_DIYControls.Controls.Add(checkboxSelect);
                panel_add_DIYControls.Controls.Add(txtBoxName);
                panel_add_DIYControls.Controls.Add(txtBoxValue);
                panel_add_DIYControls.Controls.Add(submitButton);
            }
        }

        private void SubmitButtonOnClick(object sender, EventArgs eventArgs)
        {
            var btnClk = (Button)sender;

            var id = GetNumber(btnClk.Name) - 1;

            var txtBox = (TextBox)panel_add_DIYControls.Controls.Find("txtValue" +
                Convert.ToString(id + 1), true)[0];

            var value = Convert.ToDouble(txtBox.Text) * 1000.0;
            try
            {
                var tmpMessageStand = Math.Floor(value).ToString();
                SendMessageAndEnqueue(2, id+1, tmpMessageStand);

                label28.Text = "Loading...";
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void DIYTextboxValueChanged(object sender, EventArgs eventArgs)
        {
            var t = sender as TextBox;

            if (t != null && !IsFloatCheck(t.Text))
                t.Text = @"1";
        }

        private void CheckboxSelectOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            if (isCustomParaValidate())
            {
                var total = Convert.ToInt32(textBox_DIY_Number.Text);
                for (var i = 0; i < total; i++)
                {
                    var checkboxSelect = (CheckBox)panel_add_DIYControls.Controls.Find("checkBox_Def" + 
                        Convert.ToString(i + 1), true)[0];

                    var txtBox = new TextBox[2]; //用控件数组来定义每一行的TextBox,总共3个TextBox
                    txtBox[0] = (TextBox)panel_add_DIYControls.Controls.Find("txtName" + 
                        Convert.ToString(i + 1), true)[0];
                    txtBox[1] = (TextBox)panel_add_DIYControls.Controls.Find("txtValue" + 
                        Convert.ToString(i + 1), true)[0];

                    var btn = (Button)panel_add_DIYControls.Controls.Find("buttonSubmit" + 
                        Convert.ToString(i + 1), true)[0];

                    if (checkboxSelect.Checked)
                    {
                        txtBox[0].Enabled = true;
                        txtBox[1].Enabled = true;
                        btn.Enabled = true;
                    }
                    else
                    {
                        txtBox[0].Enabled = false;
                        txtBox[1].Enabled = false;
                        btn.Enabled = false;
                    }
                }
            }

        }

        private void button_DIY_Modify_Click(object sender, EventArgs e)
        {
            //=====================================
            //数据头和封包
            var NeedSend = "";
            var head = "#";
            var end = "$";
            //=====================================
            try
            {
                var combinedString = SendCustomParaAll();
                NeedSend = head + combinedString + end;

                mySerialPort.Write(NeedSend);

                var tmpHandle = new GetEchoForm(0, NeedSend);
                _queueEchoControl.Enqueue(tmpHandle);
                timer_Send2GetEcho.Start();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private string SendCustomParaAll()
        {
            var NeedSend = "";
            var total = Convert.ToInt32(textBox_DIY_Number.Text);
            for (var i = 0; i < total; i++)
            {
                var checkboxSelect = (CheckBox) panel_add_DIYControls.Controls.Find("checkBox_Def" +
                                                                                    Convert.ToString(i + 1), true)[0];

                var txtBox = new TextBox[2]; //用控件数组来定义每一行的TextBox,总共3个TextBox
                txtBox[0] = (TextBox) panel_add_DIYControls.Controls.Find("txtName" + Convert.ToString(i + 1), true)[0];
                txtBox[1] = (TextBox) panel_add_DIYControls.Controls.Find("txtValue" + Convert.ToString(i + 1), true)[0];

                var tmpMessageStand = txtBox[1].Text;

                if (checkboxSelect.Checked)
                {
                    NeedSend += FormPackage_NOHead_NOEnd(2, i + 1, tmpMessageStand);
                }
            }
            return NeedSend;
        }

        #endregion

        #region 4.实时变量参数

        private void InitRealtime()
        {
            var sp = new SharedPreferences(SavefileName);
            if (sp.ConfigFileExists && isRealTimeValide())
            {
                DrawRealTimePannel(sp, true);
            }
        }

        private void button_electricity_NumConfirm_Click(object sender, EventArgs e)
        {
            if (isRealTimeValide())
            {
                var sp = new SharedPreferences(SavefileName);
                if (sp.ConfigFileExists)
                {
                    DrawRealTimePannel(sp, false);
                }
                SaveConfig(SavefileName);
            }
        }

        private void DrawRealTimePannel(SharedPreferences sp, bool byInit)
        {
            panel_Electricity.AutoScroll = true;
            panel_Electricity.Controls.Clear();
            var total = int.Parse(textBox_Realtime_Number.Text);

            for (var i = 0; i < total; i++)
            {
                var labelElect = new TextBox();
                labelElect.Size = new Size(100, 20); //label大小                   
                labelElect.Location = new Point(30 + 120*0, 30*(i + 1));
                labelElect.Name = "txtElectName" + Convert.ToString(i + 1);
                labelElect.Text = @"实时变量" + Convert.ToString(i + 1);
                labelElect.TextAlign = HorizontalAlignment.Center;
                if (byInit)
                {
                    var elecName = string.Format("textElectName{0}", i + 1);
                    labelElect.Text = sp.GetString(elecName, "1.0");
                }

                var txtBoxValue = new TextBox();
                txtBoxValue.Size = new Size(50, 50);
                txtBoxValue.Location = new Point(30 + 120*1, 30*(i + 1));
                txtBoxValue.Name = "txtElectValue" + Convert.ToString(i + 1);
                txtBoxValue.TextAlign = HorizontalAlignment.Center;
                if (byInit)
                {
                    var elecValue = string.Format("txtElectValue{0}", i + 1);
                    txtBoxValue.Text = sp.GetString(elecValue, "1.0");
                }

                panel_Electricity.Controls.Add(labelElect);
                panel_Electricity.Controls.Add(txtBoxValue);
            }
        }

        private void textBox_Realtime_Number_TextChanged(object sender, EventArgs e)
        {
            var t = sender as TextBox;

            if (t != null && !IsIntegerCheck(t.Text))
                t.Text = @"10";
        }
        
        private bool isRealTimeValide()
        {
            int number = Convert.ToInt16(textBox_Realtime_Number.Text);
            if (number < 1)
            {
                MessageBox.Show(@"请输入需要的参数数量！", @"错误");
                return false;
            }
            return true;
        }

        #endregion

        #region 7.虚拟示波器

        private void InitzedGraph()
        {
            var myPane = zedGraph_local.GraphPane;
            myPane.Title.IsVisible = false;
            myPane.XAxis.Title.Text = "time";
            myPane.YAxis.Title.Text = "Value";

            //show grid
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;

            // Align the Y axis labels so they are flush to the axis
            myPane.YAxis.Scale.Align = AlignP.Inside;

            // Manually set the axis range
            myPane.XAxis.Scale.Min = _xminScale;
            myPane.XAxis.Scale.Max = _xmaxScale;
            myPane.YAxis.Scale.Min = _yminScale;
            myPane.YAxis.Scale.Max = _ymaxScale;

            // Fill the axis background with a gradient
            myPane.Chart.Fill = new Fill(Color.White, Color.LightGray, 45.0f);

            // OPTIONAL: Show tooltips when the mouse hovers over a point
            zedGraph_local.IsShowPointValues = true;
            zedGraph_local.PointValueEvent += MyPointValueHandler;

            for (var i = 0; i < ScopeNumber; i++)
            {
                zedGrpahNames[i] = new ZedGrpahName();
                coOb[i] = new CallObject();
            }

            var sp = new SharedPreferences(SavefileName);
            for (var i = 0; i < ScopeNumber; i++)
            {
                zedGrpahNames[i].listZed.Add(Convert.ToDouble(zedGrpahNames[i].x), 0);
                var curve = string.Format("波形{0}", i + 1);

                //自己定义的变量名称
                var txtName_CustomPara = string.Format("SCOPE_TextName{0}", i + 1);
                var getName = sp.GetString(txtName_CustomPara, @"波形" + Convert.ToString(i + 1));
                zedGraph_local.GraphPane.AddCurve(getName.Trim() != "" ? getName : curve, zedGrpahNames[i].listZed,
                    _colorLine[i%8],
                    SymbolType.None);
            }

            refleshZedPane(zedGraph_local);
        }

        private void Init_pane_Scope()
        {
            var sp = new SharedPreferences(SavefileName);

            panel_Scope.AutoScroll = true;
            var total = ScopeNumber;
            for (var i = 0; i < total; i++)
            {
                var checkDrawing = new CheckBox();
                checkDrawing.Size = new Size(15, 15);
                checkDrawing.Location = new Point(20 + i%8*77, 10 + i/8*60);
                checkDrawing.Name = "checkBox_Def" + Convert.ToString(i + 1);
                checkDrawing.Text = "";

                var checkState = string.Format("SCOPE_CheckState{0}", i + 1);
                checkDrawing.Checked = sp.GetBoolean(checkState, false);
                checkDrawing.CheckedChanged += CheckDrawingOnCheckedChanged;

                var txtBoxName = new TextBox();
                txtBoxName.Size = new Size(50, 20); //textbox大小                   
                txtBoxName.Location = new Point(41 + i%8*77, 7 + i/8*60);
                txtBoxName.Name = "txtName" + Convert.ToString(i + 1);
                txtBoxName.TextAlign = HorizontalAlignment.Center;
                var txtName_CustomPara = string.Format("SCOPE_TextName{0}", i + 1);
                var getName = sp.GetString(txtName_CustomPara, @"波形" + Convert.ToString(i + 1));
                txtBoxName.Text = getName.Trim() != "" ? getName : string.Format("波形{0}", i + 1);
                txtBoxName.BackColor = _colorLine[i%8];
                txtBoxName.TextChanged += ScopeTextNameChanged;
                txtBoxName.Enabled = checkDrawing.Checked;

                var buttonNewForm = new Button();
                buttonNewForm.Size = new Size(66, 23);
                buttonNewForm.Location = new Point(25 + i%8*77, 34 + i/8*60);
                buttonNewForm.Name = "buttonDraw" + Convert.ToString(i + 1);
                buttonNewForm.Text = @"独立图像";
                buttonNewForm.Click += ButtonNewFormOnClick;
                buttonNewForm.Enabled = checkDrawing.Checked;

                panel_Scope.Controls.Add(checkDrawing);
                panel_Scope.Controls.Add(txtBoxName);
                panel_Scope.Controls.Add(buttonNewForm);
            }
        }

        private void timer_fresh_Tick(object sender, EventArgs e)
        {
            refleshZedPane(zedGraph_local);
        }

        private string MyPointValueHandler(ZedGraphControl control, GraphPane pane,
            CurveItem curve, int iPt)
        {
            var pt = curve[iPt];
            return curve.Label.Text + " is " + pt.Y.ToString("f2") + " units at " + pt.X.ToString("f1");
        }

        private void refleshZedPane(ZedGraphControl zedGraph)
        {
            zedGraph.AxisChange();
            zedGraph.Invalidate();
        }

        private void ButtonNewFormOnClick(object sender, EventArgs eventArgs)
        {
            var btnClk = (Button) sender;

            var id = GetNumber(btnClk.Name) - 1;
            if (zedGrpahNames[id].IsSingleWindowShowed == false)
            {
                zedGrpahNames[id].IsSingleWindowShowed = true;

                FindTextboxAndShowWindow(id);
            }
            else
            {
                MessageBox.Show(@"不能重复创建！已经有了一个窗口！");
            }
        }

        private void FindTextboxAndShowWindow(int id)
        {
            var txtBox = (TextBox) panel_Scope.Controls.Find("txtName" +
                                                             Convert.ToString(id + 1), true)[0];

            var singleWindow = new ZedGraphSingleWindow(id, coOb[id], txtBox.Text);
            singleWindow.SingnleClosedEvent += SingleWindowClosed_RecvInfo;
            singleWindow.Show();
        }

        private static int GetNumber(string str)
        {
            var result = 0;
            if (!string.IsNullOrEmpty(str))
            {
                // 正则表达式剔除非数字字符（不包含小数点.） 
                str = Regex.Replace(str, @"[^\d.\d]", "");
                // 如果是数字，则转换为decimal类型 
                if (Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$"))
                {
                    result = int.Parse(str);
                }
            }
            return result;
        }

        #region Event

        private void ScopeTextNameChanged(object sender, EventArgs eventArgs)
        {
            var txtBox = sender as TextBox;

            var id = GetNumber(txtBox.Name) - 1;

            zedGraph_local.GraphPane.CurveList[id].Label.Text = txtBox.Text;

            refleshZedPane(zedGraph_local);
        }

        private void CheckDrawingOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            var checkchangBox = (CheckBox) sender;
            var id = GetNumber(checkchangBox.Name) - 1;

            var txtBox = (TextBox) panel_Scope.Controls.Find("txtName" +
                                                             Convert.ToString(id + 1), true)[0];

            var buttonNewForm = new Button();
            buttonNewForm = (Button) panel_Scope.Controls.Find("buttonDraw" +
                                                               Convert.ToString(id + 1), true)[0];

            if (checkchangBox.Checked)
            {
                txtBox.Enabled = true;
                buttonNewForm.Enabled = true;

                if (mySerialPort.IsOpen)
                {
                    timer_fresh.Start();
                }
                else
                {
                    for (var j = 0; j < zedGrpahNames[id].ZedPoint.ZedListX.Count; j++)
                    {
                        var x = zedGrpahNames[id].ZedPoint.ZedListX.ElementAt(j);
                        var y = zedGrpahNames[id].ZedPoint.ZedListY.ElementAt(j);
                        zedGrpahNames[id].listZed.Add(x, y);
                    }
                }
            }
            else
            {
                txtBox.Enabled = false;
                buttonNewForm.Enabled = false;

                if (!mySerialPort.IsOpen)
                {
                    var upLimit = zedGrpahNames[id].listZed.Count;
                    zedGrpahNames[id].listZed.RemoveRange(0, upLimit);
                }
            }

            refleshZedPane(zedGraph_local);
        }

        private void button_ClearDrawing_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < ScopeNumber; i++) //首先清除所有的曲线数据
            {
                zedGrpahNames[i].listZed.RemoveRange(0, zedGrpahNames[i].listZed.Count);
                zedGrpahNames[i].x = -1;
            }

            zedGraph_local.GraphPane.XAxis.Scale.MinAuto = true;
            zedGraph_local.GraphPane.XAxis.Scale.MaxAuto = true;
            zedGraph_local.GraphPane.YAxis.Scale.MinAuto = true;
            zedGraph_local.GraphPane.YAxis.Scale.MaxAuto = true;

            refleshZedPane(zedGraph_local);
        }

        private void SingleWindowClosed_RecvInfo(int id)
        {
            zedGrpahNames[id].IsSingleWindowShowed = false;
        }

        #endregion

        #endregion

        #region 8.全局参数

        private void button_ReadAllSettings_Click(object sender, EventArgs e)
        {
            try
            {
                label28.Text = @"Loading....";

                var tmpMessage = FormPackage(3, 1, "0");

                SendMessageAndEnqueue(3, 1, tmpMessage);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void button_force_stop_Click(object sender, EventArgs e)
        {
            //清空现在的所有队列
            _queueEchoControl.Clear();
            tmpSendHandle.Flag = 1;

            try
            {
                var noMeaning = "0";
                SendMessageAndEnqueue(3, 2, noMeaning);

                label28.Text = @"停车指令发送中...";
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        #endregion

        #region 历史数据

        private void button_History_Click(object sender, EventArgs e) //如果可能请使用多线程！！！
        {
            saveFileDialog_History.Title = "导出配置";
            saveFileDialog_History.Filter = "配置文件(*.ini)|*.ini|所有类型(*.*)|(*.*)";
            saveFileDialog_History.RestoreDirectory = true;
            if (saveFileDialog_History.ShowDialog() == DialogResult.OK)
            {
                var fileName = saveFileDialog_History.FileName;
                saveFileDialog_History.FileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                //MessageBox.Show(fileName + filter);
                var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                var sw = new StreamWriter(fs, Encoding.Default);

                WritePIDValueToText(sw, fileName);
                WriteCustomParaSetupToText(sw, fileName);
                WriteScopeSetupToText(sw, fileName);

                //...
                sw.Close();
            }
        }

        private void button_loadHistory_Click(object sender, EventArgs e)
        {
            openFileDialog_History.Title = "导入配置文件";
            openFileDialog_History.Filter = "配置文件(*.ini)|*.ini|所有类型(*.*)|(*.*)";
            openFileDialog_History.RestoreDirectory = true;
            openFileDialog_History.FileName = "";
            if (openFileDialog_History.ShowDialog() == DialogResult.OK)
            {
                var fileName = openFileDialog_History.FileName;
                openFileDialog_History.FileName = fileName.Substring(fileName.LastIndexOf("\\") + 1);
                //MessageBox.Show(fileName + filter);
                var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                var sr = new StreamReader(fs, Encoding.Default);

                var lines = new List<string>();

                while (sr.Peek() > -1)
                {
                    lines.Add(sr.ReadLine());
                }

                if (lines.ElementAt(0).Contains("CarType"))
                {
                    var countLine = 0;

                    ReadPIDFromText(lines, countLine);
                    ReadCustomParaFromText(lines, countLine);
                    ReadScopeFromText(lines, countLine);
                }
                _isLoadHistory = true;

                sr.Close();
            }
        }

        private void WritePIDValueToText(StreamWriter sw, string filename)
        {
            //小车参数
            sw.WriteLine("CarType:{0}", radioButton_FourWheel.Checked ? "1" : "0");
            if (radioButton_FourWheel.Checked) //四轮车PID
            {
                //舵机PID参数
                var strSteer = "P" + textBox_Steer_P.Text + " I" + textBox_Steer_I.Text + " D" +
                               textBox_Steer_D.Text;
                sw.WriteLine("Steer:{0}", strSteer);

                //电机PID参数
                var strMotor = "P" + textBox_Motor_P.Text + " I" + textBox_Motor_I.Text + " D" +
                               textBox_Motor_D.Text;
                sw.WriteLine("Motor:{0}", strMotor);
            }
            else if (radioButton_BalanceCar.Checked) //直立车PID参数的获取
            {
                //直立PID
                var strStand = "P" + textBox_Stand_P.Text + " I" + textBox_Stand_I.Text + " D" +
                               textBox_Stand_D.Text;
                sw.WriteLine("Stand:{0}", strStand);

                //速度PID
                var strSpeed = "P" + textBox_Speed_P.Text + " I" + textBox_Speed_I.Text + " D" +
                               textBox_Speed_D.Text;
                sw.WriteLine("Speed:{0}", strSpeed);

                //方向PID
                var strDirection = "P" + textBox_Direction_P.Text + " I" + textBox_Direction_I.Text + " D" +
                                   textBox_Direction_D.Text;
                sw.WriteLine("Direction:{0}", strDirection);
            }
        }

        private void WriteCustomParaSetupToText(StreamWriter sw, string fileName)
        {
            //自定义参数
            var totalCustomPara = 0;
            if (textBox_DIY_Number.Text == "")
            {
                //essageBox.Show(@"请重新输入需要的参数数量！", @"错误");
                totalCustomPara = 0;
            }
            totalCustomPara = Convert.ToInt32(textBox_DIY_Number.Text);

            sw.WriteLine("DIY_Num:{0}", totalCustomPara);

            for (var i = 0; i < totalCustomPara; i++)
            {
                var txtBox = new TextBox[2]; //用控件数组来定义每一行的TextBox,总共3个TextBox
                txtBox[0] =
                    (TextBox) panel_add_DIYControls.Controls.Find("txtName" + Convert.ToString(i + 1), true)[0];
                txtBox[1] =
                    (TextBox) panel_add_DIYControls.Controls.Find("txtValue" + Convert.ToString(i + 1), true)[0];

                var strName = txtBox[0].Text;
                var strValue = txtBox[1].Text;

                sw.WriteLine("{0}:{1}", strName, strValue);
            }
        }

        private void WriteScopeSetupToText(StreamWriter sw, string fileName)
        {
            //示波器参数
            var countScope = 0;
            var checkDrawing = new CheckBox[ScopeNumber];
            var txtBox_Scope = new TextBox[ScopeNumber];
            var buttonNewForm = new Button[ScopeNumber];

            for (var i = 0; i < ScopeNumber; i++)
            {
                checkDrawing[i] =
                    (CheckBox)
                        panel_Scope.Controls.Find("checkBox_Def" + Convert.ToString(i + 1), true)[0];

                txtBox_Scope[i] =
                    (TextBox) panel_Scope.Controls.Find("txtName" + Convert.ToString(i + 1), true)[0];

                buttonNewForm[i] =
                    (Button) panel_Scope.Controls.Find("buttonDraw" + Convert.ToString(i + 1), true)[0];

                if (checkDrawing[i].Checked)
                    ++countScope;
            }
            sw.WriteLine("ScopeLineNumber:{0}", countScope);

            for (var i = 0; i < ScopeNumber; i++)
            {
                if (checkDrawing[i].Checked)
                {
                    sw.WriteLine("ScopeLineOrder:{0}", i);
                    sw.WriteLine("PointNumber:{0}", zedGrpahNames[i].listZed.Count);

                    for (var j = 0; j < zedGrpahNames[i].listZed.Count; j++)
                    {
                        sw.Write("{0}", zedGrpahNames[i].listZed.ElementAt(j).ToString().Replace(" ", ""));
                    }
                    sw.WriteLine("");
                }
            }
        }

        private void ReadPIDFromText(List<string> lines, int countLine)
        {
            int carType = Convert.ToInt16(lines.ElementAt(countLine).Split(':').ElementAt(1));
            countLine++;

            if (carType == 0) //直立车
            {
                ReadBalancePIDFromText(lines, countLine);
            }
            else if (carType == 1) //四轮车
            {
                ReadFourwheelPIDFromText(lines, countLine);
            }
        }

        private void ReadBalancePIDFromText(List<string> lines, int countLine)
        {
            var indexP = lines.ElementAt(countLine).IndexOf('P');
            var indexI = lines.ElementAt(countLine).IndexOf(" I", StringComparison.Ordinal);
            var indexD = lines.ElementAt(countLine).IndexOf(" D", StringComparison.Ordinal);

            //直立
            var standP = lines.ElementAt(countLine).Substring(indexP + 1, indexI - indexP - 1);
            var standI = lines.ElementAt(countLine).Substring(indexI + 2, indexD - indexI - 1).Trim();
            var standD = lines.ElementAt(countLine).Substring(indexD + 2);
            textBox_Stand_P.Text = standP;
            textBox_Stand_I.Text = standI;
            textBox_Stand_D.Text = standD;
            ++countLine;

            //速度
            indexP = lines.ElementAt(countLine).IndexOf('P');
            indexI = lines.ElementAt(countLine).IndexOf(" I", StringComparison.Ordinal);
            indexD = lines.ElementAt(countLine).IndexOf(" D", StringComparison.Ordinal);
            var speedP = lines.ElementAt(countLine).Substring(indexP + 1, indexI - indexP - 1);
            var speedI = lines.ElementAt(countLine).Substring(indexI + 2, indexD - indexI - 1).Trim();
            var speedD = lines.ElementAt(countLine).Substring(indexD + 2);
            textBox_Speed_P.Text = speedP;
            textBox_Speed_I.Text = speedI;
            textBox_Speed_D.Text = speedD;
            ++countLine;

            //方向
            indexP = lines.ElementAt(countLine).IndexOf('P');
            indexI = lines.ElementAt(countLine).IndexOf(" I", StringComparison.Ordinal);
            indexD = lines.ElementAt(countLine).IndexOf(" D", StringComparison.Ordinal);
            var directionP = lines.ElementAt(countLine).Substring(indexP + 1, indexI - indexP - 1);
            var directionI = lines.ElementAt(countLine).Substring(indexI + 2, indexD - indexI - 1).Trim();
            var directionD = lines.ElementAt(countLine).Substring(indexD + 2);
            textBox_Direction_P.Text = directionP;
            textBox_Direction_I.Text = directionI;
            textBox_Direction_D.Text = directionD;
            ++countLine;
        }

        private void ReadFourwheelPIDFromText(List<string> lines, int countLine)
        {
            var indexP = lines.ElementAt(countLine).IndexOf('P');
            var indexI = lines.ElementAt(countLine).IndexOf(" I", StringComparison.Ordinal);
            var indexD = lines.ElementAt(countLine).IndexOf(" D", StringComparison.Ordinal);

            //舵机
            var steerP = lines.ElementAt(countLine).Substring(indexP + 1, indexI - indexP - 1);
            var steerI = lines.ElementAt(countLine).Substring(indexI + 2, indexD - indexI - 1).Trim();
            var steerD = lines.ElementAt(countLine).Substring(indexD + 2);
            textBox_Steer_P.Text = steerP;
            textBox_Steer_I.Text = steerI;
            textBox_Steer_D.Text = steerD;
            ++countLine;

            //电机
            indexP = lines.ElementAt(countLine).IndexOf('P');
            indexI = lines.ElementAt(countLine).IndexOf(" I", StringComparison.Ordinal);
            indexD = lines.ElementAt(countLine).IndexOf(" D", StringComparison.Ordinal);
            var motorP = lines.ElementAt(countLine).Substring(indexP + 1, indexI - indexP - 1);
            var motorI = lines.ElementAt(countLine).Substring(indexI + 2, indexD - indexI - 1).Trim();
            var motorD = lines.ElementAt(countLine).Substring(indexD + 2);
            textBox_Motor_P.Text = motorP;
            textBox_Motor_I.Text = motorI;
            textBox_Motor_D.Text = motorD;
            ++countLine;
        }

        private void ReadCustomParaFromText(List<string> lines, int countLine)
        {
            //自定义参数
            if (lines.ElementAt(countLine).Contains("DIY_Num"))
            {
                int CustomParaNum = Convert.ToInt16(lines.ElementAt(countLine).Split(':').ElementAt(1));
                ++countLine;

                if (CustomParaNum == Convert.ToInt16(textBox_DIY_Number.Text))
                {
                    for (var i = 0; i < CustomParaNum; i++)
                    {
                        var checkboxSelect = new CheckBox();
                        checkboxSelect =
                            (CheckBox)
                                panel_add_DIYControls.Controls.Find("checkBox_Def" + Convert.ToString(i + 1),
                                    true)[0];

                        var txtBox = new TextBox[2]; //用控件数组来定义每一行的TextBox,总共3个TextBox
                        txtBox[0] =
                            (TextBox)
                                panel_add_DIYControls.Controls.Find("txtName" + Convert.ToString(i + 1), true)[0
                                    ];
                        txtBox[1] =
                            (TextBox)
                                panel_add_DIYControls.Controls.Find("txtValue" + Convert.ToString(i + 1), true)[
                                    0];

                        var numberName = lines.ElementAt(countLine).Split(':').ElementAt(0);
                        var numberValue = lines.ElementAt(countLine).Split(':').ElementAt(1);

                        txtBox[0].Text = numberName;
                        txtBox[1].Text = numberValue;

                        ++countLine;
                    }
                }
                else
                {
                    var txtBoxName = new TextBox();
                    var txtBoxValue = new TextBox();
                    var checkboxSelect = new CheckBox();

                    for (var i = 0; i < CustomParaNum; i++)
                    {
                        //是否启用的选项
                        checkboxSelect = new CheckBox();
                        checkboxSelect.Size = new Size(50, 20);
                        checkboxSelect.Location = new Point(10 + 70*0, 30*i); //textbox坐标
                        checkboxSelect.Name = "checkBox_Def" + Convert.ToString(i + 1);
                        checkboxSelect.Text = "";
                        checkboxSelect.CheckedChanged += CheckboxSelectOnCheckedChanged;
                        checkboxSelect.Checked = true;

                        //名字
                        txtBoxName = new TextBox();
                        txtBoxName.Size = new Size(50, 50); //textbox大小                   
                        txtBoxName.Location = new Point(10 + 70*1, 30*i); //textbox坐标
                        txtBoxName.Name = "txtName" + Convert.ToString(i + 1); //设定控件名称
                        txtBoxName.TextAlign = HorizontalAlignment.Center;
                        txtBoxName.Text = lines.ElementAt(countLine).Split(':').ElementAt(0);
                        txtBoxName.Enabled = true;

                        //值
                        txtBoxValue = new TextBox();
                        txtBoxValue.Size = new Size(50, 50); //textbox大小                   
                        txtBoxValue.Location = new Point(10 + 70*2, 30*i); //textbox坐标
                        txtBoxValue.Name = "txtValue" + Convert.ToString(i + 1); //设定控件名称
                        txtBoxValue.Text = "1.00";
                        txtBoxValue.TextAlign = HorizontalAlignment.Center;
                        txtBoxValue.Enabled = true;
                        txtBoxValue.Text = lines.ElementAt(countLine).Split(':').ElementAt(1);

                        ++countLine;

                        panel_add_DIYControls.Controls.Add(checkboxSelect);
                        panel_add_DIYControls.Controls.Add(txtBoxName); //把"名字"加入到panel中
                        panel_add_DIYControls.Controls.Add(txtBoxValue); //把"值"加入到panel中
                    }
                }
            }
        }

        private void ReadScopeFromText(List<string> lines, int countLine)
        {
            //Scope
            for (var i = 0; i < ScopeNumber; i++) //首先清除所有的曲线数据
            {
                zedGrpahNames[i].listZed.RemoveRange(0, zedGrpahNames[i].listZed.Count);
            }

            if (lines.ElementAt(countLine).Contains("ScopeLineNumber"))
            {
                int scopeNum = Convert.ToInt16(lines.ElementAt(countLine).Split(':').ElementAt(1));
                ++countLine;

                for (var i = 0; i < scopeNum; i++)
                {
                    if (lines.ElementAt(countLine).Contains("ScopeLineOrder")) //第几个曲线
                    {
                        int scopeOrder = Convert.ToInt16(lines.ElementAt(countLine).Split(':').ElementAt(1));
                        ++countLine;
                        if (lines.ElementAt(countLine).Contains("PointNumber")) //点数量
                        {
                            int scopePointNum =
                                Convert.ToInt16(lines.ElementAt(countLine).Split(':').ElementAt(1));
                            ++countLine;

                            char[] charsplit = {'(', ',', ')'};
                            var xyPoint = lines.ElementAt(countLine)
                                .Split(charsplit, StringSplitOptions.RemoveEmptyEntries);
                            ++countLine;

                            for (var j = 0; j < scopePointNum; j++)
                            {
                                var x = Convert.ToDouble(xyPoint[j*2]);
                                var y = Convert.ToDouble(xyPoint[j*2 + 1]);
                                zedGrpahNames[scopeOrder].listZed.Add(x, y);
                                zedGrpahNames[scopeOrder].ZedPoint.ZedListX.Add(x);
                                zedGrpahNames[scopeOrder].ZedPoint.ZedListY.Add(y);
                            }
                        }
                    }
                }
                //UpdateScope Here
                zedGraph_local.GraphPane.XAxis.Scale.MaxAuto = true;
                zedGraph_local.GraphPane.XAxis.Scale.MinAuto = true;
                zedGraph_local.GraphPane.YAxis.Scale.MaxAuto = true;
                zedGraph_local.GraphPane.YAxis.Scale.MinAuto = true;

                refleshZedPane(zedGraph_local);
            } //Scope End
        }
        #endregion

        #region 9.匹配性判断工具

        private string FormPackage(int father, int child, string sendMessgage)
        {
            var result = "";

            var head = "#";
            var end = "$";
            var sFather = father.ToString();
            var sChild = child.ToString();

            result = head + "|" + sFather + "|" + sChild + "|" + sendMessgage + "|" + end;

            return result;
        }

        private string FormPackage_NOHead_NOEnd(int father, int child, string sendMessgage)
        {
            var result = "";

            var sFather = father.ToString();
            var sChild = child.ToString();

            result = "|" + sFather + "|" + sChild + "|" + sendMessgage + "|";

            return result;
        }

        private void SendMessageAndEnqueue(int father, int child, string messgae)
        {
            messgae = FormPackage(father, child, messgae);
            mySerialPort.Write(messgae);

            var tmpHandle = new GetEchoForm(0, messgae);
            _queueEchoControl.Enqueue(tmpHandle);

            if (!timer_Send2GetEcho.Enabled)
                timer_Send2GetEcho.Start();
        }

        private Boolean IsIntegerCheck(string text)
        {
            var reg = new Regex("^[0-9]{1,}$");

            if (!reg.IsMatch(text))
            {
                MessageBox.Show(@"请输入整数！");
                return false;
            }
            return true;
        }

        private Boolean IsFloatCheck(string text)
        {
            var reg = new Regex("^((-{0,1}[0-9]+[\\.]?[0-9]{0,3})|-{0,1}[0-9]{1,})$");

            if (!reg.IsMatch(text))
            {
                MessageBox.Show(@"请输入精度为3的浮点或整数！");
                return false;
            }
            return true;
        }

        #endregion
    }
}