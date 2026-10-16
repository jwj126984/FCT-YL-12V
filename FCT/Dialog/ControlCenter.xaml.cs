using Common;
using Common.Can;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FCT.Dialog
{
    public partial class ControlCenter : Window
    {
        private bool isPowerOn6322B = false;
        private bool isPowerOnN8331 = false;
        private bool isPowerOn3905D = false;
        private bool isTesting = false;
        private BackgroundWorker testWorker;

        // �̵���״̬����
        private bool innerRelayActive = false;
        private bool outerRelayActive = false;
        private bool relay6322BActive = false;
        private bool ammeterRelayActive = false;
        private bool currentForwardActive = false;
        private bool currentReverseActive = false;

        public ControlCenter()
        {
            InitializeComponent();
            InitializeControls();
            InitializeTestWorker();
        }

        private void InitializeControls()
        {
            // �ȳ�ʼ��UI���������������
            InitializeComboBoxes();

            // ���ó�ʼ״̬
            UpdatePowerButtonState6322B();
            UpdatePowerButtonStateN8331();
            UpdatePowerButtonState3905D();
            UpdateRelayStatus();

            // ��ʼ���̵�����ť״̬
            UpdateRelayButtonStates();
        }

        private void InitializeComboBoxes()
        {
            try
            {
                // ��ʼ������������Ͽ�
                if (cmbTestType != null)
                {
                    cmbTestType.Items.Clear();
                    cmbTestType.Items.Add(new ComboBoxItem() { Content = "��ѹ", IsSelected = true });
                    cmbTestType.Items.Add(new ComboBoxItem() { Content = "����" });
                    cmbTestType.SelectedIndex = 0;
                }

                // ��ʼ��������Ͽ�
                if (cmbRange != null)
                {
                    UpdateRangeOptions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��ʼ���ؼ�ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeTestWorker()
        {
            testWorker = new BackgroundWorker();
            testWorker.WorkerSupportsCancellation = true;
            testWorker.DoWork += TestWorker_DoWork;
            testWorker.RunWorkerCompleted += TestWorker_RunWorkerCompleted;
        }

        #region 6322B��Դ�����¼�
        private void SetVoltage6322B_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (double.TryParse(txtVoltage6322B.Text, out double voltage) && voltage >= 0)
                {
                    // �������õ�ѹ��Ӳ���ӿ�
                    SetVoltage6322B(voltage);
                    MessageBox.Show($"��ѹ����Ϊ: {voltage} V", "���óɹ�", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("��������Ч�ĵ�ѹֵ�����ڵ���0��", "�������", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"���õ�ѹʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReadVoltage6322B_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ���ö�ȡ��ѹ��Ӳ���ӿ�
                double voltage = ReadActualVoltage6322B();
                tbActualVoltage6322B.Text = $"{voltage:F2} V";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��ȡ��ѹʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetCurrent6322B_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (double.TryParse(txtCurrent6322B.Text, out double current) && current >= 0)
                {
                    // �������õ�����Ӳ���ӿ�
                    SetCurrent6322B(current);
                    MessageBox.Show($"��������Ϊ: {current} A", "���óɹ�", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("��������Ч�ĵ���ֵ�����ڵ���0��", "�������", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"���õ���ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReadCurrent6322B_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ���ö�ȡ������Ӳ���ӿ�
                double current = ReadActualCurrent6322B();
                tbActualCurrent6322B.Text = $"{current:F3} A";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��ȡ����ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PowerSwitch6322B_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isPowerOn6322B = !isPowerOn6322B;

                if (isPowerOn6322B)
                {
                    // ������Դ
                    PowerOn6322B();
             //       MessageBox.Show("��Դ�ѿ���", "��Դ״̬", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // �رյ�Դ
                    PowerOff6322B();
              //      MessageBox.Show("��Դ�ѹر�", "��Դ״̬", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdatePowerButtonState6322B();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��Դ����ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
                isPowerOn6322B = !isPowerOn6322B; // �ָ�״̬
                UpdatePowerButtonState6322B();
            }
        }
        #endregion

        #region N8331��Դ�����¼�
        private void SetVoltageN8331_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (double.TryParse(txtVoltageN8331.Text, out double voltage) && voltage >= 0)
                {
                    // �������õ�ѹ��Ӳ���ӿ�
                    SetVoltageN8331(voltage);
                    MessageBox.Show($"��ѹ����Ϊ: {voltage} V", "���óɹ�", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("��������Ч�ĵ�ѹֵ�����ڵ���0��", "�������", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"���õ�ѹʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReadVoltageN8331_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ���ö�ȡ��ѹ��Ӳ���ӿ�
                double voltage = ReadActualVoltageN8331();
                tbActualVoltageN8331.Text = $"{voltage:F3} V";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��ȡ��ѹʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetCurrentN8331_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (double.TryParse(txtCurrentN8331.Text, out double current) && current >= 0)
                {
                    // �������õ�����Ӳ���ӿ�
                    SetCurrentN8331(current);
                    MessageBox.Show($"��������Ϊ: {current} A", "���óɹ�", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("��������Ч�ĵ���ֵ�����ڵ���0��", "�������", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"���õ���ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReadCurrentN8331_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ���ö�ȡ������Ӳ���ӿ�
                double current = ReadActualCurrentN8331();
                tbActualCurrentN8331.Text = $"{current:F3} A";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��ȡ����ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PowerSwitchN8331_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isPowerOnN8331 = !isPowerOnN8331;

                if (isPowerOnN8331)
                {
                    // ������Դ
                    PowerOnN8331();
                    //       MessageBox.Show("��Դ�ѿ���", "��Դ״̬", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // �رյ�Դ
                    PowerOffN8331();
                    //      MessageBox.Show("��Դ�ѹر�", "��Դ״̬", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                UpdatePowerButtonStateN8331();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��Դ����ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
                isPowerOnN8331 = !isPowerOnN8331; // �ָ�״̬
                UpdatePowerButtonStateN8331();
            }
        }
        #endregion

        #region ��װ������¼�
        private void FixtureDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // �����ξ���ѹӲ���ӿ�
                FixtureDown();
            //    MessageBox.Show("�ξ���ѹ", "����ִ��", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�ξ���ѹʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FixtureUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // �����ξ���̧Ӳ���ӿ�
                FixtureUp();
            //    MessageBox.Show("�ξ���̧", "����ִ��", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�ξ���̧ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InnerRelay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                innerRelayActive = !innerRelayActive;
                UpdateRelayButtonState(btnInnerRelay, innerRelayActive);
                UpdateRelayStatus();
                ControlRelay("���̵ܼ���", innerRelayActive);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�������̵ܼ���ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OuterRelay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                outerRelayActive = !outerRelayActive;
                UpdateRelayButtonState(btnOuterRelay, outerRelayActive);
                UpdateRelayStatus();
                ControlRelay("���̵ܼ���", outerRelayActive);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�������̵ܼ���ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Relay6322B_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                relay6322BActive = !relay6322BActive;
                UpdateRelayButtonState(btnRelay6322B, relay6322BActive);
                UpdateRelayStatus();
                ControlRelay("6322B�̵���", relay6322BActive);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"����6322B�̵���ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AmmeterRelay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ammeterRelayActive = !ammeterRelayActive;
                UpdateRelayButtonState(btnAmmeterRelay, ammeterRelayActive);
                UpdateRelayStatus();
                ControlRelay("�������̵���", ammeterRelayActive);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"���Ƶ������̵���ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CurrentForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                currentForwardActive = !currentForwardActive;
                if (currentForwardActive && currentReverseActive)
                {
                    // ������������رշ���
                    currentReverseActive = false;
                    UpdateRelayButtonState(btnCurrentReverse, false);
                }
                UpdateRelayButtonState(btnCurrentForward, currentForwardActive);
                UpdateRelayStatus();
                ControlRelay("��������", currentForwardActive);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"���Ƶ�������ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CurrentReverse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                currentReverseActive = !currentReverseActive;
                if (currentReverseActive && currentForwardActive)
                {
                    // ������������ر�����
                    currentForwardActive = false;
                    UpdateRelayButtonState(btnCurrentForward, false);
                }
                UpdateRelayButtonState(btnCurrentReverse, currentReverseActive);
                UpdateRelayStatus();
                ControlRelay("��������", currentReverseActive);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"���Ƶ�������ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region N8331�����¼�
        private void TestType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbTestType != null && cmbTestType.SelectedItem != null)
                {
                    UpdateRangeOptions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�л���������ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Range_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // ���̸ı䴦��
                if (cmbRange != null && cmbRange.SelectedItem != null)
                {
                    // ����Ӳ����������
                    UpdateHardwareRange();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�л�����ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (testWorker != null && !testWorker.IsBusy)
                {
                    isTesting = true;
                    btnStartTest.IsEnabled = false;
                    btnStopTest.IsEnabled = true;
                    tbTestStatus.Text = "������...";
                    tbTestStatus.Foreground = Brushes.Blue;

                    testWorker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��������ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (testWorker != null && testWorker.IsBusy)
                {
                    testWorker.CancelAsync();
                    isTesting = false;
                    btnStopTest.IsEnabled = false;
                    tbTestStatus.Text = "������ֹͣ";
                    tbTestStatus.Foreground = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ֹͣ����ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region ��������
        private void UpdatePowerButtonState6322B()
        {
            if (btnPowerSwitch6322B != null)
            {
                btnPowerSwitch6322B.Content = isPowerOn6322B ? "��Դ�ر�" : "��Դ����";
                btnPowerSwitch6322B.Background = isPowerOn6322B ? Brushes.LightCoral : Brushes.LightGreen;
            }
        }

        private void UpdatePowerButtonStateN8331()
        {
            if (btnPowerSwitchN8331 != null)
            {
                btnPowerSwitchN8331.Content = isPowerOnN8331 ? "��Դ�ر�" : "��Դ����";
                btnPowerSwitchN8331.Background = isPowerOnN8331 ? Brushes.LightCoral : Brushes.LightGreen;
            }
        }

        private void UpdatePowerButtonState3905D()
        {
            if (btnPowerSwitch3905D != null)
            {
                btnPowerSwitch3905D.Content = isPowerOn3905D ? "��Դ�ر�" : "��Դ����";
                btnPowerSwitch3905D.Background = isPowerOn3905D ? Brushes.LightCoral : Brushes.LightGreen;
            }
        }

        private void UpdateRelayStatus()
        {
            if (tbRelayStatus != null)
            {
                string status = "����ļ̵���: ";
                bool anyActive = false;

                if (innerRelayActive) { status += "���� "; anyActive = true; }
                if (outerRelayActive) { status += "���� "; anyActive = true; }
                if (relay6322BActive) { status += "6322B "; anyActive = true; }
                if (ammeterRelayActive) { status += "������ "; anyActive = true; }
                if (currentForwardActive) { status += "���� "; anyActive = true; }
                if (currentReverseActive) { status += "���� "; anyActive = true; }

                if (!anyActive)
                {
                    status = "���м̵���δ����";
                }

                tbRelayStatus.Text = status;
            }
        }

        private void UpdateRelayButtonStates()
        {
            UpdateRelayButtonState(btnInnerRelay, innerRelayActive);
            UpdateRelayButtonState(btnOuterRelay, outerRelayActive);
            UpdateRelayButtonState(btnRelay6322B, relay6322BActive);
            UpdateRelayButtonState(btnAmmeterRelay, ammeterRelayActive);
            UpdateRelayButtonState(btnCurrentForward, currentForwardActive);
            UpdateRelayButtonState(btnCurrentReverse, currentReverseActive);
        }

        private void UpdateRelayButtonState(Button button, bool isActive)
        {
            if (button != null)
            {
                button.Background = isActive ? Brushes.LightGreen : Brushes.LightGray;
                button.Foreground = isActive ? Brushes.DarkGreen : Brushes.Black;
            }
        }

        private void UpdateRangeOptions()
        {
            if (cmbRange == null || cmbTestType == null)
                return;

            try
            {
                // ���浱ǰѡ��
                object currentSelection = cmbRange.SelectedItem;

                cmbRange.Items.Clear();

                var selectedTestType = cmbTestType.SelectedItem as ComboBoxItem;
                if (selectedTestType != null)
                {
                    if (selectedTestType.Content.ToString() == "��ѹ")
                    {
                        // ��ѹ����ѡ��
                        cmbRange.Items.Add("100mV");
                        cmbRange.Items.Add("1V");
                        cmbRange.Items.Add("10V");
                        cmbRange.Items.Add("100V");
                        
                    }
                    else
                    {
                        // ��������ѡ��
                        cmbRange.Items.Add("10mA");
                        cmbRange.Items.Add("100mA");
                        cmbRange.Items.Add("1A");
                        cmbRange.Items.Add("3A");
                        cmbRange.Items.Add("10A");
                    }

                    // �ָ�ѡ�������Ĭ��
                    if (cmbRange.Items.Count > 0)
                    {
                        if (currentSelection != null && cmbRange.Items.Contains(currentSelection))
                        {
                            cmbRange.SelectedItem = currentSelection;
                        }
                        else
                        {
                            cmbRange.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��������ѡ��ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
        #endregion

        #region Ӳ���ӿڷ��� (��Ҫ����ʵ��Ӳ��ʵ��)
        private void SetVoltage6322B(double voltage)
        {
            CommonData.IT6322.SetVol(voltage);
        }

        private void SetCurrent6322B(double current)
        {
            CommonData.IT6322.SetCurr(current);
        }

        private double ReadActualVoltage6322B()
        {
            return double.Parse(CommonData.IT6322.SelectVol());
        }

        private double ReadActualCurrent6322B()
        {
            return double.Parse(CommonData.IT6322.SelectCurr());
        }

        private void PowerOn6322B()
        {
            CommonData.IT6322.SetOutput(true);
        }

        private void PowerOff6322B()
        {
            CommonData.IT6322.SetOutput(false);
        }


        private void SetVoltageN8331(double voltage)
        {
            CommonData.N8331.SetVol4(voltage / 4);
            // ʵ�����õ�ѹ��Ӳ������
            // HardwareInterface.SetVoltage(voltage);
            Console.WriteLine($"���õ�ѹ: {voltage}V");
        }

        private void SetCurrentN8331(double current)
        {
            // ʵ�����õ�����Ӳ������
            // HardwareInterface.SetCurrent(current);
            Console.WriteLine($"���õ���: {current}A");
        }

        private double ReadActualVoltageN8331()
        {
            // ʵ�ֶ�ȡʵ�ʵ�ѹ
             return CommonData.N8331.GetVol4()[0];

            // ģ������
            //Random rand = new Random();
            //return rand.NextDouble() * 5.0;
        }

        private double ReadActualCurrentN8331()
        {
            // ʵ�ֶ�ȡʵ�ʵ���
            // return HardwareInterface.ReadCurrent();

            // ģ������
            Random rand = new Random();
            return rand.NextDouble() * 0.1;
        }

        private void PowerOnN8331()
        {
            CommonData.N8331.SetOutPut4(true);
            // ʵ�ֿ�����Դ
            // HardwareInterface.PowerOn();
            Console.WriteLine("��Դ����");
        }

        private void PowerOffN8331()
        {
            CommonData.N8331.SetOutPut4(false);
            // ʵ�ֹرյ�Դ
            // HardwareInterface.PowerOff();
            Console.WriteLine("��Դ�ر�");
        }

        private void PowerOn3905D()
        {
            CommonData.M3905D.SetOUTPut(1);
            // ʵ�ֿ�����Դ
            Console.WriteLine("M3905D��Դ����");
        }

        private void PowerOff3905D()
        {
            CommonData.M3905D.SetOUTPut(0);
            // ʵ�ֹرյ�Դ
            Console.WriteLine("M3905D��Դ�ر�");
        }
        private void FixtureDown()
        {
            NewCanHelper.ControlZhiJu(true);
            // ʵ���ξ���ѹ
            // HardwareInterface.FixtureDown();
            Console.WriteLine("�ξ���ѹ");
        }

        private void FixtureUp()
        {
            NewCanHelper.ControlZhiJu(false);
            // ʵ���ξ���̧
            // HardwareInterface.FixtureUp();
            Console.WriteLine("�ξ���̧");
        }

        private void ControlRelay(string relayName, bool state)
        {
            if (state == false)
            {
                NewCanHelper.CloseTotalCurrentCollection();
            }
            else
            {
                if (relayName == "���̵ܼ���")
                {
                    NewCanHelper.MOSControlVoltageEnable(true, false, false);
                }
                if (relayName == "���̵ܼ���")
                {
                    NewCanHelper.MOSControlVoltageEnable(true, true, false);
                }
                if (relayName == "��������")
                {
                    NewCanHelper.CtrolTotalCurrentCollection(true);
                }
                if (relayName == "��������")
                {
                    NewCanHelper.CtrolTotalCurrentCollection(true);
                }
            }
            
            

            // ʵ�̵ּ�������
            // HardwareInterface.ControlRelay(relayName, state);
            Console.WriteLine($"�̵��� {relayName} {(state ? "����" : "�ر�")}");
        }

        private void UpdateHardwareRange()
        {
            // ����Ӳ����������
            if (cmbRange != null && cmbRange.SelectedItem != null)
            {
                // HardwareInterface.SetRange(cmbRange.SelectedItem.ToString());
                Console.WriteLine($"��������: {cmbRange.SelectedItem}");
            }
        }

        private double ReadTestValue()
        {
            var selectedTestType = cmbTestType.SelectedItem as ComboBoxItem;
            if (selectedTestType != null)
            {
                if (selectedTestType.Content.ToString() == "电压")
                {
                    return CommonData.NXI_6400.GetVResult();
                }
               
            }
            return 0;
        }
        #endregion

        #region ���Թ����߳�
        private void TestWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (worker != null && !worker.CancellationPending)
            {
                try
                {
                    double testValue = ReadTestValue();

                    // ����UI����Ҫ���̵߳��ã�
                    Dispatcher.Invoke(() =>
                    {
                        if (tbTestValue != null)
                            tbTestValue.Text = testValue.ToString("F3");
                    });

                    System.Threading.Thread.Sleep(500); // 500msˢ��һ��
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"���Թ����г���: {ex.Message}");
                }
            }

            if (e != null)
                e.Cancel = true;
        }

        private void TestWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (btnStartTest != null)
                btnStartTest.IsEnabled = true;
            if (btnStopTest != null)
                btnStopTest.IsEnabled = false;

            if (tbTestStatus != null)
            {
                if (e != null)
                {
                    if (e.Cancelled)
                    {
                        tbTestStatus.Text = "������ֹͣ";
                        tbTestStatus.Foreground = Brushes.Orange;
                    }
                    else if (e.Error != null)
                    {
                        tbTestStatus.Text = $"���Դ���: {e.Error.Message}";
                        tbTestStatus.Foreground = Brushes.Red;
                    }
                    else
                    {
                        tbTestStatus.Text = "�������";
                        tbTestStatus.Foreground = Brushes.Green;
                    }
                }
                else
                {
                    tbTestStatus.Text = "�����쳣����";
                    tbTestStatus.Foreground = Brushes.Red;
                }
            }
        }

        #endregion

        private void SetVoltage3905d_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (double.TryParse(txtVoltage3905D.Text, out double voltage) && voltage >= 0)
                {
                    // �������õ�ѹ��Ӳ���ӿ�
                    CommonData.M3905D.SetVoltage(voltage);
                    MessageBox.Show($"��ѹ����Ϊ: {voltage} V", "���óɹ�", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("��������Ч�ĵ�ѹֵ�����ڵ���0��", "�������", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"���õ�ѹʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReadVoltage3905d_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ���ö�ȡ��ѹ��Ӳ���ӿ�
                double voltage = CommonData.M3905D.GetMEASVoltage();
                tbActualVoltage3905D.Text = $"{voltage:F2} V";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��ȡ��ѹʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetCurrent3905d_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (double.TryParse(txtCurrent3905D.Text, out double current) && current >= 0)
                {
                    // �������õ�����Ӳ���ӿ�
                    CommonData.M3905D.SetCurrent(current);

                    MessageBox.Show($"��������Ϊ: {current} A", "���óɹ�", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("��������Ч�ĵ���ֵ�����ڵ���0��", "�������", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"���õ���ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReadCurrent3905d_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ���ö�ȡ������Ӳ���ӿ�
                double current = CommonData.M3905D.GetMEASCurrent();
                tbActualCurrent3905D.Text = $"{current:F3} A";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��ȡ����ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PowerSwitch3905D_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isPowerOn3905D = !isPowerOn3905D;

                if (isPowerOn3905D)
                {
                    // ������Դ
                    PowerOn3905D();
                }
                else
                {
                    // �رյ�Դ
                    PowerOff3905D();
                }

                UpdatePowerButtonState3905D();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"��Դ����ʱ����: {ex.Message}", "����", MessageBoxButton.OK, MessageBoxImage.Error);
                isPowerOn3905D = !isPowerOn3905D; // �ָ�״̬
                UpdatePowerButtonState3905D();
            }
        }
    }
}