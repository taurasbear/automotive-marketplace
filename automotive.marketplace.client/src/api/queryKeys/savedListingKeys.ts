export const savedListingKeys = {
  all: () => ["savedListing"] as const,
  list: () => [...savedListingKeys.all(), "list"] as const,
};
