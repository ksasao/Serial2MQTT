# Serial2MQTT
Serial Port to MQTT converter written in .NET Core 3. Easy to customize by C#-like scripts.

## Usage
```
Serial2MQTT [COM_PORT_NAME] [MQTT_BROKER_ADDRESS] [SCRIPT(*.sm)]
```

## Get started
0. Setup MQTT Broker
[Eclipse Mosquitto](https://mosquitto.org/) is recommended.

1. Write ```sample.sm``` script.
```
void Setup(SerialPort serial)
{
    // Sample
    serial.BaudRate = 9600;
    serial.StopBits = StopBits.One;
    serial.NewLine = "\n";
    serial.DtrEnable = true;
    serial.Parity = Parity.None;
}
void OnSerialReceived(string data)
{
    // Sample
    Console.WriteLine($"{PortName} => {data}");
    var json = new
    {
        Data = "World"
    };
    Publish("Hello", json);
}
```

2. Run
```
$ Serial2MQTT.exe COM10 192.168.3.40 sample.sm
Connecting to 192.168.3.40:1883
Connected.
COM10 => device_id 80DCDD98
192.168.3.40/Hello <= {"Data":"World"}
```

## Usage
If you don't want to use serial port (use MQTT pub/sub only), set [COM_PORT_NAME] as "".
```
Serial2MQTT "" 192.168.3.40 pubsub_disconfort_index.sm
```