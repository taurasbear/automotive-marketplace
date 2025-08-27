import { GetAllListingsQuery } from "../types/GetAllListingsQuery";

export const listingKeys = {
  all: () => ["listing"],
  bySearchParams: (query: GetAllListingsQuery) => [...listingKeys.all(), query],
};
