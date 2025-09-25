export type UpdateListingCommand = {
  id: string;
  price: number;
  description?: string;
  colour?: string;
  vin?: string;
  power: number;
  engineSize: number;
  mileage: number;
  isSteeringWheelRight: boolean;
  city: string;
  isUsed: boolean;
  year: number;
};
