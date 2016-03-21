using System;
using System.Windows.Forms;
using leomon;

namespace Freescale_debug
{
    //class Configuration
    public partial class Form1 : Form
    {
        private void SaveConfig(string fileName)
        {
            var editor = new Editor();

            WritePortSetup(editor);
            WritePidSetup(editor);
            WriteRealTimeSetup(editor);
            WriteDiySetup(editor);
            WriteScopeSetup(editor);

            WriteToFile(fileName, editor);
        }

        private void LoadConfig(string fileName)
        {
            var sp = new SharedPreferences(fileName);
            if (sp.ConfigFileExists)
            {
                ReadPortSetup(sp);
                ReadPIDSetup(sp);
                ReadDIYNumberSetup(sp);
                ReadRealTimeSetup(sp);

                textBox_receive.Clear();
            }
            else
            {
                //载入默认设置
                ResetToDefaultSettings();
            }
        }

        private void WritePortSetup(Editor editor)
        {
            if (hasPorts)
            {
                //保存选中的端口
                editor.PutInt32("selectedPort", comboBox_port.SelectedIndex);
                //保存设置的波特率
                editor.PutInt32("baudRate", comboBox_baudrate.SelectedIndex);
                //保存设置的校验方式
                editor.PutInt32("parity", comboBox_parity.SelectedIndex);
                //保存设置的数据位
                editor.PutInt32("dataBits", comboBox_databit.SelectedIndex);
                //保存设置的停止位
                editor.PutInt32("stopBits", comboBox_stopbit.SelectedIndex);
                //保存自动发送数据时间间隔
                editor.PutInt32("intervalTime", Convert.ToInt32(textBox_sendPeriod.Text));
                //保存接收方式
                editor.PutBoolean("acceptChar", checkBox_receiveHex.Checked);
                //保存标志变量，即是否在接收框中显示信息。
                editor.PutBoolean("showInfo", showInfo);
            }
        }

        private void WritePidSetup(Editor editor)
        {
            //保存PID参数选择情况
            editor.PutBoolean("radioButton_carType", radioButton_FourWheel.Checked);

            if (radioButton_FourWheel.Checked) //四轮车PID
            {
                WritePidFourWheel(editor);
            }
            else if (radioButton_BalanceCar.Checked) //直立车PID参数的获取
            {
                WritePidBalance(editor);
            }
        }

        private void WritePidFourWheel(Editor editor)
        {
            //舵机PID参数
            editor.PutString("Steer_P", textBox_Steer_P.Text);
            editor.PutString("Steer_I", textBox_Steer_I.Text);
            editor.PutString("Steer_D", textBox_Steer_D.Text);

            //电机PID参数
            editor.PutString("Motor_P", textBox_Motor_P.Text);
            editor.PutString("Motor_I", textBox_Motor_I.Text);
            editor.PutString("Motor_D", textBox_Motor_D.Text);
        }

        private void WritePidBalance(Editor editor)
        {
            //直立PID
            editor.PutString("Stand_P", textBox_Stand_P.Text);
            editor.PutString("Stand_I", textBox_Stand_I.Text);
            editor.PutString("Stand_D", textBox_Stand_D.Text);

            //速度PID
            editor.PutString("Speed_P", textBox_Speed_P.Text);
            editor.PutString("Speed_I", textBox_Speed_I.Text);
            editor.PutString("Speed_D", textBox_Speed_D.Text);

            //方向PID
            editor.PutString("Direction_P", textBox_Direction_P.Text);
            editor.PutString("Direction_I", textBox_Direction_I.Text);
            editor.PutString("Direction_D", textBox_Direction_D.Text);
        }

        private void WriteDiySetup(Editor editor)
        {
            //保存自定义参数的数量
            editor.PutInt32("DIY_Number", Convert.ToInt32(textBox_DIY_Number.Text));

            //保存自定义参数的其他信息
            var total = Convert.ToInt32(textBox_DIY_Number.Text);
            for (var i = 0; i < total; i++)
            {
                var controlCheckState = panel_add_DIYControls.Controls.Find("checkBox_Def" + Convert.ToString(i + 1), true);
                if (controlCheckState.Length > 0)
                {
                    var checkboxSelect = (CheckBox)controlCheckState[0];

                    var checkState = string.Format("DIY_CheckState{0}", i + 1);
                    editor.PutBoolean(checkState, checkboxSelect.Checked);
                }

                var controlName = panel_add_DIYControls.Controls.Find("txtName" + Convert.ToString(i + 1), true);
                if (controlName.Length > 0)
                {
                    var txtBox = (TextBox)controlName[0];

                    var txtName = string.Format("DIY_TextName{0}", i + 1);
                    editor.PutString(txtName, txtBox.Text);
                }

                var controlValue = panel_add_DIYControls.Controls.Find("txtValue" + Convert.ToString(i + 1), true);
                if (controlValue.Length > 0)
                {
                    var txtBox = (TextBox)controlValue[0];

                    var txtValue = string.Format("DIY_TextValue{0}", i + 1);
                    editor.PutString(txtValue, txtBox.Text);
                }

                var controlButton = panel_add_DIYControls.Controls.Find("buttonSubmit" + Convert.ToString(i + 1), true);
                if (controlButton.Length > 0)
                {
                    var button = (Button)controlButton[0];

                    var buttonName = string.Format("buttonSubmit{0}", i + 1);
                    editor.PutString(buttonName, "修改");
                }
            }
        }

        private void WriteRealTimeSetup(Editor editor)
        {
            //保存实时变量数量
            editor.PutString("RealtimeNum", textBox_Realtime_Number.Text);

            //保存实时变量的其他信息
            var total = Convert.ToInt32(textBox_Realtime_Number.Text);
            for (var i = 0; i < total; i++)
            {
                var controlName = panel_Electricity.Controls.Find("txtElectName" + Convert.ToString(i + 1), true);
                if (controlName.Length > 0)
                {
                    var txtBox = (TextBox)controlName[0];

                    var txtNameElect = string.Format("textElectName{0}", i + 1);
                    editor.PutString(txtNameElect, txtBox.Text);
                }

                var controlValue = panel_Electricity.Controls.Find("txtElectValue" + Convert.ToString(i + 1), true);
                if (controlValue.Length > 0)
                {
                    var txtBox = (TextBox) controlValue[0];
                    var txtValueElect = string.Format("txtElectValue{0}", i + 1);
                    editor.PutString(txtValueElect, txtBox.Text);
                }
            }
        }

        private void WriteScopeSetup(Editor editor)
        {
            //保存虚拟示波器的变量情况
            var totalScope = ScopeNumber;
            for (var i = 0; i < totalScope; i++)
            {

                var controlCheckState = panel_Scope.Controls.Find("checkBox_Def" + Convert.ToString(i + 1), true);
                if (controlCheckState.Length > 0)
                {
                    var checkboxSelect = (CheckBox)controlCheckState[0];

                    var checkState = string.Format("SCOPE_CheckState{0}", i + 1);
                    editor.PutBoolean(checkState, checkboxSelect.Checked);
                }


                var controlName = panel_Scope.Controls.Find("txtName" + Convert.ToString(i + 1), true);
                if (controlName.Length > 0)
                {
                    var txtBox = (TextBox)controlName[0];

                    var txtName = string.Format("SCOPE_TextName{0}", i + 1);
                    editor.PutString(txtName, txtBox.Text);
                }
            }
        }

        private static void WriteToFile(string fileName, Editor editor)
        {
            //=================================================================
            var sp = new SharedPreferences(fileName);
            sp.Save(editor);
        }

        private void ReadPortSetup(SharedPreferences sp)
        {
            //读取选中的端口
            if (hasPorts)
            {
                //判断当前端口的数量，小于则表示是OK。
                if (sp.GetInt32("selectedPort", 0) < comboBox_port.Items.Count)
                    comboBox_port.SelectedIndex = sp.GetInt32("selectedPort", 0);
                else
                {
                    comboBox_port.SelectedIndex = 0;
                }
            }
            //读取设置的波特率
            comboBox_baudrate.SelectedIndex = sp.GetInt32("baudRate", 0);
            //读取设置的校验方式
            comboBox_parity.SelectedIndex = sp.GetInt32("parity", 0);
            //读取设置的数据位
            comboBox_databit.SelectedIndex = sp.GetInt32("dataBits", 0);
            //读取设置的停止位
            comboBox_stopbit.SelectedIndex = sp.GetInt32("stopBits", 0);
            //读取自动发送数据时间间隔
            textBox_sendPeriod.Text = sp.GetInt32("intervalTime", 500).ToString();
            //读取接收方式
            checkBox_receiveHex.Checked = sp.GetBoolean("acceptChar", true);
            //读取标志变量，即是否在接收框中显示信息。
            showInfo = sp.GetBoolean("showInfo", true);
            button_receivepause.Text = showInfo ? "暂停接收显示" : "继续接收显示";
        }

        private void ReadPIDSetup(SharedPreferences sp)
        {
            //保存的车型
            radioButton_FourWheel.Checked = sp.GetBoolean("radioButton_carType", true);
            if (!radioButton_FourWheel.Checked)
                radioButton_BalanceCar.Checked = true;

            if (radioButton_FourWheel.Checked) //四轮车PID
                ReadPIDFourWheel(sp);
            else if (radioButton_BalanceCar.Checked) //直立车PID参数的获取
                ReadPIDBalanceCar(sp);
        }

        private void ReadPIDFourWheel(SharedPreferences sp)
        {
            //舵机PID参数
            textBox_Steer_P.Text = sp.GetString("Steer_P", "1.0");
            textBox_Steer_I.Text = sp.GetString("Steer_I", "1.0");
            textBox_Steer_D.Text = sp.GetString("Steer_D", "1.0");

            //电机PID参数
            textBox_Motor_P.Text = sp.GetString("Motor_P", "1.0");
            textBox_Motor_I.Text = sp.GetString("Motor_I", "1.0");
            textBox_Motor_D.Text = sp.GetString("Motor_D", "1.0");
        }

        private void ReadPIDBalanceCar(SharedPreferences sp)
        {
            //直立PID
            textBox_Stand_P.Text = sp.GetString("Stand_P", "1.0");
            textBox_Stand_I.Text = sp.GetString("Stand_I", "1.0");
            textBox_Stand_D.Text = sp.GetString("Stand_D", "1.0");

            //速度PID
            textBox_Speed_P.Text = sp.GetString("Speed_P", "1.0");
            textBox_Speed_I.Text = sp.GetString("Speed_I", "1.0");
            textBox_Speed_D.Text = sp.GetString("Speed_D", "1.0");

            //方向PID
            textBox_Direction_P.Text = sp.GetString("Direction_P", "1.0");
            textBox_Direction_I.Text = sp.GetString("Direction_I", "1.0");
            textBox_Direction_D.Text = sp.GetString("Direction_D", "1.0");
        }

        private void ReadDIYNumberSetup(SharedPreferences sp)
        {
            textBox_DIY_Number.Text = sp.GetInt32("DIY_Number", 1).ToString();
        }

        private void ReadRealTimeSetup(SharedPreferences sp)
        {
            textBox_Realtime_Number.Text = sp.GetString("RealtimeNum", "1");
        }

        private void ResetToDefaultSettings()
        {
            var editor = new Editor();

            ResetPortSetup();
            ResetPIDSetup(editor);
            ResetCustomSetup(editor);
            ResetRealTimeSetup(editor);
            ResetScopeSetup(editor);

            WriteToFile(SavefileName, editor);
            LoadConfig(SavefileName);
        }

        private void ResetPortSetup()
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

        private void ResetPIDSetup(Editor editor)
        {
            editor.PutBoolean("radioButton_carType", false);

            //舵机PID参数
            editor.PutString("Steer_P", "1.0");
            editor.PutString("Steer_I", "1.0");
            editor.PutString("Steer_D", "1.0");

            //电机PID参数
            editor.PutString("Motor_P", "1.0");
            editor.PutString("Motor_I", "1.0");
            editor.PutString("Motor_D", "1.0");

            //直立PID
            editor.PutString("Stand_P", "1.0");
            editor.PutString("Stand_I", "1.0");
            editor.PutString("Stand_D", "1.0");

            //速度PID
            editor.PutString("Speed_P", "1.0");
            editor.PutString("Speed_I", "1.0");
            editor.PutString("Speed_D", "1.0");

            //方向PID
            editor.PutString("Direction_P", "1.0");
            editor.PutString("Direction_I", "1.0");
            editor.PutString("Direction_D", "1.0");
        }

        private void ResetCustomSetup(Editor editor)
        {
            int diyNumber = 1;
            editor.PutInt32("DIY_Number", diyNumber);

            for (var i = 0; i < diyNumber; i++)
            {
                var checkState = string.Format("DIY_CheckState{0}", i + 1);
                editor.PutBoolean(checkState, false);

                var txtNameDIY = string.Format("DIY_TextName{0}", i + 1);
                editor.PutString(txtNameDIY, "Names");

                var txtValueDIY = string.Format("DIY_TextValue{0}", i + 1);
                editor.PutString(txtValueDIY, "1.0");

                var buttonDIY = string.Format("buttonSubmit{0}", i + 1);
                editor.PutString(buttonDIY, @"修改");
            }
        }

        private void ResetRealTimeSetup(Editor editor)
        {
            int realtimeNumber = 1;
            editor.PutInt32("RealtimeNum", realtimeNumber);

            for (var i = 0; i < realtimeNumber; i++)
            {
                var txtName = string.Format("textElectName{0}", i + 1);
                editor.PutString(txtName, "Name");

                var txtValue = string.Format("txtElectValue{0}", i + 1);
                editor.PutString(txtValue, "0");
            }
        }

        private void ResetScopeSetup(Editor editor)
        {
            var totalScope = ScopeNumber;

            for (var i = 0; i < totalScope; i++)
            {
                var checkState = string.Format("SCOPE_CheckState{0}", i + 1);
                editor.PutBoolean(checkState, false);

                var txtNameDIY = string.Format("SCOPE_TextName{0}", i + 1);
                editor.PutString(txtNameDIY, "波形" + (i+1));
            }
        }
    }
}