export type GetListingByIdResponse = {
  makeName: string;
  modelName: string;
  price: number;
  description?: string;
  colour?: string;
  vin?: string;
  powerKw: number;
  engineSizeMl: number;
  mileage: number;
  isSteeringWheelRight: boolean;
  city: string;
  isUsed: boolean;
  year: number;
  transmissionName: string;
  fuelName: string;
  doorCount: number;
  bodyTypeName: string;
  drivetrainName: string;
  sellerName: string;
  sellerId: string;
  images: Image[];
};

type Image = {
  url: string;
  altText: string;
};
