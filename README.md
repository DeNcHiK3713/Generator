# Generator
This project generate C/C++ header file, that contains offsets of il2cpp library.

## Usage example:
0. Download latest release [here](https://github.com/DeNcHiK3713/Generator/releases/latest/download/Generator.zip "here").

1. Unzip it somewhere.

2. Then fill in template.json.

3. Download keystone binaries [here](https://github.com/keystone-engine/keystone/releases/tag/0.9.2 "here"), depending on your OS,
and extract keystone.dll to same folder.

3. Then you can type run following command:

```bash
generator template.json .\path\to\script.json .\path\to\libil2cpp.so ARM
```

Or you can use bat files.
## Credirts:
https://github.com/Perfare/Il2CppDumper

https://github.com/keystone-engine/keystone/tree/master/bindings/csharp
