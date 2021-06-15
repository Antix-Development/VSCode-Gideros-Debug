# Gideros Debugger

NOTE: This project is dead due to the inability of the Gideros community to particpate. It remains here in case that somebody else finds it of some use.

Gideros Debugger is a modified version of the [VSCodeLuaDebug](https://github.com/devcat-studio/VSCodeLuaDebug/blob/master/Extension/README.md) by [devCAT](https://github.com/devcat-studio).

Gideros Debugger exists to enable Gideros projects to be launched from within VSCode.

## Usage
Open your Gideros project folder in VSCode using the *File > Open Folder* menu option or the *CTRL + K CTRL + O* hotkey combination.

Once your project folder is open, you will need to create the *launch.json file* which the extension will use to launch your Gideros project inside the Gideros Player. This can be done by selecting the *Run > Add Configuration* menu option, and then clicking *Gideros Player* from the drop-down selector.

The *launch.json* file will be created and opened for you in a new VSCode editor tab. Locate the following line in the file...

    "giderosPath": "C:/Gideros",

Edit the path to the path where your Gideros installation is, then save the *launch.json* file, and close it's editor tab.

You can now press *F5* and your Gideros project should be launched inside the Gideros Player.

If your project didn't launch, review the above steps and if it still doesn't work... raise an issue in GitHub, or browse to the [Gideros Forum thread](http://forum.giderosmobile.com/discussion/8382/gideros-vscode-integration) for this extension.

_Note_ Any output from your project will appear in VSCodes *DEBUG CONSOLE*.

## Help and Bugs
Feel free to open an issue if you have issues or questions. Just head on over to the [issues page](https://github.com/Antix-Development/VSCode-Gideros-Debug/issues) on the Gideros Debugger repository.
