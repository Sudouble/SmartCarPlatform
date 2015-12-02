#define MULTITHREAD //多线程收发模式，注释本句则使用单线程模式
//相对单线收发模式，占用系统资源稍微大些，但是执行效果更好，尤其是在大数据收发时的UI反应尤其明显       

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using leomon;
using ZedGraph;

namespace Freescale_debug
{
    public partial class Form1 : Form
    {
        public delegate void UpdateAcceptTextBoxTextHandler(string text);

        //ZedGraph
        private const int ScopeNumber = 8; //示波器能画曲线的数量

        private readonly Color[] _colorLine =
        {
            Color.Green, Color.DodgerBlue, Color.Brown, Color.Chartreuse,
            Color.CornflowerBlue, Color.Red, Color.Yellow, Color.Gray
        };

        private readonly int[] _countZedgraph = new int[ScopeNumber];
        private readonly PointPairList[] _listZed = new PointPairList[ScopeNumber];
        //发送串口数据的队列，直到收到有效数据为止
        private readonly Queue _queueEchoControl = new Queue(); //根据这个来判断是不是要进行重发
        private readonly List<int> _recieveBuff = new List<int>();
        private const string SavefileName = "串口助手配置.xml";
        private readonly bool[] _singleWindowShowed = new bool[ScopeNumber];
        private readonly double[] _valueZed = new double[ScopeNumber];
        private readonly double _xminScale = 0;
        private readonly List<double>[] _zedListX = new List<double>[ScopeNumber];
        private readonly List<double>[] _zedListY = new List<double>[ScopeNumber];
        //SerialPort Flags
        private bool _closing; //是否正在关闭串口，执行Application.DoEvents，并阻止再次
        private Bitmap _bitmapOld;
        //ccd
        private string _ccdTtt = "";
        //ZedGraph 窗体间传参
        public CallObject[] coOb = new CallObject[ScopeNumber];
        private bool _dequeueFirstTime = true;
        private double _directionDOld;
        private double _directionIOld;
        //方向PID
        private double _directionPOld;
        private bool _isLoadHistory;
        //自定义参数的一些变量
        private bool _isLoadingDiyok = false;
        private bool _isReading;
        private bool _listening; //是否没有执行完invoke相关操作  
        private double motor_D_old;
        private double motor_I_old;
        //电机PID参数
        private double motor_P_old;
        private ReceivedDataType myReceivedDataType = ReceivedDataType.CharType;
        private SendDataType mySendDataType = SendDataType.CharType;
        private bool needWrite = false;
        private string RecievedStringAdd = ""; //接收到的数据
        private int retryCount;
        private bool showInfo = true;
        private double speed_D_old;
        private double speed_I_old;
        //速度PID
        private double speed_P_old;
        private double stand_D_old;
        private double stand_I_old;
        //直立PID
        private double stand_P_old;
        private double steer_D_old;
        private double steer_I_old;
        //PID旧的值
        //舵机PID参数
        private double steer_P_old;
        private StreamWriter sw;
        private GetEchoForm tmpSendHandle = new GetEchoForm();
        private int totalReceivedBytes;
        public UpdateAcceptTextBoxTextHandler UpdateTextHandler;
        private int _xmaxScale = 100;
        private double _ymaxScale = 100;
        private double _yminScale = -10;

        #region 5. 摄像头参数

        private void Camera_DrawActual(Bitmap bitmap, string cameraData)
        {
            var bitmapData =
                bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadWrite, bitmap.PixelFormat);

            var bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat)/8;
            var byteCount = bitmapData.Stride*bitmap.Height;
            var pixels = new byte[byteCount];
            var ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            var heightInPixels = bitmapData.Height;
            var widthInBytes = bitmapData.Width*bytesPerPixel;

            for (var y = 0; y < heightInPixels; y++)
            {
                var currentLine = y*bitmapData.Stride;

                for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int grey = Convert.ToInt16(cameraData.ElementAt(Convert.ToInt16(y + x/4)));

                    // calculate new pixel value
                    pixels[currentLine + x] = (byte) grey;
                    pixels[currentLine + x + 1] = (byte) grey;
                    pixels[currentLine + x + 2] = (byte) grey;
                    pixels[currentLine + x + 3] = 255;
                }
            }
            // copy modified bytes back
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bitmap.UnlockBits(bitmapData);
        }

        #endregion

        //线程的委托//使线程可以改变控件的值
        private delegate void DoWorkUiThreadDelegate(string recvString, List<int> recvByte);

        private enum ReceivedDataType
        {
            CharType,
            HexType
        };

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
            //检查可用端口
            CheckAvailablePorts();
            //开始更新时间和日期标签
            //updateDateTimer.Start();
            //载入配置信息
            LoadConfig(SavefileName);

            //button_DIY_NumConfirm_Click(sender, e);
            //isLoadingDIYOK = true;


            _bitmapOld = new Bitmap(128, pictureBox_CCD_Path.Height);
            for (var i = 0; i < 128; i++)
            {
                for (var j = 0; j < pictureBox_CCD_Path.Height; j++)
                {
                    _bitmapOld.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                }
            }

            Init_TablePagePIDSettings();
            Init_pane_Scope();
            for (var i = 0; i < ScopeNumber; i++)
            {
                _zedListX[i] = new List<double>();
                _zedListY[i] = new List<double>();
            }
            InitzedGraph();

            Init_DIY_DynamicControls(Convert.ToInt16(textBox_DIY_Number.Text));
            InitElectricity(Convert.ToInt16(textBox_Electricity_Number.Text));

            //LoadHistory();
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

        private void UnPakageReceivedUiThread()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DoWorkUiThreadDelegate(UnPakageReceived), RecievedStringAdd, _recieveBuff);
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
            _isReading = true;
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
                    //text = sp.ReadExisting();
                    //RecievedStringAdd += text;

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
                                Invoke(new DoWorkUiThreadDelegate(UnPakageReceived), RecievedStringAdd, _recieveBuff);
                            }
                            RecievedStringAdd = null;
                            _recieveBuff.Clear();
                        }
                    }
                }
                totalReceivedBytes += size;
                Invoke(UpdateTextHandler, text);
                //Thread.Sleep(50);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
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
                if (_dequeueFirstTime && _queueEchoControl.Count > 0)
                {
                    var send_message = (GetEchoForm) _queueEchoControl.Dequeue();
                    tmpSendHandle = send_message;
                    _dequeueFirstTime = false;
                }

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
                    ////设置自动重发次数上限
                    //if (retryCount > 50)
                    //{
                    //    tmpSendHandle.Flag = 1;
                    //    label28.Text = @"本次超过最大重发次数";
                    //}
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void UnPakageReceived(string recvStr, List<int> recBuff)
        {
            // 1 2 3    45 6 7               8
            //#|1|3.31|0||1|1|P1200I2100D3310|$
            //需要找到其中的长度
            var count_length = 0;
            if (recvStr != null)
            {
                int headCheck = recvStr.IndexOf("#|");
                int endCheck = recvStr.LastIndexOf("|$");
                if (headCheck != -1 && endCheck != -1)
                {
                    //截取需要的段：
                    var needed_str = recvStr.Substring(headCheck, endCheck - headCheck + 2);

                    string[] splitted_Message;
                    //判断是不是CCD或者摄像头数据
                    var judge_CDD_Camera = needed_str.Split('|');
                    if (judge_CDD_Camera.Length > 1 &&
                        Convert.ToInt16(judge_CDD_Camera.ElementAt(1)) == 1 || //camera
                        Convert.ToInt16(judge_CDD_Camera.ElementAt(1)) == 2) //ccd
                    {
                        count_length = 4;
                    }
                    else
                    {
                        //如果不是上面的数据类型就开始计数
                        count_length += needed_str.Where((t, i) => needed_str.ElementAt(i) == '|').Count();
                    }

                    splitted_Message = needed_str.Split('|');

                    //审查分隔后的数据是不是符合要求——避免进不需要的判断（避免传输错误）
                    for (var i = 0; 4*(i + 1) < splitted_Message.Length; i++)
                    {
                        int father = 0, child = 0;
                        try
                        {
                            //加入是空字符串，舍弃本次
                            if (splitted_Message.ElementAt(1 + 4*i).Trim() == "" ||
                                splitted_Message.ElementAt(2 + 4*i).Trim() == "")
                                return;

                            father = Convert.ToInt16(splitted_Message.ElementAt(1 + 4*i));
                            child = Convert.ToInt16(splitted_Message.ElementAt(2 + 4*i));
                        }
                        catch (Exception ee)
                        {
                            //到了这里就表示解析出错了。
                            return;
                        }

                        //表示不符合数据格式要求
                        if ((father < 0 || father > 8) && (child < 0 || child > 200))
                        {
                            return;
                        }
                    }

                    for (var k = 0; k < count_length/4; k++)
                    {
                        if (Convert.ToInt16(splitted_Message.ElementAt(1 + 4*k)) == 1 && checkBox_Camera_ONOFF.Checked)
                            //摄像头数据
                        {
                            var firstRightIndex = needed_str.IndexOf('(');
                            var indexEnd = needed_str.LastIndexOf('|');
                            var cameraMessgageAdd = needed_str.Substring(firstRightIndex,
                                indexEnd - firstRightIndex);

                            var leftIndex = cameraMessgageAdd.IndexOf('(');
                            var middleIndex = cameraMessgageAdd.IndexOf('+');
                            var rightIndex = cameraMessgageAdd.IndexOf(')');
                            int widthCamera =
                                Convert.ToInt16(cameraMessgageAdd.Substring(leftIndex + 1, middleIndex - leftIndex - 1));
                            int heightCamera =
                                Convert.ToInt16(cameraMessgageAdd.Substring(middleIndex + 1,
                                    rightIndex - middleIndex - 1));
                            var cameraData = cameraMessgageAdd.Substring(rightIndex + 1);

                            //label_CCD_Width.Text = @"CCD宽度：" + length;

                            if (cameraData.Length == widthCamera*heightCamera)
                            {
                                //========================================================================================
                                //很牛的LockBits方式！！比SetPixel效率高很多！
                                var bitmap = new Bitmap(widthCamera, heightCamera);
                                Camera_DrawActual(bitmap, cameraData);
                                pictureBox_CameraActual.Image = bitmap; //在控件上显示图片
                                //End of 实时图像显示====================================================================
                                //显示轨迹部分

                                if (Convert.ToInt16(splitted_Message.ElementAt(2 + 4*k)) == 1) //摄像头1
                                {
                                }
                                else if (Convert.ToInt16(splitted_Message.ElementAt(2 + 4*k)) == 2) //摄像头2
                                {
                                }
                                else if (Convert.ToInt16(splitted_Message.ElementAt(2 + 4*k)) == 3) //摄像头3
                                {
                                }
                            }
                        }
                        else if (Convert.ToInt16(splitted_Message.ElementAt(1 + 4*k)) == 2 && checkBox_CCD_ONOFF.Checked)
                            //CCD数据
                        {
                            var firstRightIndex = needed_str.IndexOf('(');
                            var indexEnd = needed_str.LastIndexOf('|');
                            var ccdMessgageAdd = needed_str.Substring(firstRightIndex,
                                indexEnd - firstRightIndex);

                            var leftIndex = ccdMessgageAdd.IndexOf('(');
                            var rightIndex = ccdMessgageAdd.IndexOf(')');
                            var length = ccdMessgageAdd.Substring(leftIndex + 1, rightIndex - leftIndex - 1);
                            var ccdData = ccdMessgageAdd.Substring(rightIndex + 1);

                            label_CCD_Width.Text = @"CCD宽度：" + length;

                            if (ccdData.Length == Convert.ToInt16(length))
                            {
                                var greyValue = new List<int>();
                                //找到recvBuff中的相应段
                                for (var i = 0; i < _recieveBuff.Count - 6; i++)
                                {
                                    if (recBuff.ElementAt(i) == '#' &&
                                        recBuff.ElementAt(i + 1) == '|' &&
                                        recBuff.ElementAt(i + 2) == '2' &&
                                        recBuff.ElementAt(i + 3) == '|' &&
                                        //recBuff.ElementAt(i + 4) == '1' &&
                                        recBuff.ElementAt(i + 5) == '|' &&
                                        recBuff.ElementAt(i + 6) == '(')
                                    {
                                        for (var j = i + 11; j < ccdData.Length + i + 11; j++)
                                        {
                                            greyValue.Add(_recieveBuff[j]);
                                        }
                                        break;
                                    }
                                }

                                ////将采集的值保存到txt
                                //FileStream fs = new FileStream("CCD_Data.txt", FileMode.Append, FileAccess.Write);
                                //StreamWriter sw_ccd = new StreamWriter(fs, Encoding.Default);

                                ////ccd参数
                                //for (int i = 0; i < ccdData.Length; i++)
                                //{
                                //    sw_ccd.Write("{0} ", greyValue.ElementAt(i));
                                //}
                                //sw_ccd.WriteLine("\r\n");

                                //sw_ccd.Close();
                                //fs.Close();

                                ////算法处理后的图像
                                ////int threSold = Otsu(greyValue);
                                //int threSold = Average_ccd(greyValue);
                                //label_CCD_Thresold.Text = @"阈值" + threSold.ToString() + CCD_FindBoard(greyValue);
                                //for (int i = 0; i < greyValue.Count; i++)
                                //{
                                //    if (greyValue.ElementAt(i) > threSold)
                                //        greyValue[i] = 255;
                                //    else
                                //        greyValue[i]= 0;
                                //}
                                //Bitmap bitmap_algrithm = new Bitmap(ccdData.Length, pictureBox_CCD_Actual.Height);
                                //CCD_DrawActual(bitmap_algrithm, greyValue);
                                //pictureBox_CCD_deal.Image = bitmap_algrithm; //在控件上显示图片

                                //End of 实时图像显示====================================================================
                                //显示轨迹部分

                                if (Convert.ToInt16(splitted_Message.ElementAt(2 + 4*k)) == 1) //CCD1
                                {
                                    //========================================================================================
                                    //很牛的LockBits方式！！比SetPixel效率高很多！
                                    var bitmap = new Bitmap(ccdData.Length, pictureBox_CCD_Actual.Height);
                                    CCD_DrawActual(bitmap, greyValue);
                                    pictureBox_CCD_Actual.Image = bitmap; //在控件上显示图片

                                    //绘制路径曲线
                                    CCD_DrawPath(_bitmapOld, greyValue);
                                    pictureBox_CCD_Path.Image = _bitmapOld;
                                }
                                else if (Convert.ToInt16(splitted_Message.ElementAt(2 + 4*k)) == 2) //CCD2
                                {
                                    //========================================================================================
                                    //很牛的LockBits方式！！比SetPixel效率高很多！
                                    var bitmap = new Bitmap(ccdData.Length, pictureBox_CCD_deal.Height);
                                    CCD_DrawActual(bitmap, greyValue);
                                    pictureBox_CCD_deal.Image = bitmap; //在控件上显示图片
                                }
                                else if (Convert.ToInt16(splitted_Message.ElementAt(2 + 4*k)) == 3) //CCD3
                                {
                                    var bitmap = new Bitmap(ccdData.Length, pictureBox_CCD_deal.Height);
                                    CCD_DrawActual(bitmap, greyValue);
                                    pictureBox_CCD3.Image = bitmap; //在控件上显示图片
                                }
                            }
                        }
                        else if (Convert.ToInt16(splitted_Message.ElementAt(1 + 4*k)) == 3) //实时参数
                        {
                            int Elect_num = Convert.ToInt16(splitted_Message.ElementAt(2 + 4*k));
                            if (Elect_num <= Convert.ToInt16(textBox_Electricity_Number.Text)) //发送的传感器数据比现有的小
                            {
                                var txtBox = new TextBox();
                                txtBox =
                                    (TextBox)
                                        panel_Electricity.Controls.Find(
                                            "txtElectValue" + Convert.ToString(Elect_num), true)[0];

                                //将参数赋值到相应的版块
                                var value = Convert.ToDouble(splitted_Message.ElementAt(3 + 4*k))/1000;
                                txtBox.Text = value.ToString();
                            }
                        }
                            #region 示波器显示部分

                        else if (Convert.ToInt16(splitted_Message.ElementAt(1 + 4*k)) == 4) //传送到示波器的数据
                        {
                            var hasChecked_Item = false;

                            var total = ScopeNumber;
                            var checkDrawing = new CheckBox[ScopeNumber];
                            var txtBox = new TextBox[ScopeNumber]; //用控件数组来定义每一行的TextBox,总共3个TextBox
                            var buttonNewForm = new Button[ScopeNumber];

                            if (_isLoadHistory)
                            {
                                _isLoadHistory = false;
                                for (var i = 0; i < ScopeNumber; i++) //首先清除所有的曲线数据
                                {
                                    _listZed[i].RemoveRange(0, _listZed[i].Count);
                                    _countZedgraph[i] = -1;
                                }
                            }

                            for (var i = 0; i < total; i++)
                            {
                                checkDrawing[i] =
                                    (CheckBox)
                                        panel_Scope.Controls.Find("checkBox_Def" + Convert.ToString(i + 1), true)[0];

                                txtBox[i] =
                                    (TextBox)
                                        panel_Scope.Controls.Find("txtName" + Convert.ToString(i + 1), true)[0];

                                buttonNewForm[i] =
                                    (Button)
                                        panel_Scope.Controls.Find("buttonDraw" + Convert.ToString(i + 1), true)[0];

                                if (checkDrawing[i].Checked && hasChecked_Item == false)
                                {
                                    //如果有被选择的话，刷新绘图窗口
                                    hasChecked_Item = true;
                                    timer_fresh.Start();
                                }
                                // zedGraph_local.GraphPane.CurveList.Clear();
                            }

                            for (var i = 0; i < total; i++)
                            {
                                if (checkDrawing[i].Checked &&
                                    Convert.ToInt16(splitted_Message.ElementAt(2 + 4*k)) == i + 1)
                                {
                                    hasChecked_Item = true;
                                    //ZedGraph绘图
                                    var j = Convert.ToInt16(splitted_Message.ElementAt(2 + 4*k)) - 1;
                                    var value = Convert.ToDouble(splitted_Message.ElementAt(3 + 4*k))/1000;
                                    _valueZed[i] = value;

                                    if ((_xmaxScale - _countZedgraph[j]) < (_xmaxScale/5)) //自动改变坐标轴范围
                                    {
                                        _xmaxScale += _xmaxScale/5;
                                        zedGraph_local.GraphPane.XAxis.Scale.Max = _xmaxScale;
                                    }
                                    if (_ymaxScale - _valueZed[i] < _ymaxScale/5) //自动改变坐标轴范围
                                    {
                                        //YmaxScale += YmaxScale/5;
                                        _ymaxScale += -_ymaxScale + 1.3*_valueZed[i];
                                        zedGraph_local.GraphPane.YAxis.Scale.Max = _ymaxScale;
                                    }
                                    if (_yminScale - _valueZed[i] > _yminScale/5) //自动改变坐标轴范围
                                    {
                                        _yminScale += _yminScale/5;
                                        zedGraph_local.GraphPane.YAxis.Scale.Min = _yminScale;
                                    }

                                    _countZedgraph[j] += 1;

                                    _listZed[j].Add(Convert.ToDouble(_countZedgraph[j]), value);
                                    _zedListX[i].Add(Convert.ToDouble(_countZedgraph[j]));
                                    _zedListY[i].Add(value);
                                    //MessageBox.Show("In Test Unpakage");

                                    if (_singleWindowShowed[i])
                                        coOb[i].CallEvent(_countZedgraph[j], value);
                                    //string curve = string.Format("curve {0}", i);
                                    //zedGraph_local.GraphPane.AddCurve(curve, listZed[j], color_line[i], 
                                    //    SymbolType.None);
                                }
                            }

                            if (hasChecked_Item)
                            {
                                //zedGraph_local.AxisChange();
                                ////z1.Invalidate();
                                //zedGraph_local.Refresh();
                            }
                        }
                            #endregion

                        else if (Convert.ToInt16(splitted_Message.ElementAt(1 + 4*k)) == 5) //大批量读取参数值(PID)
                        {
                            int type = Convert.ToInt16(splitted_Message.ElementAt(2 + 4*k));
                            var valuePID = splitted_Message.ElementAt(3 + 4*k);

                            var indexP = valuePID.IndexOf('P');
                            var indexI = valuePID.IndexOf("I", StringComparison.Ordinal);
                            var indexD = valuePID.IndexOf("D", StringComparison.Ordinal);

                            if (indexP != -1 && indexI != -1 && indexD != -1)
                            {
                                var valueP =
                                    (Convert.ToInt16(valuePID.Substring(indexP + 1, indexI - indexP - 1))/1000.0)
                                        .ToString(CultureInfo.InvariantCulture);
                                var valueI =
                                    (Convert.ToInt16(valuePID.Substring(indexI + 1, indexD - indexI - 1))/1000.0)
                                        .ToString(CultureInfo.InvariantCulture);
                                var valueD =
                                    (Convert.ToInt16(valuePID.Substring(indexD + 1))/1000.0).ToString(
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
                        else if (Convert.ToInt16(splitted_Message.ElementAt(1 + 4*k)) == 6) //大批量读取参数值(diy)
                        {
                            int child = Convert.ToInt16(splitted_Message.ElementAt(2 + 4*k));
                            var valueDIY =
                                (Convert.ToDouble(splitted_Message.ElementAt(3 + 4*k))/1000.0).ToString(
                                    CultureInfo.InvariantCulture);

                            //给相应控件赋值
                            var total = Convert.ToInt32(textBox_DIY_Number.Text);

                            if (child < total)
                            {
                                var checkboxSelect = new CheckBox();
                                checkboxSelect =
                                    (CheckBox)
                                        panel_add_DIYControls.Controls.Find(
                                            "checkBox_Def" + Convert.ToString(child),
                                            true)[0];

                                var txtBox = new TextBox[2]; //用控件数组来定义每一行的TextBox,总共3个TextBox
                                txtBox[0] =
                                    (TextBox)
                                        panel_add_DIYControls.Controls.Find("txtName" + Convert.ToString(child),
                                            true)[0
                                            ];
                                txtBox[1] =
                                    (TextBox)
                                        panel_add_DIYControls.Controls.Find("txtValue" + Convert.ToString(child),
                                            true)[
                                                0];

                                if (checkboxSelect.Checked)
                                {
                                    txtBox[1].Text = valueDIY;
                                }
                            }
                        }
                        else if (Convert.ToInt16(splitted_Message.ElementAt(1 + 4*k)) == 7 || //PID返回的设置信息
                                 Convert.ToInt16(splitted_Message.ElementAt(1 + 4*k)) == 8) //自定义参数返回的设置信息
                        {
                            //确保有收到ACK信号
                            if (splitted_Message.ElementAt(3 + 4*k).Contains("ACK"))
                            {
                                if (_queueEchoControl.Count == 0) //队列中无元素
                                {
                                    //关闭进程
                                    //queueEchoControl.Dequeue();
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
                        }
                        else if (Convert.ToInt16(splitted_Message.ElementAt(1 + 4*k)) == 9) //紧急停车标志位
                        {
                            //确保有收到ACK信号
                            if (splitted_Message.ElementAt(3 + 4*k).Contains("ACK"))
                            {
                                if (_queueEchoControl.Count == 0) //队列中无元素
                                {
                                    //关闭进程
                                    //queueEchoControl.Dequeue();
                                    tmpSendHandle.Flag = 1;
                                    timer_Send2GetEcho.Stop();
                                }
                                else
                                {
                                    tmpSendHandle.Flag = 1;
                                }
                                label28.Text = "OK! Stopped! " + _queueEchoControl.Count;
                                retryCount = 0;
                            }
                        }

                        if (Convert.ToInt16(splitted_Message.ElementAt(1 + 4*k)) == 5 || //PID返回的ECHO信息
                            Convert.ToInt16(splitted_Message.ElementAt(1 + 4*k)) == 6) //自定义参数返回的ECHO信息
                        {
                            //if (splitted_Message.ElementAt(3 + 4 * k).Contains("ACK"))
                            //{
                            if (_queueEchoControl.Count == 0) //队列中无元素
                            {
                                //关闭进程
                                //queueEchoControl.Dequeue();
                                tmpSendHandle.Flag = 1;
                                timer_Send2GetEcho.Stop();
                            }
                            else
                            {
                                tmpSendHandle.Flag = 1;
                            }
                            label28.Text = "OK!! + Remain:" + _queueEchoControl.Count;
                            retryCount = 0;
                            //}
                        }
                    }
                }
            }
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
            _isReading = false;
            //acceptRichTextBox.Clear();
            //sendRichTextBox.Clear();
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

            //try
            //{
            //    //mySerialPort.ReadTimeout = (int)intervalTimeNumericUpDown.Value;
            //    //mySerialPort.WriteTimeout = (int)intervalTimeNumericUpDown.Value;
            //}
            //catch(Exception ee)
            //{
            //    mySerialPort.ReadTimeout = 500;
            //    mySerialPort.WriteTimeout = 500;
            //    MessageBox.Show(ee.Message);
            //}
            //更新状态栏的显示
            //statusDisplayToolStripStatusLabel.Text = string.Format(
            //    "当前打开的端口 {0}，波特率 {1}，奇偶校验 {2}， 数据位 {3}，停止位 {4}",
            //    portNameComboBox.SelectedItem, baudRateComboBox.SelectedItem, parityComboBox.SelectedItem,
            //    dataBitsComboBox.SelectedItem, stopBitsComboBox.SelectedItem);
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

        private void ResetToDefaultSettings()
        {
            //默认波特率
            comboBox_baudrate.SelectedItem = "9600";
            //默认不校验
            comboBox_parity.SelectedIndex = 0;
            //默认数据位设置为8位
            comboBox_databit.SelectedIndex = 0;
            //默认停止位设置为1位
            comboBox_stopbit.SelectedIndex = 0;
        }

        private int GetSelectedBaudRate()
        {
            var baudRate = 0;
            if (!(int.TryParse(comboBox_baudrate.SelectedItem.ToString(), out baudRate)))
            {
                baudRate = 9600;
            }
            return baudRate;
        }

        private int GetSelectedDataBits()
        {
            var dataBits = 8;
            if (!(int.TryParse(comboBox_databit.SelectedItem.ToString(), out dataBits)))
            {
                MessageBox.Show("转换失败！");
            }
            return dataBits;
        }

        private Parity GetSelectedParity()
        {
            var parity = Parity.None;
            switch (comboBox_parity.SelectedItem.ToString())
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
                    _hasStartSend = true;
                    //SetSerialPortPropertiesBeforeSending();

                    timer_autoSend.Enabled = true;
                    timer_autoSend.Interval = Convert.ToInt32(textBox_sendPeriod.Text);
                    timer_autoSend.Start();
                }
                else
                {
                    _autoSend = false;
                    checkBox_sendAuto.Enabled = true;
                    button_sendmessage.Text = "发送数据";
                    //initialText = string.Empty;
                    _hasStartSend = false;
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
            //车的传感器类型

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

        private void button_ModifyPID_Click(object sender, EventArgs e)
        {
            if (radioButton_FourWheel.Checked) //四轮车PID
            {
                //舵机PID参数
                var steer_P = (int) (Convert.ToDouble(textBox_Steer_P.Text)*1000);
                var steer_I = (int) (Convert.ToDouble(textBox_Steer_I.Text)*1000);
                var steer_D = (int) (Convert.ToDouble(textBox_Steer_D.Text)*1000);

                //电机PID参数
                var motor_P = (int) (Convert.ToDouble(textBox_Motor_P.Text)*1000);
                var motor_I = (int) (Convert.ToDouble(textBox_Motor_I.Text)*1000);
                var motor_D = (int) (Convert.ToDouble(textBox_Motor_D.Text)*1000);

                var tmpMessageSteer = "P" + steer_P + "I" + steer_I + "D" + steer_D;
                var tmpMessageMotor = "P" + motor_P + "I" + motor_I + "D" + motor_D;

                try
                {
                    var NeedSend = "";
                    var head = "#";
                    var end = "$";

                    //PID值需要连续发送
                    //以下是其处理方式
                    //舵机PID
                    if (Math.Abs(steer_P_old - steer_P) > 0 || //有变化才发送
                        Math.Abs(steer_I_old - steer_I) > 0 ||
                        Math.Abs(steer_D_old - steer_D) > 0)
                    {
                        //mySerialPort.Write(FormPackage(1, 1, tmpMessageSteer));
                        NeedSend += FormPackage_NOHead_NOEnd(1, 1, tmpMessageSteer);
                        steer_P_old = steer_P;
                        steer_I_old = steer_I;
                        steer_D_old = steer_D;
                    }

                    //电机PID
                    if (Math.Abs(motor_P_old - motor_P) > 0 || //有变化才发送
                        Math.Abs(motor_I_old - motor_I) > 0 ||
                        Math.Abs(motor_D_old - motor_D) > 0)
                    {
                        //mySerialPort.Write(FormPackage(1, 2, tmpMessageMotor));
                        NeedSend += FormPackage_NOHead_NOEnd(1, 2, tmpMessageSteer);
                        motor_P_old = motor_P;
                        motor_I_old = motor_I;
                        motor_D_old = motor_D;
                    }

                    if (NeedSend.Length > 0)
                    {
                        NeedSend = head + NeedSend + end;
                        mySerialPort.Write(NeedSend);

                        var tmpHandle = new GetEchoForm(0, NeedSend);
                        _queueEchoControl.Enqueue(tmpHandle);
                        timer_Send2GetEcho.Start();
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }
            }
            else if (radioButton_BalanceCar.Checked) //直立车PID参数的获取
            {
                //直立PID
                var stand_P = (int) (Convert.ToDouble(textBox_Stand_P.Text)*1000);
                var stand_I = (int) (Convert.ToDouble(textBox_Stand_I.Text)*1000);
                var stand_D = (int) (Convert.ToDouble(textBox_Stand_D.Text)*1000);

                //速度PID
                var speed_P = (int) (Convert.ToDouble(textBox_Speed_P.Text)*1000);
                var speed_I = (int) (Convert.ToDouble(textBox_Speed_I.Text)*1000);
                var speed_D = (int) (Convert.ToDouble(textBox_Speed_D.Text)*1000);

                //方向PID
                var direction_P = (int) (Convert.ToDouble(textBox_Direction_P.Text)*1000);
                var direction_I = (int) (Convert.ToDouble(textBox_Direction_I.Text)*1000);
                var direction_D = (int) (Convert.ToDouble(textBox_Direction_D.Text)*1000);

                var tmpMessageStand = "P" + stand_P + "I" + stand_I + "D" + stand_D;
                var tmpMessageSpeed = "P" + speed_P + "I" + speed_I + "D" + speed_D;
                var tmpMessageDirection = "P" + direction_P + "I" + direction_I + "D" + direction_D;

                try
                {
                    var NeedSend = "";
                    var head = "#";
                    var end = "$";
                    //直立PID
                    if (Math.Abs(stand_P_old - stand_P) > 0 || //有变化才发送
                        Math.Abs(stand_I_old - stand_I) > 0 ||
                        Math.Abs(stand_D_old - stand_D) > 0)
                    {
                        //mySerialPort.Write(FormPackage(1, 3, tmpMessageStand));
                        NeedSend += FormPackage_NOHead_NOEnd(1, 3, tmpMessageStand);
                        stand_P_old = stand_P;
                        stand_I_old = stand_I;
                        stand_D_old = stand_D;
                    }

                    //速度PID
                    if (Math.Abs(speed_P_old - speed_P) > 0 ||
                        Math.Abs(speed_I_old - speed_I) > 0 ||
                        Math.Abs(speed_D_old - speed_D) > 0)
                    {
                        //mySerialPort.Write(FormPackage(1, 4, tmpMessageSpeed));
                        NeedSend += FormPackage_NOHead_NOEnd(1, 4, tmpMessageSpeed);
                        speed_P_old = speed_P;
                        speed_I_old = speed_I;
                        speed_D_old = speed_D;
                    }

                    //方向PID
                    if (Math.Abs(_directionPOld - direction_P) > 0 ||
                        Math.Abs(_directionIOld - direction_I) > 0 ||
                        Math.Abs(_directionDOld - direction_D) > 0)
                    {
                        //mySerialPort.Write(FormPackage(1, 5, tmpMessageDirection));
                        NeedSend += FormPackage_NOHead_NOEnd(1, 5, tmpMessageDirection);
                        _directionPOld = direction_P;
                        _directionIOld = direction_I;
                        _directionDOld = direction_D;
                    }
                    if (NeedSend.Length > 0)
                    {
                        NeedSend = head + NeedSend + end;
                        mySerialPort.Write(NeedSend);

                        var tmpHandle = new GetEchoForm(0, NeedSend);
                        _queueEchoControl.Enqueue(tmpHandle);
                        timer_Send2GetEcho.Start();
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }
            }
            else
            {
                MessageBox.Show(@"无有效车型信息！");
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
                tmpMessageSteer = FormPackage(1, 1, tmpMessageSteer);
                mySerialPort.Write(tmpMessageSteer);

                var tmpHandle = new GetEchoForm(0, tmpMessageSteer);
                _queueEchoControl.Enqueue(tmpHandle);
                timer_Send2GetEcho.Start();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void button_ModifyPID_Motor_Click(object sender, EventArgs e)
        {
            //电机PID参数
            var Motor_P = Convert.ToDouble(textBox_Motor_P.Text)*1000;
            var Motor_I = Convert.ToDouble(textBox_Motor_I.Text)*1000;
            var Motor_D = Convert.ToDouble(textBox_Motor_D.Text)*1000;


            var tmpMessageSteer = "P" + Math.Floor(Motor_P).ToString(CultureInfo.InvariantCulture) +
                                  "I" + Math.Floor(Motor_I).ToString(CultureInfo.InvariantCulture) +
                                  "D" + Math.Floor(Motor_D).ToString(CultureInfo.InvariantCulture);

            try
            {
                SaveConfig(SavefileName);
                tmpMessageSteer = FormPackage(1, 2, tmpMessageSteer);
                mySerialPort.Write(tmpMessageSteer);

                var tmpHandle = new GetEchoForm(0, tmpMessageSteer);
                _queueEchoControl.Enqueue(tmpHandle);
                timer_Send2GetEcho.Start();
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
                tmpMessageStand = FormPackage(1, 3, tmpMessageStand);
                mySerialPort.Write(tmpMessageStand);

                label28.Text = @"Loading....";

                var tmpHandle = new GetEchoForm(0, tmpMessageStand);
                _queueEchoControl.Enqueue(tmpHandle);
                timer_Send2GetEcho.Start();
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
                tmpMessageSpeed = FormPackage(1, 4, tmpMessageSpeed);
                mySerialPort.Write(tmpMessageSpeed);

                label28.Text = @"Loading....";
                var tmpHandle = new GetEchoForm(0, tmpMessageSpeed);
                _queueEchoControl.Enqueue(tmpHandle);
                timer_Send2GetEcho.Start();
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
                tmpMessageDirection = FormPackage(1, 5, tmpMessageDirection);
                mySerialPort.Write(tmpMessageDirection);

                label28.Text = @"Loading....";
                var tmpHandle = new GetEchoForm(0, tmpMessageDirection);
                _queueEchoControl.Enqueue(tmpHandle);
                timer_Send2GetEcho.Start();
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
            if (textBox_DIY_Number.Text == "") //判断数量是否为空
            {
                MessageBox.Show(@"请输入需要的参数数量！", @"错误");
                return;
            }

            //出现弹窗，说明确认更改数量
            var dr = MessageBox.Show(@"确认更改数量？", @"修改", MessageBoxButtons.OKCancel);
            if (dr == DialogResult.OK)
            {
                panel_add_DIYControls.AutoScroll = true; //为panel添加滚动条
                panel_add_DIYControls.Controls.Clear(); //清空已有
                var total = int.Parse(textBox_DIY_Number.Text); //题目总数
                var txtBoxName = new TextBox();
                var txtBoxValue = new TextBox();
                var checkboxSelect = new CheckBox();
                var submitButton = new Button();

                for (var i = 0; i < total; i++)
                {
                    //是否启用的选项
                    checkboxSelect = new CheckBox();
                    checkboxSelect.Size = new Size(50, 20);
                    checkboxSelect.Location = new Point(30 + 70*0, 30*(i + 1)); //textbox坐标
                    checkboxSelect.Name = "checkBox_Def" + Convert.ToString(i + 1);
                    checkboxSelect.Text = (i + 1).ToString();
                    checkboxSelect.CheckedChanged += CheckboxSelectOnCheckedChanged;
                    checkboxSelect.Checked = false;

                    //名字
                    txtBoxName = new TextBox();
                    txtBoxName.Size = new Size(50, 50); //textbox大小                   
                    txtBoxName.Location = new Point(30 + 70*1, 30*(i + 1)); //textbox坐标
                    txtBoxName.Name = "txtName" + Convert.ToString(i + 1); //设定控件名称
                    txtBoxName.TextAlign = HorizontalAlignment.Center;
                    txtBoxName.Text = @"数据" + Convert.ToString(i + 1);
                    txtBoxName.Enabled = false;

                    //值
                    txtBoxValue = new TextBox();
                    txtBoxValue.Size = new Size(50, 50); //textbox大小                   
                    txtBoxValue.Location = new Point(30 + 70*2, 30*(i + 1)); //textbox坐标
                    txtBoxValue.Name = "txtValue" + Convert.ToString(i + 1); //设定控件名称
                    txtBoxValue.Text = "1.00";
                    txtBoxValue.TextAlign = HorizontalAlignment.Center;
                    txtBoxValue.Enabled = false;

                    //按钮
                    submitButton = new Button();
                    submitButton.Size = new Size(50, 20); //textbox大小                   
                    submitButton.Location = new Point(30 + 70*3, 30*(i + 1)); //textbox坐标
                    submitButton.Name = "buttonSubmit" + Convert.ToString(i + 1); //设定控件名称
                    submitButton.Text = @"修改";
                    submitButton.Click += SubmitButtonOnClick;
                    submitButton.Enabled = false;

                    panel_add_DIYControls.Controls.Add(checkboxSelect);
                    panel_add_DIYControls.Controls.Add(txtBoxName); //把"名字"加入到panel中
                    panel_add_DIYControls.Controls.Add(txtBoxValue); //把"值"加入到panel中
                    panel_add_DIYControls.Controls.Add(submitButton);
                }

                SaveConfig(SavefileName);
            }
        }

        private void Init_DIY_DynamicControls(int number)
        {
            var sp = new SharedPreferences(SavefileName);
            if (sp.ConfigFileExists)
            {
                if (number < 1)
                {
                    MessageBox.Show(@"请输入需要的参数数量！", @"错误");
                    return;
                }
                panel_add_DIYControls.AutoScroll = true; //为panel添加滚动条
                panel_add_DIYControls.Controls.Clear(); //清空已有
                var txtBoxName = new TextBox();
                var txtBoxValue = new TextBox();
                var checkboxSelect = new CheckBox();

                var submitButton = new Button();

                for (var i = 0; i < number; i++)
                {
                    //是否启用的选项
                    checkboxSelect = new CheckBox();
                    checkboxSelect.Size = new Size(50, 20);
                    checkboxSelect.Location = new Point(30 + 70*0, 30*(i + 1)); //textbox坐标
                    checkboxSelect.Name = "checkBox_Def" + Convert.ToString(i + 1);
                    checkboxSelect.Text = (i + 1).ToString();
                    //状态栏选择情况
                    var checkState = string.Format("DIY_CheckState{0}", i + 1);
                    checkboxSelect.Checked = sp.GetBoolean(checkState, true);
                    checkboxSelect.CheckedChanged += CheckboxSelectOnCheckedChanged;


                    //名字
                    txtBoxName = new TextBox();
                    txtBoxName.Size = new Size(50, 50); //textbox大小                   
                    txtBoxName.Location = new Point(30 + 70*1, 30*(i + 1)); //textbox坐标
                    txtBoxName.Name = "txtName" + Convert.ToString(i + 1); //设定控件名称
                    txtBoxName.TextAlign = HorizontalAlignment.Center;
                    //自己定义的变量名称
                    var txtName_DIY = string.Format("DIY_TextName{0}", i + 1);
                    txtBoxName.Text = sp.GetString(txtName_DIY, "数据");
                    txtBoxName.TextChanged += DIYTextboxNameChanged;

                    //值
                    txtBoxValue = new TextBox();
                    txtBoxValue.Size = new Size(50, 50); //textbox大小                   
                    txtBoxValue.Location = new Point(30 + 70*2, 30*(i + 1)); //textbox坐标
                    txtBoxValue.Name = "txtValue" + Convert.ToString(i + 1); //设定控件名称
                    //自定义参数的数值
                    var txtValue_DIY = string.Format("DIY_TextValue{0}", i + 1);
                    txtBoxValue.Text = sp.GetString(txtValue_DIY, "1.0");
                    txtBoxValue.TextAlign = HorizontalAlignment.Center;

                    //按钮
                    submitButton = new Button();
                    submitButton.Size = new Size(50, 20); //textbox大小                   
                    submitButton.Location = new Point(30 + 70*3, 30*(i + 1)); //textbox坐标
                    submitButton.Name = "buttonSubmit" + Convert.ToString(i + 1); //设定控件名称
                    submitButton.Text = @"修改";
                    submitButton.Click += SubmitButtonOnClick;
                    //submitButton.Enabled = false;

                    if (!checkboxSelect.Checked)
                    {
                        txtBoxName.Enabled = false;
                        txtBoxValue.Enabled = false;
                        submitButton.Enabled = false;
                    }

                    panel_add_DIYControls.Controls.Add(checkboxSelect);
                    panel_add_DIYControls.Controls.Add(txtBoxName); //把"名字"加入到panel中
                    panel_add_DIYControls.Controls.Add(txtBoxValue); //把"值"加入到panel中
                    panel_add_DIYControls.Controls.Add(submitButton);
                }
            }
        }

        private void SubmitButtonOnClick(object sender, EventArgs eventArgs)
        {
            var btn_clk = (Button) sender;

            var id = GetNumber(btn_clk.Name) - 1;

            var txtBox = new TextBox(); //获取这个曲线的名称
            txtBox = (TextBox) panel_add_DIYControls.Controls.Find("txtValue" + Convert.ToString(id + 1), true)[0];

            var value = Convert.ToDouble(txtBox.Text)*1000.0;
            try
            {
                var tmpMessageStand = FormPackage(2, id + 1, Math.Floor(value).ToString());
                mySerialPort.Write(tmpMessageStand);

                label28.Text = "Loading...";
                var tmpHandle = new GetEchoForm(0, tmpMessageStand);
                _queueEchoControl.Enqueue(tmpHandle);
                timer_Send2GetEcho.Start();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
            //throw new NotImplementedException();
        }

        private void DIYTextboxNameChanged(object sender, EventArgs eventArgs)
        {
            var t = sender as TextBox;

            //TextBox textBox = (TextBox)sender;

            //textBox.Text = textBox.Text;
            //MessageBox.Show("Change DIYtxt");
            //throw new NotImplementedException();
        }

        private void CheckboxSelectOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            if (textBox_DIY_Number.Text == "")
            {
                MessageBox.Show(@"请输入需要的参数数量！", @"错误");
                return;
            }
            var total = Convert.ToInt32(textBox_DIY_Number.Text);
            for (var i = 0; i < total; i++)
            {
                var checkboxSelect = new CheckBox();
                checkboxSelect =
                    (CheckBox) panel_add_DIYControls.Controls.Find("checkBox_Def" + Convert.ToString(i + 1), true)[0];

                var txtBox = new TextBox[2]; //用控件数组来定义每一行的TextBox,总共3个TextBox
                txtBox[0] = (TextBox) panel_add_DIYControls.Controls.Find("txtName" + Convert.ToString(i + 1), true)[0];
                txtBox[1] = (TextBox) panel_add_DIYControls.Controls.Find("txtValue" + Convert.ToString(i + 1), true)[0];

                var btn = new Button();
                btn = (Button) panel_add_DIYControls.Controls.Find("buttonSubmit" + Convert.ToString(i + 1), true)[0];

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

            //throw new NotImplementedException();
        }

        private void button_DIY_Modify_Click(object sender, EventArgs e)
        {
            if (textBox_DIY_Number.Text == "")
            {
                MessageBox.Show(@"请输入需要的参数数量！", @"错误");
                return;
            }
            //=====================================
            //数据头和封包
            var NeedSend = "";
            var head = "#";
            var end = "$";
            //=====================================

            var total = Convert.ToInt32(textBox_DIY_Number.Text);
            for (var i = 0; i < total; i++)
            {
                var checkboxSelect = new CheckBox();
                checkboxSelect =
                    (CheckBox) panel_add_DIYControls.Controls.Find("checkBox_Def" + Convert.ToString(i + 1), true)[0];

                var txtBox = new TextBox[2]; //用控件数组来定义每一行的TextBox,总共3个TextBox
                txtBox[0] = (TextBox) panel_add_DIYControls.Controls.Find("txtName" + Convert.ToString(i + 1), true)[0];
                txtBox[1] = (TextBox) panel_add_DIYControls.Controls.Find("txtValue" + Convert.ToString(i + 1), true)[0];

                var tmpMessageStand = txtBox[1].Text;

                if (checkboxSelect.Checked)
                {
                    //第 i+1 个自定义参数
                    NeedSend += FormPackage_NOHead_NOEnd(2, i + 1, tmpMessageStand);
                }
            }

            try
            {
                NeedSend = head + NeedSend + end;
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

        #endregion

        #region 4.实时变量参数

        private void InitElectricity(int number)
        {
            var sp = new SharedPreferences(SavefileName);
            if (sp.ConfigFileExists)
            {
                if (number < 1)
                {
                    MessageBox.Show(@"请输入需要的参数数量！", @"错误");
                    return;
                }
                panel_Electricity.AutoScroll = true; //为panel添加滚动条
                panel_Electricity.Controls.Clear(); //清空已有
                var total = int.Parse(textBox_Electricity_Number.Text); //题目总数
                var labelElect = new TextBox();
                var txtBoxValue = new TextBox();

                for (var i = 0; i < total; i++)
                {
                    //名字
                    labelElect = new TextBox();
                    labelElect.Size = new Size(100, 20); //label大小                   
                    labelElect.Location = new Point(30 + 120*0, 30*(i + 1)); //textbox坐标
                    labelElect.Name = "txtElectName" + Convert.ToString(i + 1); //设定控件名称
                    //labelElect.TextAlign = ContentAlignment.MiddleCenter;
                    labelElect.Text = @"实时变量" + Convert.ToString(i + 1);
                    labelElect.TextAlign = HorizontalAlignment.Center;
                    //读取相应的值
                    var elec_Name = string.Format("textElectName{0}", i + 1);
                    labelElect.Text = sp.GetString(elec_Name, "1.0");

                    //值
                    txtBoxValue = new TextBox();
                    txtBoxValue.Size = new Size(50, 50); //textbox大小                   
                    txtBoxValue.Location = new Point(30 + 120*1, 30*(i + 1)); //textbox坐标
                    txtBoxValue.Name = "txtElectValue" + Convert.ToString(i + 1); //设定控件名称
                    txtBoxValue.TextAlign = HorizontalAlignment.Center;

                    //读取相应的值
                    var elec_Value = string.Format("txtElectValue{0}", i + 1);
                    txtBoxValue.Text = sp.GetString(elec_Value, "1.0");

                    panel_Electricity.Controls.Add(labelElect);
                    panel_Electricity.Controls.Add(txtBoxValue); //把"值"加入到panel中
                }
            }
        }

        private void button_electricity_NumConfirm_Click(object sender, EventArgs e)
        {
            if (textBox_Electricity_Number.Text == "")
            {
                MessageBox.Show(@"请输入需要的参数数量！", @"错误");
                return;
            } //判断题目数是否未填
            panel_Electricity.AutoScroll = true; //为panel添加滚动条
            panel_Electricity.Controls.Clear(); //清空已有
            var total = int.Parse(textBox_Electricity_Number.Text); //题目总数
            var labelElect = new TextBox();
            var txtBoxValue = new TextBox();

            for (var i = 0; i < total; i++)
            {
                //名字
                labelElect = new TextBox();
                labelElect.Size = new Size(100, 20); //label大小                   
                labelElect.Location = new Point(30 + 120*0, 30*(i + 1)); //textbox坐标
                labelElect.Name = "txtElectName" + Convert.ToString(i + 1); //设定控件名称
                //labelElect.BringToFront();
                labelElect.Text = @"实时变量" + Convert.ToString(i + 1);
                labelElect.TextAlign = HorizontalAlignment.Center;
                //labelElect.Enabled = false;

                //值
                txtBoxValue = new TextBox();
                txtBoxValue.Size = new Size(50, 50); //textbox大小
                txtBoxValue.Location = new Point(30 + 120*1, 30*(i + 1)); //textbox坐标
                txtBoxValue.Name = "txtElectValue" + Convert.ToString(i + 1); //设定控件名称
                txtBoxValue.Text = "1.00";
                txtBoxValue.TextAlign = HorizontalAlignment.Center;
                //txtBoxValue.Enabled = false;

                panel_Electricity.Controls.Add(labelElect);
                panel_Electricity.Controls.Add(txtBoxValue); //把"值"加入到panel中
            }

            SaveConfig(SavefileName);
        }

        private void Init_DI_DynamicControls(int number)
        {
            var sp = new SharedPreferences(SavefileName);
            if (sp.ConfigFileExists)
            {
                if (number < 1)
                {
                    MessageBox.Show(@"请输入需要的参数数量！", @"错误");
                    return;
                }
                panel_add_DIYControls.AutoScroll = true; //为panel添加滚动条
                panel_add_DIYControls.Controls.Clear(); //清空已有
                var txtBoxName = new TextBox();
                var txtBoxValue = new TextBox();
                var checkboxSelect = new CheckBox();

                var submitButton = new Button();

                for (var i = 0; i < number; i++)
                {
                    //是否启用的选项
                    checkboxSelect = new CheckBox();
                    checkboxSelect.Size = new Size(50, 20);
                    checkboxSelect.Location = new Point(30 + 70*0, 30*(i + 1)); //textbox坐标
                    checkboxSelect.Name = "checkBox_Def" + Convert.ToString(i + 1);
                    checkboxSelect.Text = (i + 1).ToString();
                    //状态栏选择情况
                    var checkState = string.Format("DIY_CheckState{0}", i + 1);
                    checkboxSelect.Checked = sp.GetBoolean(checkState, true);
                    checkboxSelect.CheckedChanged += CheckboxSelectOnCheckedChanged;


                    //名字
                    txtBoxName = new TextBox();
                    txtBoxName.Size = new Size(50, 50); //textbox大小                   
                    txtBoxName.Location = new Point(30 + 70*1, 30*(i + 1)); //textbox坐标
                    txtBoxName.Name = "txtName" + Convert.ToString(i + 1); //设定控件名称
                    txtBoxName.TextAlign = HorizontalAlignment.Center;
                    //自己定义的变量名称
                    var txtName_DIY = string.Format("DIY_TextName{0}", i + 1);
                    txtBoxName.Text = sp.GetString(txtName_DIY, "数据");
                    txtBoxName.TextChanged += DIYTextboxNameChanged;

                    //值
                    txtBoxValue = new TextBox();
                    txtBoxValue.Size = new Size(50, 50); //textbox大小                   
                    txtBoxValue.Location = new Point(30 + 70*2, 30*(i + 1)); //textbox坐标
                    txtBoxValue.Name = "txtValue" + Convert.ToString(i + 1); //设定控件名称
                    //自定义参数的数值
                    var txtValue_DIY = string.Format("DIY_TextValue{0}", i + 1);
                    txtBoxValue.Text = sp.GetString(txtValue_DIY, "1.0");
                    txtBoxValue.TextAlign = HorizontalAlignment.Center;

                    //按钮
                    submitButton = new Button();
                    submitButton.Size = new Size(50, 20); //textbox大小                   
                    submitButton.Location = new Point(30 + 70*3, 30*(i + 1)); //textbox坐标
                    submitButton.Name = "buttonSubmit" + Convert.ToString(i + 1); //设定控件名称
                    submitButton.Text = @"修改";
                    submitButton.Click += SubmitButtonOnClick;
                    //submitButton.Enabled = false;

                    if (!checkboxSelect.Checked)
                    {
                        txtBoxName.Enabled = false;
                        txtBoxValue.Enabled = false;
                        submitButton.Enabled = false;
                    }

                    panel_add_DIYControls.Controls.Add(checkboxSelect);
                    panel_add_DIYControls.Controls.Add(txtBoxName); //把"名字"加入到panel中
                    panel_add_DIYControls.Controls.Add(txtBoxValue); //把"值"加入到panel中
                    panel_add_DIYControls.Controls.Add(submitButton);
                }
            }
        }

        #endregion

        #region 6.CCD图像

        private string testCCD_Image()
        {
            var randInts = new int[128];
            var result = "";
            var rand = new Random();
            for (var i = 0; i < 128; i++)
            {
                randInts[i] = rand.Next(1, 255);
                result += Convert.ToChar(randInts[i]).ToString();
                //result += Convert.ToChar(0).ToString();
            }
            return result;
        }

        private int Average_ccd(List<int> p)
        {
            var sum = p.Select((t, i) => p.ElementAt(i)).Sum();

            var thresold = sum/p.Count;

            if (thresold > 230)
                thresold = 100;
            else if (thresold < 150)
                thresold = 200;
            else
            {
                thresold = Otsu(p);
            }

            return thresold;
        }

        private int Otsu(List<int> p)
        {
            //处理全是黑色时候的情况

            var threshold = 0;
            int g = 0, max = 0;
            int total = 0, total_low = 0;
            int u0 = 0, u1 = 0, count = 0, cnt = 0;
            var tmpData = new int[256];
            var j = 0;
            for (j = 5; j <= 122; j++)
            {
                tmpData[p.ElementAt(j)]++;
                total += p.ElementAt(j);
            }
            for (j = 0; j <= 254; j++)
            {
                cnt = tmpData[j];
                if (cnt == 0) continue; // 优化加速
                count += tmpData[j];
                total_low += cnt*j;
                u0 = total_low/count;
                if (count >= 118) break; // 优化加速 122 - 5+1
                u1 = (total - total_low)/(118 - count);
                g = ((u0 - u1)*(u0 - u1))*((count*(118 - count)))/16384;
                if (g > max)
                {
                    max = g;
                    threshold = j;
                }
            }

            return threshold;
        }

        private string CCD_FindBoard(List<int> p)
        {
            var black = "Border";
            for (var i = 0; i < p.Count - 1; i++)
            {
                if ((p.ElementAt(i) == 255 && p.ElementAt(i + 1) == 0) ||
                    (p.ElementAt(i) == 0 && p.ElementAt(i + 1) == 255))
                {
                    black += i + " ";
                }
            }
            return black;
        }

        private void CCD_DrawActual(Bitmap bitmap, List<int> recvBuff)
        {
            var bitmapData =
                bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadWrite, bitmap.PixelFormat);

            var bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat)/8;
            var byteCount = bitmapData.Stride*bitmap.Height;
            var pixels = new byte[byteCount];
            var ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            var heightInPixels = bitmapData.Height;
            var widthInBytes = bitmapData.Width*bytesPerPixel;

            for (var y = 0; y < heightInPixels; y++)
            {
                var currentLine = y*bitmapData.Stride;

                for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    var grey = recvBuff.ElementAt(Convert.ToInt16(x/4));

                    // calculate new pixel value
                    pixels[currentLine + x] = (byte) grey;
                    pixels[currentLine + x + 1] = (byte) grey;
                    pixels[currentLine + x + 2] = (byte) grey;
                    pixels[currentLine + x + 3] = 255;
                }
            }
            // copy modified bytes back
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bitmap.UnlockBits(bitmapData);
        }

        private void CCD_DrawPath(Bitmap bitmap, List<int> recBuff)
        {
            var bitmapData =
                bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadWrite, bitmap.PixelFormat);

            var bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat)/8;
            var byteCount = bitmapData.Stride*bitmap.Height;
            var pixels = new byte[byteCount];
            var ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            var heightInPixels = bitmapData.Height;
            var widthInBytes = bitmapData.Width*bytesPerPixel;

            for (var y = 1; y < heightInPixels; y++)
            {
                var currentLine = y*bitmapData.Stride;
                var formerLine = (y - 1)*bitmapData.Stride;

                for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int oldBlue = pixels[currentLine + x];
                    int oldGreen = pixels[currentLine + x + 1];
                    int oldRed = pixels[currentLine + x + 2];

                    pixels[formerLine + x] = (byte) oldBlue;
                    pixels[formerLine + x + 1] = (byte) oldGreen;
                    pixels[formerLine + x + 2] = (byte) oldRed;
                    pixels[formerLine + x + 3] = 255;
                }
            }

            for (var y = heightInPixels - 1; y < heightInPixels; y++)
            {
                var currentLine = y*bitmapData.Stride;

                for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    var leng = recBuff.Count;
                    var grey = recBuff.ElementAt(Convert.ToInt16(x/4));

                    // calculate new pixel value
                    pixels[currentLine + x] = (byte) grey;
                    pixels[currentLine + x + 1] = (byte) grey;
                    pixels[currentLine + x + 2] = (byte) grey;
                    pixels[currentLine + x + 3] = 255;
                }
            }
            // copy modified bytes back
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            bitmap.UnlockBits(bitmapData);
        }

        private void ProcessUsingLockbits(Bitmap processedBitmap)
        {
            var bitmapData =
                processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height),
                    ImageLockMode.ReadWrite, processedBitmap.PixelFormat);

            var bytesPerPixel = Image.GetPixelFormatSize(processedBitmap.PixelFormat)/8;
            var byteCount = bitmapData.Stride*processedBitmap.Height;
            var pixels = new byte[byteCount];
            var ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            var heightInPixels = bitmapData.Height;
            var widthInBytes = bitmapData.Width*bytesPerPixel;

            for (var y = 0; y < heightInPixels; y++)
            {
                var currentLine = y*bitmapData.Stride;
                for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    int oldBlue = pixels[currentLine + x];
                    int oldGreen = pixels[currentLine + x + 1];
                    int oldRed = pixels[currentLine + x + 2];

                    // calculate new pixel value
                    pixels[currentLine + x] = (byte) oldBlue;
                    pixels[currentLine + x + 1] = (byte) oldGreen;
                    pixels[currentLine + x + 2] = (byte) oldRed;
                }
            }

            // copy modified bytes back
            Marshal.Copy(pixels, 0, ptrFirstPixel, pixels.Length);
            processedBitmap.UnlockBits(bitmapData);
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
                _listZed[i] = new PointPairList();
                coOb[i] = new CallObject();
            }

            var sp = new SharedPreferences(SavefileName);
            for (var i = 0; i < ScopeNumber; i++)
            {
                _listZed[i].Add(Convert.ToDouble(_countZedgraph[i]), 0);
                var curve = string.Format("波形{0}", i + 1);

                //自己定义的变量名称
                var txtName_DIY = string.Format("SCOPE_TextName{0}", i + 1);
                var getName = sp.GetString(txtName_DIY, @"波形" + Convert.ToString(i + 1));
                zedGraph_local.GraphPane.AddCurve(getName.Trim() != "" ? getName : curve, _listZed[i], _colorLine[i%8],
                    SymbolType.None);
            }

            // up the proper scrolling parameters
            zedGraph_local.AxisChange();
            // Make sure the Graph gets redrawn
            zedGraph_local.Invalidate();
        }

        private void Init_pane_Scope()
        {
            var sp = new SharedPreferences(SavefileName);

            panel_Scope.AutoScroll = true;
            var total = ScopeNumber;
            var checkDrawing = new CheckBox();
            var txtBoxName = new TextBox();
            var buttonNewForm = new Button();

            for (var i = 0; i < total; i++)
            {
                //是否启用的选项
                checkDrawing = new CheckBox();
                checkDrawing.Size = new Size(15, 15);
                checkDrawing.Location = new Point(20 + i%8*77, 10 + i/8*60); //textbox坐标
                checkDrawing.Name = "checkBox_Def" + Convert.ToString(i + 1);
                checkDrawing.Text = "";
                //状态栏选择情况
                var checkState = string.Format("SCOPE_CheckState{0}", i + 1);
                checkDrawing.Checked = sp.GetBoolean(checkState, false);
                checkDrawing.CheckedChanged += CheckDrawingOnCheckedChanged;

                //名字
                txtBoxName = new TextBox();
                txtBoxName.Size = new Size(50, 20); //textbox大小                   
                txtBoxName.Location = new Point(41 + i%8*77, 7 + i/8*60); //textbox坐标
                txtBoxName.Name = "txtName" + Convert.ToString(i + 1); //设定控件名称
                txtBoxName.TextAlign = HorizontalAlignment.Center;
                //自己定义的变量名称
                var txtName_DIY = string.Format("SCOPE_TextName{0}", i + 1);
                var getName = sp.GetString(txtName_DIY, @"波形" + Convert.ToString(i + 1));
                txtBoxName.Text = getName.Trim() != "" ? getName : string.Format("波形{0}", i + 1);
                txtBoxName.BackColor = _colorLine[i%8];
                txtBoxName.TextChanged += ScopeTextNameChanged;
                //txtBoxName.ForeColor = Color.Black;
                txtBoxName.Enabled = checkDrawing.Checked;

                //值
                buttonNewForm = new Button();
                buttonNewForm.Size = new Size(66, 23); //button大小
                buttonNewForm.Location = new Point(25 + i%8*77, 34 + i/8*60); //textbox坐标
                buttonNewForm.Name = "buttonDraw" + Convert.ToString(i + 1); //设定控件名称
                buttonNewForm.Text = @"独立图像";
                buttonNewForm.Click += ButtonNewFormOnClick;
                buttonNewForm.Enabled = checkDrawing.Checked;

                panel_Scope.Controls.Add(checkDrawing);
                panel_Scope.Controls.Add(txtBoxName); //把"名字"加入到panel中
                panel_Scope.Controls.Add(buttonNewForm); //把"值"加入到panel中
            }
        }

        private void timer_fresh_Tick(object sender, EventArgs e)
        {
            zedGraph_local.AxisChange();
            ////z1.Invalidate();
            zedGraph_local.Refresh();
        }

        private void ScopeTextNameChanged(object sender, EventArgs eventArgs) //曲线的名字改变事件
        {
            var txtBox = sender as TextBox;

            var id = GetNumber(txtBox.Name) - 1;

            zedGraph_local.GraphPane.CurveList[id].Label.Text = txtBox.Text;

            zedGraph_local.Invalidate();
            //throw new NotImplementedException();
        }

        private string MyPointValueHandler(ZedGraphControl control, GraphPane pane,
            CurveItem curve, int iPt)
        {
            // Get the PointPair that is under the mouse
            var pt = curve[iPt];

            return curve.Label.Text + " is " + pt.Y.ToString("f2") + " units at " + pt.X.ToString("f1");
        }

        private void CheckDrawingOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            var total = ScopeNumber;

            var checkchangBox = (CheckBox) sender;
            var id = GetNumber(checkchangBox.Name) - 1;

            var txtBox = new TextBox(); //用控件数组来定义每一行的TextBox,总共3个TextBox
            txtBox = (TextBox) panel_Scope.Controls.Find("txtName" + Convert.ToString(id + 1), true)[0];

            var buttonNewForm = new Button();
            buttonNewForm = (Button) panel_Scope.Controls.Find("buttonDraw" + Convert.ToString(id + 1), true)[0];

            if (checkchangBox.Checked)
            {
                txtBox.Enabled = true;
                buttonNewForm.Enabled = true;

                if (!mySerialPort.IsOpen)
                {
                    for (var j = 0; j < _zedListX[id].Count; j++)
                    {
                        _listZed[id].Add(_zedListX[id].ElementAt(j), _zedListY[id].ElementAt(j));
                    }
                }
                else
                {
                    timer_fresh.Start();
                }
            }
            else
            {
                txtBox.Enabled = false;
                buttonNewForm.Enabled = false;

                if (!mySerialPort.IsOpen)
                {
                    _listZed[id].RemoveRange(0, _listZed[id].Count);
                }
            }
            //throw new NotImplementedException();

            zedGraph_local.AxisChange();
            ////z1.Invalidate();
            zedGraph_local.Refresh();
        }

        private void ButtonNewFormOnClick(object sender, EventArgs eventArgs)
        {
            var btn_clk = (Button) sender;

            var id = GetNumber(btn_clk.Name) - 1;

            if (_singleWindowShowed[id] == false)
            {
                _singleWindowShowed[id] = true;

                var txtBox = new TextBox(); //获取这个曲线的名称
                txtBox = (TextBox) panel_Scope.Controls.Find("txtName" + Convert.ToString(id + 1), true)[0];

                var singleWindow = new ZedGraphSingleWindow(id, coOb[id], txtBox.Text);

                singleWindow.SingnleClosedEvent += SingleWindowClosed_RecvInfo;

                singleWindow.Show();
            }
            else
            {
                MessageBox.Show(@"不能重复创建！已经有了一个窗口！");
            }
            //throw new NotImplementedException();
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

        private void button_ClearDrawing_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < ScopeNumber; i++) //首先清除所有的曲线数据
            {
                _listZed[i].RemoveRange(0, _listZed[i].Count);
                _countZedgraph[i] = -1;
            }

            zedGraph_local.GraphPane.XAxis.Scale.MinAuto = true;
            zedGraph_local.GraphPane.XAxis.Scale.MaxAuto = true;
            zedGraph_local.GraphPane.YAxis.Scale.MinAuto = true;
            zedGraph_local.GraphPane.YAxis.Scale.MaxAuto = true;

            // up the proper scrolling parameters
            zedGraph_local.AxisChange();
            // Make sure the Graph gets redrawn
            zedGraph_local.Invalidate();
        }

        private void SingleWindowClosed_RecvInfo(int id)
        {
            //MessageBox.Show("recve: " + id.ToString());
            _singleWindowShowed[id] = false;
        }

        #endregion

        #region 8.全局参数

        private void button_ReadAllSettings_Click(object sender, EventArgs e)
        {
            try
            {
                var tmpMessage = FormPackage(3, 1, "0");
                mySerialPort.Write(tmpMessage);

                label28.Text = @"Loading....";
                var tmpHandle = new GetEchoForm(0, tmpMessage);
                _queueEchoControl.Enqueue(tmpHandle);
                timer_Send2GetEcho.Start();
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
            _dequeueFirstTime = false;

            try
            {
                var tmpMessage = FormPackage(3, 2, "0");
                mySerialPort.Write(tmpMessage);

                label28.Text = @"Loading....";
                var tmpHandle = new GetEchoForm(0, tmpMessage);
                _queueEchoControl.Enqueue(tmpHandle);
                timer_Send2GetEcho.Start();
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

                //自定义参数
                var totalDIY = 0;
                if (textBox_DIY_Number.Text == "")
                {
                    //essageBox.Show(@"请重新输入需要的参数数量！", @"错误");
                    totalDIY = 0;
                }
                totalDIY = Convert.ToInt32(textBox_DIY_Number.Text);

                sw.WriteLine("DIY_Num:{0}", totalDIY);

                for (var i = 0; i < totalDIY; i++)
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

                //示波器参数
                var countScope = 0;
                var checkDrawing = new CheckBox[ScopeNumber];
                var txtBox_Scope = new TextBox[ScopeNumber]; //用控件数组来定义每一行的TextBox,总共3个TextBox
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

                //将曲线数据保存
                //MessageBox.Show(listZed[0].ElementAt(1).ToString());
                //(1,3)
                for (var i = 0; i < ScopeNumber; i++)
                {
                    if (checkDrawing[i].Checked)
                    {
                        sw.WriteLine("ScopeLineOrder:{0}", i);
                        sw.WriteLine("PointNumber:{0}", _listZed[i].Count);

                        for (var j = 0; j < _listZed[i].Count; j++)
                        {
                            sw.Write("{0}", _listZed[i].ElementAt(j).ToString().Replace(" ", ""));
                        }
                        sw.WriteLine("");
                    }
                }

                //...
                sw.Close();
                fs.Close();
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
                    int carType = Convert.ToInt16(lines.ElementAt(countLine).Split(':').ElementAt(1));
                    countLine++;

                    if (carType == 0) //直立车
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
                    else if (carType == 1) //四轮车
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

                    //自定义参数
                    if (lines.ElementAt(countLine).Contains("DIY_Num"))
                    {
                        int diyNum = Convert.ToInt16(lines.ElementAt(countLine).Split(':').ElementAt(1));
                        ++countLine;

                        if (diyNum == Convert.ToInt16(textBox_DIY_Number.Text))
                        {
                            for (var i = 0; i < diyNum; i++)
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

                            for (var i = 0; i < diyNum; i++)
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

                    //Scope
                    for (var i = 0; i < ScopeNumber; i++) //首先清除所有的曲线数据
                    {
                        _listZed[i].RemoveRange(0, _listZed[i].Count);
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
                                        _listZed[scopeOrder].Add(x, y);
                                        _zedListX[scopeOrder].Add(x);
                                        _zedListY[scopeOrder].Add(y);
                                    }
                                }
                            }
                        }
                        //UpdateScope Here
                        zedGraph_local.GraphPane.XAxis.Scale.MaxAuto = true;
                        zedGraph_local.GraphPane.XAxis.Scale.MinAuto = true;
                        zedGraph_local.GraphPane.YAxis.Scale.MaxAuto = true;
                        zedGraph_local.GraphPane.YAxis.Scale.MinAuto = true;

                        zedGraph_local.AxisChange();
                        zedGraph_local.Invalidate();
                    } //Scope End
                }
                _isLoadHistory = true;

                sr.Close();
                fs.Close();
            }
        }

        #endregion
    }
}