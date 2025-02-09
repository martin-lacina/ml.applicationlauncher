# ML.ApplicationLauncher / Universal Application Launcher

[![Basic validation](https://github.com/martin-lacina/ml.applicationlauncher/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/martin-lacina/ml.applicationlauncher/actions/workflows/dotnet.yml)

Simple WPF Windows desktop application to provide user the ability to launch commands or processed with click of a button based on a JSON configuration file `CommandDefinitions.json` located in the executable folder.

Notice from author:
> It is a quick hack tool and definitely not "state of the art" piece of software (as you can see there are not even any unit tests...). Use at your own risk, you have been warned.

## Look and feel

![image](https://github.com/user-attachments/assets/98d4d068-5d6c-4255-82a6-88066d80471d)

Main window title contains file name of used configuration file name and indication the application is running with administrator priviledges (usually if you use `ML.ApplicationLauncher.Shell.Admin.exe`).

Toolbar buttons from left to right

| Button | Description |
| ------ | ----------- |
| Exit | Exists application |
| Load commands | Reloads list of commands. Use after changes to the configuration JSON file. <br> Note: JSON parsing is not resilient and it will not load malformed JSON file (like missing commas `,`). You need to fix the issue manually and try again. |
| Edit commands | Allows to open Notepad to edit the JSON file. <br> Note: It does not work well on Windows 11 (24H2) with new Notepad. |
| Clear last executed time | Removes time when the command was last executed from all buttons. |
| About application | Displays dialog with basic information about application, copyright and license. |

## Configuration

JSON configuration file `CommandDefinitions.json` can be created as a copy of `CommandDefinitions.examples.json`.
The example file is used as a fallback in case main file does not exist.

Configuration consists of two types of elements `Groups` and `Processes` while root is list `Groups` that gets rendered as vertical stacks in the UI as "top level groups".

### Groups

Each `Group` has following properties:

| Property | Type | Description | Presence |
| -------- | ---- | ----------- | -------- |
| DisplayName | string | Name of group to show in UI | Mandatory |
| CanLaunch | boolean | Controls if the group can launch all its children with one button. <br> Ignored for top level group like `false` is used  | Optional <br> Default `true` for child `Group` |
| Groups | List of `Groups` | List of nested child groups | Optional |
| Processes | List of `Processes` | List of nested processes | Optional |

### Processes

Each `Process` has following properties:

| Property | Type | Description | Presence |
| -------- | ---- | ----------- | -------- |
| DisplayName | string | Name of group to show in UI | Mandatory |
| Executable| string | Name and full path to executable. <br> Non existent executable leads to disabling the run button (unless `ExecutionMode` is `Raw`) | Mandatory |
| Arguments | List of strings | `Executable` commandline parameters | Optional |
| ExecutionMode | string/enum | Desired execution mode. <br> One of `PowerShell`, `Direct`, `Standalone`, `Raw` | Optional. <br> Default `PowerShell` |
| Hidden| boolean | Allows to hide process entry from UI without removing it from configuration. Such entry is ignored during configuration loading. | Optional. <br> Default `false` |
| WorkingDirectory | Allows to specify working directory for the executed process | Optional. <br> Default unspecified |

#### Execution modes

Application supports several execution modes.

* `PowerShell` process is launched using `Windows PowerShell` (`powershell.exe`, not `pwsh.exe`) in `Windows Terminal` window with ID `ML.ApplicationLauncher` to group launched processed in single Window
  * Launch button checks executable presence if it can be clicked
  * Useful for launching simle commands like `dotnet restore`
  * `Windows Terminal` parameters: ` -w ML.ApplicationLauncher new-tab --title PowerShell -c "{processToLaunch.Executable} {executableArguments}"`
* `Direct` process is launched directly in `Windows Terminal` window with ID `ML.ApplicationLauncher` to group launched processed in single Window
  * Launch button checks executable presence if it can be clicked
  * Useful for launching `cmd.exe` directly
  * `Windows Terminal` parameters: ` -w ML.ApplicationLauncher new-tab --title "{processToLaunch.Executable}" {executableArguments}`
* `Standalone` process is launched directly without `Windows Terminal`
  * Useful for launching UI applications like Notepad
  * Launch button checks executable presence if it can be clicked
* `Raw` is same as `Standalone`, but there is no check if the executable exists
  * Useful for running simple commands you know will work like `cmd.exe` without full path

## Local build

### Prerequisities

* Installed .NET SDK compatible with [`global.json`](global.json), i. e. .NET SDK 8
* Installed PowerShell Core 7

### Steps to build

* Decide if you want `Debug` or `Release` build,. `Release` is meant for general usage.
* Run one of following commands
  * `Publish-Release.cmd` in Windows commandline `cmd.exe` in repository root to create `Release` build
  * `Publish-Debug.cmd` in Windows commandline `cmd.exe` in repository root to create `Debug` build
  * Directly run `Build\Scripts\Publish-Release.ps1` in PowerShell Core 7 `pwsh.exe` to create `Release` build or specify additional parameter `-Config 'Debug'` to create `Debug` build
* Review the build output for any red errors
* The build output folder is `Publish\ML.ApplicationLauncher`
