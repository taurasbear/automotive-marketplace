export type CreateVariantCommand = {
  modelId: string;
  year: number;
  fuelId: string;
  transmissionId: string;
  bodyTypeId: string;
  isCustom: boolean;
  doorCount: number;
  powerKw: number;
  engineSizeMl: number;
};
