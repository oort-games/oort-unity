# Oort Unity

Oort Unity is a Unity Package Manager (UPM) package that provides reusable runtime components, editor tools, utilities, and sample content for Unity projects.

## Requirements

- Unity `6000.3.10f1` or later
- Git installed and available from the command line

## Installation

### Install via Git URL

Oort Unity can be installed directly through the Unity Package Manager using a Git URL.

1. Open `Window > Package Management > Package Manager`.
2. Click the `+` button in the upper-left corner.
3. Select `Install package from Git URL...`.
4. Enter one of the following URLs.

#### Install the latest version

```text
https://github.com/oort-games/oort-unity.git
```

This installs the latest package version from the `main` branch.

#### Install a specific version

Append a Git tag after `#` to install a specific release.

```text
https://github.com/oort-games/oort-unity.git#v0.0.1
```

Replace `v0.0.1` with the version you want to install.

### Install through manifest.json

You can also add Oort Unity directly to your project's `Packages/manifest.json`.

```json
{
  "dependencies": {
    "com.oortgames.oortunity": "https://github.com/oort-games/oort-unity.git#v0.0.1"
  }
}
```

To use the latest version from the `main` branch, omit the version tag.

```json
{
  "dependencies": {
    "com.oortgames.oortunity": "https://github.com/oort-games/oort-unity.git"
  }
}
```

## Samples

Sample content can be imported through the Unity Package Manager.

1. Open `Window > Package Management > Package Manager`.
2. Select `Oort Unity`.
3. Open the `Samples` section.
4. Click `Import` next to the sample you want to use.

Imported samples are copied to:

```text
Assets/Samples/Oort Unity/<version>/
```

## Package Structure

```text
com.oortgames.oortunity/
├── Runtime/
├── Editor/
├── Samples~/
├── Tests/
├── package.json
├── CHANGELOG.md
└── Third Party Notices.md
```

- `Runtime`: Runtime code included in player builds.
- `Editor`: Unity Editor-only tools and extensions.
- `Samples~`: Optional samples available through Package Manager.
- `Tests`: Package tests.

## Documentation

Documentation is available from the `Documentation` link in Unity Package Manager.

## Changelog

See [CHANGELOG.md](./CHANGELOG.md) for version history and release notes.

## License

This project is licensed under the MIT License.

See [LICENSE](./LICENSE.md) for details.

## Author

Oort Games  
Contact: `oortgames@gmail.com`
