type Image = {
  url: string;
  altText: string;
};

export type GetMyListingsResponse = {
  id: string;
  price: number;
  mileage: number;
  isUsed: boolean;
  city: string;
  status: string;
  year: number;
  makeName: string;
  modelName: string;
  thumbnail: Image | null;
  images: Image[];
  imageCount: number;
  defectCount: number;
  fuelName: string;
  transmissionName: string;
  engineSizeMl: number;
  powerKw: number;
  likeCount: number;
  conversationCount: number;
};
