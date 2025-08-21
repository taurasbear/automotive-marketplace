import { GetAllListingsQuery } from "@/shared/types/dto/listing/GetAllListingsQuery";

export const listingKeys = {
  all: () => ["listing"],
  bySearchParams: (query: GetAllListingsQuery) => [...listingKeys.all(), query],
};
