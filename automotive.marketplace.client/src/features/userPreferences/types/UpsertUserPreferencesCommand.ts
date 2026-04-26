export type UpsertUserPreferencesCommand = {
  valueWeight: number;
  efficiencyWeight: number;
  reliabilityWeight: number;
  mileageWeight: number;
  conditionWeight: number;
  autoGenerateAiSummary: boolean;
  enableVehicleScoring: boolean;
};
