# Data Table

## Required behavior

- Toolbar with primary action, refresh, export when useful, and scoped filters.
- Search text must use `lowercase + trim + contains` through the backing API/query.
- Filter status/enum/date/range/owner/module separately from free-text search.
- Sortable important columns.
- Pagination with server-side total count.
- Sticky header and sticky action column for wide business tables.
- Empty state, skeleton/loading state, and error feedback.
- Row action buttons for view/edit/delete or workflow actions based on permission.
- Boolean fields must render as compact icons/badges, not raw `true/false` or text-only `Có/Không`.

## Full-column rule

Tables must not be MVP/demo-only. Each table must expose the important columns from the service DTO/domain.

For wide entities:

- Keep core identity columns visible: code/no, name/title, status, owner, date.
- Add business columns: amount, customer, source, rating, stage, tax, phone/email, etc.
- Use compact templates, badges, column chooser, responsive hiding, or detail drawer instead of removing columns.
- Entity code/name should be clickable when a detail page exists.
- True/false business columns should use the shared boolean icon component with accessible label/title.

## Search implementation contract

Expected behavior:

```text
input: "  AN  "
normalized: "an"
match: field.ToLower().Contains("an")
```

This must work consistently in EF Core repositories, in-memory queries, report queries, and API clients.
