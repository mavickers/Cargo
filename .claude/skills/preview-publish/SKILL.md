---
name: preview-publish
description: Tag a release, push the tag, and create GitHub Release notes. Use when the user wants to publish a new version.
allowed-tools: Bash, Read, Glob, Grep, AskUserQuestion
---

# Preview Publish

Prepare and publish a new release by tagging, pushing, and creating GitHub Release notes.

## Steps

1. **Verify clean state.** Run `git status` — warn the user if there are uncommitted changes and stop.

2. **Get the Nerdbank version.** Build in Release mode and find the `.nupkg` filename to determine the version Nerdbank calculated:
   - `dotnet build Cargo/Cargo.csproj --configuration Release --verbosity quiet`
   - Find the newest `.nupkg` in `Cargo/bin/Release/` to extract the version number

3. **Check for conflicts.** Verify the version doesn't already exist as a git tag (`git tag -l <version>`). If it does, warn the user and stop.

4. **Show the version and ask for confirmation.** Display the version number and ask the user to confirm before proceeding.

5. **Tag the commit.** `git tag <version>`

6. **Push the tag.** `git push origin <version>` — this triggers the GitHub Actions workflow that publishes to NuGet.

7. **Draft release notes.** Compare the new tag against the previous tag:
   - `git tag --sort=-creatordate` to find the previous tag
   - `git log <previous-tag>..HEAD --oneline` to get the commit list
   - Draft release notes organized into sections (Breaking Changes, New Features, Bug Fixes, Internal) based on the commits. Only include sections that have content.

8. **Show the release notes** in a fenced code block and ask the user to confirm or revise.

9. **Create the GitHub Release.** Use `gh release create <version> --title "<version>" --notes "<notes>"`.

10. **Confirm.** Display the release URL and note that the GitHub Action will handle the NuGet publish.

## Important

- Do not proceed past confirmation steps without user approval.
- Do not create the tag or release if there are uncommitted changes.
- The NuGet publish is handled by the GitHub Actions workflow triggered by the tag push — do not attempt to push to NuGet directly.
