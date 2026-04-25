export type ScoreFactor = {
  score: number;
  status: "scored" | "missing";
  weight: number;
};

export type GetListingScoreResponse = {
  overallScore: number;
  value: ScoreFactor;
  efficiency: ScoreFactor;
  reliability: ScoreFactor;
  mileage: ScoreFactor;
  hasMissingFactors: boolean;
  missingFactors: string[];
};
