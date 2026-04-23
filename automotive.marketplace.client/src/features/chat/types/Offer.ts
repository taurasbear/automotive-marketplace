export type OfferStatus =
  | "Pending"
  | "Accepted"
  | "Declined"
  | "Countered"
  | "Expired";

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
