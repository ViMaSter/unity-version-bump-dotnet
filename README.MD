# Unity Version Bump [![integration test status](https://github.com/ViMaSter/unity-version-bump/actions/workflows/integration.yml/badge.svg)](https://github.com/ViMaSter/unity-version-bump/actions/workflows/integration.yml) [![unit test status](https://github.com/ViMaSter/unity-version-bump/actions/workflows/unit.yml/badge.svg)](https://github.com/ViMaSter/unity-version-bump/actions/workflows/unit.yml) [![codecov](https://codecov.io/gh/ViMaSter/unity-version-bump/branch/main/graph/badge.svg?token=5Q8BPW6MQU)](https://codecov.io/gh/ViMaSter/unity-version-bump)

Creates pull requests for outdated Unity Editor and UPM package versions.

## Usage

Simply add this file as `.github/workflows/unityVersionCheck.yml`.  
It's is an [opinionated example](https://basecamp.com/gettingreal/04.6-make-opinionated-software) which
- every day at 03:00 a.m.
- checks for new
  - [stable stream releases](https://unity3d.com/get-unity/download/archive)
  - [long-term support (LTS) stream releases](https://unity3d.com/unity/qa/lts-releases)
- and creates a PR if
  - the Unity project 
    - at the root of the repository this action is called from <sup>(see [My project is inside a subdirectory of my repository](#my-project-is-inside-a-subdirectory-of-my-repository))</sup>
    - is older than any found release

```yml
name: Check for new Unity version
on:
  schedule:
    - cron: '* 3 * * *'

jobs:
  updateCheck:
    name: Create PR for newer Unity version
    runs-on: ubuntu-latest
    steps:
      - name: Checkout your repository
        uses: actions/checkout@v2

      - name: Create PR if needed
        uses: ViMaSter/unity-version-bump@v1
        with:
          releaseStreams: Stable,LTS
```


## Advanced usage

<details>
  <summary id="my-project-is-inside-a-subdirectory-of-my-repository">My project is inside a subdirectory of my repository</summary>

Assuming Unity's `Assets` directory resides at `[YOUR_REPOSITORY_ROOT]/projects/my-game/Assets`, add the `projectPath` [input parameter](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions#jobsjob_idstepswith) as follows:

```yml
      - name: Create PR if needed
        uses: ViMaSter/unity-version-bump@v1
        with:
          projectPath: projects/my-game # <-- add this line
          releaseStreams: Stable,LTS
```
</details>

<details>
  <summary>I would like to also get notified for alpha/beta stream releases</summary>

The `releaseStreams` parameter accepts a comma-separated list of the following values **in any order**:
  - `Alpha`
  - `Beta`
  - `Stable`
  - `LTS`
  - `Patch`

**This parameter has no default and needs to be set to at least one release stream.**

### To include beta stream releases next to regular releases...
```yml
      - name: Create PR if needed
        uses: ViMaSter/unity-version-bump@v1
        with:
          projectPath: projects/my-game
          releaseStreams: Stable,LTS,Beta # <-- set this line
```
### To include alpha and beta stream releases next to regular releases...
```yml
      - name: Create PR if needed
        uses: ViMaSter/unity-version-bump@v1
        with:
          projectPath: projects/my-game
          releaseStreams: Stable,LTS,Beta,Alpha # <-- set this line
```
</details>


<details>
  <summary>I would like to only get notified for <a href="https://unity3d.com/unity/qa/lts-releases">long-term support (LTS) stream releases</a></summary>

The `releaseStreams` parameter accepts a comma-separated list of the following values **in any order**:
  - `Alpha`
  - `Beta`
  - `Stable`
  - `LTS`
  - `Patch`

**This parameter has no default and needs to be set to at least one release stream.**

### To **only** include [long-term support (LTS) stream releases](https://unity3d.com/unity/qa/lts-releases)
```yml
      - name: Create PR if needed
        uses: ViMaSter/unity-version-bump@v1
        with:
          projectPath: projects/my-game
          releaseStreams: LTS # <-- set this line
```
### To **only** include beta stream releases
```yml
      - name: Create PR if needed
        uses: ViMaSter/unity-version-bump@v1
        with:
          projectPath: projects/my-game
          releaseStreams: Beta # <-- set this line
```
</details>
