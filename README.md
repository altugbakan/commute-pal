# CommutePal

Tiny Windows app that asks how you commuted today and keeps monthly totals for the WFH allowance paperwork.

## Install

1. Download `CommutePal.exe` from the latest [release](../../releases/latest) and put it somewhere permanent (e.g. `C:\Tools\CommutePal\`).
2. Run it once. It registers itself to open at Windows sign-in.

## How it works

- At sign-in, if today isn't logged yet, a tiny popup with four icons appears (Bike, Car, Public transport, Home; hover for the name). Click one and it closes. Esc dismisses it without logging. If today is already logged, nothing appears.
- Open the exe by hand to see this month and last month totals, change today's entry, or turn the sign-in prompt off.
- Data is stored as one CSV per month in `%APPDATA%\CommutePal\` (e.g. `2026-08.csv`, rows of `date,mode`).

## Build

```
dotnet publish CommutePal/CommutePal.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

Pushing a tag like `v1.0.1` builds the exe on GitHub Actions and attaches it to a release.

## Development

Visual Studio launch profiles (the dropdown next to Run): **CommutePal** (follows Windows theme), **Dark**, **Light**, **Popup (dark)**, **Popup (light)**.

Command-line flags behind them:

| Flag | Effect |
|------|--------|
| `--startup` | What the sign-in registration uses: show the popup, or exit silently if today is logged |
| `--popup` | Always show the popup (testing) |
| `--dark` / `--light` | Force the theme instead of following Windows |

Layout: `MainWindow.xaml` is the full app, `PopupWindow.xaml` the sign-in prompt, `Controls/MonthPicker.xaml` the date picker. Shared styles, icons and theme colours live in `Themes/`.

Icons by [Lucide](https://lucide.dev) (ISC), see THIRD_PARTY_NOTICES.md.
