export type OfferStatus =
  | "Pending"
  | "Accepted"
  | "Declined"
  | "Countered"
  | "Expired"
  | "Cancelled";

export type Offer = {
  id: string;
  amount: number;
  listingPrice: number;
  percentageOff: number;
  status: OfferStatus;
  expiresAt: string;
  initiatorId: string;
  parentOfferId?: string;
};
