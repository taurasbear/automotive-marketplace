export type GetListingAiSummaryResponse = {
  summary: string | null;
  isGenerated: boolean;
  fromCache: boolean;
  unavailableFactors: string[];
};
