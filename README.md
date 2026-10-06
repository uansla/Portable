# SafeEject Portable v1.0

轻量 USB / TF 卡 / 移动硬盘安全弹出工具，用于解决关闭窗口、卸载加密卷后设备仍无法“安全删除”的情况。

## 功能

- 显示 USB 存储设备名称、容量和盘符
- 选择单个设备安全弹出
- 优先调用 Windows 原生 PnP 安全移除 API
- PowerShell 兼容后备路径，兼容 Windows 7
- 不默认结束 Explorer，不使用 `mountvol /p` 强制卸载
- 单文件 `SafeEject.exe`
- 目标 Windows 7 / 8 / 8.1 / 10 / 11（Windows 7 建议 SP1）
- 管理员权限运行

## 发布

GitHub Actions 自动构建 Win32 单文件版本：

`SafeEject-Portable-v1.0.0-win32.zip`

正常使用只需要解压并运行 `SafeEject.exe`，PowerShell 后备脚本已经嵌入 EXE。

## 原理

程序首先通过 Windows `CM_Request_Device_Eject` 请求安全移除；Windows 会根据设备状态和驱动返回成功或占用/拒绝原因。该 API 是 Windows 官方提供的可移动设备安全移除接口。

