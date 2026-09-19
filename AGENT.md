# ExamHub — agent rules

## Style
- Caveman + ponytail always on (full). Reply in Vietnamese, keep it short. Code first, at most 3 lines of explanation.

## Finding code
- Repo has `.codegraph/` → use `codegraph_explore` BEFORE grep/find/reading files. One call returns source + callers + blast radius.

## Workflow
- Big work (feature, refactor, behavior change, planning): `superpowers:brainstorming` first, then code.
- Bug fix: `superpowers:systematic-debugging` first. Fix the root cause, never patch the symptom.
- Small edits / theory questions: skip the skills, just do it.

## UI
- React/TS under `exam_hub_web/` → skill `vercel-react-best-practices`.
- New UI / visual redesign → skill `frontend-design` (aesthetic direction, typography, palette).
- Mockup / wireframe → skill `design`.
- Follow existing patterns in `src/pages`, `src/components`. No new UI libraries.

## Stack
- API: .NET, `exam_hub_api/` — versions are centralized in Directory.Packages.props, never pin them in a .csproj.
- Web: React + TS + Vite, `exam_hub_web/`.
- Dev stack runs via `docker compose -f exam_hub_api/compose.yaml up`.

## Don't
- No commit/push unless asked.
- No new dependency for something a few lines of code can do.
- No new file when editing an existing one is enough.

## Model (user switches with `/model`; the agent cannot switch by itself)
- Planning / brainstorming / hard debugging → Opus 5 High.
- Regular coding, small edits → Sonnet 5 Medium.
- If the agent notices the wrong model is active, mention it in one line — never try to switch.

## Splitting a plan across subagents
After a plan is approved, slice it by stack boundary before writing any code:

1. **Pin the contract first, in the main session.** Endpoint path, request/response DTO, status codes. Write it into the plan. Both sides code against that text, so neither has to wait for the other.
2. **Group the steps into three buckets:**
   - backend-only (`exam_hub_api/**`) → `dotnet-api`
   - frontend-only (`exam_hub_web/**`) → `react-ui`
   - shared or cross-cutting (schema + contract + config, anything both sides read) → keep in the main session, do it first.
3. **Fan out the two buckets in one message** so they run in parallel. Only when neither depends on the other's output — the pinned contract is what makes that true.
4. **Each brief is self-contained**: goal, exact files/symbols to touch, the contract, the acceptance check, the test command. A subagent sees none of this conversation; "the thing we discussed" means nothing to it.
5. **Main session integrates**: read both reports, wire the pieces, run the full build + tests, report real output.

Do NOT fan out when: the work is one or two files, steps are sequential, or the split would cost more explaining than doing. Then just do it inline.

## Review
- Small diff, nothing else running → `/code-review`. No cold start, cheaper.
- Bigger change, or the main session is busy with the next step → fan out the `reviewer` agent in the same message as that step, so the review runs in the background instead of blocking.
- `reviewer` is read-only. Findings come back to the main session; only the main session applies fixes, and only after judging each finding — a subagent's report is a claim, not a verdict.

## Task loop
0. **Start clean.** `/clear` between unrelated tasks; `/compact` only when continuity is genuinely needed — it costs a summarization pass and keeps a lossy tail. Batch every config change (this file, `.claude/agents/`, MCP, plugins) at session start, then restart: the system prefix is cached, and editing it mid-session throws that cache away for every turn after it.
1. **Understand.** One `codegraph_explore` call naming the symbol or file — not a grep/read loop. Read further only where that output is thin.
2. **Plan.** Big work → brainstorm, then write the plan to `docs/plans/<slug>.md` with the API contract pinned. `/clear`, then execute from the file: the design conversation costs more to carry than the plan does to re-read.
3. **Execute.** Sonnet. Split per "Splitting a plan across subagents". Never re-read a file just edited.
4. **Verify.** Targeted tests only — `dotnet test exam_hub_api/ExamHub.Tests --filter <Name>`, `npm --prefix exam_hub_web run test -- <path>`. Keep shell output small: `dotnet build -v q --nologo`, `git diff --stat` before any full diff, `| head` on anything long. Never paste a whole file back into the conversation.
5. **Review, then commit** — see "## Review". Commit only when asked.
