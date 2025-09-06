import Listings from "@/app/pages/Listings";
import { getAllListingsOptions } from "@/features/listing";
import { ListingSearchSchema } from "@/features/search";
import { createFileRoute } from "@tanstack/react-router";
import { zodValidator } from "@tanstack/zod-adapter";

export const Route = createFileRoute("/listings")({
  component: Listings,
  validateSearch: zodValidator(ListingSearchSchema),
  loaderDeps: ({ search }) => ({ search }),
  loader: async ({ context: { queryClient }, deps: { search } }) => {
    await queryClient.prefetchQuery({
      ...getAllListingsOptions(search),
      retry: false,
    });
  },
  notFoundComponent: () => <div>404 Not Found</div>,
});
