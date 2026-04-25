export type GetListingByIdResponse = {
  id: string;
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
  municipalityId: string;
  municipalityName: string;
  isUsed: boolean;
  year: number;
  transmissionName: string;
  fuelName: string;
  doorCount: number;
  bodyTypeName: string;
  drivetrainName: string;
  sellerName: string;
  sellerId: string;
  status: string;
  images: Image[];
  defects: ListingDefectDto[];
};

export type ListingDefectDto = {
  id: string;
  defectCategoryId?: string;
  defectCategoryName?: string;
  customName?: string;
  images: { url: string; altText: string }[];
};

type Image = {
  url: string;
  altText: string;
};
