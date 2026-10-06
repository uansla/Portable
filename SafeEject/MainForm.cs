using System;
using System.Drawing;
using System.Windows.Forms;

namespace SafeEject
{
    public sealed class MainForm : Form
    {
        private readonly ListBox list = new ListBox();
        private readonly Button refresh = new Button();
        private readonly Button eject = new Button();
        private readonly Label status = new Label();

        public MainForm()
        {
            Text = "SafeEject Portable 1.0";
            Width = 720; Height = 430; StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            list.SetBounds(15,15,674,280); list.Font = new Font("Microsoft YaHei UI",10);
            refresh.Text="刷新设备"; refresh.SetBounds(15,310,130,32);
            eject.Text="安全弹出选中设备"; eject.SetBounds(160,310,180,32);
            status.SetBounds(15,355,674,40); status.Text="正在扫描 USB / 移动存储设备…";
            Controls.AddRange(new Control[]{list,refresh,eject,status});
            refresh.Click += delegate { LoadDevices(); };
            eject.Click += delegate { EjectSelected(); };
            Shown += delegate { LoadDevices(); };
        }

        private void LoadDevices()
        {
            list.Items.Clear();
            try {
                foreach(var d in DeviceManager.GetUsbDisks()) list.Items.Add(d);
                status.Text=list.Items.Count==0?"未发现 USB 存储设备。":"发现 "+list.Items.Count+" 个 USB 存储设备。";
            } catch(Exception ex){ status.Text="扫描失败："+ex.Message; }
        }

        private void EjectSelected()
        {
            var d=list.SelectedItem as DeviceInfo;
            if(d==null){status.Text="请先选择要弹出的设备。";return;}
            if(MessageBox.Show("确定安全移除以下设备？\r\n\r\n"+d.ToString(),"SafeEject",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
            string msg; var ok=Ejector.TryEject(d,out msg); status.Text=msg;
            if(ok) LoadDevices(); else MessageBox.Show(msg,"无法弹出",MessageBoxButtons.OK,MessageBoxIcon.Warning);
        }
    }
}
