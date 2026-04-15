export type GetAllListingsResponse = {
  id: string;
  isUsed: boolean;
  year: string;
  make: string;
  model: string;
  mileage: number;
  price: number;
  engineSizeMl: number;
  powerKw: number;
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
