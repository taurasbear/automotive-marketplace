import Listings from "@/pages/Listings";
import { createFileRoute } from "@tanstack/react-router";
import { z } from "zod";
import { zodValidator } from "@tanstack/zod-adapter";

const listingSearchSchema = z.object({
  makeId: z.string().optional(),
  modelId: z.string().optional(),
  city: z.string().optional(),
  isUsed: z.boolean().optional(),
  yearFrom: z.coerce.number().optional(),
  yearTo: z.coerce.number().optional(),
  priceFrom: z.coerce.number().optional(),
  priceTo: z.coerce.number().optional(),
});

export const Route = createFileRoute("/listings")({
  component: Listings,
  validateSearch: zodValidator(listingSearchSchema),
});
