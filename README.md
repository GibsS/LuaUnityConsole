# LuaUnityConsole
A simple lua console for Unity3D that uses NLua 

This can be a practical tool, whether you want to implement simple debugging functionality like 
object spawning and teleportation or if you want to implement quick and dirty first version of features 
to come such as scene selection or character stat evolution.

# How to setup

1. Download the repository in your asset folder
2. Add the LuaConsole.cs script to one of the GameObject
3. If need be, configure the console by calling LuaConsole's functions :
```cs
LuaConsole console = LuaConsole.luaConsole;

console.registerObject("some_name", some_obj);
console.registerGameObject(some_go);
console.registerNamespace("System.IO");

// register the default location where scripts are saved
console.setScriptRoot("Some_absolute_path");
```

# "Native" functions

LuaConsole has default functions that allow you to interact with the console :

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

# Other features

## Multiline command

You can start a command in the console, press enter and, if the command is 
not deemed "finished" (missing "end", missing end bracket "}" of lua table),
continue it in the next command.

## History

Use up and down arrow to move through past commands.

# Contact

This is a very modest console that can serve many purposes within the development of a unity project.
If you have any questions or if you want to contribute, feel free to ask.

email : emerick.gibson@hotmail.fr
