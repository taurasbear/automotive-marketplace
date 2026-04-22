export type SearchListingsResponse = {
  id: string;
  makeName: string;
  modelName: string;
  year: number;
  price: number;
  mileage: number;
  city: string;
  sellerName: string;
  firstImageUrl?: string;
};
