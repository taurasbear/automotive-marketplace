export type GetListingComparisonAiSummaryResponse = {
  summary: string | null;
  isGenerated: boolean;
  fromCache: boolean;
  unavailableFactors: string[];
};
