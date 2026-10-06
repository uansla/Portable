# ============================================
# SafeEject Portable
# Windows 7/8/10/11
# ============================================


$ErrorActionPreference="SilentlyContinue"


Write-Host ""
Write-Host "Scanning removable devices..."
Write-Host ""


# 获取可移动设备

$devices = Get-CimInstance Win32_DiskDrive |
Where-Object {

$_.InterfaceType -match "USB"

}



if(!$devices)
{

Write-Host "No USB device found"

exit

}



foreach($disk in $devices)
{


Write-Host "Device:"
Write-Host $disk.Model



# 获取分区

$parts = Get-CimAssociatedInstance `
-InputObject $disk `
-Association Win32_DiskDriveToDiskPartition



foreach($part in $parts)
{


$volumes = Get-CimAssociatedInstance `
-InputObject $part `
-Association Win32_LogicalDiskToPartition



foreach($vol in $volumes)
{


$drive=$vol.DeviceID


Write-Host ""
Write-Host "Found:"
Write-Host $drive



# ---------------------------------
# 关闭Explorer占用
# ---------------------------------

Write-Host "Closing Explorer handles..."


Stop-Process `
-Name explorer `
-Force



Start-Sleep -Seconds 2



# ---------------------------------
# Flush缓存
# ---------------------------------


Write-Host "Flushing cache..."


[System.IO.File]::WriteAllBytes(
"$drive\flush.tmp",
[byte[]](0)
)


Remove-Item `
"$drive\flush.tmp" `
-Force



# ---------------------------------
# 卸载卷
# ---------------------------------


Write-Host "Dismount volume..."


mountvol $drive /p



Start-Sleep -Seconds 3



}

}


}



# 重启Explorer

Start-Process explorer.exe



Write-Host ""
Write-Host "================================"
Write-Host "Safe eject completed"
Write-Host "You can remove device now"
Write-Host "================================"

