import { z } from "zod";

export const ListingSearchSchema = z.object({
  makeId: z.string().optional(),
  modelId: z.string().optional(),
  city: z.string().optional(),
  isUsed: z.boolean().optional(),
  yearFrom: z.coerce.number().optional(),
  yearTo: z.coerce.number().optional(),
  priceFrom: z.coerce.number().optional(),
  priceTo: z.coerce.number().optional(),
});
