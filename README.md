# SafeEject Tray v1.1

一个常驻 Windows 任务栏通知区域的 USB / TF 卡 / 移动硬盘安全弹出工具。

## 使用方式

1. 第一次运行 **SafeEject.exe** 时使用“以管理员身份运行”。
2. 程序不会打开主窗口，而是在 Windows 任务栏右下角通知区域显示 SafeEject 图标。
3. 第一次管理员运行会自动创建“开机自动启动”任务。
4. 以后插入 U 盘、TF 卡或移动硬盘后，直接点击任务栏右下角 SafeEject 图标。
5. 菜单会列出当前检测到的 USB 存储设备，点击对应设备即可安全弹出。
6. 弹出成功后可以直接拔出设备，不需要再次打开 EXE。

## v1.1 改进

- 改为常驻通知区域，不再弹出主窗口。
- 左键或右键点击托盘图标都可以直接选择设备。
- 插入/拔出设备后通过“刷新设备”重新读取。
- 管理员首次运行后自动创建 Windows 登录启动任务。
- 使用 Windows 原生 PnP 安全移除 API。
- 去掉 PowerShell 后备脚本和自包含 .NET Core Runtime。
- 改用 **.NET Framework 4.8**，发布 EXE 大幅缩小。
- 单 EXE 发布，不需要额外 DLL。
- 兼容 Windows 7 SP1、Windows 8/8.1、Windows 10、Windows 11。
- 不强制结束 Explorer，不使用 `mountvol /p`。

## 注意

- Windows 7 需要安装 .NET Framework 4.8（建议 Windows 7 SP1）。
- 首次安装/运行必须有管理员权限，因为程序需要创建高权限登录启动任务并执行设备安全移除。
- 如果 Windows 报告设备仍在使用，先关闭正在访问 U 盘/TF 卡的程序，再点击托盘图标重试。
- v1.1 使用原生 PnP 弹出流程，不再通过 PowerShell Disable 设备，避免把“禁用设备”误当成“安全弹出”。

## 发布文件

GitHub Actions 会生成：

`SafeEject-Tray-v1.1.0.zip`

压缩包内只有：

`SafeEject-Portable-v1.1.0.exe`

首次运行建议右键 EXE → **以管理员身份运行**。