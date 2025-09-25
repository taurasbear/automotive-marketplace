import { VALIDATION } from "@/constants/validation";
import { validation } from "@/utils/validation";
import z from "zod";

export const CarFormSchema = z.object({
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
  makeId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a make" }),
  modelId: z
    .string()
    .regex(VALIDATION.GUID.REGEX, { error: "Please select a model" }),
});
