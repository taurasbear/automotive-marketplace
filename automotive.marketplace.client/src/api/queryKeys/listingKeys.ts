import { GetAllListingsQuery } from "@/features/listingList";

export const listingKeys = {
  all: () => ["listing"],
  bySearchParams: (query: GetAllListingsQuery) => [...listingKeys.all(), query],
  byId: (id: string) => [...listingKeys.all(), id],
  score: (id: string) => [...listingKeys.all(), id, "score"],
  aiSummary: (id: string, language: string = "lt") => [
    ...listingKeys.all(),
    id,
    "ai-summary",
    language,
  ],
  comparisonAiSummary: (a: string, b: string, language: string = "lt") => [
    ...listingKeys.all(),
    "comparison-ai-summary",
    a,
    b,
    language,
  ],
  sellerInsights: (id: string) => [...listingKeys.all(), id, "seller-insights"],
  comparison: (a: string, b: string) => [
    ...listingKeys.all(),
    "comparison",
    a,
    b,
  ],
  search: (q: string) => [...listingKeys.all(), "search", q],
};
