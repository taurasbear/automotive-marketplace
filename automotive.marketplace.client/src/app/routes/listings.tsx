import Listings from "@/app/pages/Listings";
import { getAllListingsOptions } from "@/features/listing";
import { createFileRoute } from "@tanstack/react-router";
import { zodValidator } from "@tanstack/zod-adapter";
import { z } from "zod";

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
  loaderDeps: ({ search }) => ({ search }),
  loader: async ({ context: { queryClient }, deps: { search } }) => {
    await queryClient.prefetchQuery({
      ...getAllListingsOptions(search),
      retry: false,
    });
  },
  notFoundComponent: () => <div>404 Not Found</div>,
});
