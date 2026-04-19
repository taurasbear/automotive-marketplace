# Variant Admin UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add full CRUD admin UI for vehicle variants, switch shared selects to entity-ID mode, and delete the dead `carList` feature.

**Architecture:** Three independent work areas: (1) update four shared select/toggle components to use entity endpoints instead of enum endpoints so they emit GUIDs; (2) build the full variantList CRUD feature (schema → types → mutations → form → dialogs → table → page); (3) delete the dead carList folder.

**Tech Stack:** React 19, TypeScript, TanStack Query v5, React Hook Form, Zod v4, shadcn/ui, Lucide icons.

---

## File Map

### Modified files
| File | Change |
|------|--------|
| `src/components/forms/select/FuelSelect.tsx` | Switch from `getFuelTypesOptions` → `getAllFuelsOptions`; value=`id`, label=`name` |
| `src/components/forms/select/BodyTypeSelect.tsx` | Switch from `getBodyTypesOptions` → `getAllBodyTypesOptions`; value=`id`, label=`name` |
| `src/components/forms/TransmissionToggleGroup.tsx` | Switch from `getTransmissionTypesOptions` → `getAllTransmissionsOptions`; value=`id`, label=`name` |
| `src/components/forms/DrivetrainToggleGroup.tsx` | Switch from `getDrivetrainTypesOptions` → `getAllDrivetrainsOptions`; value=`id`, label=`name` |
| `src/features/variantList/components/VariantListTable.tsx` | Add `makeId` prop, View/Edit/Delete action buttons per row |
| `src/features/variantList/index.ts` | Export `VariantListTable` and `CreateVariantDialog` |
| `src/app/pages/Variants.tsx` | Add `CreateVariantDialog` button when model selected; pass `selectedMake` to table |

### Created files
| File | Purpose |
|------|---------|
| `src/features/variantList/schemas/variantFormSchema.ts` | Zod schema validating all variant form fields |
| `src/features/variantList/types/VariantFormData.ts` | Type inferred from `variantFormSchema` |
| `src/features/variantList/types/CreateVariantCommand.ts` | API command shape for POST |
| `src/features/variantList/types/UpdateVariantCommand.ts` | API command shape for PUT |
| `src/features/variantList/types/DeleteVariantCommand.ts` | API command shape for DELETE |
| `src/features/variantList/api/useCreateVariant.ts` | Mutation hook for `POST /Variant/Create` |
| `src/features/variantList/api/useUpdateVariant.ts` | Mutation hook for `PUT /Variant/Update/{id}` |
| `src/features/variantList/api/useDeleteVariant.ts` | Mutation hook for `DELETE /Variant/Delete/{id}` |
| `src/features/variantList/components/VariantForm.tsx` | 4-col grid form for create/edit |
| `src/features/variantList/components/CreateVariantDialog.tsx` | Dialog wrapping VariantForm for create |
| `src/features/variantList/components/EditVariantDialog.tsx` | Dialog wrapping VariantForm for edit |
| `src/features/variantList/components/EditVariantDialogContent.tsx` | Content component (receives variant data directly, no re-fetch) |
| `src/features/variantList/components/ViewVariantDialog.tsx` | Dialog wrapper for read-only view |
| `src/features/variantList/components/ViewVariantDialogContent.tsx` | Read-only display of all variant fields |

### Deleted
| Path | Reason |
|------|--------|
| `src/features/carList/` (entire folder) | Dead code — backend Car API replaced by Variant |

---

## Task 1: Switch `FuelSelect` to entity-based options

**Files:**
- Modify: `src/components/forms/select/FuelSelect.tsx`

- [ ] **Step 1: Replace the import and data source**

Replace the entire file content:

```tsx
import { getAllFuelsOptions } from "@/api/enum/getAllFuelsOptions";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";
import { SelectRootProps } from "@/types/ui/selectRootProps";
import { useQuery } from "@tanstack/react-query";

type FuelSelectProps = SelectRootProps & {
  className?: string;
};

const FuelSelect = ({ className, ...props }: FuelSelectProps) => {
  const { data: fuelsQuery } = useQuery(getAllFuelsOptions);

  const fuels = fuelsQuery?.data || [];

  return (
    <div className={cn(className)}>
      <Select {...props}>
        <SelectTrigger className="w-full">
          <SelectValue placeholder="Petrol" />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>Fuel types</SelectLabel>
            {fuels.map((fuel) => (
              <SelectItem key={fuel.id} value={fuel.id}>
                {fuel.name}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default FuelSelect;
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
cd automotive.marketplace.client && npx tsc --noEmit 2>&1 | head -30
```

Expected: no errors related to FuelSelect.

- [ ] **Step 3: Commit**

```bash
git add src/components/forms/select/FuelSelect.tsx
git commit -m "feat: switch FuelSelect to entity-based options (GUID values)"
```

---

## Task 2: Switch `BodyTypeSelect` to entity-based options

**Files:**
- Modify: `src/components/forms/select/BodyTypeSelect.tsx`

- [ ] **Step 1: Replace the import and data source**

```tsx
import { getAllBodyTypesOptions } from "@/api/enum/getAllBodyTypesOptions";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { cn } from "@/lib/utils";
import { SelectRootProps } from "@/types/ui/selectRootProps";
import { useQuery } from "@tanstack/react-query";

type BodytypeSelectProps = SelectRootProps & {
  className?: string;
};

const BodyTypeSelect = ({ className, ...props }: BodytypeSelectProps) => {
  const { data: bodyTypesQuery } = useQuery(getAllBodyTypesOptions);

  const bodyTypes = bodyTypesQuery?.data || [];

  return (
    <div className={cn(className)}>
      <Select {...props}>
        <SelectTrigger className="w-full">
          <SelectValue placeholder="Sedan" />
        </SelectTrigger>
        <SelectContent>
          <SelectGroup>
            <SelectLabel>Body types</SelectLabel>
            {bodyTypes.map((body) => (
              <SelectItem key={body.id} value={body.id}>
                {body.name}
              </SelectItem>
            ))}
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  );
};

export default BodyTypeSelect;
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
npx tsc --noEmit 2>&1 | head -30
```

Expected: no errors related to BodyTypeSelect.

- [ ] **Step 3: Commit**

```bash
git add src/components/forms/select/BodyTypeSelect.tsx
git commit -m "feat: switch BodyTypeSelect to entity-based options (GUID values)"
```

---

## Task 3: Switch `TransmissionToggleGroup` to entity-based options

**Files:**
- Modify: `src/components/forms/TransmissionToggleGroup.tsx`

- [ ] **Step 1: Replace the import and data source**

```tsx
import { getAllTransmissionsOptions } from "@/api/enum/getAllTransmissionsOptions";
import { toggleVariants } from "@/components/ui/toggle";
import { cn } from "@/lib/utils";
import * as ToggleGroupPrimitive from "@radix-ui/react-toggle-group";
import { useQuery } from "@tanstack/react-query";
import { type VariantProps } from "class-variance-authority";
import { ToggleGroup, ToggleGroupItem } from "../ui/toggle-group";

type TransmissionToggleGroupProps = React.ComponentProps<
  typeof ToggleGroupPrimitive.Root
> &
  VariantProps<typeof toggleVariants>;

const TransmissionToggleGroup = ({
  className,
  ...props
}: TransmissionToggleGroupProps) => {
  const { data: transmissionsQuery } = useQuery(getAllTransmissionsOptions);

  const transmissions = transmissionsQuery?.data || [];

  return (
    <div className={cn(className)}>
      <ToggleGroup {...props}>
        {transmissions.map((transmission) => (
          <ToggleGroupItem
            key={transmission.id}
            value={transmission.id}
            className="flex-none"
          >
            {transmission.name}
          </ToggleGroupItem>
        ))}
      </ToggleGroup>
    </div>
  );
};

export default TransmissionToggleGroup;
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
npx tsc --noEmit 2>&1 | head -30
```

- [ ] **Step 3: Commit**

```bash
git add src/components/forms/TransmissionToggleGroup.tsx
git commit -m "feat: switch TransmissionToggleGroup to entity-based options (GUID values)"
```

---

## Task 4: Switch `DrivetrainToggleGroup` to entity-based options

**Files:**
- Modify: `src/components/forms/DrivetrainToggleGroup.tsx`

- [ ] **Step 1: Replace the import and data source**

```tsx
import { getAllDrivetrainsOptions } from "@/api/enum/getAllDrivetrainsOptions";
import { toggleVariants } from "@/components/ui/toggle";
import { cn } from "@/lib/utils";
import * as ToggleGroupPrimitive from "@radix-ui/react-toggle-group";
import { useQuery } from "@tanstack/react-query";
import { type VariantProps } from "class-variance-authority";
import { ToggleGroup, ToggleGroupItem } from "../ui/toggle-group";

type DrivetrainToggleGroupProps = React.ComponentProps<
  typeof ToggleGroupPrimitive.Root
> &
  VariantProps<typeof toggleVariants>;

const DrivetrainToggleGroup = ({
  className,
  ...props
}: DrivetrainToggleGroupProps) => {
  const { data: drivetrainsQuery } = useQuery(getAllDrivetrainsOptions);

  const drivetrains = drivetrainsQuery?.data || [];

  return (
    <div className={cn(className)}>
      <ToggleGroup {...props}>
        {drivetrains.map((drivetrain) => (
          <ToggleGroupItem
            key={drivetrain.id}
            value={drivetrain.id}
          >
            {drivetrain.name}
          </ToggleGroupItem>
        ))}
      </ToggleGroup>
    </div>
  );
};

export default DrivetrainToggleGroup;
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
npx tsc --noEmit 2>&1 | head -30
```

- [ ] **Step 3: Commit**

```bash
git add src/components/forms/DrivetrainToggleGroup.tsx
git commit -m "feat: switch DrivetrainToggleGroup to entity-based options (GUID values)"
```

---

## Task 5: Create variant form schema and types

**Files:**
- Create: `src/features/variantList/schemas/variantFormSchema.ts`
- Create: `src/features/variantList/types/VariantFormData.ts`
- Create: `src/features/variantList/types/CreateVariantCommand.ts`
- Create: `src/features/variantList/types/UpdateVariantCommand.ts`
- Create: `src/features/variantList/types/DeleteVariantCommand.ts`

- [ ] **Step 1: Create `variantFormSchema.ts`**

Create `src/features/variantList/schemas/variantFormSchema.ts`:

```ts
import { VALIDATION } from "@/constants/validation";
import { validation } from "@/utils/validation";
import z from "zod";

export const variantFormSchema = z.object({
  makeId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a make" }),
  modelId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a model" }),
  year: z.coerce
    .number<number>()
    .min(VALIDATION.YEAR.MIN, {
      error: validation.minSize({ label: "Year", size: VALIDATION.YEAR.MIN }),
    })
    .max(new Date().getFullYear(), {
      error: validation.maxSize({
        label: "Year",
        size: new Date().getFullYear(),
      }),
    }),
  fuelId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a fuel type" }),
  transmissionId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a transmission" }),
  bodyTypeId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a body type" }),
  doorCount: z.coerce
    .number<number>()
    .min(VALIDATION.DOOR_COUNT.MIN, {
      error: validation.minSize({
        label: "Door count",
        size: VALIDATION.DOOR_COUNT.MIN,
      }),
    })
    .max(VALIDATION.DOOR_COUNT.MAX, {
      error: validation.maxSize({
        label: "Door count",
        size: VALIDATION.DOOR_COUNT.MAX,
      }),
    }),
  powerKw: z.coerce
    .number<number>()
    .min(VALIDATION.POWER.MIN, {
      error: validation.minSize({ label: "Power", size: VALIDATION.POWER.MIN, unit: "kW" }),
    })
    .max(VALIDATION.POWER.MAX, {
      error: validation.maxSize({ label: "Power", size: VALIDATION.POWER.MAX, unit: "kW" }),
    }),
  engineSizeMl: z.coerce
    .number<number>()
    .min(VALIDATION.ENGINE_SIZE.MIN, {
      error: validation.minSize({
        label: "Engine size",
        size: VALIDATION.ENGINE_SIZE.MIN,
        unit: "ml",
      }),
    })
    .max(VALIDATION.ENGINE_SIZE.MAX, {
      error: validation.maxSize({
        label: "Engine size",
        size: VALIDATION.ENGINE_SIZE.MAX,
        unit: "ml",
      }),
    }),
  isCustom: z.boolean(),
});
```

- [ ] **Step 2: Create `VariantFormData.ts`**

Create `src/features/variantList/types/VariantFormData.ts`:

```ts
import z from "zod";
import { variantFormSchema } from "../schemas/variantFormSchema";

export type VariantFormData = z.infer<typeof variantFormSchema>;
```

- [ ] **Step 3: Create `CreateVariantCommand.ts`**

Create `src/features/variantList/types/CreateVariantCommand.ts`:

```ts
export type CreateVariantCommand = {
  modelId: string;
  year: number;
  fuelId: string;
  transmissionId: string;
  bodyTypeId: string;
  isCustom: boolean;
  doorCount: number;
  powerKw: number;
  engineSizeMl: number;
};
```

- [ ] **Step 4: Create `UpdateVariantCommand.ts`**

Create `src/features/variantList/types/UpdateVariantCommand.ts`:

```ts
export type UpdateVariantCommand = {
  id: string;
  modelId: string;
  year: number;
  fuelId: string;
  transmissionId: string;
  bodyTypeId: string;
  isCustom: boolean;
  doorCount: number;
  powerKw: number;
  engineSizeMl: number;
};
```

- [ ] **Step 5: Create `DeleteVariantCommand.ts`**

Create `src/features/variantList/types/DeleteVariantCommand.ts`:

```ts
export type DeleteVariantCommand = {
  id: string;
};
```

- [ ] **Step 6: Verify TypeScript compiles**

```bash
npx tsc --noEmit 2>&1 | head -30
```

Expected: no new errors.

- [ ] **Step 7: Commit**

```bash
git add src/features/variantList/schemas/ src/features/variantList/types/
git commit -m "feat: add variant form schema and command types"
```

---

## Task 6: Create variant mutation hooks

**Files:**
- Create: `src/features/variantList/api/useCreateVariant.ts`
- Create: `src/features/variantList/api/useUpdateVariant.ts`
- Create: `src/features/variantList/api/useDeleteVariant.ts`

- [ ] **Step 1: Create `useCreateVariant.ts`**

Create `src/features/variantList/api/useCreateVariant.ts`:

```ts
import { variantKeys } from "@/api/queryKeys/variantKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { CreateVariantCommand } from "../types/CreateVariantCommand";

const createVariant = (body: CreateVariantCommand) =>
  axiosClient.post<void>(ENDPOINTS.VARIANT.CREATE, body);

export const useCreateVariant = () =>
  useMutation({
    mutationFn: createVariant,
    meta: {
      successMessage: "Successfully created variant!",
      errorMessage: "Sorry, we couldn't create your variant",
      invalidatesQuery: variantKeys.all(),
    },
  });
```

- [ ] **Step 2: Create `useUpdateVariant.ts`**

Create `src/features/variantList/api/useUpdateVariant.ts`:

```ts
import { variantKeys } from "@/api/queryKeys/variantKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { UpdateVariantCommand } from "../types/UpdateVariantCommand";

const updateVariant = ({ id, ...body }: UpdateVariantCommand) =>
  axiosClient.put<void>(`${ENDPOINTS.VARIANT.UPDATE}/${id}`, body);

export const useUpdateVariant = () =>
  useMutation({
    mutationFn: updateVariant,
    meta: {
      successMessage: "Successfully updated variant!",
      errorMessage: "Sorry, we couldn't update the variant",
      invalidatesQuery: variantKeys.all(),
    },
  });
```

- [ ] **Step 3: Create `useDeleteVariant.ts`**

Create `src/features/variantList/api/useDeleteVariant.ts`:

```ts
import { variantKeys } from "@/api/queryKeys/variantKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { DeleteVariantCommand } from "../types/DeleteVariantCommand";

const deleteVariant = ({ id }: DeleteVariantCommand) =>
  axiosClient.delete<void>(`${ENDPOINTS.VARIANT.DELETE}/${id}`);

export const useDeleteVariant = () =>
  useMutation({
    mutationFn: deleteVariant,
    meta: {
      successMessage: "Successfully deleted variant!",
      errorMessage: "Sorry, we had trouble deleting the variant",
      invalidatesQuery: variantKeys.all(),
    },
  });
```

- [ ] **Step 4: Verify TypeScript compiles**

```bash
npx tsc --noEmit 2>&1 | head -30
```

- [ ] **Step 5: Commit**

```bash
git add src/features/variantList/api/useCreateVariant.ts \
        src/features/variantList/api/useUpdateVariant.ts \
        src/features/variantList/api/useDeleteVariant.ts
git commit -m "feat: add variant mutation hooks (create, update, delete)"
```

---

## Task 7: Create `VariantForm` component

**Files:**
- Create: `src/features/variantList/components/VariantForm.tsx`

- [ ] **Step 1: Create `VariantForm.tsx`**

Create `src/features/variantList/components/VariantForm.tsx`:

```tsx
import BodyTypeSelect from "@/components/forms/select/BodyTypeSelect";
import FuelSelect from "@/components/forms/select/FuelSelect";
import MakeSelect from "@/components/forms/select/MakeSelect";
import ModelSelect from "@/components/forms/select/ModelSelect";
import TransmissionToggleGroup from "@/components/forms/TransmissionToggleGroup";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { variantFormSchema } from "../schemas/variantFormSchema";
import { VariantFormData } from "../types/VariantFormData";

type VariantFormProps = {
  variant: VariantFormData;
  onSubmit: (formData: VariantFormData) => Promise<void>;
  className?: string;
};

const VariantForm = ({ variant, onSubmit, className }: VariantFormProps) => {
  const form = useForm<VariantFormData>({
    defaultValues: variant,
    resolver: zodResolver(variantFormSchema),
  });

  const handleSubmit = async (formData: VariantFormData) => {
    await onSubmit(formData);
    form.reset();
  };

  const selectedMake = form.watch("makeId");

  return (
    <div className={cn(className)}>
      <Form {...form}>
        <form
          className="grid w-full min-w-3xs grid-cols-4 gap-x-6 gap-y-6 md:gap-x-12 md:gap-y-8"
          onSubmit={form.handleSubmit(handleSubmit)}
        >
          <FormField
            name="makeId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Make*</FormLabel>
                <FormControl>
                  <MakeSelect
                    isAllMakesEnabled={false}
                    onValueChange={field.onChange}
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="modelId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Model*</FormLabel>
                <FormControl>
                  <ModelSelect
                    isAllModelsEnabled={false}
                    disabled={!selectedMake}
                    onValueChange={field.onChange}
                    selectedMake={selectedMake}
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="year"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Year*</FormLabel>
                <FormControl>
                  <Input type="number" min={1900} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="fuelId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Fuel type*</FormLabel>
                <FormControl>
                  <FuelSelect onValueChange={field.onChange} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="transmissionId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Transmission*</FormLabel>
                <FormControl>
                  <TransmissionToggleGroup
                    type="single"
                    onValueChange={field.onChange}
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="bodyTypeId"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Body type*</FormLabel>
                <FormControl>
                  <BodyTypeSelect onValueChange={field.onChange} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="doorCount"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-1 flex flex-col justify-start">
                <FormLabel>Doors*</FormLabel>
                <FormControl>
                  <Input type="number" min={1} max={9} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="powerKw"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-1 flex flex-col justify-start">
                <FormLabel>Power (kW)*</FormLabel>
                <FormControl>
                  <Input type="number" min={5} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="engineSizeMl"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-col justify-start">
                <FormLabel>Engine size (ml)*</FormLabel>
                <FormControl>
                  <Input type="number" min={300} {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            name="isCustom"
            control={form.control}
            render={({ field }) => (
              <FormItem className="col-span-2 flex flex-row items-center gap-2">
                <FormControl>
                  <Checkbox
                    checked={field.value}
                    onCheckedChange={field.onChange}
                  />
                </FormControl>
                <FormLabel className="mt-0">Custom variant</FormLabel>
                <FormMessage />
              </FormItem>
            )}
          />
          <Button type="submit" className="col-span-4">
            Confirm
          </Button>
        </form>
      </Form>
    </div>
  );
};

export default VariantForm;
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
npx tsc --noEmit 2>&1 | head -30
```

- [ ] **Step 3: Commit**

```bash
git add src/features/variantList/components/VariantForm.tsx
git commit -m "feat: add VariantForm component"
```

---

## Task 8: Create `CreateVariantDialog`

**Files:**
- Create: `src/features/variantList/components/CreateVariantDialog.tsx`

- [ ] **Step 1: Create `CreateVariantDialog.tsx`**

Create `src/features/variantList/components/CreateVariantDialog.tsx`:

```tsx
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useState } from "react";
import { useCreateVariant } from "../api/useCreateVariant";
import { VariantFormData } from "../types/VariantFormData";
import VariantForm from "./VariantForm";

type CreateVariantDialogProps = {
  modelId: string;
  makeId: string;
};

const CreateVariantDialog = ({ modelId, makeId }: CreateVariantDialogProps) => {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  const { mutateAsync: createVariantAsync } = useCreateVariant();

  const handleSubmit = async (formData: VariantFormData) => {
    const { makeId: _, ...command } = formData;
    await createVariantAsync(command);
    setIsOpen(false);
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <Button>Add variant</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create new variant</DialogTitle>
        </DialogHeader>
        <VariantForm
          variant={{
            makeId,
            modelId,
            year: new Date().getFullYear(),
            fuelId: "",
            transmissionId: "",
            bodyTypeId: "",
            doorCount: 4,
            powerKw: 100,
            engineSizeMl: 1600,
            isCustom: false,
          }}
          onSubmit={handleSubmit}
        />
      </DialogContent>
    </Dialog>
  );
};

export default CreateVariantDialog;
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
npx tsc --noEmit 2>&1 | head -30
```

- [ ] **Step 3: Commit**

```bash
git add src/features/variantList/components/CreateVariantDialog.tsx
git commit -m "feat: add CreateVariantDialog"
```

---

## Task 9: Create `EditVariantDialog` + `EditVariantDialogContent`

**Files:**
- Create: `src/features/variantList/components/EditVariantDialogContent.tsx`
- Create: `src/features/variantList/components/EditVariantDialog.tsx`

- [ ] **Step 1: Create `EditVariantDialogContent.tsx`**

Create `src/features/variantList/components/EditVariantDialogContent.tsx`:

```tsx
import { DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { VariantFormData } from "../types/VariantFormData";
import { Variant } from "../types/Variant";
import VariantForm from "./VariantForm";

type EditVariantDialogContentProps = {
  variant: Variant;
  makeId: string;
  onSubmit: (formData: VariantFormData) => Promise<void>;
};

const EditVariantDialogContent = ({
  variant,
  makeId,
  onSubmit,
}: EditVariantDialogContentProps) => {
  return (
    <div>
      <DialogHeader>
        <DialogTitle>Edit variant ({variant.year})</DialogTitle>
      </DialogHeader>
      <VariantForm
        variant={{
          makeId,
          modelId: variant.modelId,
          year: variant.year,
          fuelId: variant.fuelId,
          transmissionId: variant.transmissionId,
          bodyTypeId: variant.bodyTypeId,
          doorCount: variant.doorCount,
          powerKw: variant.powerKw,
          engineSizeMl: variant.engineSizeMl,
          isCustom: variant.isCustom,
        }}
        onSubmit={onSubmit}
      />
    </div>
  );
};

export default EditVariantDialogContent;
```

- [ ] **Step 2: Create `EditVariantDialog.tsx`**

Create `src/features/variantList/components/EditVariantDialog.tsx`:

```tsx
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTrigger } from "@/components/ui/dialog";
import { Pencil } from "lucide-react";
import { useState } from "react";
import { useUpdateVariant } from "../api/useUpdateVariant";
import { Variant } from "../types/Variant";
import { VariantFormData } from "../types/VariantFormData";
import EditVariantDialogContent from "./EditVariantDialogContent";

type EditVariantDialogProps = {
  variant: Variant;
  makeId: string;
};

const EditVariantDialog = ({ variant, makeId }: EditVariantDialogProps) => {
  const [isOpen, setIsOpen] = useState<boolean>(false);

  const { mutateAsync: updateVariantAsync } = useUpdateVariant();

  const handleSubmit = async (formData: VariantFormData) => {
    const { makeId: _, ...command } = formData;
    await updateVariantAsync({ id: variant.id, ...command });
    setIsOpen(false);
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Pencil />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <EditVariantDialogContent
          variant={variant}
          makeId={makeId}
          onSubmit={handleSubmit}
        />
      </DialogContent>
    </Dialog>
  );
};

export default EditVariantDialog;
```

- [ ] **Step 3: Verify TypeScript compiles**

```bash
npx tsc --noEmit 2>&1 | head -30
```

- [ ] **Step 4: Commit**

```bash
git add src/features/variantList/components/EditVariantDialogContent.tsx \
        src/features/variantList/components/EditVariantDialog.tsx
git commit -m "feat: add EditVariantDialog and EditVariantDialogContent"
```

---

## Task 10: Create `ViewVariantDialog` + `ViewVariantDialogContent`

**Files:**
- Create: `src/features/variantList/components/ViewVariantDialogContent.tsx`
- Create: `src/features/variantList/components/ViewVariantDialog.tsx`

- [ ] **Step 1: Create `ViewVariantDialogContent.tsx`**

Create `src/features/variantList/components/ViewVariantDialogContent.tsx`:

```tsx
import {
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Variant } from "../types/Variant";

type ViewVariantDialogContentProps = {
  variant: Variant;
};

const ViewVariantDialogContent = ({
  variant,
}: ViewVariantDialogContentProps) => {
  return (
    <>
      <DialogHeader>
        <DialogTitle>Variant details</DialogTitle>
        <DialogDescription>Read-only view</DialogDescription>
      </DialogHeader>
      <div className="grid gap-4">
        <p>Year: {variant.year}</p>
        <p>Fuel: {variant.fuelName}</p>
        <p>Transmission: {variant.transmissionName}</p>
        <p>Body type: {variant.bodyTypeName}</p>
        <p>Doors: {variant.doorCount}</p>
        <p>Power: {variant.powerKw} kW</p>
        <p>Engine size: {variant.engineSizeMl} ml</p>
        <p>Custom: {variant.isCustom ? "Yes" : "No"}</p>
      </div>
    </>
  );
};

export default ViewVariantDialogContent;
```

- [ ] **Step 2: Create `ViewVariantDialog.tsx`**

Create `src/features/variantList/components/ViewVariantDialog.tsx`:

```tsx
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTrigger } from "@/components/ui/dialog";
import { Eye } from "lucide-react";
import { Variant } from "../types/Variant";
import ViewVariantDialogContent from "./ViewVariantDialogContent";

type ViewVariantDialogProps = {
  variant: Variant;
};

const ViewVariantDialog = ({ variant }: ViewVariantDialogProps) => {
  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button variant="secondary">
          <Eye />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <ViewVariantDialogContent variant={variant} />
      </DialogContent>
    </Dialog>
  );
};

export default ViewVariantDialog;
```

- [ ] **Step 3: Verify TypeScript compiles**

```bash
npx tsc --noEmit 2>&1 | head -30
```

- [ ] **Step 4: Commit**

```bash
git add src/features/variantList/components/ViewVariantDialogContent.tsx \
        src/features/variantList/components/ViewVariantDialog.tsx
git commit -m "feat: add ViewVariantDialog and ViewVariantDialogContent"
```

---

## Task 11: Update `VariantListTable` with action buttons

**Files:**
- Modify: `src/features/variantList/components/VariantListTable.tsx`

- [ ] **Step 1: Replace `VariantListTable.tsx`**

Replace the full file content:

```tsx
import { getVariantsByModelIdOptions } from "@/features/variantList/api/getVariantsByModelIdOptions";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { cn } from "@/lib/utils";
import { useQuery } from "@tanstack/react-query";
import { Trash } from "lucide-react";
import { useDeleteVariant } from "../api/useDeleteVariant";
import EditVariantDialog from "./EditVariantDialog";
import ViewVariantDialog from "./ViewVariantDialog";

type VariantListTableProps = {
  modelId: string;
  makeId: string;
  className?: string;
};

const VariantListTable = ({
  modelId,
  makeId,
  className,
}: VariantListTableProps) => {
  const { data: variantsQuery, isLoading, isError } = useQuery(
    getVariantsByModelIdOptions(modelId),
  );

  const { mutateAsync: deleteVariantAsync } = useDeleteVariant();

  const handleDelete = async (id: string) => {
    await deleteVariantAsync({ id });
  };

  if (isLoading) return <p>Loading variants…</p>;
  if (isError) return <p>Failed to load variants.</p>;

  const variants = variantsQuery?.data || [];

  return (
    <div className={cn(className)}>
      <Table>
        <TableCaption>A list of variants</TableCaption>
        <TableHeader>
          <TableRow>
            <TableHead>Year</TableHead>
            <TableHead>Fuel</TableHead>
            <TableHead>Transmission</TableHead>
            <TableHead>Body type</TableHead>
            <TableHead>Doors</TableHead>
            <TableHead>Power (kW)</TableHead>
            <TableHead>Engine (ml)</TableHead>
            <TableHead>Custom</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {variants.map((v) => (
            <TableRow key={v.id}>
              <TableCell>{v.year}</TableCell>
              <TableCell>{v.fuelName}</TableCell>
              <TableCell>{v.transmissionName}</TableCell>
              <TableCell>{v.bodyTypeName}</TableCell>
              <TableCell>{v.doorCount}</TableCell>
              <TableCell>{v.powerKw}</TableCell>
              <TableCell>{v.engineSizeMl}</TableCell>
              <TableCell>{v.isCustom ? "Yes" : "No"}</TableCell>
              <TableCell>
                <ViewVariantDialog variant={v} />
                <EditVariantDialog variant={v} makeId={makeId} />
                <Button
                  variant="secondary"
                  onClick={() => handleDelete(v.id)}
                >
                  <Trash />
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
};

export default VariantListTable;
```

- [ ] **Step 2: Verify TypeScript compiles**

```bash
npx tsc --noEmit 2>&1 | head -30
```

- [ ] **Step 3: Commit**

```bash
git add src/features/variantList/components/VariantListTable.tsx
git commit -m "feat: add View/Edit/Delete action buttons to VariantListTable"
```

---

## Task 12: Update `variantList/index.ts` and `Variants.tsx`

**Files:**
- Modify: `src/features/variantList/index.ts`
- Modify: `src/app/pages/Variants.tsx`

- [ ] **Step 1: Update `index.ts`**

Replace the full file content of `src/features/variantList/index.ts`:

```ts
export { default as CreateVariantDialog } from "./components/CreateVariantDialog";
export { default as VariantListTable } from "./components/VariantListTable";
```

- [ ] **Step 2: Update `Variants.tsx`**

Replace the full file content of `src/app/pages/Variants.tsx`:

```tsx
import MakeSelect from "@/components/forms/select/MakeSelect";
import ModelSelect from "@/components/forms/select/ModelSelect";
import { CreateVariantDialog, VariantListTable } from "@/features/variantList";
import { useState } from "react";

const Variants = () => {
  const [selectedMake, setSelectedMake] = useState<string | undefined>();
  const [selectedModel, setSelectedModel] = useState<string | undefined>();

  const handleMakeChange = (value: string) => {
    setSelectedMake(value);
    setSelectedModel(undefined);
  };

  return (
    <div className="grid items-center justify-center space-y-5 pt-10">
      <div className="flex w-full items-center gap-4">
        <MakeSelect
          isAllMakesEnabled={false}
          label="Make"
          value={selectedMake ?? ""}
          onValueChange={handleMakeChange}
          className="w-48"
        />
        <ModelSelect
          isAllModelsEnabled={false}
          label="Model"
          selectedMake={selectedMake}
          value={selectedModel ?? ""}
          onValueChange={setSelectedModel}
          className="w-48"
        />
        {selectedModel && selectedMake && (
          <CreateVariantDialog modelId={selectedModel} makeId={selectedMake} />
        )}
      </div>
      {selectedModel && selectedMake && (
        <VariantListTable
          modelId={selectedModel}
          makeId={selectedMake}
          className="max-w-5xl"
        />
      )}
    </div>
  );
};

export default Variants;
```

- [ ] **Step 3: Verify TypeScript compiles**

```bash
npx tsc --noEmit 2>&1 | head -30
```

Expected: no errors.

- [ ] **Step 4: Commit**

```bash
git add src/features/variantList/index.ts src/app/pages/Variants.tsx
git commit -m "feat: wire CreateVariantDialog and makeId into Variants page"
```

---

## Task 13: Delete `carList` feature

**Files:**
- Delete: `src/features/carList/` (entire folder)

- [ ] **Step 1: Confirm no external imports**

```bash
grep -r "from.*features/carList" src/ --include="*.tsx" --include="*.ts"
```

Expected: no output (empty — confirmed no external consumers).

- [ ] **Step 2: Delete the folder**

```bash
rm -rf src/features/carList
```

- [ ] **Step 3: Verify TypeScript compiles**

```bash
npx tsc --noEmit 2>&1 | head -30
```

Expected: no errors.

- [ ] **Step 4: Commit**

```bash
git add -A src/features/carList
git commit -m "chore: delete dead carList feature (replaced by variantList)"
```

---

## Task 14: Final build verification

- [ ] **Step 1: Full TypeScript check**

```bash
cd automotive.marketplace.client && npx tsc --noEmit
```

Expected: exits with code 0, no output.

- [ ] **Step 2: Lint check**

```bash
npm run lint
```

Expected: no errors.

- [ ] **Step 3: Production build**

```bash
npm run build
```

Expected: build succeeds with no errors.

- [ ] **Step 4: Final commit if any lint auto-fixes were needed**

```bash
git add -A && git commit -m "chore: post-implementation lint fixes" || echo "nothing to commit"
```
