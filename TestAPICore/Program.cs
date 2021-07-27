using Intel.RealSense;
using System;

Context ctx = new Context();
var list = ctx.QueryDevices(); // Get a snapshot of currently connected devices
if (list.Count == 0)
    throw new Exception("No device detected. Is it plugged in?");
Device dev = list[0];

var pipe = new Pipeline(ctx);
PipelineProfile selection = pipe.Start();

Sensor sensor = selection.Device.Sensors[0];
float scale = sensor.DepthScale;

Console.WriteLine(scale);

Console.ReadLine();