import { VALIDATION } from "@/constants/validation";
import i18n from "@/lib/i18n/i18n";
import { validation } from "@/utils/validation";
import z from "zod";

export const variantFormSchema = z.object({
  makeId: z.string().regex(VALIDATION.GUID.REGEX, {
    error: () => i18n.t("pleaseSelect", { field: "make", ns: "validation" }),
  }),
  modelId: z.string().regex(VALIDATION.GUID.REGEX, {
    error: () => i18n.t("pleaseSelect", { field: "model", ns: "validation" }),
  }),
  fuelId: z.string().regex(VALIDATION.GUID.REGEX, {
    error: () =>
      i18n.t("pleaseSelect", { field: "fuel type", ns: "validation" }),
  }),
  transmissionId: z.string().regex(VALIDATION.GUID.REGEX, {
    error: () =>
      i18n.t("pleaseSelect", {
        field: "transmission",
        ns: "validation",
      }),
  }),
  bodyTypeId: z.string().regex(VALIDATION.GUID.REGEX, {
    error: () =>
      i18n.t("pleaseSelect", { field: "body type", ns: "validation" }),
  }),
  doorCount: z.coerce
    .number<number>()
    .min(VALIDATION.DOOR_COUNT.MIN, {
      error: () =>
        validation.minSize({
          label: "Door count",
          size: VALIDATION.DOOR_COUNT.MIN,
        }),
    })
    .max(VALIDATION.DOOR_COUNT.MAX, {
      error: () =>
        validation.maxSize({
          label: "Door count",
          size: VALIDATION.DOOR_COUNT.MAX,
        }),
    }),
  powerKw: z.coerce
    .number<number>()
    .min(VALIDATION.POWER.MIN, {
      error: () =>
        validation.minSize({
          label: "Power",
          size: VALIDATION.POWER.MIN,
          unit: "kW",
        }),
    })
    .max(VALIDATION.POWER.MAX, {
      error: () =>
        validation.maxSize({
          label: "Power",
          size: VALIDATION.POWER.MAX,
          unit: "kW",
        }),
    }),
  engineSizeMl: z.coerce
    .number<number>()
    .min(VALIDATION.ENGINE_SIZE.MIN, {
      error: () =>
        validation.minSize({
          label: "Engine size",
          size: VALIDATION.ENGINE_SIZE.MIN,
          unit: "ml",
        }),
    })
    .max(VALIDATION.ENGINE_SIZE.MAX, {
      error: () =>
        validation.maxSize({
          label: "Engine size",
          size: VALIDATION.ENGINE_SIZE.MAX,
          unit: "ml",
        }),
    }),
  isCustom: z.boolean(),
});
