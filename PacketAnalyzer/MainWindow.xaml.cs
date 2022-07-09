using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PacketAnalyzer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            header.Text = "xxx.xxx.xxx.xxx	xxx.xxx.xxx.xxx	0001	0002	0003	0004	0005	0006	0007	0008	0009	0010	0011	0012\n";
            Log.Text = "xxx.xxx.xxx.xxx	xxx.xxx.xxx.xxx	0001	0002	0003	0004	0005	0006	0007	0008	0009	0010	0011	0012\n";
        }


        private PacketTool PacketTool;

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if(PacketTool != null)
            {
                PacketTool.PacketReceive -= PacketTool_PacketReceive;
                PacketTool.Close();
                PacketTool = null;
            }

            PacketTool = new PacketTool();
            PacketTool.PacketReceive += PacketTool_PacketReceive;
        }

        private void PacketTool_PacketReceive(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                Log.AppendText((string)sender);
            }));
            
        }

        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            if (PacketTool != null)
            {
                PacketTool.PacketReceive -= PacketTool_PacketReceive;
                PacketTool.Close();
                PacketTool = null;
            }         
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (PacketTool != null)
            {
                PacketTool.PacketReceive -= PacketTool_PacketReceive;
                PacketTool.Close();
                PacketTool = null;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Text = "xxx.xxx.xxx.xxx	xxx.xxx.xxx.xxx	0001	0002	0003	0004	0005	0006	0007	0008	0009	0010	0011	0012\n";
        }

        private void FliterButton_Click(object sender, RoutedEventArgs e)
        {
            if (PacketTool != null)
            {
                PacketTool.datafliter = fliter.Text;
            }
        }

        private void fliter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PacketTool != null)
            {
                PacketTool.datafliter = fliter.Text;
            }
        }
    }
}
