import { z } from "zod";

export const ListingSearchSchema = z.object({
  makeId: z.string().optional().catch(undefined),
  modelId: z.string().optional().catch(undefined),
  city: z.string().optional().catch(undefined),
  isUsed: z.boolean().optional().catch(undefined),
  yearFrom: z.coerce.number().optional().catch(undefined),
  yearTo: z.coerce.number().optional().catch(undefined),
  priceFrom: z.coerce.number().optional().catch(undefined),
  priceTo: z.coerce.number().optional().catch(undefined),
});
