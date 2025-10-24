# MTAFRAT Change Log 📋

## v1.0.5 *(current)* 🆕

#### 🌟 Improvements:
 - Added a link in the 'About' dialog window pointing to the GitHub's application repository page.
 - Description text in the 'About' dialog window is now translated.
 - Tab navigation has been adjusted to reflect the new added controls.

#### 🛠️ Fixes:
 - Button "Clear cache" in the plugin tabs was not properly translated.

#### 📦 Installer changes:

 - The installer now asks for confirmation before overwriting the contents of the 'plugins' folder during installation, allowing to preserve any custom or modified plugins.

## v1.0.4 🔄

#### 🌟 Improvements:
 - Added a button in the `Settings` panel to run all selected plugins on demand.
 - Added a button in the `Settings` panel to clear previous log entries on plugin execution.
 - Each plugin tab now has a button to clear its plugin cache.
 - Improved resource management for Selenium services (`ChromeDriverService` object is now disposed appropriately).
 - Other minor internal resource management improvements with disposable objects and Garbage Collector.
 - Added logic and status messages to differentiate between registration and application forms.
 - Plugins 'HD-Olimpo' and 'HDZero' now also checks for an open application form. (Other plugins may be updated to do the same in future releases.)

#### 🛠️ Fixes:
 - The application used to throw an unhandled exception when no supported language was detected; this is now resolved by automatically falling back to English.
 - Fixed an issue that allowed to show the main window when double-clicking the system tray icon during the initial splash screen.
 - Fixed a minor, internal issue where function `CanFocusNextPluginButtonControl` did not return a value on all code paths.

#### 📦 Installer changes:

The installer now requires admin privileges (preventing errors when creating the 'cache' folder) and prompts whether to delete the 'plugins' folder on uninstall, preserving any custom or modified plugins.

## v1.0.3 🔄
Initial public release on GitHub.

## v1.0.2 🔄
Private release.

## v1.0.1 🔄
Private release.

## v1.0.0 🔄
Private release.