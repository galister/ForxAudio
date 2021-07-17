# ForxAudio

ForxAudio is a simple systray utility for Windows 10 that will re-apply your selected audio devices whenever Windows decides to change them (e.g. due to a driver change or plugging something new in).

Upon first run, simply right-click the tray icon to set your preferred devices. Clicking "Run on Boot" will cause the app to copy itself to `%LocalAppData%\ForxAudio` and link its exe into the user's Startup folder.

To unlink, simply uncheck "Run on Boot". Note this does not remove the app from `%LocalAppData%\ForxAudio`.

In case you're updating to a newer version, run the new version, uncheck and re-check "Run on Boot" as this will re-install the `%LocalAppData%\ForxAudio` folder.

The toggle "Enable These Devices at All Times" toggles the main functionality of the app. Unchecking this will cause the app to pause - keep running but essentially not do anything - until it's turned back on.

## Libraries used

- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) [MIT License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md)

- [AudioSwitcher](https://github.com/xenolightning/AudioSwitcher) [MS-PL License](https://github.com/xenolightning/AudioSwitcher/blob/master/LICENSE)
