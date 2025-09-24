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
  power: z.coerce
    .number<number>()
    .int()
    .min(VALIDATION.POWER.MIN, {
      error: validation.minSize({
        label: "Power",
        size: VALIDATION.POWER.MIN,
        unit: "kW",
      }),
    })
    .max(VALIDATION.POWER.MAX, {
      error: validation.maxSize({
        label: "Power",
        size: VALIDATION.POWER.MAX,
        unit: "kw",
      }),
    }),
  engineSize: z.coerce
    .number<number>()
    .int()
    .min(
      VALIDATION.ENGINE_SIZE.MIN,
      validation.minSize({
        label: "Engine size",
        size: VALIDATION.ENGINE_SIZE.MIN,
        unit: "ml",
      }),
    )
    .max(VALIDATION.ENGINE_SIZE.MAX, {
      error: validation.maxSize({
        label: "Engine size",
        size: VALIDATION.ENGINE_SIZE.MAX,
        unit: "ml",
      }),
    }),
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
  modelId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a model" }),
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
  transmission: z.string().nonempty({
    error: "Please select a transmission type",
  }),
  fuel: z.string().nonempty({
    error: "Please select a fuel type",
  }),
  bodyType: z.string().nonempty({
    error: "Please select a body type",
  }),
  drivetrain: z.string().nonempty({
    error: "Please select a drivetrain type",
  }),
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
  images: z
    .array(
      z.instanceof(Blob, { error: "You did not upload a valid image file" }),
    )
    .min(1, { error: "You must upload at least one image" }),
});
