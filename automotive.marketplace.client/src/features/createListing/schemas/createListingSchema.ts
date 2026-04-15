import { VALIDATION } from "@/constants/validation";
import { validation } from "@/utils/validation";
import { z } from "zod";

export const CreateListingSchema = z.object({
  price: z.coerce
    .number<number>()
    .min(VALIDATION.PRICE.MIN, {
      error: validation.minSize({
        label: "Price",
        size: VALIDATION.PRICE.MIN,
        unit: "€",
      }),
    })
    .max(VALIDATION.PRICE.MAX, {
      error: validation.maxSize({
        label: "Price",
        size: VALIDATION.PRICE.MAX,
        unit: "€",
      }),
    }),
  description: z
    .string()
    .max(VALIDATION.TEXT.LONG, {
      error: validation.maxLength({
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
      message: "VIN code must have 17 characters and not include I, O, or Q",
    }),
  powerKw: z.coerce.number<number>().int().max(VALIDATION.POWER.MAX, {
    error: validation.maxSize({ label: "Engine power", size: VALIDATION.POWER.MAX, unit: "kW" }),
  }).optional(),
  engineSizeMl: z.coerce.number<number>().int().max(VALIDATION.ENGINE_SIZE.MAX, {
    error: validation.maxSize({ label: "Engine size", size: VALIDATION.ENGINE_SIZE.MAX, unit: "ml" }),
  }).optional(),
  mileage: z.coerce
    .number<number>()
    .int()
    .min(VALIDATION.MILEAGE.MIN, {
      error: validation.minSize({
        label: "Mileage",
        size: VALIDATION.MILEAGE.MIN,
        unit: "km",
      }),
    })
    .max(VALIDATION.MILEAGE.MAX, {
      error: validation.maxSize({
        label: "Mileage",
        size: VALIDATION.MILEAGE.MAX,
        unit: "km",
      }),
    }),
  isSteeringWheelRight: z.boolean(),
  makeId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a make" }),
  modelId: z.string().optional(),
  variantId: z
    .string()
    .regex(VALIDATION.GUID.REGEX)
    .optional()
    .or(z.literal("")),
  city: z
    .string()
    .nonempty({ error: "City cannot be empty" })
    .max(VALIDATION.NAME.LONG, {
      error: validation.maxLength({
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
    error: "Please select a drivetrain type",
  }),
  doorCount: z.coerce.number<number>().int().max(VALIDATION.DOOR_COUNT.MAX, {
    error: validation.maxSize({ label: "Door count", size: VALIDATION.DOOR_COUNT.MAX }),
  }).optional(),
  images: z
    .array(
      z.instanceof(Blob, { error: "You did not upload a valid image file" }),
    )
    .min(1, { error: "You must upload at least one image" }),
}).superRefine((data, ctx) => {
  if (!data.variantId) {
    if (!data.modelId || !VALIDATION.GUID.REGEX.test(data.modelId))
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["modelId"], message: "Please select a model" });
    if (!data.fuelId || !VALIDATION.GUID.REGEX.test(data.fuelId))
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["fuelId"], message: "Please select a fuel type" });
    if (!data.transmissionId || !VALIDATION.GUID.REGEX.test(data.transmissionId))
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["transmissionId"], message: "Please select a transmission" });
    if (!data.bodyTypeId || !VALIDATION.GUID.REGEX.test(data.bodyTypeId))
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["bodyTypeId"], message: "Please select a body type" });
    if (!data.doorCount || data.doorCount < VALIDATION.DOOR_COUNT.MIN)
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["doorCount"], message: "Enter door count" });
    if (!data.powerKw || data.powerKw < VALIDATION.POWER.MIN)
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["powerKw"], message: "Enter engine power" });
    if (!data.engineSizeMl || data.engineSizeMl < VALIDATION.ENGINE_SIZE.MIN)
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["engineSizeMl"], message: "Enter engine size" });
    if (!data.year || data.year < VALIDATION.YEAR.MIN)
      ctx.addIssue({ code: z.ZodIssueCode.custom, path: ["year"], message: "Enter year" });
  }
});
