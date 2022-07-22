# Unity Version Bump [![integration test status](https://github.com/ViMaSter/unity-version-bump/actions/workflows/integration.yml/badge.svg)](https://github.com/ViMaSter/unity-version-bump/actions/workflows/integration.yml) [![unit test status](https://github.com/ViMaSter/unity-version-bump/actions/workflows/unit.yml/badge.svg)](https://github.com/ViMaSter/unity-version-bump/actions/workflows/unit.yml) [![codecov](https://codecov.io/gh/ViMaSter/unity-version-bump/branch/main/graph/badge.svg?token=5Q8BPW6MQU)](https://codecov.io/gh/ViMaSter/unity-version-bump)

Creates pull requests for outdated Unity Editor and UPM package versions.

## Usage

The following file is an [opinionated example](https://basecamp.com/gettingreal/04.6-make-opinionated-software) which
- every day at 03:00 a.m.
- checks for new
  - [stable stream releases](https://unity3d.com/get-unity/download/archive)
  - [long-term support (LTS) stream releases](https://unity3d.com/unity/qa/lts-releases)
- and creates a PR if
  - the Unity project
    - at the root of the repository <sup>(see [My Unity project is inside a subdirectory of my repository](#my-unity-project-is-inside-a-subdirectory-of-my-repository))</sup>
    - is older than any found release

Add a file with the following content at `.github/workflows/unityVersionCheck.yml` inside your repository to run enable this tool:

```yml
name: Check for new Unity version
on:
  schedule:
    - cron: "* 3 * * *"

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
          githubToken: ${{ secrets.GITHUB_TOKEN }}
```

## Advanced usage

<details>
  <summary id="my-unity-project-is-inside-a-subdirectory-of-my-repository">My Unity project is inside a subdirectory of my repository</summary>

Assuming Unity's `Assets` directory resides at `[YOUR_REPOSITORY_ROOT]/projects/my-game/Assets`, add the `unityProjectPath` [input parameter](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions#jobsjob_idstepswith) as follows:

```yml
- name: Create PR if needed
  uses: ViMaSter/unity-version-bump@v1
  with:
    unityProjectPath: projects/my-game # <-- add this line
    releaseStreams: Stable,LTS
    githubToken: ${{ secrets.GITHUB_TOKEN }}
```
</details>

<details>
  <summary>Also creating PRs for alpha/beta stream releases</summary>

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
    releaseStreams: Stable,LTS,Beta # <-- set this line
    githubToken: ${{ secrets.GITHUB_TOKEN }}
```

### To include alpha and beta stream releases next to regular releases...

```yml
- name: Create PR if needed
  uses: ViMaSter/unity-version-bump@v1
  with:
    releaseStreams: Stable,LTS,Beta,Alpha # <-- set this line
    githubToken: ${{ secrets.GITHUB_TOKEN }}
```
</details>

<details>
  <summary>Get notified for <b>only</b> <a href="https://unity3d.com/unity/qa/lts-releases">long-term support (LTS) stream releases</a></summary>

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
    releaseStreams: LTS # <-- set this line
    githubToken: ${{ secrets.GITHUB_TOKEN }}
```

### To **only** include beta stream releases

```yml
- name: Create PR if needed
  uses: ViMaSter/unity-version-bump@v1
  with:
    releaseStreams: Beta # <-- set this line
    githubToken: ${{ secrets.GITHUB_TOKEN }}
```
</details>

<details>
  <summary>Highlighting pull requests created by this action (using labels)</summary>

The `pullRequestLabels` parameter accepts a comma-separated list of labels.

**Make sure each label exists as [valid label](https://docs.github.com/en/issues/using-labels-and-milestones-to-track-work/managing-labels) for your repository. Labels missing here, might not be added or cause this action to fail.**

### To add a "dependencies" label to each created pull request...

```yml
- name: Create PR if needed
  uses: ViMaSter/unity-version-bump@v1
  with:
    pullRequestLabels: dependencies # <-- set this line
    releaseStreams: Stable,LTS
    githubToken: ${{ secrets.GITHUB_TOKEN }}
```

### To add a "dependencies" and "unity" label to each created pull request...

```yml
- name: Create PR if needed
  uses: ViMaSter/unity-version-bump@v1
  with:
    pullRequestLabels: dependencies,unity # <-- set this line
    releaseStreams: Stable,LTS
    githubToken: ${{ secrets.GITHUB_TOKEN }}
```
</details>

<details>
  <summary>Using generated data like the current Unity version, newest Unity version or pull request ID in another action</summary>

  This action creates the following [output parameters](https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#setting-an-output-parameter):
  | Output parameter...     | ...which describes...                                                               | ...is set when...                   | Example value    |
  | ----------------------- | ----------------------------------------------------------------------------------- | ----------------------------------- | ---------------- |
  | `has-newer-version`     | Whether or not a newer version of Unity exists inside the specified release streams | Always                              | `True` / `False` |
  | `current-unity-version` | The version of Unity currently used inside this repository                          | Always                              | `2020.3.15f2`    |
  | `newest-unity-version`  | The newest version of Unity available inside the specified release streams          | If newer Unity Version is available | `2021.3.0f1`     |
  | `pull-request-id`       | The ID of the pull request created                                                  | If newer Unity Version is available | `37`             |
</details>
