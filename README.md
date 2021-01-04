# Gideros Debugger

Gideros Debugger is a modified version of the [VSCodeLuaDebug](https://github.com/devcat-studio/VSCodeLuaDebug/blob/master/Extension/README.md) by [devCAT](https://github.com/devcat-studio).

It's function is to enable Gideros projects to be launched from within VSCode.

## Installation

From a terminal window, cd to the folder where the extensions .vsix file is located and execute the following command...

    code --install-extension gideros-debug-1.0.0.vsix

Optionally you can first install the [Install .VSIX](https://marketplace.visualstudio.com/items?itemName=fabiospampinato.vscode-install-vsix) extension by [Fabio Spampinato](https://marketplace.visualstudio.com/publishers/fabiospampinato), then right-click the extension and install directly from the context menu inside VSCode.

## Usage
Open your Gideros project folder in VSCode using the *File > Open Folder* menu option or the *CTRL + K CTRL + O* hotkey combination.

Once your project folder is open, you will need to create the *launch.json file* which the extension will use to launch your Gideros project inside the Gideros Player. This can be done by selecting the *Run > Add Configuration* menu option, and then clicking *Gideros Player* from the drop-down selector.

The *launch.json* file will be created and opened for you in a new VSCode editor tab. Locate the following lines in the file...

    "giderosPath": "C:/Gideros",
    "gprojPath": "${workspaceRoot}/GPROJ.gproj",

Edit the filename to match the name of your projects .gproj file, and change the path to your Gideros instalation path. Save the *launch.json* file, and close it's editor tab.

You can now press *F5* and your Gideros project should be launched inside the Gideros Player.

If your project didn't launch

## Help and Bugs
Feel free to open an issue if you have issues or questions. Just head on over to the [issues page](https://github.com/Antix-Development/VSCode-Gideros-Debug/issues) on the Gideros Debugger repository.
