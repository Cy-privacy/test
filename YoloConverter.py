import torch
from ultralytics import YOLO

# Load the PyTorch model
model = YOLO('lib/best.pt')

# Export to ONNX format
model.export(format='onnx', dynamic=True, simplify=True)
print("Model converted successfully to ONNX format")