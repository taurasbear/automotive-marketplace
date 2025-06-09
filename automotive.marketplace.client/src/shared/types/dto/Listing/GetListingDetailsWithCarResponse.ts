export interface GetListingsDetailsWithCarResponse {
  listingsDetailsWithCar: ListingDetailsWithCar[];
}

export interface ListingDetailsWithCar {
  used: boolean;
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
}
