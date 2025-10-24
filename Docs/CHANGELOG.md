# MTAFRAT Change Log 📋

## v1.0.4 *(current)* 🆕

#### 🚀 New Features:
 - Added a button in the `Settings` panel to run all selected plugins on demand.
 - Added a button in the `Settings` panel to clear previous log entries on plugin execution.
 - Each plugin tab now has a button to clear its plugin cache.

#### 🌟 Improvements:
 - Improved resource management for Selenium services (`ChromeDriverService` object is now disposed appropriately).
 - Other minor internal resource management improvements with disposable objects and Garbage Collector.
 - Added logic and status messages to differentiate between registration and application forms.
 - Plugins 'HD-Olimpo' and 'HDZero' now also checks for an open application form. (Other plugins may be updated to do the same in future releases.)

#### 🛠️ Fixes:
 - The application used to throw an unhandled exception when no supported language was detected; this is now resolved by automatically falling back to English.
 - Fixed an issue that allowed to show the main window when double-clicking the system tray icon during the initial splash screen.
 - Fixed a minor, internal issue where function `CanFocusNextPluginButtonControl` did not return a value on all code paths.

## v1.0.3 🔄
Initial public release on GitHub.

## v1.0.2 🔄
Private release.

## v1.0.1 🔄
Private release.

## v1.0.0 🔄
Private release.