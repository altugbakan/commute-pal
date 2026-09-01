# CommutePal

Tiny Windows app that asks how you commuted today and keeps monthly totals for the WFH allowance paperwork.

## Install

1. Download `CommutePal.exe` from the latest [release](../../releases/latest) and put it somewhere permanent (e.g. `C:\Tools\CommutePal\`).
2. Run it once. It registers itself to open at Windows sign-in.

## How it works

- At sign-in, if today isn't logged yet, a small popup with four buttons appears: Bike, Car, Public transport, Home. Click one and it closes. If today is already logged, nothing appears.
- Open the exe by hand to see this month and last month totals, change today's entry, or turn the sign-in prompt off.
- Data is stored as one CSV per month in `%APPDATA%\CommutePal\` (e.g. `2026-08.csv`, rows of `date,mode`).

## Build

```
dotnet publish CommutePal/CommutePal.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

Pushing a tag like `v1.0.1` builds the exe on GitHub Actions and attaches it to a release.
