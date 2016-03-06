using System;
using System.Drawing;
using System.Security.AccessControl;
using System.Windows.Forms;
using ZedGraph;

namespace Freescale_debug
{
    public delegate void SingleWindowClosedDelegate(int id);
    public partial class ZedGraphSingleWindow : Form
    {
        public event SingleWindowClosedDelegate SingnleClosedEvent;

        private readonly int curveNumber;  //曲线号
        private readonly string curveName; //曲线命
        private double _valueXStart;
        private int _zedWidth;
        private Boolean _pauseFlag;

        private readonly Color[] _colorLine = {Color.Green, Color.DodgerBlue, Color.Brown, Color.Chartreuse,
                                             Color.CornflowerBlue, Color.Red, Color.Yellow, Color.Gray};

        private readonly PointPairList _listZed = new PointPairList();

        public ZedGraphSingleWindow(int id, CallObject coV, string name)
        {
            InitializeComponent();

            coV.ValueUpdatedEvent += co_UpdateCurveEvent;
            curveNumber = id;
            curveName = name;
            Text = name + @"——曲线" + (curveNumber+1) + @"——" + @"飞思卡尔调试平台 V1.1";
        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        private void ZedGraph_SingleWindow_Load(object sender, EventArgs e)
        {
            InitzedGraph();
        }

        private void ZedGraph_SingleWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (SingnleClosedEvent != null) 
                SingnleClosedEvent(curveNumber);

            DialogResult = DialogResult.OK;
        }

        private void ZedGraph_SingleWindow_Resize(object sender, EventArgs e)
        {
            zedGraph_Single.Location = new Point(10, 10);
            // Leave a small margin around the outside of the control
            zedGraph_Single.Size = new Size(ClientRectangle.Width - 100,
                ClientRectangle.Height - 20);
        }

        private void InitzedGraph()
        {
            GraphPane myPane = zedGraph_Single.GraphPane;

            myPane.Title.IsVisible = false;
            myPane.XAxis.Title.Text = "time";
            myPane.YAxis.Title.Text = "Value";

            //show grid
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;

            // Align the Y axis labels so they are flush to the axis
            myPane.YAxis.Scale.Align = AlignP.Inside;

            // Fill the axis background with a gradient
            myPane.Chart.Fill = new Fill(Color.White, Color.LightGray, 45.0f);

            // OPTIONAL: Add a custom context menu item
            // OPTIONAL: Show tooltips when the mouse hovers over a point
            zedGraph_Single.IsShowPointValues = true;
            zedGraph_Single.PointValueEvent += MyPointValueHandler;

            zedGraph_Single.GraphPane.AddCurve(curveName, _listZed, _colorLine[curveNumber],
                SymbolType.None);

            refleshZedPane(zedGraph_Single);
        }

        private void refleshZedPane(ZedGraphControl zedGraph)
        {
            zedGraph.AxisChange();
            zedGraph.Invalidate();
        }

        /// <summary>
        /// Display customized tooltips when the mouse hovers over a point
        /// </summary>
        private string MyPointValueHandler(ZedGraphControl control, GraphPane pane,
                        CurveItem curve, int iPt)
        {
            // Get the PointPair that is under the mouse
            PointPair pt = curve[iPt];

            return curve.Label.Text + " is " + pt.Y.ToString("f2") + " units at " + pt.X.ToString("f1") + " days";
        }

        public void co_UpdateCurveEvent(double x, double y)
        {
            //listZed.Add(x, y);
            //timer_fresh.Start();
            if (!_pauseFlag)
            {
                if (_zedWidth == 0)
                {
                    //x轴无限制
                    _listZed.Add(_valueXStart++, y);
                }
                else
                {
                    //x轴宽度有限制
                    if (_valueXStart < _zedWidth)
                    {
                        _listZed.Add(_valueXStart++, y);
                    }
                    else
                    {
                        zedGraph_Single.GraphPane.XAxis.Scale.Min = (int) _valueXStart - _zedWidth < 0
                            ? 0
                            : (int) _valueXStart - _zedWidth;
                        zedGraph_Single.GraphPane.XAxis.Scale.Max = (int) _valueXStart + 0.2*_zedWidth;
                        _listZed.Add(_valueXStart++, y);
                    }
                }

                refleshZedPane(zedGraph_Single);
            }
        }

        private void button_ClearData_Click(object sender, EventArgs e)
        {
            _listZed.RemoveRange(0, _listZed.Count);
            _valueXStart = 0;
            zedGraph_Single.GraphPane.XAxis.Scale.MinAuto = true;
            zedGraph_Single.GraphPane.XAxis.Scale.MaxAuto = true;

            zedGraph_Single.GraphPane.YAxis.Scale.MinAuto = true;
            zedGraph_Single.GraphPane.YAxis.Scale.MaxAuto = true;

            refleshZedPane(zedGraph_Single);
        }

        private void timer_fresh_Tick(object sender, EventArgs e)
        {
            refleshZedPane(zedGraph_Single);
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            TextBox txtbox = sender as TextBox;
            if (txtbox != null && txtbox.Text.Trim() == @"")
            {
                txtbox.Text = @"请输入X轴宽度";
                txtbox.ForeColor = Color.Gray;
            }
        }
        private void textBox1_Enter(object sender, EventArgs e)
        {
            TextBox txtbox = sender as TextBox;
            if (txtbox != null && txtbox.Text.Trim() == @"请输入X轴宽度")
            {
                txtbox.Text = @"";
                txtbox.ForeColor = Color.Black;
            }
        }
        private void button_ShowRange_Click(object sender, EventArgs e)
        {
            //对x的范围进行更改
            if (textBox1.Text.Trim() != @"请输入X轴宽度")
            {
                _zedWidth = Convert.ToInt16(textBox1.Text);
            }
            else
                MessageBox.Show(@"无数字输入，请重新输入");
        }

        private void button_pause_Click(object sender, EventArgs e)
        {
            if (button_pause.Text == @"暂停")
            {
                _pauseFlag = true;
                button_pause.Text = @"继续";
            }
            else
            {
                _pauseFlag = false;
                button_pause.Text = @"暂停";
            }
        }
    }

    //进行窗体间传参的公共接口
    public delegate void ValueUpdatedHandler(double x, double y);
    public class CallObject
    {
        public event ValueUpdatedHandler ValueUpdatedEvent;

        public void CallEvent(double x, double y)
        {
            var onValueUpdatedEvent = ValueUpdatedEvent;
            if (onValueUpdatedEvent != null) onValueUpdatedEvent(x, y);
        }
    }
}
