# Xemu Manager — Plan

## Principles

- **Core has no UI dependency.** Everything that talks to the file system, xemu, the network or disc images lives in `XemuManager.Core` and is unit-tested. The Avalonia project only contains Views and ViewModels.
- **Never touch the user's global xemu config by accident.** Read it, and write only through explicit user actions or into separate per-game config files.
- **Never ship or download copyrighted files** (MCPX, BIOS, Microsoft dashboard, games).
- **Linux and Steam Deck are first-class**, not an afterthought: test on them every milestone.

## Milestones

### M0 — Skeleton ✅
- Solution with `Core`, `Desktop` (Avalonia + CommunityToolkit.Mvvm) and `Core.Tests`
- README, plan, git repo

### M1 — Find xemu and read its config
- Locate the xemu executable: user-picked path, portable folder, default install locations, Linux Flatpak (`app.xemu.xemu`)
- Locate `xemu.toml` per platform (`%APPDATA%\xemu\xemu`, `~/.local/share/xemu/xemu`, `~/Library/Application Support/xemu/xemu`, Flatpak data dir, portable mode)
- Read/write `xemu.toml` with a TOML library (e.g. Tomlyn), preserving unknown keys
- Settings screen: xemu path, config path

### M2 — Setup wizard
- Pick MCPX boot ROM, flash BIOS, EEPROM; validate by size/hash where possible
- Hard disk: use an existing image or download the open-source [xemu-dashboard](https://github.com/xemu-project/xemu-dashboard) `xbox_hdd.qcow2`
- Write the chosen paths into `xemu.toml` (`[sys.files]`)
- Show a clear "ready / missing X" status

### M3 — Game library
- Add/remove library folders, scan for `.iso` / `.xiso`
- Parse XDVDFS (XISO) and Redump images (Redump has a video partition before the game partition — detect the offset)
- Read `default.xbe`: certificate → title ID, title name, region; extract the title image if present
- Cache the library as JSON; incremental rescans
- Library view: grid of covers + list view, search

### M4 — Launch
- Launch `xemu -dvd_path <iso>` (with `-full-screen` option)
- Per-game config: copy the global `xemu.toml`, apply overrides, pass `-config_path`
- Track process state (running / exited), last played, play time
- Play time per game even when the user swaps discs from the xemu menu, via QMP:
  - Launch with `-qmp tcp:127.0.0.1:<port>,server,nowait` (xemu forwards unknown args to QEMU)
  - `query-block` → device `ide0-cd1`, `inserted.file` = current disc; map it to the library by path
  - `DEVICE_TRAY_MOVED` event (with `timestamp`) marks a disc change; QMP disconnect marks exit
  - `xemu.toml` is no use here: xemu only saves it on exit, and `-dvd_path` is never written to it
  - Not verified yet: test by hand with `telnet` that the event fires on a disc swap

### M5 — Install and update xemu
- Query GitHub releases of `xemu-project/xemu`
- Windows: download zip, extract to a managed folder
- Linux: AppImage download, or detect Flatpak
- macOS: download and extract the `.app`
- "Update available" indicator

### M6 — Compatibility and metadata
- Import the compatibility list from the xemu website data, match by title ID
- Box art source (to be decided)

### M7 — Steam Deck and extras
- Add games as Non-Steam shortcuts (`shortcuts.vdf`)
- Controller navigation (SDL3) and a big-screen mode
- Optional: install a dashboard into the HDD image (FATX), save management via the xemu-dashboard FTP server

## Packaging

| Platform | Format |
|---|---|
| Windows | Self-contained zip (later: installer) |
| Linux / Steam Deck | AppImage, later Flatpak |
| macOS | `.app` in a zip / dmg |

Built by GitHub Actions for each platform on tagged releases.
