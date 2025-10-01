export type GetAllListingsResponse = {
  id: string;
  isUsed: boolean;
  year: string;
  make: string;
  model: string;
  mileage: number;
  price: number;
  engineSize: number;
  power: number;
  fuelType: string;
  transmission: string;
  city: string;
  description: string;
  images: Image[];
};

type Image = {
  url: string;
  altText: string;
};
