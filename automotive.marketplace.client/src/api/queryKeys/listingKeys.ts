import { GetAllListingsQuery } from "@/features/listingList";

export const listingKeys = {
  all: () => ["listing"],
  bySearchParams: (query: GetAllListingsQuery) => [...listingKeys.all(), query],
  byId: (id: string) => [...listingKeys.all(), id],
};
