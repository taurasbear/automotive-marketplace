export type GetListingByIdResponse = {
  make: string;
  model: string;
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
  transmission: string;
  fuel: string;
  doorCount: number;
  bodyType: string;
  drivetrain: string;
  seller: string;
  images: Image[];
};

type Image = {
  url: string;
  altText: string;
};
