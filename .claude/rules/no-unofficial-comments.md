# Comment Policy: Official Documentation Comments Only

This rule applies to Claude and all agents/subagents working in this repository.

## Rule

Never write unofficial, informal, or momentary comments in code. The **only**
comments permitted in this codebase are official documentation comments.

### Allowed

- C# XML documentation comments (`///`) on public/internal APIs: `<summary>`,
  `<param>`, `<returns>`, `<remarks>`, `<exception>`, `<typeparam>`, etc.
- JSDoc documentation comments (`/** ... */`) on exported/public JavaScript
  functions in `wwwroot/*.js`.
- Documentation comments that describe **what an API is and how to use it** —
  stable information that remains true regardless of who edits the code next.

### Forbidden

- Momentary comments tied to a fix or change request, e.g.:
  - `// fixed per review feedback`
  - `// changed this to handle the new variant`
  - `// TODO: revisit later`, `// HACK`, `// temporary workaround`
- Narration comments that restate what the next line does, e.g.
  `// increment the counter`.
- Comments explaining why an edit was made or where it came from
  (`// added because tests failed`) — that belongs in the commit message or
  PR description, not the code.
- Commented-out code left behind after a change.
- Conversational notes addressed to a reviewer or to the user.

## How to apply

- When adding or modifying code, do not add any comment unless it is a proper
  documentation comment on an API surface.
- If an explanation of a change is needed, put it in the commit message, PR
  description, or your response to the user — never in the source.
- When editing existing code, do not introduce new non-documentation comments;
  if you encounter forbidden comments in lines you are already changing,
  remove them rather than preserving them.
