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
  minYear: z.coerce.number().positive().optional().catch(undefined),
  maxYear: z.coerce.number().positive().optional().catch(undefined),
  minPrice: z.coerce.number().positive().optional().catch(undefined),
  maxPrice: z.coerce.number().positive().optional().catch(undefined),
  minMileage: z.coerce.number().positive().optional().catch(undefined),
  maxMileage: z.coerce.number().positive().optional().catch(undefined),
  minPower: z.coerce.number().positive().optional().catch(undefined),
  maxPower: z.coerce.number().positive().optional().catch(undefined),
});
