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
