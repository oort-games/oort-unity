# Utilities

Oort Unity runtime utilities are available under:

```csharp
OortUnity.Utilities
```

---

## FileUtility

`FileUtility` provides reusable file-system operations.

### Features

- Create directories
- Create parent directories
- Read text files
- Read binary files
- Write text files
- Write binary files
- Append text
- Copy files
- Move files
- Delete files
- Check file existence
- Get file size
- Get last-write time
- Try-based APIs for operations that may fail

### Write Text

```csharp
using OortUnity.Utilities;

string path = "Save/Data.json";

FileUtility.WriteAllText(path, json);
```

Parent directories are created when required by write operations.

### Read Text

```csharp
string text = FileUtility.ReadAllText(path);
```

### Try Pattern

Try-based methods can be used when file-system failures should be handled without propagating supported I/O exceptions to the caller.

```csharp
if (!FileUtility.TryReadAllText(path, out string text))
{
    Debug.LogWarning("Failed to read file.");
}
```

### File Operations

Representative operations include:

```csharp
FileUtility.CreateDirectory(path);
FileUtility.CreateParentDirectory(filePath);
FileUtility.Copy(sourcePath, destinationPath);
FileUtility.Move(sourcePath, destinationPath);
FileUtility.Delete(path);
```

---

## PathUtility

`PathUtility` provides reusable path and file-name operations.

### Features

- Sanitize file names
- Normalize file extensions
- Normalize path separators
- Compare paths
- Check parent and child path relationships
- Generate unique file names
- Generate unique file paths

### Normalize Extension

```csharp
string extension = PathUtility.NormalizeExtension("png");
```

Result:

```text
.png
```

### Normalize Path

```csharp
string path = PathUtility.NormalizePath(
    @"Assets\Editor\Tools\Screenshot");
```

Result:

```text
Assets/Editor/Tools/Screenshot
```

### Sanitize File Name

```csharp
string fileName = PathUtility.SanitizeFileName(fileName);
```

Invalid file-name characters are replaced using the configured replacement character.

### Compare Paths

```csharp
bool same = PathUtility.IsSamePath(pathA, pathB);
bool child = PathUtility.IsSubPathOf(childPath, parentPath);
```

Path comparison follows the platform's path case-sensitivity rules.

### Unique File Name

```csharp
string fileName = PathUtility.GetUniqueFileName(
    folderPath,
    "GameView",
    "png");
```

Possible results:

```text
GameView.png
GameView_1.png
GameView_2.png
```

`GetUniqueFilePath` can be used when the full path is required instead of only the file name.
