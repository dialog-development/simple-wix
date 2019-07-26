"C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe" "Interior Partition Tools.wxs"
"C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe" "Interior Partition Tools.wixobj" -sice:ICE91
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.16299.0\x64\signtool.exe" sign /sm /t http://timestamp.verisign.com/scripts/timstamp.dll "Interior Partition Tools.msi"
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.16299.0\x64\signtool.exe" verify /pa "Interior Partition Tools.msi"
pause