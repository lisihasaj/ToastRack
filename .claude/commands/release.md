---
description: Commit pending work, merge to main, tag a version, publish to NuGet, and deploy the sample to GitHub Pages
argument-hint: "[version, e.g. 0.2.0 — omitted = infer from CHANGELOG/semver]"
allowed-tools: Bash(git *), Bash(gh *), Bash(dotnet *)
---

Release ToastRack. There are two deploy targets, driven by two different triggers:

- **NuGet package** — tag-driven. MinVer derives the package version from git tags, and
  pushing a `v*` tag triggers `.github/workflows/release.yml`, which builds, tests, packs,
  and publishes to NuGet.org (Trusted Publishing) and creates a GitHub Release.
- **Sample app on GitHub Pages** — branch-driven. Pushing to `main` triggers
  `.github/workflows/deploy-pages.yml`, which publishes the Blazor WebAssembly sample to
  https://lisihasaj.github.io/ToastRack/.

Both fire from this command: the push in step 5 deploys the demo, the tag in step 6
publishes the package. The demo therefore goes live *before* the package — that is
expected, since the demo tracks `main`. Your job is everything up to and including
pushing that tag, then watching both workflows.

Because several workflows now trigger on a push to `main` (`ci.yml` and
`deploy-pages.yml`), never select a run with a bare `gh run list --limit 1` — it returns
whichever run GitHub registered most recently, which is a race. Always pin runs by both
workflow and commit, as the steps below do.

Target version: $ARGUMENTS (if empty, infer it in step 3).

## Steps

1. **Validate locally before touching git history.** From the repo root run:
   - `dotnet build --configuration Release`
   - `dotnet test --configuration Release --no-build`
   - `dotnet format --verify-no-changes --no-restore`

   If anything fails, STOP and report — do not commit, merge, or tag on a red build.
   If only formatting fails, run `dotnet format`, include the result in the commit,
   and re-verify.

2. **Commit pending changes.** Review `git status` and `git diff`, write a conventional
   commit message summarizing the actual changes (e.g. `fix: ...`, `feat: ...`), and
   commit everything relevant. Do not commit build output (`artifacts/`, `bin/`, `obj/`).

3. **Determine the version.** If no version was passed as an argument:
   - Read the latest tag: `git tag --sort=-v:refname` (ignore non-`v*` tags).
   - Read the `## [Unreleased]` section of `CHANGELOG.md` and pick the semver bump:
     breaking changes → major (minor while still 0.x), new features → minor,
     fixes/tweaks only → patch.
   - State the chosen version and one-line reasoning before proceeding.

4. **Update CHANGELOG.md.** Rename `## [Unreleased]` to `## [X.Y.Z] - <today's date>`
   and add a fresh empty `## [Unreleased]` heading above it. Commit as
   `chore: prepare vX.Y.Z release`.

5. **Merge to main.**
   - If already on `main`: just `git pull --rebase origin main` first (abort and report
     on conflicts).
   - If on a feature branch: push it, then merge into main with a merge commit
     (`git checkout main && git pull --rebase origin main && git merge --no-ff <branch>`).
     On conflicts, STOP and report — never resolve release-blocking conflicts silently.
   - Push main, then capture the exact commit so runs can be pinned to it:
     `SHA=$(git rev-parse HEAD)`.
   - Wait for CI to pass on that commit:
     `gh run watch $(gh run list --workflow ci.yml --commit $SHA --limit 1 --json databaseId --jq '.[0].databaseId')`.
     If CI fails, STOP and report the failure — do not tag on a red build.

   This push also deploys the sample to GitHub Pages. Verify it in step 5a.

5a. **Verify the GitHub Pages deploy.**
   - `gh run watch $(gh run list --workflow deploy-pages.yml --commit $SHA --limit 1 --json databaseId --jq '.[0].databaseId')`
   - If the run list is empty or `gh` reports
     `HTTP 404: workflow deploy-pages.yml not found on the default branch`, the workflow
     file has not reached `main` yet (expected on the first release that introduces it).
     Say so and continue — do not treat it as a failure.
   - If the deploy fails, report it with `gh run view --log-failed`, but do NOT stop the
     release: the demo site is independent of the NuGet package. Ask the user whether to
     continue to the tag step or fix Pages first.
   - Note: Pages must be enabled once at **Settings → Pages → Build and deployment →
     Source: GitHub Actions**. If the run fails with a permissions or "Pages site not
     found" error, that setting is the likely cause — tell the user.

6. **Tag and push the tag.** This is the deploy trigger — confirm with the user before
   this step, showing the version and the commit it will tag. Then:
   - `git tag vX.Y.Z`
   - `git push origin main vX.Y.Z`

7. **Watch the Release workflow to completion:**
   - `gh run watch $(gh run list --workflow release.yml --limit 1 --json databaseId --jq '.[0].databaseId')`
     (`release.yml` triggers only on `v*` tags, so it has no competing runs to race with.)
   - On failure, fetch the failing job logs (`gh run view --log-failed`) and report the
     cause. Do NOT delete or re-push the tag to retry without explicit user approval.

8. **Report both deploy targets.** On success, state:
   - the published version and NuGet URL (`https://www.nuget.org/packages/ToastRack/X.Y.Z`
     — note it may take a few minutes to index),
   - the GitHub Release URL,
   - the live demo (`https://lisihasaj.github.io/ToastRack/`) and whether its deploy
     succeeded, was skipped, or failed in step 5a.

   If either target failed, say which one plainly rather than reporting the release as
   fully successful.

## Rules

- Never `--force` push, never delete tags, never skip tests.
- The `nuget` environment on the release job may require manual approval in GitHub —
  if the run sits waiting, tell the user to approve it at the run URL.
- Never select a workflow run with a bare `gh run list --limit 1`. Pin by `--workflow`
  and `--commit`, since multiple workflows trigger on a push to `main`.
- A failed Pages deploy never blocks the NuGet release, and a failed NuGet release never
  gets "fixed" by redeploying Pages. Treat the two targets independently and report them
  independently.
- If the working tree is clean and main already matches the last tag, there is nothing
  to release — say so instead of minting an empty version. If the user explicitly wants
  only to refresh the live demo, that does not need a release at all: run the Pages
  workflow directly with `gh workflow run deploy-pages.yml --ref main`.
