# Story Ready Workflow Instructions (SM Agent)

<critical>The workflow execution engine is governed by: {project_root}/bmad/core/tasks/workflow.xml</critical>
<critical>You MUST have already loaded and processed: {installed_path}/workflow.yaml</critical>
<critical>Communicate all responses in {communication_language}</critical>

<workflow>

<critical>This workflow is run by SM agent AFTER user reviews a drafted story and confirms it's ready for development</critical>
<critical>NO SEARCHING - SM agent reads status file TODO section to know which story was drafted</critical>
<critical>Simple workflow: Update story file status, move story TODO → IN PROGRESS, move next story BACKLOG → TODO</critical>

<step n="1" goal="Read status file and identify the TODO story">

<action>Read {output_folder}/bmm-workflow-status.md</action>
<action>Navigate to "### Implementation Progress (Phase 4 Only)" section</action>
<action>Find "#### TODO (Needs Drafting)" section</action>

<action>Extract story information:</action>

- todo_story_id: The story ID (e.g., "1.1", "auth-feature-1", "login-fix")
- todo_story_title: The story title
- todo_story_file: The exact story file path

<critical>DO NOT SEARCH for stories - the status file tells you exactly which story is in TODO</critical>

</step>

<step n="2" goal="Update the story file status">

<action>Read the story file: {story_dir}/{todo_story_file}</action>

<action>Find the "Status:" line (usually at the top)</action>

<action>Update story file:</action>

- Change: `Status: Draft`
- To: `Status: Ready`

<action>Save the story file</action>

</step>

<step n="3" goal="Move story from TODO → IN PROGRESS in status file">

<action>Open {output_folder}/bmm-workflow-status.md</action>

<action>Update "#### TODO (Needs Drafting)" section:</action>

Read the BACKLOG section to get the next story. If BACKLOG is empty:

#### TODO (Needs Drafting)

(No more stories to draft - all stories are drafted or complete)

If BACKLOG has stories, move the first BACKLOG story to TODO:

#### TODO (Needs Drafting)

- **Story ID:** {{next_backlog_story_id}}
- **Story Title:** {{next_backlog_story_title}}
- **Story File:** `{{next_backlog_story_file}}`
- **Status:** Not created OR Draft (needs review)
- **Action:** SM should run `create-story` workflow to draft this story

<action>Update "#### IN PROGRESS (Approved for Development)" section:</action>

Move the TODO story here:

#### IN PROGRESS (Approved for Development)

- **Story ID:** {{todo_story_id}}
- **Story Title:** {{todo_story_title}}
- **Story File:** `{{todo_story_file}}`
- **Story Status:** Ready
- **Context File:** `{{context_file_path}}` (if exists, otherwise note "Context not yet generated")
- **Action:** DEV should run `dev-story` workflow to implement this story

<action>Update "#### BACKLOG (Not Yet Drafted)" section:</action>

Remove the first story from the BACKLOG table (the one we just moved to TODO).

If BACKLOG had 1 story and is now empty:

| Epic                          | Story | ID  | Title | File |
| ----------------------------- | ----- | --- | ----- | ---- |
| (empty - all stories drafted) |       |     |       |      |

**Total in backlog:** 0 stories

<action>Update story counts in "#### Epic/Story Summary" section:</action>

- Decrement backlog_count by 1 (if story was moved from BACKLOG → TODO)
- Keep in_progress_count = 1
- Keep todo_count = 1 or 0 (depending on if there's a next story)

</step>

<step n="4" goal="Update Decision Log, Progress, and Next Action">

<action>Add to "## Decision Log" section:</action>

```
- **{{date}}**: Story {{todo_story_id}} ({{todo_story_title}}) marked ready for development by SM agent. Moved from TODO → IN PROGRESS. {{#if next_story}}Next story {{next_story_id}} moved from BACKLOG → TODO.{{/if}}
```

<template-output file="{{status_file_path}}">current_step</template-output>
<action>Set to: "story-ready (Story {{todo_story_id}})"</action>

<template-output file="{{status_file_path}}">current_workflow</template-output>
<action>Set to: "story-ready (Story {{todo_story_id}}) - Complete"</action>

<template-output file="{{status_file_path}}">progress_percentage</template-output>
<action>Calculate per-story weight: remaining_40_percent / total_stories / 5</action>
<action>Increment by: {{per_story_weight}} \* 1 (story-ready weight is ~1% per story)</action>

<action>Update "### Next Action Required" section:</action>

```
**What to do next:** Generate context for story {{todo_story_id}}, then implement it

**Command to run:** Run 'story-context' workflow to generate implementation context (or skip to dev-story)

**Agent to load:** bmad/bmm/agents/sm.md (for story-context) OR bmad/bmm/agents/dev.md (for dev-story)
```

<action>Save bmm-workflow-status.md</action>

</step>

<step n="5" goal="Automatically invoke story-context workflow">

<critical>Story-ready workflow now automatically generates Story Context XML (AC #14 from Story 3.1b)</critical>
<critical>This eliminates the manual step that was causing context XML to be forgotten</critical>

<action>Load workflow.xml from {project-root}/bmad/core/tasks/workflow.xml</action>
<action>Execute workflow with parameters:</action>

- workflow-config: "{project-root}/bmad/bmm/workflows/4-implementation/story-context/workflow.yaml"
- story_path: "{{story_dir}}/{{todo_story_file}}"
- auto_update_status: false (story-ready already updated status)
- non_interactive: true (no user prompts during auto-generation)

<action>Wait for story-context workflow to complete</action>
<action>Capture context file path from workflow output</action>

<critical>If story-context fails, do NOT halt - continue to step 6 and warn user</critical>

</step>

<step n="6" goal="Confirm completion to user">

<action>Display summary</action>

**Story Marked Ready for Development, {user_name}!**

✅ Story file updated: `{{todo_story_file}}` → Status: Ready
✅ Status file updated: Story moved TODO → IN PROGRESS
✅ Story Context XML auto-generated: `{{context_file_path}}`
{{#if next_story}}✅ Next story moved: BACKLOG → TODO ({{next_story_id}}: {{next_story_title}}){{/if}}
{{#if no_more_stories}}✅ All stories have been drafted - backlog is empty{{/if}}

{{#if story_context_failed}}
⚠️ **Warning:** Story Context XML generation failed. You may need to run `story-context` workflow manually before implementing this story.
{{/if}}

**Current Story (IN PROGRESS):**

- **ID:** {{todo_story_id}}
- **Title:** {{todo_story_title}}
- **File:** `{{todo_story_file}}`
- **Context File:** `{{context_file_path}}`
- **Status:** Ready for development

**Next Steps:**

Load DEV agent and run `dev-story` workflow to begin implementation.

**Automation Note:**

Story Context XML is now automatically generated when marking stories ready (Story 3.1b AC #14).
This ensures DEV agent always has comprehensive context without manual intervention.

</step>

</workflow>
