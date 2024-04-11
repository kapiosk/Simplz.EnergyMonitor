## Installation

Create Service energymonitor.service

```bash
[Unit]
Description=EnergyMonitor
After=network-online.target

[Service]
ExecStart=/home/pi/.dotnet/dotnet PathTo.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=EnergyMonitor
User=pi
Environment=TAPO_IPS=
Environment=TAPO_EMAIL=
Environment=TAPO_PASSWORD=
Environment=INFLUXDB_TOKEN=
Environment=INFLUXDB_URL=
Environment=INFLUXDB_DATABASE=

[Install]
WantedBy=default.target
```
