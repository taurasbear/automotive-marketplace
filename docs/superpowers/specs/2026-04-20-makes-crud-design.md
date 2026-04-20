# Makes CRUD Feature Design

## Problem

The `Make` entity only supports `GetAll` today. Admins have no way to create, edit, or delete makes through the application. This spec defines a full CRUD feature for Makes, mirroring the existing Models CRUD pattern with one UX improvement: a delete confirmation dialog.

## Approach

Mirror the Models CRUD pattern (Option A). No shared abstractions — each feature stays self-contained in its own folder. One UX improvement over Models: the delete action in the Makes table triggers an `AlertDialog` confirmation instead of firing immediately.

---

## Backend

### Permissions

Add three new values to `Automotive.Marketplace.Domain/Enums/Permission.cs`:

```csharp
ViewMakes,
CreateMakes,
ManageMakes,
```

### New CQRS Handlers

All new handlers live under `Automotive.Marketplace.Application/Features/MakeFeatures/`.

| Feature folder   | Type    | Permission guard              |
|------------------|---------|-------------------------------|
| `GetMakeById/`   | Query   | `ViewMakes`                   |
| `CreateMake/`    | Command | `ManageMakes`, `CreateMakes`  |
| `UpdateMake/`    | Command | `ManageMakes`                 |
| `DeleteMake/`    | Command | `ManageMakes`                 |

**`GetMakeByIdQuery`**: `{ Id: Guid }` → `GetMakeByIdResponse`

**`GetMakeByIdResponse`**:
```csharp
Guid Id
string Name
DateTime CreatedAt
DateTime? ModifiedAt
string CreatedBy
string ModifiedBy
```

**`CreateMakeCommand`**: `{ Name: string }` → `IRequest` (returns void, 201 Created)

**`UpdateMakeCommand`**: `{ Id: Guid, Name: string }` → `IRequest` (returns void, 204 NoContent)

**`DeleteMakeCommand`**: `{ Id: Guid }` → `IRequest` (returns void, 204 NoContent)

### GetAllMakesResponse Update

Add audit fields to `GetAllMakesResponse` so the admin table can display them. Backward-compatible — the dropdown (`MakeSelect`) only reads `id`/`name`.

```csharp
Guid Id
string Name
DateTime CreatedAt
DateTime? ModifiedAt
string CreatedBy
string ModifiedBy
```

### MakeMappings.cs Updates

Add mappings for all new commands and responses:
- `Make → GetMakeByIdResponse`
- `CreateMakeCommand → Make`
- `UpdateMakeCommand → Make`

### MakeController.cs Updates

| Method   | Route              | Body/Query                  | Permission guard              | Returns    |
|----------|--------------------|------------------------------|-------------------------------|------------|
| GET      | `/Make/GetAll`     | —                            | none (public)                 | 200 OK     |
| GET      | `/Make/GetById`    | `?id=<guid>` (query)         | `ViewMakes`                   | 200 OK     |
| POST     | `/Make/Create`     | `{ name }` (body)            | `ManageMakes`, `CreateMakes`  | 201 Created|
| PUT      | `/Make/Update`     | `{ id, name }` (body)        | `ManageMakes`                 | 204 NoContent |
| DELETE   | `/Make/Delete`     | `?id=<guid>` (query)         | `ManageMakes`                 | 204 NoContent |

---

## Frontend

### Shared file updates

**`src/constants/endpoints.ts`** — extend `MAKE`:
```ts
MAKE: {
  GET_ALL:  "/Make/GetAll",
  GET_BY_ID: "/Make/GetById",
  CREATE:   "/Make/Create",
  UPDATE:   "/Make/Update",
  DELETE:   "/Make/Delete",
}
```

**`src/types/make/GetAllMakesResponse.ts`** — add audit fields:
```ts
export type GetAllMakesResponse = {
  id: string;
  name: string;
  createdAt: string;
  modifiedAt: string | null;
  createdBy: string;
  modifiedBy: string;
};
```

**`src/api/queryKeys/makeKeys.ts`** — add detail key:
```ts
export const makeKeys = {
  all: () => ["make"],
  byId: (id: string) => ["make", id],
};
```

### New feature: `src/features/makeList/`

```
makeList/
  api/
    getMakeByIdOptions.ts        ← useSuspenseQuery in EditMakeDialogContent
    useCreateMake.ts
    useDeleteMake.ts
    useUpdateMake.ts
  components/
    MakeListTable.tsx            ← AlertDialog delete confirmation (UX improvement)
    CreateMakeDialog.tsx
    EditMakeDialog.tsx
    EditMakeDialogContent.tsx
    MakeForm.tsx                 ← single "Name" field (no parent selector)
  schemas/
    makeFormSchema.ts            ← z.object({ name: z.string().min(1) })
  types/
    CreateMakeCommand.ts         ← { name: string }
    DeleteMakeCommand.ts         ← { id: string }
    GetMakeByIdQuery.ts          ← { id: string }
    GetMakeByIdResponse.ts       ← mirrors BE response
    MakeFormData.ts              ← { name: string }
    UpdateMakeCommand.ts         ← { id: string; name: string }
  index.ts                       ← exports CreateMakeDialog, MakeListTable
```

**`getAllMakesOptions`** from `@/api/make/getAllMakesOptions` is reused (not duplicated) — the feature imports it directly since it already exists and is shared with `MakeSelect`.

### Delete confirmation UX

`MakeListTable` replaces the inline delete button with an `AlertDialog`:
- Trigger: trash icon button (same as Models)
- Dialog: "Are you sure? This action cannot be undone."
- Confirm: calls `deleteAsync` and closes dialog
- Cancel: closes dialog

---

## Out of Scope

- Pagination for the makes table
- Bulk delete
- Validation that a make has no associated models before deletion (no cascade protection)
- Any changes to the Models feature
