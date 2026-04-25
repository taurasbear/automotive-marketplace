export const myListingKeys = {
  all: () => ["myListings"] as const,
  engagements: (listingId: string) =>
    ["myListings", "engagements", listingId] as const,
};
