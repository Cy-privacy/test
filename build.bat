@echo off
echo Converting YOLO model to ONNX format...
python YoloConverter.py

echo Building C# application...
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

echo Build complete! The executable is in bin\Release\net6.0-windows\win-x64\publish\
pause