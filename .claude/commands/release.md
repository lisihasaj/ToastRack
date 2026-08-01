---
description: Commit pending work, then release only the targets that actually changed — NuGet package (tag-driven) and/or the GitHub Pages sample (branch-driven)
argument-hint: "[version, e.g. 0.2.0 — omitted = infer from CHANGELOG/semver]"
allowed-tools: Bash(git *), Bash(gh *), Bash(dotnet *)
---

Release ToastRack. There are two independent deploy targets, driven by two different
triggers and by two different sets of files:

- **NuGet package** — tag-driven. MinVer derives the package version from git tags, and
  pushing a `v*` tag triggers `.github/workflows/release.yml`, which builds, tests, packs,
  and publishes to NuGet.org (Trusted Publishing) and creates a GitHub Release.
  Only changes **outside** `samples/ToastRack.Sample/` can affect it.
- **Sample app on GitHub Pages** — branch-driven. Pushing to `main` triggers
  `.github/workflows/deploy-pages.yml`, which publishes the Blazor WebAssembly sample to
  https://lisihasaj.github.io/ToastRack/. Only changes **inside**
  `samples/ToastRack.Sample/` can affect it.

**Neither target is released unconditionally.** Step 3 decides which targets are in scope
by diffing against the last released tag, and every later step is conditional on that
decision. Never mint a version, edit `CHANGELOG.md`, or push a tag when nothing outside
the sample changed; never claim a demo deploy when nothing inside the sample changed.

Because several workflows trigger on a push to `main` (`ci.yml` and `deploy-pages.yml`),
never select a run with a bare `gh run list --limit 1` — it returns whichever run GitHub
registered most recently, which is a race. Always pin runs by both workflow and commit, as
the steps below do.

Target version: $ARGUMENTS (if empty, infer it in step 4).

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

3. **Determine which targets are in scope.** This is the gate for everything that follows.
   - Find the last released tag: `LAST=$(git tag --list 'v*' --sort=-v:refname | head -1)`.
     If there is no `v*` tag yet, this is the first release: treat **both** targets as in
     scope and skip the rest of this step.
   - List everything unreleased: `git diff --name-only $LAST..HEAD`.
     Run this **after** step 2 so pending work is included, and note that it covers commits
     already merged to `main` since `$LAST`, not just the current branch.
   - Split the file list:
     - **Pages scope** — any path under `samples/ToastRack.Sample/`.
     - **Package scope** — any path *not* under `samples/ToastRack.Sample/`
       (`src/`, `tests/`, `Directory.Build.props`, `.github/`, `README.md`,
       `CHANGELOG.md`, `LICENSE`, `ToastRack.slnx`, …).
     - Ignore paths that cannot affect either target: `artifacts/`, `bin/`, `obj/`, and
       `.DS_Store`. A diff consisting solely of these counts as *no change*.
   - State the verdict explicitly before proceeding, with the file counts that justify it:
     - **Both in scope** → run every step below.
     - **Package only** (nothing under `samples/ToastRack.Sample/`) → run steps 4–8, but
       skip step 6a and report the demo as *not redeployed, unchanged*. Note that pushing
       `main` in step 6 still triggers `deploy-pages.yml` — that republishes byte-identical
       content and is harmless, so do not treat it as a deploy of new work.
     - **Pages only** (everything under `samples/ToastRack.Sample/`) → **skip steps 4, 5,
       7 and 8 entirely**: no version bump, no `CHANGELOG.md` release entry, no tag, no
       NuGet release. Run step 6 (push `main`, which deploys the demo) and step 6a, then
       report per step 9.
     - **Neither** → there is nothing to release. Say so and STOP; do not commit, tag, or
       push an empty version.

4. **Determine the version.** *(Skip when the package is out of scope.)*
   If no version was passed as an argument:
   - Read the latest tag: `git tag --sort=-v:refname` (ignore non-`v*` tags).
   - Read the `## [Unreleased]` section of `CHANGELOG.md` and pick the semver bump:
     breaking changes → major (minor while still 0.x), new features → minor,
     fixes/tweaks only → patch.
   - Base the bump on the **package-scope** changes from step 3. Sample-only entries in
     `## [Unreleased]` describe the demo, not the package, and must not by themselves
     justify a version bump.
   - State the chosen version and one-line reasoning before proceeding.

5. **Update CHANGELOG.md.** *(Skip when the package is out of scope.)*
   Rename `## [Unreleased]` to `## [X.Y.Z] - <today's date>` and add a fresh empty
   `## [Unreleased]` heading above it. Commit as `chore: prepare vX.Y.Z release`.

6. **Merge to main.** *(Always runs — it is how the demo deploys and how CI validates.)*
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

6a. **Verify the GitHub Pages deploy.** *(Only when Pages is in scope per step 3.)*
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

7. **Tag and push the tag.** *(Skip when the package is out of scope.)*
   This is the NuGet deploy trigger — confirm with the user before this step, showing the
   version and the commit it will tag. Then:
   - `git tag vX.Y.Z`
   - `git push origin main vX.Y.Z`

8. **Watch the Release workflow to completion.** *(Skip when the package is out of scope.)*
   - `gh run watch $(gh run list --workflow release.yml --limit 1 --json databaseId --jq '.[0].databaseId')`
     (`release.yml` triggers only on `v*` tags, so it has no competing runs to race with.)
   - On failure, fetch the failing job logs (`gh run view --log-failed`) and report the
     cause. Do NOT delete or re-push the tag to retry without explicit user approval.

9. **Report both deploy targets — including the ones deliberately skipped.** State for each:
   - **NuGet**: either the published version and URL
     (`https://www.nuget.org/packages/ToastRack/X.Y.Z` — note it may take a few minutes to
     index) plus the GitHub Release URL, **or** "skipped — no changes outside
     `samples/ToastRack.Sample/` since `<last tag>`", naming that tag.
   - **GitHub Pages**: either the live demo (`https://lisihasaj.github.io/ToastRack/`) and
     whether its deploy succeeded or failed, **or** "skipped — no changes inside
     `samples/ToastRack.Sample/` since `<last tag>`".

   If either target failed, say which one plainly rather than reporting the release as
   fully successful. A skipped target is a correct outcome, not a failure — report it as
   such.

## Rules

- Never `--force` push, never delete tags, never skip tests.
- Never bump the version, edit `CHANGELOG.md`'s release heading, or push a tag when
  step 3 found no package-scope changes. A sample-only change is a demo deploy, not a
  release, and must not consume a version number.
- Never report a demo deployment as part of the release when step 3 found no Pages-scope
  changes, even though pushing `main` re-runs `deploy-pages.yml`.
- The `nuget` environment on the release job may require manual approval in GitHub —
  if the run sits waiting, tell the user to approve it at the run URL.
- Never select a workflow run with a bare `gh run list --limit 1`. Pin by `--workflow`
  and `--commit`, since multiple workflows trigger on a push to `main`.
- A failed Pages deploy never blocks the NuGet release, and a failed NuGet release never
  gets "fixed" by redeploying Pages. Treat the two targets independently and report them
  independently.
- If the working tree is clean and main already matches the last tag, there is nothing
  to release — say so instead of minting an empty version. If the user explicitly wants
  only to refresh the live demo without any new commits, that does not need a release at
  all: run the Pages workflow directly with
  `gh workflow run deploy-pages.yml --ref main`.
- If the user explicitly asks to release a target that step 3 ruled out of scope (e.g. to
  force a package republish), do it and say that it overrides the scope check.
