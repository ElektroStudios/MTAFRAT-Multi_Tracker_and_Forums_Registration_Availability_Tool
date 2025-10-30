# MTAFRAT Change Log 📋

## v1.1.1 *(current)* 🆕

#### 🌟 Improvements:
 - Optimized memory usage by reutilizing a single shared context menu for the "Open Website" button of all dynamic plugins instead of creating one context menu per each plugin, reducing memory consumption from ~620 MB to ~500 MB at application startup.
 - Replaced the spanish terms "solicitud de inscripción" with "solicitud de membresía" to improve clarity and user understanding.

## v1.1.0 🔄

#### 🌟 Improvements:
 - Refactored all plugin JSON files to include three URLs: login page, registration page, and application page.
 - Added a context menu to the 'Open Website' button in each plugin panel, allowing users to choose which URL to open.
 - Reworked all plugin VB source code files to simplify overall logic and structure.
 - All eligible plugins (websites) now also checks too for whether an application form is open.
 - Added a new option in the `Settings` panel to allow UI notifications when a plugin detects an open application form.
 - Minor adjustments were made to improve the clarity and descriptiveness of log messages.
 - Added an application manifest file to require Administrator privileges, which should avoid previous issues related to cache folders creation at runtime.
 - Added six new helper methods for plugin developers:
   - `PrintMessage`
   - `PrintMessageFormat`
   - `DefaultRegistrationFormCheckProcedure`
   - `DefaultApplicationFormCheckProcedure`
   - `EvaluateRegistrationFormState`
   - `EvaluateApplicationFormState`
     Note that the `Evaluate*Formstate` functions are more intended for internal use, so they are not documented in the included README file.
 - Updated Selenium nuget package to version 4.38.0

#### 🛠️ Fixes:
 - The `Settings` panel loses focus during automatic plugin execution.
 - The "Remember Current Settings" button position was misaligned.
 - Automatic scrolling in the log TextBox of each plugin panel stopped working properly when the panel lost focus.

## v1.0.5 🔄

#### 🌟 Improvements:
 - Added a link in the 'About' dialog window pointing to the GitHub's application repository page.
 - Description text in the 'About' dialog window is now translated.
 - Tab navigation has been adjusted to reflect the new added controls.

#### 🛠️ Fixes:
 - Button "Clear cache" in the plugin tabs was not properly translated.

#### 📦 Installer changes:

 - The installer now asks for confirmation before overwriting the contents of the 'plugins' folder during installation, allowing to preserve any custom or modified plugins, allowing to preserve any custom or modified plugins.

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

 - The installer now requires admin privileges (preventing errors when creating the 'cache' folder) and prompts whether to delete the 'plugins' folder on uninstall, allowing to preserve any custom or modified plugins.

## v1.0.3 🔄
Initial public release on GitHub.

## v1.0.2 🔄
Private release.

## v1.0.1 🔄
Private release.

## v1.0.0 🔄
Private release.