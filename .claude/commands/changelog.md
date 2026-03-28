---
name: changelog
description: Generate changelog entries for Tsundoku by analyzing git commits since the last version tag
user_invocable: true
---

# Generate Changelog

Generate user-facing changelog entries for the Tsundoku app by analyzing git history.

## Usage

```txt
/changelog <version>
```

Where `<version>` is the new version string (e.g., `2.0.0`). If omitted, read the current version from the `<Version>` property in `Src/Tsundoku.csproj`.

## Changelog File

Entries are stored in `Src/Assets/changelog.json`. This JSON file is an embedded resource read by the app at runtime. Format:

```json
{
  "2.0.0": [
    "Entry one",
    "Entry two"
  ],
  "2.1.0": [
    "Entry one"
  ]
}
```

## Workflow

1. **Determine version range.** Find the most recent git tag:
   ```bash
   git tag --sort=-creatordate | head -1
   ```
   Use that tag as the base. If no tags exist, use the initial commit.

2. **Gather context.** Run these in parallel:
   - `git log <last-tag>..HEAD --pretty=format:"%h %s%n%b"` — commit messages with bodies
   - `git diff <last-tag>..HEAD --stat` — file change summary
   - `git diff <last-tag>..HEAD --name-status` — added/modified/deleted files

3. **Read the full diff.** The diff is essential — commit messages alone are not enough.
   - Run `git diff <last-tag>..HEAD` and capture the output.
   - **If the diff is large (>8000 chars):** write it to a temp file, read the file in chunks using the Read tool (with offset/limit), then delete the temp file when done:
     ```bash
     git diff <last-tag>..HEAD > /tmp/changelog-diff.patch
     ```
     Read it in chunks:
     ```
     Read /tmp/changelog-diff.patch (offset=0, limit=500)
     Read /tmp/changelog-diff.patch (offset=500, limit=500)
     ... continue until fully read ...
     ```
     Clean up when done:
     ```bash
     rm /tmp/changelog-diff.patch
     ```
   - **If the diff is small (<8000 chars):** read it directly from the bash output.
   - Focus on behavioral changes, not implementation details.

4. **Write user-facing changelog notes.** These are NOT commit messages — they describe what the user experiences differently. Rules:
   - Each entry is one short sentence, imperative mood (e.g., "Cover images are now stored at 2x resolution")
   - Group related commits into single entries
   - Skip internal refactors, CI changes, and code cleanup unless they affect the user
   - Keep to 3-8 entries — concise, not exhaustive
   - **User action items are REQUIRED.** If any change means the user should do something (refresh series, re-upload covers, re-download data, change a setting, etc.), add a dedicated entry that starts with an action verb telling them exactly what to do. For example:
     - "Refresh existing series via the edit dialog to get higher quality cover images"
     - "Re-upload custom covers for sharper display on high-DPI screens"
   - These action entries should come AFTER the feature entries that explain what changed and why

5. **Write to `Src/Assets/changelog.json`.** Read the existing file, add the new version entry, and write it back. Preserve all existing entries. The new version key should be added at the END of the JSON object so entries are in chronological order.

6. **Also output the raw bullet list** to the conversation for use in GitHub releases:

```txt
- <entry 1>
- <entry 2>
- <entry 3>
```

## Important

- DO NOT just reword commit messages. Read the actual code changes and describe behavioral impact.
- DO NOT include entries about things users cannot see (internal perf, code style, CI pipeline).
- DO include entries about new features, changed behavior, fixed bugs, and required user actions.
- Always clean up temp files after reading.
- The JSON file is the source of truth — do NOT edit `Src/Models/Changelog.cs` directly.
