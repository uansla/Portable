using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SafeEject
{
    internal sealed class MainForm : ApplicationContext
    {
        private readonly NotifyIcon _tray;
        private readonly ContextMenuStrip _menu;
        private bool _refreshing;

        public MainForm()
        {
            _menu = new ContextMenuStrip();
            _tray = new NotifyIcon
            {
                Icon = SystemIcons.Shield,
                Text = "SafeEject - USB安全弹出",
                Visible = true,
                ContextMenuStrip = _menu
            };

            _tray.MouseClick += TrayMouseClick;
            _tray.DoubleClick += delegate { ShowDevices(); };

            RefreshMenu();
            InstallAutoStart();
        }

        private void TrayMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                ShowDevices();
        }

        private void ShowDevices()
        {
            RefreshMenu();
            _menu.Show(Cursor.Position);
        }

        private void RefreshMenu()
        {
            if (_refreshing) return;
            _refreshing = true;
            try
            {
                _menu.Items.Clear();

                var devices = DeviceManager.GetUsbDisks();
                if (devices.Count == 0)
                {
                    var empty = new ToolStripMenuItem("没有检测到 USB / TF 存储设备")
                    {
                        Enabled = false
                    };
                    _menu.Items.Add(empty);
                }
                else
                {
                    foreach (var device in devices)
                    {
                        var item = new ToolStripMenuItem(FormatDevice(device));
                        item.Tag = device;
                        item.Click += EjectMenuItem_Click;
                        _menu.Items.Add(item);
                    }
                }

                _menu.Items.Add(new ToolStripSeparator());
                var refresh = new ToolStripMenuItem("刷新设备");
                refresh.Click += delegate { RefreshMenu(); };
                _menu.Items.Add(refresh);

                var startup = new ToolStripMenuItem("开机自动启动")
                {
                    Checked = StartupManager.IsInstalled()
                };
                startup.Click += delegate
                {
                    if (StartupManager.IsInstalled())
                        StartupManager.Uninstall();
                    else
                        StartupManager.Install();
                    RefreshMenu();
                };
                _menu.Items.Add(startup);

                _menu.Items.Add(new ToolStripSeparator());
                var exit = new ToolStripMenuItem("退出 SafeEject");
                exit.Click += delegate { ExitThread(); };
                _menu.Items.Add(exit);
            }
            catch (Exception ex)
            {
                _menu.Items.Clear();
                _menu.Items.Add(new ToolStripMenuItem("读取设备失败: " + ex.Message) { Enabled = false });
            }
            finally
            {
                _refreshing = false;
            }
        }

        private static string FormatDevice(DeviceInfo d)
        {
            var letters = string.IsNullOrWhiteSpace(d.Letters) ? "无盘符" : d.Letters;
            var gb = d.Size > 0 ? (d.Size / 1024d / 1024d / 1024d).ToString("0.##") + " GB" : "容量未知";
            return letters + "  |  " + d.Model + "  |  " + gb;
        }

        private void EjectMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            var device = item == null ? null : item.Tag as DeviceInfo;
            if (device == null) return;

            string message;
            var ok = Ejector.TryEject(device, out message);

            if (ok)
            {
                _tray.ShowBalloonTip(1800, "SafeEject", "已安全弹出：" + device.Letters, ToolTipIcon.Info);
                RefreshMenu();
            }
            else
            {
                _tray.ShowBalloonTip(3000, "SafeEject", message, ToolTipIcon.Warning);
            }
        }

        private void InstallAutoStart()
        {
            try
            {
                if (!StartupManager.IsInstalled())
                    StartupManager.Install();
            }
            catch
            {
                // The tray utility must remain usable even if task creation is unavailable.
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_tray != null)
                {
                    _tray.Visible = false;
                    _tray.Dispose();
                }
                if (_menu != null) _menu.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}