import { VALIDATION } from "@/constants/validation";
import i18n from "@/lib/i18n/i18n";
import { validation } from "@/utils/validation";
import { z } from "zod";

export const CreateListingSchema = z
  .object({
    price: z.coerce
      .number<number>()
      .min(VALIDATION.PRICE.MIN, {
        error: () =>
          validation.minSize({
            label: "Price",
            size: VALIDATION.PRICE.MIN,
            unit: "€",
          }),
      })
      .max(VALIDATION.PRICE.MAX, {
        error: () =>
          validation.maxSize({
            label: "Price",
            size: VALIDATION.PRICE.MAX,
            unit: "€",
          }),
      }),
    description: z
      .string()
      .max(VALIDATION.TEXT.LONG, {
        error: () =>
          validation.maxLength({
            label: "Description",
            length: VALIDATION.TEXT.LONG,
          }),
      })
      .optional(),
    colour: z.string().optional(),
    vin: z
      .string()
      .optional()
      .refine((val) => !val || VALIDATION.VIN.REGEX.test(val), {
        error: () => i18n.t("vinInvalid", { ns: "validation" }),
      }),
    powerKw: z.coerce
      .number<number>()
      .int()
      .max(VALIDATION.POWER.MAX, {
        error: () =>
          validation.maxSize({
            label: "Engine power",
            size: VALIDATION.POWER.MAX,
            unit: "kW",
          }),
      })
      .optional(),
    engineSizeMl: z.coerce
      .number<number>()
      .int()
      .max(VALIDATION.ENGINE_SIZE.MAX, {
        error: () =>
          validation.maxSize({
            label: "Engine size",
            size: VALIDATION.ENGINE_SIZE.MAX,
            unit: "ml",
          }),
      })
      .optional(),
    mileage: z.coerce
      .number<number>()
      .int()
      .min(VALIDATION.MILEAGE.MIN, {
        error: () =>
          validation.minSize({
            label: "Mileage",
            size: VALIDATION.MILEAGE.MIN,
            unit: "km",
          }),
      })
      .max(VALIDATION.MILEAGE.MAX, {
        error: () =>
          validation.maxSize({
            label: "Mileage",
            size: VALIDATION.MILEAGE.MAX,
            unit: "km",
          }),
      }),
    isSteeringWheelRight: z.boolean(),
    makeId: z.string().regex(VALIDATION.GUID.REGEX, {
      error: () => i18n.t("pleaseSelect", { field: "make", ns: "validation" }),
    }),
    modelId: z.string().optional(),
    variantId: z
      .string()
      .regex(VALIDATION.GUID.REGEX)
      .optional()
      .or(z.literal("")),
    city: z
      .string()
      .nonempty({
        error: () => i18n.t("cityCannotBeEmpty", { ns: "validation" }),
      })
      .max(VALIDATION.NAME.LONG, {
        error: () =>
          validation.maxLength({
            label: "City",
            length: VALIDATION.NAME.LONG,
          }),
      }),
    isUsed: z.boolean(),
    isCustom: z.boolean().optional(),
    year: z.coerce.number<number>().int().optional(),
    transmissionId: z.string().optional(),
    fuelId: z.string().optional(),
    bodyTypeId: z.string().optional(),
    drivetrainId: z.string().regex(VALIDATION.GUID.REGEX, {
      error: () =>
        i18n.t("pleaseSelect", {
          field: "drivetrain type",
          ns: "validation",
        }),
    }),
    doorCount: z.coerce
      .number<number>()
      .int()
      .max(VALIDATION.DOOR_COUNT.MAX, {
        error: () =>
          validation.maxSize({
            label: "Door count",
            size: VALIDATION.DOOR_COUNT.MAX,
          }),
      })
      .optional(),
    images: z
      .array(
        z.instanceof(Blob, {
          error: () => i18n.t("invalidImage", { ns: "validation" }),
        }),
      )
      .min(1, {
        error: () => i18n.t("mustUploadImage", { ns: "validation" }),
      }),
    defects: z.array(z.object({
      defectCategoryId: z.string().optional(),
      customName: z.string().optional(),
      images: z.array(z.instanceof(Blob)).max(3),
    })).default([]),
  })
  .superRefine((data, ctx) => {
    if (!data.variantId) {
      if (!data.modelId || !VALIDATION.GUID.REGEX.test(data.modelId))
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["modelId"],
          message: i18n.t("pleaseSelect", { field: "model", ns: "validation" }),
        });
      if (!data.fuelId || !VALIDATION.GUID.REGEX.test(data.fuelId))
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["fuelId"],
          message: i18n.t("pleaseSelect", {
            field: "fuel type",
            ns: "validation",
          }),
        });
      if (
        !data.transmissionId ||
        !VALIDATION.GUID.REGEX.test(data.transmissionId)
      )
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["transmissionId"],
          message: i18n.t("pleaseSelect", {
            field: "transmission",
            ns: "validation",
          }),
        });
      if (!data.bodyTypeId || !VALIDATION.GUID.REGEX.test(data.bodyTypeId))
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["bodyTypeId"],
          message: i18n.t("pleaseSelect", {
            field: "body type",
            ns: "validation",
          }),
        });
      if (!data.doorCount || data.doorCount < VALIDATION.DOOR_COUNT.MIN)
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["doorCount"],
          message: i18n.t("enter", { field: "door count", ns: "validation" }),
        });
      if (!data.powerKw || data.powerKw < VALIDATION.POWER.MIN)
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["powerKw"],
          message: i18n.t("enter", { field: "engine power", ns: "validation" }),
        });
      if (!data.engineSizeMl || data.engineSizeMl < VALIDATION.ENGINE_SIZE.MIN)
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["engineSizeMl"],
          message: i18n.t("enter", { field: "engine size", ns: "validation" }),
        });
      if (!data.year || data.year < VALIDATION.YEAR.MIN)
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          path: ["year"],
          message: i18n.t("enter", { field: "year", ns: "validation" }),
        });
    }
  });
