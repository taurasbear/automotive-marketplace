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
        unit: "kW",
      }),
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
