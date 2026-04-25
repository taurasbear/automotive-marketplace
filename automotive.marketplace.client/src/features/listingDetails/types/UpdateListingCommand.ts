export type UpdateListingCommand = {
  id: string;
  price: number;
  description?: string;
  colour?: string;
  vin?: string;
  powerKw: number;
  engineSizeMl: number;
  mileage: number;
  isSteeringWheelRight: boolean;
  municipalityId: string;
  isUsed: boolean;
  year?: number;
};
