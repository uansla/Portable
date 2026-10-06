param([string]$DeviceId)
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($DeviceId)) { Write-Error 'DeviceId is required.'; exit 2 }
try {
    $disk = Get-WmiObject Win32_DiskDrive | Where-Object { $_.PNPDeviceID -eq $DeviceId }
    if (-not $disk) { throw '设备不存在或已经移除。' }
    $pnp = Get-WmiObject Win32_PnPEntity | Where-Object { $_.PNPDeviceID -eq $DeviceId } | Select-Object -First 1
    if ($pnp) {
        $result = $pnp.PSBase.InvokeMethod('Disable', $null)
        if ($result -eq 0) { Write-Output '设备已请求安全移除。'; exit 0 }
    }
    throw 'PowerShell 无法完成 PnP 弹出请求。'
} catch { Write-Error $_.Exception.Message; exit 1 }
