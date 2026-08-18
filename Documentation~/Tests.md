# Tests

Oort Unity includes package tests.

### Test Framework

```text
com.unity.test-framework: 1.1.33
```

To run tests from the installed UPM package, add Oort Unity to the
`testables` property in the consuming project's `Packages/manifest.json`:

```json
{
  "testables": [
    "com.oortgamestudio.oortunity"
  ]
}
```

### Texture Utility Coverage

Editor tests cover:

- Alpha, opacity, and tint blending
- Transparent PNG-style alpha preservation
- Clipping outside the destination texture
- Readable copies of textures with **Read/Write** disabled
- Mid-tone color preservation during readable copies and resizing
- Texture resizing
- Default Full HD watermark size and bottom-right placement
- Screenshot setting validation and watermark reset defaults

### GameObject Icon Generator Coverage

Editor tests cover:

- UI and 3D source auto-detection
- Combined child Renderer bounds
- Proportional framing padding
- Resolution limits and 3D view preset validation
- Icon Generator setting defaults and lighting configuration copies
