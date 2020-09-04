# Generator
This project generate C/C++ header file, that contains offsets of il2cpp library.


## Differences between branches
Generally you need to use it from ScriptJson branch.

But if you want more performance (less memory usage & faster execution) you can use it.
In my case, it uses ~20mb of ram instead of ~350-400mb and it runs a little faster.

This branch doesn't support comments, patches.
##### Warning! All lines must be ordered like in script.json
You can compile & run in debug mode for ordering.

## Usage example:

First fill in template.json.

Then you can type run following command:

```bash
generator template.json .\path\to\script.json
```

Or you can use bat files.
## Credirts:
https://github.com/Perfare/Il2CppDumper