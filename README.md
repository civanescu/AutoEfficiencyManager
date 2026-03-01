# Auto Efficiency Manager

Auto Efficiency Manager is a lightweight Windows console application that automatically monitors running processes and enables **Windows Efficiency Mode** for applications that exceed a defined CPU usage threshold.

It is designed to reduce power consumption and CPU load by dynamically applying power throttling to high-usage processes — while allowing you to exclude specific applications.

**It was wrote with ChatGPT**

---

## 🚀 What It Does

- Monitors all running processes every second
- Calculates per-process CPU usage
- Automatically enables **Windows Efficiency Mode** when CPU usage exceeds 20%  *The percent is in `Program.cs`, line 84.*
- Lowers process priority to `BelowNormal`
- Prevents reapplying efficiency mode repeatedly
- Allows custom exclusions via a text file
- Skips protected Windows system processes

---

## 🧠 How It Works

The application:

1. Reads excluded process names from `exclusions.txt`. *If file doesn not exist will create it with the process explorer in it*
2. Continuously scans running processes
3. Calculates CPU usage based on `TotalProcessorTime`
4. If a process exceeds 20% CPU usage:
   - Checks if Efficiency Mode is already enabled
   - Enables power throttling using `SetProcessInformation`
   - Lowers process priority
   - Adds it to a throttled tracking list
   - *The percent is in `Program.cs`, line 84.*
5. Cleans up exited processes automatically

It uses the Windows API:

- `SetProcessInformation`
- `GetProcessInformation`
- `PROCESS_POWER_THROTTLING_STATE`

---

## 📁 Project Structure

```
AutoEfficiencyManager/
│
├── Program.cs
├── EfficiencyHelper.cs
├── exclusions.txt
└── README.md
```

---

## ⚙️ Configuration

### Exclusions File

The file `exclusions.txt` contains process names that should never be throttled.

Example:

```
explorer
chrome
discord
```

Each line represents a process name (without .exe).

The file is automatically created on first run if it does not exist.

---

## 🛠️ Build Instructions

### Development Run

```
dotnet run
```

---

### Create Executable (Self-Contained, Single File)

```
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The executable will be located in:

```
bin/Release/netX.X/win-x64/publish/
```

---

## 🔐 Administrator Privileges

For full functionality, run the application as **Administrator**.

Without elevation, some system processes may not allow power throttling.

---

## 🖥 Requirements

- Windows 10 / 11
- .NET 6, 7, 8 or later (if not publishing self-contained) - *I used 10 for dev*
- 64-bit Windows recommended

---

## 📌 Important Notes

- Protected system processes are automatically ignored:
  - System
  - Idle
  - explorer
  - winlogon
  - csrss
  - services

- Efficiency Mode is only applied if not already enabled.
- Exited processes are cleaned from memory tracking automatically.

---

## 🔮 Possible Future Improvements

- Dynamic removal of efficiency mode when CPU drops
- Logging to file
- Windows Service version
- System tray application
- GUI management panel
- Per-process custom CPU thresholds

---

## 📄 License

This project is provided as-is for personal use.

---

## 👨‍💻 Author

Auto Efficiency Manager  
A lightweight Windows CPU efficiency controller.
@Keos