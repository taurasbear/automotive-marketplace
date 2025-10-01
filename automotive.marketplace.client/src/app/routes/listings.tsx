import Listings from "@/app/pages/Listings";
import { getAllListingsOptions } from "@/features/listingList";
import { ListingSearchSchema } from "@/features/search";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/listings")({
  component: Listings,
  validateSearch: ListingSearchSchema,
  loaderDeps: ({ search }) => ({ search }),
  loader: async ({ context: { queryClient }, deps: { search } }) => {
    await queryClient.prefetchQuery({
      ...getAllListingsOptions(search),
      retry: false,
    });
  },
  notFoundComponent: () => <div>404 Not Found</div>,
});
