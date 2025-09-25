import { GetAllListingsQuery } from "@/features/listing";

export const listingKeys = {
  all: () => ["listing"],
  bySearchParams: (query: GetAllListingsQuery) => [...listingKeys.all(), query],
  byId: (id: string) => [...listingKeys.all(), id],
};
