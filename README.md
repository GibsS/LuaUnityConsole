# LuaUnityConsole
A simple lua console for Unity3D that uses Mervill's Integration of NLua to Unity3D 
(https://github.com/Mervill/Unity3D-NLua)

This can be a practical tool, whether you want to implement simple debugging functionality like 
object spawning and teleportation or if you want to implement quick and dirty first version of features 
to come such as scene selection or character stat evolution.

It also comes with a "in editor" console that provides similar features.

# How to setup

1. Download the repository in your asset folder
2. Add the GameConsole.cs MonoBehaviour on one of the GameObjects
3. If need be, configure the console by calling Shell's functions :
```cs
// Shell is a static class that serves as a facade to every functionality
Shell.registerObject("some_name", some_obj);
Shell.registerGameObject(some_go);
Shell.registerNamespace("System.IO");

// register the default location where scripts are saved, if this is not called, it will default to the path to
// streaming assets
Shell.setScriptRoot("Some_absolute_path");
```

# "Native" functions

LuaConsole has default functions that allow you to interact with the console from the lua code :

```lua
print 'some stuff'
clear() 

show_editor()
show_history()
hide_editor()
hide_history()
hide()            -- Hides the whole UI

root 'Some_path'  -- Sets the default path for scripts
load 'Some_path'  -- Loads file at path in editor, if its not a rooted path,
                  -- it fetches it relatively to the root
save()            -- Saves the current script in its current path
save_to_new 'Some_path' -- Saves the script in editor at the given path

run 'Some_path'   -- Runs the script at the given path
run_editor()      -- Runs the code in the editor
```

# Logger

This plugin provides a replacement for the default Unity "Debug" logging mechanism. It hads the concept of channels and allows message to contain more information.

The logger has 5 types of message

* info
* warning
* error
* exception
* test

For each type, you can log a message using :
```cs
Shell.[The type]("Some message")
Shell.[The type]("Some message", the_calling_object)
Shell.[The type]("Some message", the_calling_object, "the channel name")
```

For example, to signal an error pertaining a missing GameObject :

```cs
Shell.error("The game Object is missing")
```

# Other features

The plugin has several smaller features, test them out.

## Multiline command

You can start a command in the console, press enter and, if the command is 
not deemed "finished" (missing "end", missing end bracket "}" of lua table),
continue it in the next command.

## History

Use up and down arrow to move through past commands.

## Stack analysis

In the editor console, selecting a log will show the call's stack. You can go to the code of every line of the stack.

# Contact

This is a very modest console that can serve many purposes within the development of a unity project.
If you have any questions or if you want to contribute, feel free to ask.

email : emerick.gibson@hotmail.fr
