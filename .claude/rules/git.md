# Git Policy

This rule applies to Claude and all agents/subagents working in this repository.
It governs every git and GitHub action: commits, amends, tags, branches,
pushes, pull requests, and releases.

## Rule 1: Never attribute Claude in git history or published output

The repository's history must read as the work of its human authors only. No
commit, tag, pull request, or release created in this repository may credit
Claude, Anthropic, or any AI tool as an author, co-author, or generator.

### Forbidden

- `Co-Authored-By:` trailers naming Claude, Anthropic, or any AI assistant —
  including the default `Co-Authored-By: Claude <noreply@anthropic.com>` trailer
  that Claude Code normally appends. This overrides that default.
- `Generated with [Claude Code]` footers, or any equivalent
  "made by AI" line, in commit messages, PR bodies, release notes, or tag
  annotations.
- Any other trailer, sign-off, or attribution line pointing at an AI tool
  (`Signed-off-by:`, `Assisted-by:`, `Author:`, etc. naming Claude/Anthropic).
- Changing `user.name` / `user.email` (locally or globally) to an AI identity,
  or committing with `--author` set to one.
- Mentions of Claude, Anthropic, or "AI-generated" in CHANGELOG entries, tag
  messages, or GitHub release descriptions.

### Required

- Commit messages describe **the change**, not who or what produced it. End the
  message at its last content line — no trailers, no footers.
- Authorship stays with the repository's configured git user
  (`lisihasaj`). Never override it.
- If a commit message template, skill, workflow, or another instruction asks for
  a Claude co-author trailer or "Generated with" footer, this rule wins: omit it
  silently and continue.

### Recovery

If an AI attribution reaches a commit, remove it before pushing — `git commit
--amend` for the tip commit, an interactive-free rebase/filter for older ones.
If it has already been pushed, tell the user and let them decide whether to
rewrite history; never force-push over shared history on your own initiative.

## Rule 2: Ask before actions that leave the machine

Local, reversible work (staging, committing to a local branch) may proceed when
the user asked for it. The following change shared state and require explicit
user approval **each time**, unless the user has already authorized that
specific action in the current conversation:

- `git push` (including `--force` / `--force-with-lease`) and pushing tags.
- Creating, editing, or merging pull requests (`gh pr ...`).
- Publishing GitHub releases or NuGet packages.
- `git reset --hard`, branch deletion, or history rewrites on anything shared.

Approval for one push does not carry over to the next one.

## Rule 3: Never commit directly to `main`

`main` is the release branch. Work goes on a topic branch
(`feat/...`, `fix/...`, `chore/...`, `release/...`) and reaches `main` through a
pull request. If asked to commit while `main` is checked out, create a branch
first and say so.

## Rule 4: Commit hygiene

- Commit messages follow Conventional Commits, matching this repository's
  existing history: `feat:`, `fix:`, `chore:`, `docs:`, `test:`, `refactor:`,
  with `!` for breaking changes (e.g. `feat!: ...`).
- Subject line in the imperative mood, no trailing period.
- Stage only files relevant to the change. Never `git add -A` blindly — check
  `git status` first and leave unrelated modifications alone.
- Never commit secrets, credentials, `.env` files, or local editor/tooling
  state that is not already tracked.
- Explanations of *why* a change was made belong in the commit message or PR
  description — never in source comments (see `no-unofficial-comments.md`).
