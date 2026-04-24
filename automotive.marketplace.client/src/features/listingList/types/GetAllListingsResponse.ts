export type GetAllListingsResponse = {
  id: string;
  isUsed: boolean;
  year: number;
  makeName: string;
  modelName: string;
  mileage: number;
  price: number;
  engineSizeMl: number;
  powerKw: number;
  fuelName: string;
  transmissionName: string;
  city: string;
  description: string;
  thumbnail: Thumbnail | null;
  isLiked: boolean;
  images: { url: string; altText: string }[];
  imageCount: number;
  defectCount: number;
};

type Thumbnail = {
  url: string;
  altText: string;
};
