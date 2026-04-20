export type SavedListing = {
  listingId: string;
  title: string;
  price: number;
  city: string;
  mileage: number;
  fuelName: string;
  transmissionName: string;
  thumbnail: { url: string; altText: string } | null;
  noteContent: string | null;
};
