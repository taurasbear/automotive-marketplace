import Listings from "@/app/pages/Listings";
import { authReady } from "@/app/routes/__root";
import { getAllListingsOptions } from "@/features/listingList";
import { ListingSearchSchema } from "@/features/search";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/listings")({
  component: Listings,
  validateSearch: ListingSearchSchema,
  loaderDeps: ({ search }) => ({ search }),
  loader: async ({ context: { queryClient }, deps: { search } }) => {
    await authReady;
    await queryClient.prefetchQuery({
      ...getAllListingsOptions({ ...search, page: search.page ?? 1, pageSize: 20 }),
      retry: false,
    });
  },
  notFoundComponent: () => <div>404 Not Found</div>,
});
