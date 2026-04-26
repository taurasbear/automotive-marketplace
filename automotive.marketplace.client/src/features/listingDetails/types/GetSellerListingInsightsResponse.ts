export type MarketPositionInsight = {
  listingPrice: number;
  marketMedianPrice: number | null;
  priceDifferencePercent: number | null;
  marketListingCount: number | null;
  daysListed: number;
  hasMarketData: boolean;
};

export type ListingQualityInsight = {
  qualityScore: number;
  hasDescription: boolean;
  hasPhotos: boolean;
  photoCount: number;
  hasVin: boolean;
  hasColour: boolean;
  suggestions: string[];
};

export type GetSellerListingInsightsResponse = {
  marketPosition: MarketPositionInsight;
  listingQuality: ListingQualityInsight;
};
