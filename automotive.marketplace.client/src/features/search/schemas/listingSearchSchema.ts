import { VALIDATION } from "@/constants/validation";
import { z } from "zod";

export const ListingSearchSchema = z.object({
  makeId: z.string().regex(VALIDATION.GUID.REGEX).optional().catch(undefined),
  models: z
    .array(z.string().regex(VALIDATION.GUID.REGEX).catch(""))
    .optional()
    .catch(undefined),
  city: z.string().optional().catch(undefined),
  isUsed: z.boolean().optional().catch(undefined),
  yearFrom: z.coerce.number().positive().optional().catch(undefined),
  yearTo: z.coerce.number().positive().optional().catch(undefined),
  priceFrom: z.coerce.number().positive().optional().catch(undefined),
  priceTo: z.coerce.number().positive().optional().catch(undefined),
});
